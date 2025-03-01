using System.Collections.Generic;
using UnityEngine;

namespace Hypnagogia.Utils {
    public static class TransformExtensions {
        /// <summary>
        /// Changes transform's parent to new one, preserving objects scale
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        public static void ReparentTo(this Transform child, Transform parent) {
            var oldscale = child.localScale;
            child.parent = parent;
            child.localScale = oldscale;
        }

        /// <summary>
        /// Returns list of transforms children. It is safe do Destroy objects from this list
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Transform[] GetChildren(this Transform parent) {
            var kids = new Transform[parent.childCount];
            for (int i = 0; i < parent.childCount; i++) {
                kids[i] = parent.GetChild(i);
            }

            return kids;
        }

        public static IEnumerable<Transform> GetFamily(this Transform parent) {
            yield return parent;
            foreach (Transform t in parent) {
                foreach (Transform t1 in t.GetFamily()) {
                    yield return t1;
                }
            }
        }

        public static void DestroyChildren(this Transform parent) {
            foreach (var child in parent.GetChildren()) {
                child.gameObject.Destroy();
            }
        }

        public static T GetComponentInAncestors<T>(this Transform t) where T : Component {
            var p = t.parent;
            while (p != null) {
                T c = p.GetComponent<T>();
                if (c) {
                    return c;
                }

                p = p.parent;
            }

            return null;
        }

        public static void SetPosition(this Transform transform, Vector3 newPosition) {
            transform.position = newPosition;
        }

        public static void SetPosition(this Transform transform, Vector2 newPosition) {
            transform.position = newPosition;
        }

        public static void ChangePosition(this Transform transform, float? x = null, float? y = null, float? z = null) {
            transform.position = transform.position.Change(x, y, z);
        }

        public static void MovePosition(this Transform transform, float? x = null, float? y = null, float? z = null) {
            transform.position = transform.position.Move(x, y, z);
        }

        public static void MovePosition(this Transform transform, Vector3 moveVector) {
            transform.position += moveVector;
        }
    }
}