using System.IO;
using UnityEngine;

namespace Hypnagogia.Utils {
    public static class ApiKeysUtils {
        const string DirPath = "IgnoredApiKeys/";

        public const string ApiKeysInfo = "Api keys are stored in 'Resources/IgnoredApiKeys/(keyName).txt' files. " +
                                          "Outside of version control. " +
                                          "Just create corresponding file locally and use file name as key name";

        public static string LoadApiKey(string keyName) {
            string filePath = Path.Combine(DirPath, keyName);
            var targetFile = Resources.Load<TextAsset>(filePath);
            if (targetFile == null) {
                Debug.LogWarning($"Couldn't find api key: {filePath}");
                return null;
            }

            return targetFile.text;
        }
    }
}
    