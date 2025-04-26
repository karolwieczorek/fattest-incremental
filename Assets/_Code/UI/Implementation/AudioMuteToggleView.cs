using FattestInc.Audio;
using Hypnagogia.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.UI.Implementation {
    public class AudioMuteToggleView : MonoBehaviour {
        [HInject] AudioSettingsDataStore audioSettingsDataStore;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] Toggle toggle;

        void Start() {
            audioSettingsDataStore.IsMuted.Bind(OnValueChanged);
        }

        void OnEnable() {
            toggle.isOn = (audioSettingsDataStore.IsMuted.Value);
            toggle.onValueChanged.AddListener(MutedValueChanged);
        }

        void OnDisable() {
            toggle.onValueChanged.RemoveListener(MutedValueChanged);
        }

        void OnDestroy() {
            if (audioSettingsDataStore != null)
                audioSettingsDataStore.IsMuted.Unbind(OnValueChanged);
        }

        void OnValueChanged(bool isMuted) {
            // if (label != null)
            //     label.text = Mathf.RoundToInt(value * 100).ToString();
        }

        void MutedValueChanged(bool isMuted) {
            audioSettingsDataStore.ChangeMuteState(isMuted);
        }
    }
}