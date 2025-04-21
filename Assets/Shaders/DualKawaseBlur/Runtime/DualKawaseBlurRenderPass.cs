using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DualKawaseBlurRenderPass : ScriptableRenderPass {
    DualKawaseBlur mVolumeComponent;
    readonly ProfilingSampler mProfilingSampler;
    readonly DualKawaseBlurSettings mSettings;

    // pass shader related
    readonly ComputeShader mPassShader;
    readonly int mDownSampleKernel;
    readonly int mUpSampleKernel;

    readonly int mBlendKernel;

    // render pass related
    RTHandle mCameraColorTexture;
    RenderTextureDescriptor mDescriptor;

    Vector2Int mOriginalSize;

    const string KBlurTextureName = "_BlurTexture";

    static readonly int SourceTextureId = Shader.PropertyToID("_SourceTexture");
    static readonly int TargetTextureId = Shader.PropertyToID("_TargetTexture");
    static readonly int TargetSizeId = Shader.PropertyToID("_TargetSize");
    static readonly int BlendRatioId = Shader.PropertyToID("_BlendRatio");

    public DualKawaseBlurRenderPass(string featureName, DualKawaseBlurSettings settings) {
        mProfilingSampler = new ProfilingSampler(featureName);
        renderPassEvent = settings.m_CopyToFrameBuffer
            ? RenderPassEvent.BeforeRenderingPostProcessing
            : RenderPassEvent.AfterRenderingSkybox;
        mSettings = settings;

        mPassShader = settings.m_DualKawaseBlurShader;
        mDownSampleKernel = mPassShader.FindKernel("DownSampleBlur");
        mUpSampleKernel = mPassShader.FindKernel("UpSampleBlur");
        mBlendKernel = mPassShader.FindKernel("LinearLerp");
    }

    public void Setup(DualKawaseBlur volumeComponent) {
        mVolumeComponent = volumeComponent;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
        // update camera color texture
        mCameraColorTexture = renderingData.cameraData.renderer.cameraColorTargetHandle;

        // update descriptor of blur textures
        mDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        mDescriptor.depthBufferBits = 0;
        mDescriptor.msaaSamples = 1;
        mDescriptor.enableRandomWrite = true;

        // update screen size
        mOriginalSize = new Vector2Int(mDescriptor.width, mDescriptor.height);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        var cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, mProfilingSampler)) {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // figure out blur iterations and blending ratio
            // ---------------------------------------------
            var blurFactor = mVolumeComponent.blurRadius.value * mVolumeComponent.blurIntensity.value + 1.0f;
            var blurAmount = Mathf.Log(blurFactor, 2.0f);
            var blurIterations = Mathf.FloorToInt(blurAmount);
            var ratio = blurAmount - blurIterations;

            // create lists to store temporary textures and sizes
            List<int> textureIDs = new();
            List<Vector2Int> textureSizes = new();

            // create final target blur texture
            var finalTextureID = Shader.PropertyToID(mSettings.m_TargetTextureName);
            cmd.GetTemporaryRT(finalTextureID, mDescriptor);
            // keep track
            textureIDs.Add(finalTextureID);
            textureSizes.Add(mOriginalSize);

            // downsample blur
            var sourceTextureSize = mOriginalSize;
            var sourceTextureID = mCameraColorTexture.nameID;
            for (var i = 0; i <= blurIterations; i++) {
                // create a new target texture
                // ---------------------------
                var targetTextureID = Shader.PropertyToID(KBlurTextureName + i);
                // plus one is necessary to zero thread group count
                Vector2Int targetTextureSize = new((sourceTextureSize.x + 1) / 2, (sourceTextureSize.y + 1) / 2);
                mDescriptor.width = targetTextureSize.x;
                mDescriptor.height = targetTextureSize.y;
                cmd.GetTemporaryRT(targetTextureID, mDescriptor);
                // keep track
                textureIDs.Add(targetTextureID);
                textureSizes.Add(targetTextureSize);

                // do the kawase blur
                DownSampleBlur(cmd, sourceTextureID, targetTextureID, targetTextureSize);

                // update the last size and ID
                sourceTextureSize = targetTextureSize;
                sourceTextureID = targetTextureID;
            }

            // upsample
            if (blurIterations != 0) {
                // create an intermediate texture for linear lerp,
                // which has the same size with the last second downsample texture
                var tempTextureID = Shader.PropertyToID(KBlurTextureName + (blurIterations + 1));
                var tempTextureSize = textureSizes[blurIterations];
                mDescriptor.width = tempTextureSize.x;
                mDescriptor.height = tempTextureSize.y;
                cmd.GetTemporaryRT(tempTextureID, mDescriptor);

                for (var i = blurIterations + 1; i >= 1; i--) {
                    var sourceID = textureIDs[i];
                    var targetID = i == blurIterations + 1 ? tempTextureID : textureIDs[i - 1];
                    var targetSize = textureSizes[i - 1];

                    // do the kawase blur
                    UpSampleBlur(cmd, sourceID, targetID, targetSize);

                    // do the linear lerp
                    if (i == blurIterations + 1) {
                        Linear(cmd, textureIDs[i - 1], tempTextureID, targetSize, ratio);
                        // swap the texture IDs
                        (tempTextureID, textureIDs[i - 1]) = (textureIDs[i - 1], tempTextureID);
                    }

                    // release current temporary texture
                    cmd.ReleaseTemporaryRT(sourceID);
                }

                // release the intermediate texture
                cmd.ReleaseTemporaryRT(tempTextureID);
            }
            else {
                UpSampleBlur(cmd, textureIDs[1], textureIDs[0], textureSizes[0]);
                Linear(cmd, mCameraColorTexture.nameID, textureIDs[0], textureSizes[0], ratio);
            }

            // blit the final result
            if (mSettings.m_CopyToFrameBuffer) {
                cmd.Blit(finalTextureID, mCameraColorTexture.nameID);
            }
            else {
                cmd.SetGlobalTexture(mSettings.m_TargetTextureName, finalTextureID);
            }

            cmd.ReleaseTemporaryRT(finalTextureID);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    Vector4 GetTextureSizeParams(Vector2Int size) {
        return new Vector4(1.0f / size.x, 1.0f / size.y);
    }

    void DownSampleBlur(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target,
        Vector2Int targetSize) {
        using (new ProfilingScope(cmd, new ProfilingSampler("DownSample Blur"))) {
            // pass data to shader
            cmd.SetComputeTextureParam(mPassShader, mDownSampleKernel, SourceTextureId, source);
            cmd.SetComputeTextureParam(mPassShader, mDownSampleKernel, TargetTextureId, target);
            cmd.SetComputeVectorParam(mPassShader, TargetSizeId, GetTextureSizeParams(targetSize));

            // dispatch shader
            mPassShader.GetKernelThreadGroupSizes(mDownSampleKernel, out uint x, out uint y, out uint _);
            int threadGroupX = Mathf.CeilToInt((float) targetSize.x / x);
            int threadGroupY = Mathf.CeilToInt((float) targetSize.y / y);
            cmd.DispatchCompute(mPassShader, mDownSampleKernel, threadGroupX, threadGroupY, 1);
        }
    }

    void UpSampleBlur(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target,
        Vector2Int targetSize) {
        using (new ProfilingScope(cmd, new ProfilingSampler("UpSample Blur"))) {
            // pass data to shader
            cmd.SetComputeTextureParam(mPassShader, mUpSampleKernel, SourceTextureId, source);
            cmd.SetComputeTextureParam(mPassShader, mUpSampleKernel, TargetTextureId, target);
            cmd.SetComputeVectorParam(mPassShader, TargetSizeId, GetTextureSizeParams(targetSize));

            // dispatch shader
            mPassShader.GetKernelThreadGroupSizes(mUpSampleKernel, out uint x, out uint y, out uint _);
            int threadGroupX = Mathf.CeilToInt((float) targetSize.x / x);
            int threadGroupY = Mathf.CeilToInt((float) targetSize.y / y);
            cmd.DispatchCompute(mPassShader, mUpSampleKernel, threadGroupX, threadGroupY, 1);
        }
    }

    void Linear(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target,
        Vector2Int sourceSize, float ratio) {
        using (new ProfilingScope(cmd, new ProfilingSampler("Linear Blend"))) {
            // pass data to shader
            cmd.SetComputeTextureParam(mPassShader, mBlendKernel, SourceTextureId, source);
            cmd.SetComputeTextureParam(mPassShader, mBlendKernel, TargetTextureId, target);
            cmd.SetComputeFloatParam(mPassShader, BlendRatioId, ratio);

            // dispatch shader
            mPassShader.GetKernelThreadGroupSizes(mBlendKernel, out uint x, out uint y, out uint _);
            var threadGroupX = Mathf.CeilToInt((float) sourceSize.x / x);
            var threadGroupY = Mathf.CeilToInt((float) sourceSize.y / y);
            cmd.DispatchCompute(mPassShader, mBlendKernel, threadGroupX, threadGroupY, 1);
        }
    }
}