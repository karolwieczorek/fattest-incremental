using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Custom/Dual Kawase Blur WebGL")]
public class DualKawaseBlurWebGl : VolumeComponent, IPostProcessComponent {
    [Space] public ClampedFloatParameter blurRadius = new(32.0f, 0.0f, 255.0f);
    public ClampedFloatParameter blurIntensity = new(0.0f, 0.0f, 1.0f);

    public bool IsActive() => active && blurIntensity.value > 0.0f;
    public bool IsTileCompatible() => false;
}