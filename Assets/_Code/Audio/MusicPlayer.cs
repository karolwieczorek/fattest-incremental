using UnityEngine;
using UnityEngine.Audio;

namespace FattestInc.Audio {
    public class MusicPlayer : MonoBehaviour {
        [SerializeField] AudioClipData audioClipData;
        [SerializeField] AudioSource _audioSource;
        float _currentVolume;
        [SerializeField] float _fadeDuration = 2f;
        float _fadeSpeed;

        [Header("Audio Mixer")] [SerializeField]
        AudioMixer _mixer;

        [SerializeField] AudioMixerGroup audioMixerGroup;

        (float min, float max) _lowPassMinMax = (500f, 22000f);
        float _lowPassTransitionDuration = 0.5f;
        float _lowPassTransitionSpeed;
        float _lowPassTransitionTimer = 0;
        float _lowPassTransitionDirection = -1f;

        const string LOW_PASS_CUTOFF = "LowPassCutoff";

        public static MusicPlayer instance = null;

        void Awake() {
            if (instance == null) {
                instance = this;

                Initialize();
            }
            else {
                Destroy(this);
            }
        }

        void Initialize() {
            _lowPassTransitionSpeed = 1f / _lowPassTransitionDuration;
        }

        // Start is called before the first frame update
        void Start() {
            _currentVolume = 0f;
            SetVolume();

            _fadeSpeed = 1f / _fadeDuration;
            audioClipData.Play(_audioSource);
            // _audioSource.Play();
        }

        // Update is called once per frame
        void Update() {
            if (_currentVolume < 1f) {
                FadeVolume();
            }

            if (!LowPassTargetReached())
                UpdateLowPassFilter();
        }

        void UpdateLowPassFilter() {
            _lowPassTransitionTimer =
                Mathf.Clamp(_lowPassTransitionTimer + Time.deltaTime * _lowPassTransitionDirection, 0,
                    _lowPassTransitionDuration);
            float t = _lowPassTransitionTimer / _lowPassTransitionDuration;
            float lowPassValue = Mathf.Lerp(_lowPassMinMax.min, _lowPassMinMax.max, t);
            _mixer.SetFloat(LOW_PASS_CUTOFF, lowPassValue);
        }

        bool LowPassTargetReached() {
            return (_lowPassTransitionTimer == 0 && _lowPassTransitionDirection < 0) ||
                   (_lowPassTransitionTimer >= _lowPassTransitionDuration && _lowPassTransitionDirection > 0);
        }

        void FadeVolume() {
            _currentVolume = Mathf.Min(_currentVolume + Time.deltaTime * _fadeSpeed, 1f);
            SetVolume();
        }

        void SetVolume() {
            var currentVolume = Mathf.Max(0.001f, _currentVolume);
            var volume = Mathf.Log10(currentVolume) * 20f;
            // Debug.Log($"MusicVolume: {volume} - {_currentVolume}");
            audioMixerGroup.audioMixer.SetFloat("MusicVolume", volume);
        }

        public void SetLowPassTranstionDirection(float f) {
            _lowPassTransitionDirection = f;
        }
    }
}