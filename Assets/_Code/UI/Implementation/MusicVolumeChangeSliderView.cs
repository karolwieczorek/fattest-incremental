using FattestInc.Audio;
using Hypnagogia.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.UI.Implementation {
    public class MusicVolumeChangeSliderView : MonoBehaviour {
        [HInject] AudioSettingsDataStore audioSettingsDataStore;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] Slider slider;

        void Start() {
            audioSettingsDataStore.MusicVolume.Bind(OnValueChanged);
        }

        void OnEnable() {
            slider.SetValueWithoutNotify(audioSettingsDataStore.MusicVolume.Value);
            slider.onValueChanged.AddListener(ChangeVolume);
        }

        void OnDisable() {
            slider.onValueChanged.RemoveListener(ChangeVolume);
        }

        void OnDestroy() {
            if (audioSettingsDataStore != null)
                audioSettingsDataStore.MusicVolume.Unbind(OnValueChanged);
        }

        void OnValueChanged(float value) {
            if (label != null)
                label.text = Mathf.RoundToInt(value * 100).ToString();
        }

        void ChangeVolume(float value) {
            audioSettingsDataStore.ChangeMusicVolume(value);
        }
    }
}