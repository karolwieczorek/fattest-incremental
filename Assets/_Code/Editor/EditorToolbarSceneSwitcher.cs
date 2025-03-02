using System.Linq;
using FattestInc.Utils;
using FattestInc.Utils.GUIHelpers;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace FattestInc {
    [InitializeOnLoad]
    internal static class EditorToolbarSceneSwitcher {
        static EditorToolbarSceneSwitcher() {
            ToolbarExtender.RightToolbarGUI.Add(OnRightToolbarGUI);
            ToolbarExtender.LeftToolbarGUI.Add(OnLeftToolbarGUI);
        }

        static void OnLeftToolbarGUI() {
            using (new GUIEnabled(EditorApplication.isPlaying == false)) {
                GUILayout.FlexibleSpace();
                ShowPlayButton();
            }
        }

        static void OnRightToolbarGUI() {
            using (new GUIEnabled(EditorApplication.isPlaying == false)) {
                ShowMenu();
                GUILayout.FlexibleSpace();
            }
        }

        static void ShowPlayButton() {
            var tex = EditorGUIUtility.IconContent(@"PlayButton").image;

            using (new GUIColor(new Color(0.34f, 1f, 0.64f))) {
                var content = new GUIContent(tex, "Start First scene");
                var guiStyle = ToolbarStyles.Command;

                var rect = ToolbarStyles.GetThickArea(GUILayoutUtility.GetRect(content, guiStyle));
                if (GUI.Button(rect, content, guiStyle)) {
                    EditorScenesUtils.StartFirstScene();
                }
            }
        }

        static void ShowMenu() {
            var content = new GUIContent("Build Scenes ");
            var guiStyle = ToolbarStyles.DropDown;
            var rect = ToolbarStyles.GetThickArea(GUILayoutUtility.GetRect(content, guiStyle));

            if (GUI.Button(rect, content, guiStyle)) {
                GenericMenu menu = new GenericMenu();

                foreach (var scene in EditorBuildSettings.scenes) {
                    bool isLoaded = EditorScenesUtils.IsSceneLoaded(scene.path);
                    menu.AddItem(new GUIContent(GetNameFromScenePath(scene.path)), isLoaded, StartScene, scene.path);
                }

                menu.ShowAsContext();
            }
        }

        static void StartScene(object pathText) {
            EditorScenesUtils.StartScene(pathText as string);
        }

        static string GetNameFromScenePath(string path) {
            return path.Split('/').Last();
        }
    }
}