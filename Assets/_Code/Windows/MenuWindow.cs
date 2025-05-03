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
        [SerializeField] Button resetButton;

        [HInject] SaveHelper saveHelper;
        [HInject] ScenesLoaderHelper scenesLoaderHelper;
        [HInject] WindowManager windowManager;


        void OnEnable() {
            WindowApiShow();
            saveButton.onClick.AddListener(SaveButtonClicked);
            loadButton.onClick.AddListener(LoadButtonClicked);
            resetButton.onClick.AddListener(ResetButtonClicked);
            // musicSlider.value
        }

        void OnDisable() {
            saveButton.onClick.RemoveListener(SaveButtonClicked);
            loadButton.onClick.RemoveListener(LoadButtonClicked);
            resetButton.onClick.RemoveListener(ResetButtonClicked);
        }

        void WindowApiShow() {
            versionLabel.text = $"Version {Application.version}";
        }

        void SaveButtonClicked() {
            saveHelper.SaveGame();
        }

        void LoadButtonClicked() {
            saveHelper.LoadGame();
            windowManager.TryCloseWindow(this);
        }

        void ResetButtonClicked() {
            saveHelper.DeleteSave();
            scenesLoaderHelper.RestartGame();
        }
    }
}