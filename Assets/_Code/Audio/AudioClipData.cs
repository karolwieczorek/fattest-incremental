using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc.Audio {
    [CreateAssetMenu(fileName = "AudioClip", menuName = "ScriptableObjects/AudioClip")]
    public class AudioClipData : ScriptableObject {
        [SerializeField] List<AudioClip> audioClips;
        [SerializeField] float pitchBase = 1f;
        [SerializeField] float pitchVariation = 0f;

        public AudioClip GetAudioClip() {
            return audioClips[Random.Range(0, audioClips.Count)];
        }

        public float GetPitchOffset() {
            float pitchVariationHalf = pitchVariation / 2f;
            return pitchBase + Random.Range(-pitchVariationHalf, pitchVariationHalf);
        }

        [Button]
        void Play() {
            
        }
    }
}