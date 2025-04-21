using System;
using System.Collections.Generic;
using System.Linq;
using Hypnagogia.Utils;
using UnityEngine;
using Zenject;

namespace FattestInc.Windows.General
{
    public class WindowManager : MonoBehaviour
    {
        [SerializeField] Transform windowsParent;
        
        readonly Dictionary<Type, WindowBase> windows = new();

        [HInject] WindowsReferencer windowsReferencer;
        [HInject] UIDataStore uiDataStore;
        [HInject] DiContainer diContainer;

        public T GetWindow<T>() where T : WindowBase
        {
            var type = typeof(T);
            if (windows.TryGetValue(type, out var window))
                return (T) window;
            
            var prefab = windowsReferencer.WindowsPrefabs.Find(p => p.GetType() == type);
            if (prefab == null)
            {
                Debug.LogError($"Prefab for window type {type.Name} not found.");
                return null;
            }

            window = diContainer.InstantiatePrefabForComponent<T>(prefab.gameObject, windowsParent);
            windows[type] = window;
            return (T)window;
        }
        
        public WindowBase GetWindowByType(Type type)
        {
            if (windows.TryGetValue(type, out var window))
                return window;
            
            var prefab = windowsReferencer.WindowsPrefabs.Find(p => p.GetType() == type);
            if (prefab == null)
            {
                Debug.LogError($"Prefab for window type {type.Name} not found.");
                return null;
            }

            window = diContainer.InstantiatePrefabForComponent(type, prefab.gameObject, windowsParent, Array.Empty<object>()) as WindowBase;
            windows[type] = window;
            return window;
        }

        public T GetOnTopWindow<T>() where T : WindowBase {
            var window = GetWindow<T>();
            if (window != null)
                MoveOnTopWindow(window);

            return window;
        }

        public T OpenWindow<T>() where T : WindowBase, ISimpleWindowOpen
        {
            var window = GetWindow<T>();
            if (window != null)
            {
                ShowWindow(window);
                return window;
            }

            Debug.LogWarning($"Window of type {typeof(T).Name} not found.");
            return null;
        }
        
        public WindowBase OpenWindow(Type windowType)
        {
            var window = GetWindowByType(windowType);
            if (window != null)
            {
                if (window is ISimpleWindowOpen windowOpen)
                    ShowWindow(windowOpen);
                else 
                    Debug.LogError("Trying to open complex window with simple command.");
                return window;
            }

            Debug.LogWarning($"Window of type {windowType.Name} not found.");
            return null;
        }

        public T OpenOnTop<T>() where T : WindowBase, ISimpleWindowOpen
        {
            T window = OpenWindow<T>();
            if (window != null)
                MoveOnTopWindow(window);

            return window;
        }
        
        public WindowBase OpenOnTop(Type windowType) {
            WindowBase window = OpenWindow(windowType);
            if (window != null)
                MoveOnTopWindow(window);

            return window;
        }

        void ShowWindow(ISimpleWindowOpen window) {
            window.Open();
            if (uiDataStore.windowBlur != null)
                uiDataStore.windowBlur.SetActive(true);
        }

        void HideWindow(ISimpleWindowClose window) {
            window.Close();
            if (uiDataStore.windowBlur != null)
                uiDataStore.windowBlur.SetActive(false);
        }

        public T OpenOnTop<T, TP>(TP parametersContainer) where TP : WindowParameters
            where T : WindowBase, IWindowParametersInitialization<TP>, ISimpleWindowOpen
        {
            var window = GetWindow<T>();
            if (window != null)
            {
                window.InitializeWithParameters(parametersContainer);
                ShowWindow(window);
                MoveOnTopWindow(window);
            }

            Debug.LogWarning($"Window of type {typeof(T).Name} not found.");
            return null;
        }

        static void MoveOnTopWindow<T>(T window) where T : WindowBase
        {
            window.transform.SetAsLastSibling();
        }

        public void CloseWindow<T>() where T : WindowBase, ISimpleWindowClose
        {
            var window = GetWindow<T>();
            CloseWindow(window);
        }

        public void CloseWindow<T>(T window) where T : WindowBase, ISimpleWindowClose
        {
            CloseWindowInternal(window, window);
        }

        bool CloseWindowInternal(WindowBase window, ISimpleWindowClose simpleWindowClose)
        {
            if (window != null && simpleWindowClose != null && ReferenceEquals(window, simpleWindowClose))
            {
                HideWindow(simpleWindowClose);
                return true;
            }

            Debug.LogWarning($"Window of type {window.GetType().Name} not found.");
            return false;
        }

        public bool TryCloseWindow<T>(T window) where T : WindowBase
        {
            if (window is ISimpleWindowClose simpleWindowClose)
                return CloseWindowInternal(window, simpleWindowClose);

            return false;
        }

        public void TryCloseAllWindows()
        {
            foreach (var (_, window) in windows)
            {
                if (window.IsVisible && window is ISimpleWindowClose simpleWindowClose)
                    HideWindow(simpleWindowClose);
            }
        }

        public void CloseWindowOnTop() {
            var minIndex = windows.Where(x => x.Value.IsVisible).Min(x => x.Value.transform.GetSiblingIndex());
            var (type, window) = windows.FirstOrDefault(x => x.Value.IsVisible && x.Value.transform.GetSiblingIndex() == minIndex);
            if (window != null && window is ISimpleWindowClose simpleClose)
                HideWindow(simpleClose);
        }
    }
}