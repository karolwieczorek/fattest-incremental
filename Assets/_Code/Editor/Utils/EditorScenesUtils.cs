using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace FattestInc.Utils {
    public static class EditorScenesUtils {
        public static string FirstScenePath {
            get {
                if (SceneManager.sceneCount > 0)
                    return EditorBuildSettings.scenes[0].path;
                return null;
            }
        }

        public static void StartScene(string path, bool start = false) {
            if (path != null) {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                    EditorSceneManager.OpenScene(path);
                    if (start)
                        EditorApplication.isPlaying = true;
                }
            }
        }

        public static bool IsSceneLoaded(string path) {
            return path.Equals(SceneManager.GetActiveScene().path);
        }

        public static void StartFirstScene() {
            StartScene(FirstScenePath, true);
        }
    }
}