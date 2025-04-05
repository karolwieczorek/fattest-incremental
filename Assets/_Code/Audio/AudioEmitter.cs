using UnityEngine;

namespace FattestInc.Audio {
    enum State {
        None,
        OnEnable,
        OnDisable
    }

    [RequireComponent(typeof(AudioSource))]
    public class AudioEmitter : MonoBehaviour {
        [SerializeField] State startAction = State.None;
        [SerializeField] State stopAction = State.None;

        [SerializeField] AudioClipData audioClip;
        [SerializeField] AudioSource _audioSource;

        AudioSource audioSource {
            get {
                if (_audioSource == null)
                    _audioSource = GetComponent<AudioSource>();
                return _audioSource;
            }
        }

        void OnEnable() {
            CheckState(State.OnEnable);
        }

        void OnDisable() {
            CheckState(State.OnDisable);
        }

        void CheckState(State currentState) {
            if (startAction == currentState)
                PlayClip();
            if (stopAction == currentState)
                StopClip();
        }

        public void Play() {
            PlayClip();
        }

        public void Stop() {
            StopClip();
        }

        public void PlayOneShot() {
            SetAudioClipAndPitch();
            audioSource.PlayOneShot(audioSource.clip);
        }

        public void PlayOneShot(AudioClipData audioClip) {
            this.audioClip = audioClip;
            PlayOneShot();
        }

        void PlayClip() {
            SetAudioClipAndPitch();
            audioSource.Play();
        }

        void SetAudioClipAndPitch() {
            audioSource.clip = audioClip.GetAudioClip();
            audioSource.pitch = audioClip.GetPitchOffset();
        }

        void StopClip() {
            audioSource.Stop();
        }
    }
}