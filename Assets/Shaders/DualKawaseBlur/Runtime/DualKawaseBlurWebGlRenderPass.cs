using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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

    const string KBlurTextureName = "_BlurTexture";
    
    static readonly int BlendRatioId = Shader.PropertyToID("_BlendRatio");

    public DualKawaseBlurWebGlRenderPass(string featureName, DualKawaseBlurWebGlSettings settings) {
        sampler = new ProfilingSampler(featureName);
        renderPassEvent = settings.copyToFrameBuffer
            ? RenderPassEvent.BeforeRenderingPostProcessing
            : RenderPassEvent.AfterRenderingSkybox;
        this.settings = settings;

        // var graphicsFormat = SystemInfo.GetCompatibleFormat(mDescriptor.graphicsFormat, FormatUsage.SetPixels);//UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
        // Debug.Log($"mDescriptor.graphicsFormat: {graphicsFormat}");
        //
        // var format = mDescriptor.graphicsFormat;
        // Debug.Log($"Default format: {format}");
        // Debug.Log("Default format");
        // foreach (var renterTextureFormat in Enum.GetValues(typeof(RenderTextureFormat)).Cast<RenderTextureFormat>()) {
        //     var isSupported = SystemInfo.SupportsRenderTextureFormat(renterTextureFormat);
        //     if (isSupported)
        //         supportedFormat = renterTextureFormat;
        //     Debug.Log($"Format: {renterTextureFormat} is supported: {isSupported}");
        // }
    }

    public void Setup(DualKawaseBlurWebGl volumeComponent) {
        this.volumeComponent = volumeComponent;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
        // update camera color texture
        mCameraColorTexture = renderingData.cameraData.renderer.cameraColorTargetHandle;

        // update descriptor of blur textures
        mDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        mDescriptor.depthBufferBits = 0;
        mDescriptor.msaaSamples = 1;
        mDescriptor.useMipMap = false;
        mDescriptor.colorFormat = supportedFormat;
        // mDescriptor.graphicsFormat = SystemInfo.GetCompatibleFormat(mDescriptor.graphicsFormat, FormatUsage.Blend);

        mDescriptor.graphicsFormat = SystemInfo.GetCompatibleFormat(GraphicsFormat.R16G16B16A16_UNorm, FormatUsage.Sample);
        mDescriptor.colorFormat = RenderTextureFormat.Default;
        
        // mDescriptor.graphicsFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Default);//UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
        // Debug.Log($"mDescriptor.graphicsFormat: {mDescriptor.graphicsFormat}");
        mDescriptor.enableRandomWrite = true;

        // update screen size
        mOriginalSize = new Vector2Int(mDescriptor.width, mDescriptor.height);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        var cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, sampler)) {
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
            var finalTextureID = Shader.PropertyToID(settings.targetTextureName);
            var tempfinalRT = RenderTexture.GetTemporary(mDescriptor.width, mDescriptor.height, 0);
            // keep track
            textureIDs.Add(tempfinalRT);
            textureSizes.Add(mOriginalSize);

            // downsample blur
            var sourceTextureSize = mOriginalSize;
            var sourceTextureID = mCameraColorTexture.nameID;
            for (var i = 0; i <= blurIterations; i++) {
                // create a new target texture
                // ---------------------------
                // var targetTextureID = Shader.PropertyToID(KBlurTextureName + i);
                // plus one is necessary to zero thread group count
                Vector2Int targetTextureSize = new((sourceTextureSize.x + 1) / 2, (sourceTextureSize.y + 1) / 2);
                mDescriptor.width = targetTextureSize.x;
                mDescriptor.height = targetTextureSize.y;
                RenderTexture tempBlendRT = RenderTexture.GetTemporary(targetTextureSize.x, targetTextureSize.y, 0);
                // keep track
                textureIDs.Add(tempBlendRT);
                textureSizes.Add(targetTextureSize);

                // do the kawase blur
                DownSampleBlur(cmd, sourceTextureID, tempBlendRT, targetTextureSize);

                // update the last size and ID
                sourceTextureSize = targetTextureSize;
                sourceTextureID = tempBlendRT;
            }

            // upsample
            if (blurIterations != 0) {
                // create an intermediate texture for linear lerp,
                // which has the same size with the last second downsample texture
                var tempTextureSize = textureSizes[blurIterations];
                mDescriptor.width = tempTextureSize.x;
                mDescriptor.height = tempTextureSize.y;
                var tempRT = RenderTexture.GetTemporary(mDescriptor.width, mDescriptor.height, 0);
                

                for (var i = blurIterations + 1; i >= 1; i--) {
                    var sourceID = textureIDs[i];
                    var targetID = i == blurIterations + 1 ? tempRT : textureIDs[i - 1];
                    var targetSize = textureSizes[i - 1];

                    // do the kawase blur
                    UpSampleBlur(cmd, sourceID, targetID, targetSize);

                    // do the linear lerp
                    if (i == blurIterations + 1) {
                        Linear(cmd, textureIDs[i - 1], tempRT, targetSize, ratio);
                        // swap the texture IDs
                        (tempRT, textureIDs[i - 1]) = (textureIDs[i - 1], tempRT);
                    }

                    RenderTexture.ReleaseTemporary(sourceID);
                }

                RenderTexture.ReleaseTemporary(tempRT);
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
            cmd.SetGlobalTexture("_MainTex", source);
            var mat = settings.blurDownMaterial;
            var textureSizeParams = GetTextureSizeParams(targetSize);
            mat.SetVector("_TexelSize", textureSizeParams);
            cmd.Blit(source, target, mat);
        }
    }

    void UpSampleBlur(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target,
        Vector2Int targetSize) {
        using (new ProfilingScope(cmd, new ProfilingSampler("UpSample Blur"))) {
            cmd.SetGlobalTexture("_MainTex", source);
            var textureSizeParams = GetTextureSizeParams(targetSize);
            var mat = settings.blurUpMaterial;
            mat.SetVector("_TexelSize", textureSizeParams);
            cmd.Blit(source, target, mat);
        }
    }
    
    void Linear(CommandBuffer cmd, RenderTargetIdentifier original, RenderTargetIdentifier blurred, Vector2Int size, float ratio)
    {
        using (new ProfilingScope(cmd, new ProfilingSampler("Linear Blend")))
        {
            int width = size.x;
            int height = size.y;
            int tempOrigID = Shader.PropertyToID("_TempOrigTex");
            cmd.Blit(original, tempOrigID);
            var tempBlendRT = RenderTexture.GetTemporary(width, height, 0);
            cmd.Blit(blurred, tempBlendRT);
            
            cmd.SetGlobalTexture("_MainTex", tempOrigID);
            
            var mat = settings.blurLinearMaterial;
            mat.SetFloat("_BlendRatio", ratio);
            mat.SetVector("_TexelSize", GetTextureSizeParams(size));

            cmd.SetGlobalTexture("_BlurTex", tempBlendRT);

            cmd.Blit(tempOrigID, blurred, mat);

            RenderTexture.ReleaseTemporary(tempBlendRT);
            cmd.ReleaseTemporaryRT(tempOrigID);
        }
    }
}