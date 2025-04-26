using FattestInc.Windows.General;
using Hypnagogia.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.Windows {
    public class MenuWindow : SimpleWindow {
        [SerializeField] TMP_Text versionLabel;
        [SerializeField] Button saveButton;
        [SerializeField] Button loadButton;
        [SerializeField] Slider musicSlider;

        [HInject] SaveHelper saveHelper;
        [HInject] WindowManager windowManager;


        void OnEnable() {
            WindowApiShow();
            saveButton.onClick.AddListener(SaveButtonClicked);
            loadButton.onClick.AddListener(LoadButtonClicked);
            // musicSlider.value
        }

        void OnDisable() {
            saveButton.onClick.RemoveListener(SaveButtonClicked);
            loadButton.onClick.RemoveListener(LoadButtonClicked);
        }

        void SaveButtonClicked() {
            saveHelper.SaveGame();
        }

        void LoadButtonClicked() {
            saveHelper.LoadGame();
            windowManager.TryCloseWindow(this);
        }

        public void WindowApiShow() {
            versionLabel.text = $"Version {Application.version}";
        }
    }
}