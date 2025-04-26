using FattestInc.UI.API;
using FattestInc.Windows.General;
using Hypnagogia.Utils;

namespace FattestInc.UI.Implementation {
    public class CloseTopWindowButton : ButtonBehaviour {
        [HInject] WindowManager windowManager;

        protected override void OnClick() {
            windowManager.CloseWindowOnTop();
        }
    }
}