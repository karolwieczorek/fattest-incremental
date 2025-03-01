using UnityEngine;
using Zenject;

namespace Hypnagogia.Utils {
    public static class DiContainerExtensions {
        public static ConcreteIdArgConditionCopyNonLazyBinder BindDataStore<T>(this DiContainer container, Transform containerTransform = null) where T : HDataStore {
            Transform parent;
            if (containerTransform != null)
                parent = HDataStore.FindOrMadeDataStoresParentInTransform(containerTransform);
            else 
                parent = HDataStore.DataStoreParentTransform;
            return container.BindInterfacesAndSelfTo<T>().FromNewComponentOnNewGameObject().UnderTransform(parent).AsSingle();
        }

        public static ConcreteIdArgConditionCopyNonLazyBinder BindDataStore<T>(this DiContainer container, string name, Transform containerTransform = null) where T : HDataStore {
            Transform parent;
            if (containerTransform != null)
                parent = HDataStore.FindOrMadeDataStoresParentInTransform(containerTransform);
            else 
                parent = HDataStore.DataStoreParentTransform;
            return container.BindInterfacesAndSelfTo<T>().FromNewComponentOnNewGameObject().WithGameObjectName(name).UnderTransform(parent).AsSingle();
        }
        
        public static IfNotBoundBinder BindSystem<T>(this DiContainer container) where T : HSystem {
            return container.BindInterfacesAndSelfTo<T>().AsSingle().NonLazy();
        }

        public static void BindDataStores(this DiContainer container, Transform transform, bool makeGroup, params System.Type[] dataStoreTypes) {
            var parent = makeGroup ? HDataStore.FindOrMadeDataStoresParentInTransform(transform) : transform;
            foreach (var dataStoreType in dataStoreTypes) {
                if (!dataStoreType.IsSubclassOf(typeof(HDataStore))) {
                    UnityEngine.Debug.LogError($"Trying to bind {dataStoreType.FullName} as System.");
                    continue;
                }
                container.BindInterfacesAndSelfTo(dataStoreType).FromNewComponentOnNewGameObject().UnderTransform(parent).AsSingle();
            }
        }

        public static void BindSystems(this DiContainer container, params System.Type[] systems) {
            foreach (var hSystem in systems) {
                if (!hSystem.IsSubclassOf(typeof(HSystem))) {
                    UnityEngine.Debug.LogError($"Trying to bind {hSystem.FullName} as System.");
                    continue;
                }
                container.BindInterfacesAndSelfTo(hSystem).AsSingle().NonLazy();
            }
        }
    }
}