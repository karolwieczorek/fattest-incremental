using System;
using Hypnagogia.Utils;
using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FattestInc.UI.API {
    // <see href="https://github.com/annulusgames/LitMotion?tab=readme-ov-file">LitMotion</see> 
    public class HUiEffectTweenScale : HUiEffect {
        public enum ScaleTarget { Self, Transform }

        [SerializeField] ScaleTarget scaleTarget = ScaleTarget.Self;
        [ShowIf(nameof(scaleTarget), ScaleTarget.Transform)]
        [SerializeField] Transform otherTransform;

        Vector3 originalScale;
        [SerializeField] float scaleFactorHover = 1.05f;
        [SerializeField] float scaleFactorClick = 0.9f;
        [SerializeField] float animationDuration = 0.1f;
        [SerializeField] Ease easeType = Ease.OutBack;

        MotionHandle scaleHandle;

        Transform TargetToScale =>
            scaleTarget switch {
                ScaleTarget.Self => transform,
                ScaleTarget.Transform => otherTransform,
                _ => throw new ArgumentOutOfRangeException()
            };

        void Awake() {
            originalScale = transform.localScale;
        }

        void CancelAndCleanUp() {
            if (scaleHandle.IsActive() || scaleHandle.IsPlaying())
                scaleHandle.Cancel();
        } 

        public override void OnPointerDown(PointerEventData eventData) {
            CancelAndCleanUp();
            scaleHandle = TargetToScale.DOScale(originalScale * scaleFactorClick, animationDuration, easeType);
        }

        public override void OnPointerUp(PointerEventData eventData) {
            CancelAndCleanUp();
            scaleHandle = TargetToScale.DOScale(originalScale, animationDuration, easeType);
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            CancelAndCleanUp();
            scaleHandle = TargetToScale.DOScale(originalScale * scaleFactorHover, animationDuration, easeType);
        }

        public override void OnPointerExit(PointerEventData eventData) {
            CancelAndCleanUp();
            scaleHandle = TargetToScale.DOScale(originalScale, animationDuration, easeType);
        }
    }
}