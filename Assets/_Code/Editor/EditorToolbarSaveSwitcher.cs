using System.Collections.Generic;
using System.Linq;
using FattestInc.Utils.GUIHelpers;
using Hypnagogia.Utils;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using Zenject;

namespace FattestInc {
    [InitializeOnLoad]
    internal static class EditorToolbarSaveSwitcher {
        const string EditorSavesKey = "EditorSaves";
        static Dictionary<string, string> editorSaves = new();

        static EditorToolbarSaveSwitcher() {
            ToolbarExtender.RightToolbarGUI.Add(OnRightToolbarGUI);
            LoadEditorSaves();
        }

        static void OnRightToolbarGUI() {
            using (new GUIEnabled(EditorApplication.isPlaying)) {
                ShowSaveButton();
                ShowLoadDropdown();
                GUILayout.FlexibleSpace();
            }
        }

        static void ShowSaveButton() {
            var tex = EditorGUIUtility.IconContent(@"SaveAs").image;
            using (new GUIColor(new Color(0.34f, 1f, 0.64f))) {
                var content = new GUIContent(tex, "Create New Editor Save");
                var guiStyle = ToolbarStyles.Command;
                var rect = ToolbarStyles.GetThickArea(GUILayoutUtility.GetRect(content, guiStyle));
                if (GUI.Button(rect, content, guiStyle)) {
                    CreateNewSave();
                }
            }
        }

        static void ShowLoadDropdown() {
            var content = new GUIContent("Saves ");
            var guiStyle = ToolbarStyles.DropDown;
            var rect = ToolbarStyles.GetThickArea(GUILayoutUtility.GetRect(content, guiStyle));
            if (GUI.Button(rect, content, guiStyle)) {
                ShowSavesMenu();
            }
        }

        static void CreateNewSave() {
            string defaultName = "EditorSave_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string saveName = EditorInputDialog.Show("Get save name", "Provide save file name", defaultName);

            if (string.IsNullOrEmpty(saveName))
                return;

            if (editorSaves.ContainsKey(saveName)) {
                Debug.LogError("Save Already exists: " + saveName);
                return;
            }

            var saveHelper = ProjectContext.Instance.Container.Resolve<SaveHelper>();
            var saveData = saveHelper.GetSaveData();
            editorSaves[saveName] = JsonUtility.ToJson(saveData);
            SaveEditorSaves();
            Debug.Log("Editor Save Created: " + saveName);
        }

        static void ShowSavesMenu() {
            var menu = new GenericMenu();
            foreach (var save in editorSaves) {
                menu.AddItem(new GUIContent(save.Key + "/Load"), false, LoadSave, save.Key);
                menu.AddItem(new GUIContent(save.Key + "/Delete"), false, DeleteSave, save.Key);
            }

            menu.ShowAsContext();
        }

        static void LoadSave(object saveName) {
            string key = saveName as string;
            if (!editorSaves.TryGetValue(key, out string data))
                return;

            Debug.Log("Loaded Editor Save: " + key);
            var saveHelper = ProjectContext.Instance.Container.Resolve<SaveHelper>();
            var saveData = JsonUtility.FromJson<SaveData>(data);
            saveHelper.LoadFromSaveData(saveData);
            saveHelper.SaveGame();

            var scenesLoaderHelper = ProjectContext.Instance.Container.Resolve<ScenesLoaderHelper>();
            scenesLoaderHelper.RestartGame();
        }

        static void DeleteSave(object saveName) {
            string key = saveName as string;
            if (!editorSaves.ContainsKey(key))
                return;

            editorSaves.Remove(key);
            SaveEditorSaves();
            Debug.Log("Editor Save Deleted: " + key);
        }

        static void LoadEditorSaves() {
            string json = EditorPrefs.GetString(EditorSavesKey, "{}");
            editorSaves = JsonUtility.FromJson<SerializableDictionary<string, string>>(json).ToDictionary();
        }

        static void SaveEditorSaves() {
            var serializableDict = new SerializableDictionary<string, string>(editorSaves);
            string json = JsonUtility.ToJson(serializableDict);
            EditorPrefs.SetString(EditorSavesKey, json);
        }
    }
}