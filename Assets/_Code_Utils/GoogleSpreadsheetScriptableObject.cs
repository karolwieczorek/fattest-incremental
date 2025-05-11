using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;

namespace Hypnagogia.Utils {
    public abstract class GoogleSpreadsheetScriptableObject : ScriptableObject {
        [FoldoutGroup("Google Settings")] [SerializeField]
        string spreadsheetId;
        [FoldoutGroup("Google Settings")] [SerializeField]
        string tabName;
        
        [ContextMenu(nameof(OpenUrl))]
        [FoldoutGroup("Google Settings")]
        [Button]
        void OpenUrl() {
            Application.OpenURL($"https://docs.google.com/spreadsheets/d/{spreadsheetId}");
        }

        [ContextMenu(nameof(ImportFromGoogleSpreadsheet))]
        [FoldoutGroup("Google Settings")] 
        [Button]
        void ImportFromGoogleSpreadsheet()
        {
            var tokenSource = new CancellationTokenSource();
            var url = GoogleSheetSettings.GetUrl(spreadsheetId, tabName);
            GetUrl(url, tokenSource.Token).Forget();
        }
        
        async UniTask GetUrl(string url, CancellationToken cancellationToken)
        {
            var www = UnityWebRequest.Get(url);
            await www.SendWebRequest().WithCancellation(cancellationToken);
            if (www.result != UnityWebRequest.Result.Success) {
                Debug.LogError($"{www.result} - {www.error}");
                return;
            }

            string json = www.downloadHandler.text;
            Debug.Log($"json: {json}");
            GUIUtility.systemCopyBuffer = json;
            var betterJson = PopX.JsonSanitiser.DoubleArrayToMemberSimple(json, "values", "elements");
            Debug.Log($"better json: {betterJson}");
            var data = JsonUtility.FromJson<GoogleSheetJson>(betterJson);
            data.values.RemoveAt(0);
            ProcessData(data);
        }

        protected abstract void ProcessData(GoogleSheetJson data);

        protected void ParseIndexString(List<string> elements, int index, out string value, string defaultValue) {
            if (elements.Count >= index + 1) {
                value = elements[index];
                return;
            }

            value = defaultValue;
        }

        protected void ParseIndexInt(List<string> elements, int index, out int value, int defaultValue = 0) {
            if (elements.Count >= index + 1 && int.TryParse(elements[index], out var parsedValue)) {
                value = parsedValue;
                return;
            }

            value = defaultValue;
        }
        
        protected void ParseIndexULong(List<string> elements, int index, out ulong value, ulong defaultValue = 0) {
            if (elements.Count >= index + 1 && ulong.TryParse(elements[index], out var parsedValue)) {
                value = parsedValue;
                return;
            }

            value = defaultValue;
        }

        protected void ParseIndexFloat(List<string> elements, int index, out float value, float defaultValue = 0) {
            if (elements.Count >= index + 1 && float.TryParse(elements[index], out var parsedValue)) {
                value = parsedValue;
                return;
            }

            value = defaultValue;
        }
    }
}