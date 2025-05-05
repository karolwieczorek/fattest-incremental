using UnityEngine;

namespace FattestInc {
    public class LocalPrefabsSpawner : MonoBehaviour {
        public string resourceFolderPath = "LocalPrefabs";

        void Start() {
            var logicPrefabs = Resources.LoadAll<GameObject>(resourceFolderPath);
            if (logicPrefabs.Length == 0) {
                Debug.Log("No prefabs found in Resources/" + resourceFolderPath);
                return;
            }

            foreach (GameObject prefab in logicPrefabs) {
                var condition = prefab.GetComponent<SpawnCondition>();
                if (condition != null) {
                    if (condition.inDebug && !Debug.isDebugBuild)
                        continue;

                    if (condition.inRelease && Debug.isDebugBuild)
                        continue;
                }

                Debug.Log($"Spawning Resources/{resourceFolderPath}/{prefab.name}");
                GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
                instance.transform.localPosition = Vector3.zero;
            }
        }
    }
}