using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DualKawaseBlurWebGlSettings {
    public bool copyToFrameBuffer = true;
    public string targetTextureName = "_BlurTexture";

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