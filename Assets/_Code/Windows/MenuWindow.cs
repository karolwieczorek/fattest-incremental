using FattestInc.Windows.General;
using TMPro;
using UnityEngine;

namespace FattestInc.Windows {
    public class MenuWindow : SimpleWindow {
        [SerializeField] TMP_Text versionLabel;

        public void WindowApiShow() {
            versionLabel.text = $"Version {Application.version}";
        }
    }
}