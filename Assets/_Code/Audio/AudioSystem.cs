using Hypnagogia.Utils;

namespace FattestInc.Audio {
    public class AudioSystem : HSystem {
        [HInject] AudioReferencer audioReferencer;
        [HInject] AudioSettingsDataStore audioSettingsDataStore;

        protected override void SystemStart() {
            base.SystemStart();

            audioSettingsDataStore.MusicVolume.Bind(OnMusicVolumeChanged);
            audioSettingsDataStore.SfxVolume.Bind(OnSfxVolumeChanged);
            audioSettingsDataStore.IsMuted.Bind(OnMutedStateChanged);
        }

        protected override void SystemStop() {
            base.SystemStop();

            audioSettingsDataStore.MusicVolume.Unbind(OnMusicVolumeChanged);
            audioSettingsDataStore.SfxVolume.Unbind(OnSfxVolumeChanged);
            audioSettingsDataStore.IsMuted.Unbind(OnMutedStateChanged);
        }

        void OnMusicVolumeChanged(float volume) {
            audioReferencer.SetMusicMixerGroupVolume(volume);
        }

        void OnSfxVolumeChanged(float volume) {
            audioReferencer.SetSfxMixerGroupVolume(volume);
        }

        void OnMutedStateChanged(bool isMuted) {
            audioReferencer.SetMuted(isMuted);
        }
    }
}