using FattestInc.UI;
using FattestInc.UI.API;
using Hypnagogia.Utils;

namespace FattestInc.Windows.General {
    public class CloseTopWindowButton : ButtonBehaviour {
        [HInject] WindowManager windowManager;

        protected override void OnClick() {
            windowManager.CloseWindowOnTop();
        }
    }
}