using FattestInc.UI.API;
using FattestInc.Windows.General;
using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc.UI.Implementation {
    public class CloseThisWindowButton : ButtonBehaviour {
        [HInject] WindowManager windowManager;
        [SerializeField] WindowBase targetWindow;

        protected override void Reset() {
            base.Reset();
            targetWindow = gameObject.GetComponentInParent<WindowBase>();
        }

        void OnValidate() {
            if (targetWindow is not ISimpleWindowClose) {
                Debug.LogError($"Window is not {nameof(ISimpleWindowClose)}");
                targetWindow = null;
            }
        }

        protected override void OnClick() {
            if (targetWindow is ISimpleWindowClose simpleWindowClose)
                windowManager.TryCloseWindow(simpleWindowClose as WindowBase);
        }
    }
}