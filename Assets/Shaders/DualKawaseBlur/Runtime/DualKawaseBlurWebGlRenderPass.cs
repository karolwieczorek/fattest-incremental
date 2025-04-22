using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DualKawaseBlurWebGlRenderPass : ScriptableRenderPass {
    DualKawaseBlurWebGl volumeComponent;
    readonly ProfilingSampler sampler;
    readonly DualKawaseBlurWebGlSettings settings;

    RenderTextureFormat supportedFormat;
    RTHandle mCameraColorTexture;
    RenderTextureDescriptor mDescriptor;

    Vector2Int mOriginalSize;

    const string BlurTextureName = "_BlurTexture";
    
    static readonly int BlendRatioId = Shader.PropertyToID("_BlendRatio");
    static readonly int TexelSizeId = Shader.PropertyToID("_TexelSize");
    static readonly int BlurTexId = Shader.PropertyToID("_BlurTex");
    static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    readonly List<RenderTexture> mTemporaryRTs = new();

    public DualKawaseBlurWebGlRenderPass(string featureName, DualKawaseBlurWebGlSettings settings) {
        sampler = new ProfilingSampler(featureName);
        renderPassEvent = settings.copyToFrameBuffer
            ? RenderPassEvent.BeforeRenderingPostProcessing
            : RenderPassEvent.AfterRenderingSkybox;
        this.settings = settings;
    }

    public void Setup(DualKawaseBlurWebGl volumeComponent) {
        this.volumeComponent = volumeComponent;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
        // update camera color texture
        mCameraColorTexture = renderingData.cameraData.renderer.cameraColorTargetHandle;
        
        // update descriptor of blur textures
        // ----------------------------------
        mDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        mDescriptor.depthBufferBits = 0;
        mDescriptor.msaaSamples = 1;
        mDescriptor.enableRandomWrite = true;
        
        // update screen size
        // ------------------
        mOriginalSize = new Vector2Int(mDescriptor.width, mDescriptor.height);
    }

    RenderTexture GetTemporaryRT(RenderTextureDescriptor desc, string name) {
        desc.width = Mathf.Max(1, desc.width); // Ensure width is at least 1
        desc.height = Mathf.Max(1, desc.height); // Ensure height is at least 1
        
        // important: For some reason WebGL really did not liked creating render texture with descriptor, 
        // I did not dig to the bottom of why was it, the current solution is to create RenderTexture with just width and height 
        
        // desc.graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
        // RenderTexture rt = RenderTexture.GetTemporary(desc);
        RenderTexture rt = RenderTexture.GetTemporary(desc.width, desc.height);
        rt.name = name; // Assign name for debugging
        mTemporaryRTs.Add(rt);
        return rt;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        var cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, sampler)) {
            ReleaseAllTemporaryRTs();

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // figure out blur iterations and blending ratio
            // ---------------------------------------------
            var blurFactor = volumeComponent.blurRadius.value * volumeComponent.blurIntensity.value + 1.0f;
            var blurAmount = Mathf.Log(blurFactor, 2.0f);
            var blurIterations = Mathf.FloorToInt(blurAmount);
            var ratio = blurAmount - blurIterations;

            // create lists to store temporary textures and sizes
            List<RenderTexture> textureIDs = new();
            List<Vector2Int> textureSizes = new();

            // create final target blur texture
            var finalTextureID = GetTemporaryRT(mDescriptor, settings.targetTextureName);

            // keep track
            textureIDs.Add(finalTextureID);
            textureSizes.Add(mOriginalSize);

            // downsample blur
            // ---------------
            var sourceTextureSize = mOriginalSize;
            var sourceTextureID = mCameraColorTexture.nameID;
            for (var i = 0; i <= blurIterations; i++) {
                // create a new target texture
                // ---------------------------

                // plus one is necessary to zero thread group count
                Vector2Int targetTextureSize = new((sourceTextureSize.x + 1) / 2, (sourceTextureSize.y + 1) / 2);
                mDescriptor.width = targetTextureSize.x;
                mDescriptor.height = targetTextureSize.y;
                var targetTextureID = GetTemporaryRT(mDescriptor, $"{BlurTextureName}" + i);
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
                var tempTextureSize = textureSizes[blurIterations];
                mDescriptor.width = tempTextureSize.x;
                mDescriptor.height = tempTextureSize.y;
                var tempTextureID =
                    GetTemporaryRT(mDescriptor,
                        $"{BlurTextureName}{(blurIterations + 1)}"); //RenderTexture.GetTemporary(mDescriptor.width, mDescriptor.height, 0);

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
                }
            }
            else {
                UpSampleBlur(cmd, textureIDs[1], textureIDs[0], textureSizes[0]);
                Linear(cmd, mCameraColorTexture.nameID, textureIDs[0], textureSizes[0], ratio);
            }

            // blit the final result
            if (settings.copyToFrameBuffer) {
                cmd.Blit(finalTextureID, mCameraColorTexture.nameID);
            }
            else {
                cmd.SetGlobalTexture(settings.targetTextureName, finalTextureID);
            }
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
        ReleaseAllTemporaryRTs();
    }

    Vector4 GetTextureSizeParams(Vector2Int size) {
        return new Vector4(1.0f / size.x, 1.0f / size.y);
    }

    void DownSampleBlur(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target,
        Vector2Int targetSize) {
        using (new ProfilingScope(cmd, new ProfilingSampler("DownSample Blur"))) {
            cmd.SetGlobalTexture(MainTexId, source);
            var mat = settings.blurDownMaterial;
            var textureSizeParams = GetTextureSizeParams(targetSize);
            mat.SetVector(TexelSizeId, textureSizeParams);
            cmd.SetGlobalVector(TexelSizeId, textureSizeParams);
            cmd.Blit(source, target, mat);
        }
    }

    void UpSampleBlur(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target,
        Vector2Int targetSize) {
        using (new ProfilingScope(cmd, new ProfilingSampler("UpSample Blur"))) {
            cmd.SetGlobalTexture(MainTexId, source);
            var textureSizeParams = GetTextureSizeParams(targetSize);
            var mat = settings.blurUpMaterial;
            mat.SetVector(TexelSizeId, textureSizeParams);
            cmd.Blit(source, target, mat);
        }
    }

    void Linear(CommandBuffer cmd, RenderTargetIdentifier original, RenderTargetIdentifier blurred, Vector2Int size,
        float ratio) {
        using (new ProfilingScope(cmd, new ProfilingSampler("Linear Blend"))) {
            cmd.SetGlobalTexture(MainTexId, original);
            var blurredRT = GetTemporaryRT(mDescriptor, "_BlurTex");
            cmd.Blit(blurred, blurredRT);
            var mat = settings.blurLinearMaterial;
            mat.SetFloat(BlendRatioId, ratio);
            mat.SetVector(TexelSizeId, GetTextureSizeParams(size));
            mat.SetTexture(BlurTexId, blurredRT);
            cmd.Blit(original, blurred, mat);
        }
    }

    void ReleaseAllTemporaryRTs() {
        foreach (var rt in mTemporaryRTs) {
            if (rt != null) {
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        mTemporaryRTs.Clear();
    }

    public override void OnCameraCleanup(CommandBuffer cmd) {
        base.OnCameraCleanup(cmd);
        ReleaseAllTemporaryRTs();
    }
}