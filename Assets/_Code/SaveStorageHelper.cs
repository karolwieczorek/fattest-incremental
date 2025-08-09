using System;
using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc {
    public static class SaveStorageUtils {
        static EncryptionMethod CurrentEncryptionMethod => EncryptionMethod.None;
        const string SaveKey = "GameSave";

        public static void Save(SaveData data) {
            string json = JsonUtility.ToJson(data);
            string encrypted = EncryptionUtils.Encrypt(CurrentEncryptionMethod, json);

            var wrapper = new SaveWrapper {
                version = Application.version, encryption = CurrentEncryptionMethod.ToString(), data = encrypted
            };

            string wrapperJson = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(SaveKey, wrapperJson);
            PlayerPrefs.Save();
        }

        public static Maybe<SaveData> Load() {
            if (!PlayerPrefs.HasKey(SaveKey))
                return Maybe<SaveData>.Empty;

            var wrapperJson = PlayerPrefs.GetString(SaveKey);
            var wrapper = JsonUtility.FromJson<SaveWrapper>(wrapperJson);

            if (Enum.TryParse<EncryptionMethod>(wrapper.encryption, out var currentEncryptionMethod)) {
                var decryptedSave = EncryptionUtils.Decrypt(currentEncryptionMethod, wrapper.data);
                var migratedSave = Migrate(wrapper.version, decryptedSave);
                if (migratedSave.IsNullOrWhitespace())
                    return Maybe<SaveData>.Empty;

                var save = JsonUtility.FromJson<SaveData>(migratedSave);
                return save;
            }

            return Maybe<SaveData>.Empty;
        }

        static string Migrate(string version, string oldSave) {
            if (version == Application.version)
                return oldSave;

            Debug.LogWarning($"[SaveSystem] Try To migrate Save version: {version} " +
                             $"to App version: {Application.version}");

            if (VersionUtils.TryParseVersion(version, out var v) && v < new Version(0, 2, 0)) {
                Debug.Log("[SaveSystem] drop the version of save data before game balance");
                return null;
            }
            if (version == "1.0.0") {
                // Handle upgrade to 1.1.0
            }

            return oldSave;
        }

        public static void DeleteSave() {
            PlayerPrefs.DeleteKey(SaveKey);
        }
    }

    [Serializable]
    public class SaveWrapper {
        public string version;
        public string encryption;
        public string data;

        public SaveWrapper() { }

        public SaveWrapper(string version, string encryption, string data) {
            this.version = version;
            this.encryption = encryption;
            this.data = data;
        }
    }
}