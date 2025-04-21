using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Custom/Dual Kawase Blur")]
public class DualKawaseBlur : VolumeComponent, IPostProcessComponent {
    [Space] public BoolParameter enable = new(false);
    [Space] public ClampedFloatParameter blurRadius = new(32.0f, 0.0f, 255.0f);
    public ClampedFloatParameter blurIntensity = new(0.0f, 0.0f, 1.0f);

    public bool IsActive() => active && enable.value && blurIntensity.value > 0.0f;
    public bool IsTileCompatible() => false;
}