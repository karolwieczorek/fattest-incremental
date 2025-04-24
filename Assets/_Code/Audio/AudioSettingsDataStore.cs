using Hypnagogia.Utils;
using Utils.PlayerPrefUtils;
using Zenject;

namespace FattestInc.Audio {
    public class AudioSettingsDataStore : HDataStore, IInitializable {
        const string MusicVolumeKey = nameof(MusicVolumeKey);
        const string SfxVolumeKey = nameof(SfxVolumeKey);
        const string MuteKey = nameof(MuteKey);

        readonly PlayerPrefFloat musicVolumePref = new(MusicVolumeKey, .5f);
        readonly PlayerPrefFloat sfxVolumePref = new(SfxVolumeKey, .5f);
        readonly PlayerPrefBool mutePref = new(MuteKey, false);

        Observable<float> musicVolumeObservable;
        Observable<float> sfxVolumeObservable;
        Observable<bool> mutedObservable;

        public IReadOnlyObservable<float> MusicVolume => musicVolumeObservable;
        public IReadOnlyObservable<float> SfxVolume => sfxVolumeObservable;
        public IReadOnlyObservable<bool> IsMuted => mutedObservable;

        public void Initialize() {
            musicVolumeObservable = new Observable<float>(musicVolumePref.Value);
            sfxVolumeObservable = new Observable<float>(sfxVolumePref.Value);
            mutedObservable = new Observable<bool>(mutePref.Value);
        }

        public void ChangeMusicVolume(float newVolume) {
            musicVolumePref.Value = newVolume;
            musicVolumeObservable.Value = newVolume;
        }

        public void ChangeSfxVolume(float newVolume) {
            sfxVolumePref.Value = newVolume;
            sfxVolumeObservable.Value = newVolume;
        }

        public void ChangeMuteState(bool isMuted) {
            mutePref.Value = isMuted;
            mutedObservable.Value = isMuted;
        }
    }
}