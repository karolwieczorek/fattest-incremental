using Hypnagogia.Utils;
using UnityEngine;
using UnityEngine.Audio;

namespace FattestInc.Audio {
    public class AudioReferencer : HReferencer {
        [SerializeField] AudioMixer audioMixer;

        [SerializeField] AudioMixerGroup masterAudioMixerGroup;
        [SerializeField] AudioMixerGroup musicAudioMixerGroup;
        [SerializeField] AudioMixerGroup sfxAudioMixerGroup;

        public void SetMusicMixerGroupVolume(float volume) {
            audioMixer.SetFloat("MusicVolume", ConvertValueToMixerValue(volume));
        }

        public void SetSfxMixerGroupVolume(float volume) {
            audioMixer.SetFloat("SfxVolume", ConvertValueToMixerValue(volume));
        }

        public void SetMuted(bool isMuted) {
            SetMasterMixerGroupVolume(isMuted ? 0f : 1f);
        }

        void SetMasterMixerGroupVolume(float volume) {
            audioMixer.SetFloat("MasterVolume", ConvertValueToMixerValue(volume));
        }

        float ConvertValueToMixerValue(float value) {
            return value * 80f - 80f; // TODO better calculation (log)
        }
    }
}