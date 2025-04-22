using System;
using System.Collections.Generic;
using System.Linq;
using FattestInc.Windows.General;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.Windows {
    public class MenuWindow : SimpleWindow {
        [SerializeField] TMP_Text versionLabel;
        [SerializeField] TMP_Dropdown colorDropdown;
        [SerializeField] TMP_Dropdown renderDropdown;
        [SerializeField] Button button;

        void Awake() {
            button.onClick.AddListener(ButtonClicked);
            renderDropdown.options.Clear();
            foreach (var renderTextureFormat in Enum.GetValues(typeof(RenderTextureFormat)).Cast<RenderTextureFormat>()) {
                renderDropdown.options.Add(new TMP_Dropdown.OptionData(renderTextureFormat.ToString()));
            }
            
        }

        void ButtonClicked() {
            var renderTextureFormat = Enum.Parse<RenderTextureFormat>(renderDropdown.options[renderDropdown.value].text);
            Debug.Log($"Creating RT with format: {renderTextureFormat}");
            var tempBlendRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, renderTextureFormat);
            RenderTexture.ReleaseTemporary(tempBlendRT);
        }


        public void WindowApiShow() {
            versionLabel.text = $"Version {Application.version}";
        }
    }
}