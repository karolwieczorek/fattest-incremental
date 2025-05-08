using UnityEditor;
using UnityEngine;

namespace FattestInc {
    public class EditorInputDialog : EditorWindow {
        string title;
        string description;
        string inputText;
        string okButton;
        string cancelButton;
        string result;
        bool shouldFocus = true;

        public static string Show(string title, string description, string defaultText, string okButton = "Ok", string cancelButton = "Cancel") {
            var window = CreateInstance<EditorInputDialog>();
            window.title = title;
            window.description = description;
            window.inputText = defaultText;
            window.okButton = okButton;
            window.cancelButton = cancelButton;
            window.minSize = new Vector2(300, 100);
            window.maxSize = new Vector2(300, 100);

            // Position window near mouse
            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            var maxPos = GUIUtility.GUIToScreenPoint(new Vector2(Screen.width, Screen.height));
            mousePos.x += 32;
            if (mousePos.x + window.position.width > maxPos.x)
                mousePos.x -= window.position.width + 64;
            if (mousePos.y + window.position.height > maxPos.y)
                mousePos.y = maxPos.y - window.position.height;

            window.position = new Rect(mousePos.x, mousePos.y, 300, 100);
            window.ShowModalUtility();
            return window.result;
        }

        void OnGUI() {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(10);

            GUI.SetNextControlName("InputField");
            inputText = EditorGUILayout.TextField(inputText);
            
            if (shouldFocus) {
                EditorGUI.FocusTextInControl("InputField");
                shouldFocus = false;
            }

            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(okButton)) {
                result = inputText;
                Close();
            }
            if (GUILayout.Button(cancelButton)) {
                result = null;
                Close();
            }
            EditorGUILayout.EndHorizontal();

            if (Event.current.isKey && Event.current.type == EventType.KeyDown) {
                if (Event.current.keyCode == KeyCode.Escape) {
                    result = null;
                    Close();
                }
                else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) {
                    result = inputText;
                    Close();
                }
            }
        }
    }
}