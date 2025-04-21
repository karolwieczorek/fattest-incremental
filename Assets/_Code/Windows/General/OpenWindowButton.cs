using System.Collections.Generic;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc.Windows.General {
    public class OpenWindowButton : ButtonBehaviour {
        [HInject] WindowManager windowManager;

        [ValueDropdown(nameof(GetWindowPrefabs))] [SerializeField]
        WindowBase windowBase;

        [SerializeField] bool closeOtherWindows = false;

        void OnValidate() {
            if (windowBase != null && windowBase is not ISimpleWindowOpen) {
                windowBase = null;
                Debug.LogError($"Window need implement {nameof(ISimpleWindowOpen)}");
            }
        }

        protected override void OnClick() {
            if (closeOtherWindows)
                windowManager.TryCloseAllWindows();
            windowManager.OpenOnTop(windowBase.GetType());
        }

#if UNITY_EDITOR
        IEnumerable<WindowBase> GetWindowPrefabs() {
            const string folderPath = "Assets/Prefabs/Windows";
            var prefabGuids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] {folderPath});

            var windowPrefabs = new List<WindowBase>();

            foreach (var guid in prefabGuids) {
                var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<WindowBase>(assetPath);

                if (prefab != null && prefab is ISimpleWindowOpen) {
                    windowPrefabs.Add(prefab);
                }
            }

            return windowPrefabs;
        }
#endif
    }
}