using System;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FattestInc.Audio {
    public static class AudioEditorUtils {
        static EditorAudioPlayer audioPlayer;
        public static EditorAudioPlayer AudioPlayer {
            get { return audioPlayer ??= new EditorAudioPlayer(); }
        }

        public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false) {
#if UNITY_EDITOR
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
     
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null
            );

            Debug.Log(method);
            method.Invoke(
                null,
                new object[] { clip, startSample, loop }
            );
#else
            Debug.LogWarning("Cannot play audio clip in non-Editor build.");
#endif
        }

        public static void StopAllClips() {
#if UNITY_EDITOR
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { },
                null
            );

            Debug.Log(method);
            method.Invoke(
                null,
                new object[] { }
            );
#else 
            Debug.LogWarning("Cannot stop all audio clips in non-Editor build.");
#endif
        }
    }
}