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
            audioMixer.SetFloat("MusicVolume", volume);
        }

        public void SetSfxMixerGroupVolume(float volume) {
            audioMixer.SetFloat("SfxVolume", volume);
        }

        public void SetMuted(bool isMuted) {
            SetMasterMixerGroupVolume(isMuted ? 0f : -80f);
        }

        void SetMasterMixerGroupVolume(float volume) {
            audioMixer.SetFloat("MasterVolume", volume);
        }
    }
}