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
        
        [FoldoutGroup("Google Settings")]
        [Button]
        void OpenUrl() {
            Application.OpenURL($"https://docs.google.com/spreadsheets/d/{spreadsheetId}");
        }

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
    }
}