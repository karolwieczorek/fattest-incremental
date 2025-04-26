using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DualKawaseBlurWebGlSettings {
    public bool copyToFrameBuffer = true;
    public string targetTextureName = "_BlurTexture";
    
    public bool m_CopyToFrameBuffer = true; // Or false if setting global texture
    public string m_TargetTextureName = "_BlurTexture"; // If not copying to frame buffer
    // public string m_TargetTextureName = "_BlurResultTexture"; // If not copying to frame buffer


    public Material blurDownMaterial;
    public Material blurUpMaterial;
    public Material blurLinearMaterial;
}

public class DualKawaseBlurWebGlRenderFeature : ScriptableRendererFeature {
    [SerializeField] DualKawaseBlurWebGlSettings settings = new();
    DualKawaseBlurWebGlRenderPass pass;

    public override void Create() {
        if (settings.blurDownMaterial == null)
            return;
        if (settings.blurUpMaterial == null)
            return;
        if (settings.blurLinearMaterial == null)
            return;

        pass = new DualKawaseBlurWebGlRenderPass(name, settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        DualKawaseBlurWebGl volumeComponent = VolumeManager.instance.stack.GetComponent<DualKawaseBlurWebGl>();
        if (!volumeComponent || !volumeComponent.IsActive())
            return;
        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        pass.Setup(volumeComponent);
        renderer.EnqueuePass(pass);
    }
}