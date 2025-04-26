using System.Collections.Generic;
using FattestInc.UI.API;
using FattestInc.Windows.General;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc.UI.Implementation {
    public class OpenWindowButton : ButtonBehaviour {
        [HInject] WindowManager windowManager;

        [ValueDropdown(nameof(GetWindowPrefabs))] [SerializeField]
        WindowBase windowBase;

        [ShowInInspector]
        WindowBase WindowBase => windowBase;

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

        IEnumerable<WindowBase> GetWindowPrefabs() {
#if UNITY_EDITOR
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
#else
            return null;
#endif
        }
    }
}