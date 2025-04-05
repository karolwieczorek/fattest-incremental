using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc.Audio {
    [CreateAssetMenu(fileName = "AudioClip", menuName = "ScriptableObjects/AudioClip")]
    public class AudioClipData : ScriptableObject {
        [SerializeField] List<AudioClip> audioClips;
        [MinMaxSlider(-3f, 3f, true)]
        [SerializeField] Vector2 pitchVariation = new(0, 0);
        
        [MinMaxSlider(0f, 1f, true)]
        [SerializeField] Vector2 volumeVariation = new(1, 1);

#if UNITY_EDITOR
        [ShowInInspector] EditorAudioPlayer EditorAudioPlayer => AudioEditorUtils.AudioPlayer;
#endif

        public AudioClip GetAudioClip() {
            return audioClips[Random.Range(0, audioClips.Count)];
        }

        public float GetVolume() {
            return Random.Range(volumeVariation.x, volumeVariation.y);
        }

        public float GetPitchOffset() {
            return Random.Range(pitchVariation.x, pitchVariation.y);
        }

        public void Play(AudioSource audioSource) {
            var volume = GetVolume();
            audioSource.volume = AudioVolumeConverter.ConvertLinearToLogarithmicVolume(volume);
            audioSource.pitch = GetPitchOffset();
            audioSource.clip = GetAudioClip();
            audioSource.Play();
            Debug.Log($"{audioSource.isPlaying}", audioSource);
        }

        [Button]
        void Play() {
            if (Application.isEditor)
                EditorAudioPlayer.Play(GetAudioClip(), GetVolume(), GetPitchOffset());
        }

        [Button]
        void Stop() {
            if (Application.isEditor)
                EditorAudioPlayer.Stop();
        }

        [OnInspectorDispose]
        void OnInspectorDispose() {
            EditorAudioPlayer.Dispose();
        }
    }
}