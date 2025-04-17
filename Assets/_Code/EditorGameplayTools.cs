using System;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc {
    public class EditorGameplayTools : MonoBehaviour {
        [HInject] ResourceFactoriesHelper resourceFactoriesHelper;

        [Button]
        void IncrementMinutes(int minutes) {
            resourceFactoriesHelper.TickAllFactories((float)TimeSpan.FromMinutes(minutes).TotalSeconds);
        }

        [Button]
        void IncrementSeconds(float seconds) {
            resourceFactoriesHelper.TickAllFactories(seconds);
        }
        
        [Button]
        void PrintAppIdentifier() {
            Debug.Log($"Application.identifier: {Application.identifier}");
        }

        [Button]
        void Encrypt(EncryptionMethod encryptionMethod, string text) {
            Debug.Log(EncryptionUtils.Encrypt(encryptionMethod, text));
        }

        [Button]
        void Decrypt(EncryptionMethod encryptionMethod, string text) {
            Debug.Log(EncryptionUtils.Decrypt(encryptionMethod, text));
        }

        [ButtonGroup("Save")]
        [Button]
        void SaveGame() {
            Debug.Log("Save");
        }

        [ButtonGroup("Save")]
        [Button]
        void LoadGame() {
            Debug.Log("Load");
        }
    }
}