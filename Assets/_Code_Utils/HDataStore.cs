using UnityEngine;

namespace Hypnagogia.Utils {
    public class HDataStore : MonoBehaviour {
        static Transform dataStoreParentTransform;
        public static Transform DataStoreParentTransform {
            get {
                if (dataStoreParentTransform == null) {
                    var dataStoresGameObject = new GameObject("DataStores");
                    DontDestroyOnLoad(dataStoresGameObject);
                    dataStoreParentTransform = dataStoresGameObject.transform;
                }
                return dataStoreParentTransform;
            }
        }

        public static Transform FindOrMadeDataStoresParentInTransform(Transform transform) {
            var parent = transform.Find("DataStores");
            if (parent != null)
                return parent;

            parent = new GameObject("DataStores").transform;
            parent.SetParent(transform);
            return parent;
        }
    }

// #if UNITY_EDITOR
//     [UnityEditor.CustomEditor(typeof(HDataStore), true)]
//     public class DataStoreBaseEditor : Sirenix.OdinInspector.Editor.OdinEditor { }
// #endif
}