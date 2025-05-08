using System.Collections.Generic;
using System.Linq;
using FattestInc.Utils.GUIHelpers;
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

        static void LoadEditorSaves() {
            string json = EditorPrefs.GetString(EditorSavesKey, "{}");
            editorSaves = JsonUtility.FromJson<SerializableDictionary<string, string>>(json).ToDictionary();
        }

        static void SaveEditorSaves() {
            var serializableDict = new SerializableDictionary<string, string>(editorSaves);
            string json = JsonUtility.ToJson(serializableDict);
            EditorPrefs.SetString(EditorSavesKey, json);
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
                    string defaultName = "EditorSave_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string saveName = EditorInputDialog.Show("Get save name", "Provide save file name", defaultName);
                    if (editorSaves.ContainsKey(saveName)) {
                        Debug.LogError("Save Already exist: " + saveName);
                        return;
                    }
                    if (!string.IsNullOrEmpty(saveName)) {
                        var saveHelper = ProjectContext.Instance.Container.Resolve<SaveHelper>();
                        var saveData = saveHelper.GetSaveData();
                        editorSaves[saveName] = JsonUtility.ToJson(saveData);
                        SaveEditorSaves();
                        Debug.Log("Editor Save Created: " + saveName);
                    }
                }
            }
        }

        static void ShowLoadDropdown() {
            var content = new GUIContent("Load Editor Save ");
            var guiStyle = ToolbarStyles.DropDown;
            var rect = ToolbarStyles.GetThickArea(GUILayoutUtility.GetRect(content, guiStyle));
            if (GUI.Button(rect, content, guiStyle)) {
                var menu = new GenericMenu();
                foreach (var save in editorSaves) {
                    menu.AddItem(new GUIContent(save.Key + "/Load"), false, LoadSave, save.Key);
                    menu.AddItem(new GUIContent(save.Key + "/Delete"), false, DeleteSave, save.Key);
                }
                menu.ShowAsContext();
            }
        }

        static void LoadSave(object saveName) {
            string key = saveName as string;
            if (editorSaves.TryGetValue(key, out string data)) {
                Debug.Log("Loaded Editor Save: " + key + " with data: " + data);
                // Here you can add logic to actually load the save data
                
                var saveHelper = ProjectContext.Instance.Container.Resolve<SaveHelper>();
                var saveData = JsonUtility.FromJson<SaveData>(data);
                saveHelper.LoadFromSaveData(saveData);
                saveHelper.SaveGame();
                var scenesLoaderHelper = ProjectContext.Instance.Container.Resolve<ScenesLoaderHelper>();
                scenesLoaderHelper.RestartGame();
                // editorSaves[saveName] = JsonUtility.ToJson(saveData);
                // var saveData = saveHelper.GetSaveData();
            }
        }

        static void DeleteSave(object saveName) {
            string key = saveName as string;
            if (editorSaves.ContainsKey(key)) {
                editorSaves.Remove(key);
                SaveEditorSaves();
                Debug.Log("Editor Save Deleted: " + key);
            }
        }
    }

    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> {
        public List<TKey> keys = new List<TKey>();
        public List<TValue> values = new List<TValue>();

        public SerializableDictionary() { }

        public SerializableDictionary(Dictionary<TKey, TValue> dict) {
            foreach (var kvp in dict) {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public Dictionary<TKey, TValue> ToDictionary() {
            var dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++) {
                dict[keys[i]] = values[i];
            }
            return dict;
        }
    }
} 