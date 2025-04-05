#if UNITY_EDITOR
#endif

namespace FattestInc.Audio {
    public static class AudioEditorUtils {
        static EditorAudioPlayer audioPlayer;
        public static EditorAudioPlayer AudioPlayer {
            get { return audioPlayer ??= new EditorAudioPlayer(); }
        }
    }
}