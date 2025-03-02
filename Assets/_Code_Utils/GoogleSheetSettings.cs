using System.Collections.Generic;

namespace Hypnagogia.Utils {
    public static class GoogleSheetSettings {
        static string apiKey = null;
        public static string ApiKey => apiKey ??= ApiKeysUtils.LoadApiKey("GoogleSheetsApiKey");

        public static string GetUrl(string spreadsheetId, string tabName) {
            var url =
                $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{tabName}?alt=json&key={ApiKey}";
            return url;
        }
    }

    [System.Serializable]
    public class GoogleSheetJson {
        public List<GoogleSheetRow> values = new();

        [System.Serializable]
        public class GoogleSheetRow {
            public List<string> elements = new();
        }
    }
}