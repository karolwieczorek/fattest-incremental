using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hypnagogia.Utils {
    [RequireComponent(typeof(CanvasRenderer))]
    public class NonDrawingGraphic : Graphic {
        public override void SetMaterialDirty() { }
        public override void SetVerticesDirty() { }

        protected override void OnPopulateMesh(VertexHelper vh) {
            vh.Clear();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects, UnityEditor.CustomEditor(typeof(NonDrawingGraphic), false)]
    public class NonDrawingGraphicEditor : UnityEditor.UI.GraphicEditor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            UnityEditor.EditorGUILayout.PropertyField(base.m_Script, Array.Empty<GUILayoutOption>());
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}