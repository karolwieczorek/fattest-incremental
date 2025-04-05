using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc.Audio {
    public class EditorAudioPlayer {
        static AudioSource myAudioSource;

        [ShowInInspector]
        public static AudioSource MyAudioSource {
            get {
                if (myAudioSource == null) {
                    myAudioSource = CreateInvisibleAudioSource();
                }

                return myAudioSource;
            }
        }

        [ShowInInspector] AudioClip clip;
        [ShowInInspector] float volume;
        [ShowInInspector] float pitch;

        public void Play(AudioClip clip, float volume, float pitch) {
            this.clip = MyAudioSource.clip = clip;
            this.volume = MyAudioSource.volume = volume;
            this.pitch = MyAudioSource.pitch = pitch;
            MyAudioSource.Play();
        }

        [ShowInInspector, ReadOnly, LabelText("Clip"), HorizontalGroup("Status")]
        [InfoBox("No AudioSource available for preview.", InfoMessageType.Warning)]
        string CurrentClipName => MyAudioSource?.clip?.name ?? "N/A";

        static AudioSource CreateInvisibleAudioSource(string name = "InvisibleAudioPlayer") {
            GameObject audioHost = new GameObject(name);
            audioHost.hideFlags = HideFlags.HideAndDontSave;
            AudioSource audioSource = audioHost.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.0f;
            return audioSource;
        }

        public void Stop() {
            if (myAudioSource == null)
                return;
            MyAudioSource.Stop();
        }

        public void Dispose() {
            if (myAudioSource != null)
                Object.DestroyImmediate(myAudioSource.gameObject);
        }
    }
}