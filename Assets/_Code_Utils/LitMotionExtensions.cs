using LitMotion;
using UnityEngine;

namespace Hypnagogia.Utils {
    public static class LitMotionExtensions {
        public static MotionHandle DOScale(this Transform target, Vector3 endValue, float duration, Ease ease = Ease.Linear) {
            var handle = LMotion.Create(target.localScale, endValue, duration).WithEase(ease);
            MotionHandle motionHandle = handle.Bind(x => target.localScale = x);
            return motionHandle;
        }
    }
}