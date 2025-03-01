using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using Hypnagogia.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace FattestInc {
    [UsedImplicitly]
    public class ScenesLoaderHelper : IInitializable {
        [HInject] ScenesLoaderReferencer scenesLoaderReferencer;
        [HInject] ZenjectSceneLoader zenjectSceneLoader;

        Maybe<SceneReference> lastLoadedScene = null;

        public void Initialize() {
            lastLoadedScene = Maybe<SceneReference>.Empty;
        }

        public void LoadGameScene() {
            Task().Forget();
            return;

            async UniTaskVoid Task() {
                await LoadSceneTask(scenesLoaderReferencer.GameScene);
            }
        }

        public bool IsSceneLoaded(SceneReference sceneReference) {
            return lastLoadedScene.TryToGetValue(out var lastLoadedSceneValue) && lastLoadedSceneValue.Path == sceneReference.Path;
        }

        public async UniTask<bool> LoadSceneTask(SceneReference sceneToLoad, LoadSceneMode loadSceneMode = LoadSceneMode.Additive) {
            if (lastLoadedScene.TryToGetValue(out var lastLoadedSceneValue) && string.IsNullOrEmpty(lastLoadedSceneValue.Path) == false) {
                await SceneManager.UnloadSceneAsync(lastLoadedSceneValue.BuildIndex);
                Debug.Log($"Unloaded {lastLoadedSceneValue.Path}");
                lastLoadedScene = Maybe<SceneReference>.Empty;
            }

            await zenjectSceneLoader.LoadSceneAsync(sceneToLoad.BuildIndex, loadSceneMode);
            lastLoadedScene = sceneToLoad;
            var scene = SceneManager.GetSceneByPath(sceneToLoad.Path);
            SceneManager.SetActiveScene(scene);
            Debug.Log($"Loaded {sceneToLoad.Path}");

            return true;
        }
    }
}