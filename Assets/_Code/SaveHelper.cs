using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;
using Hypnagogia.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace FattestInc {
    [UsedImplicitly]
    public class SaveHelper {
        [HInject] EconomyDataStore economyDataStore;
        [HInject] FactoriesReferencer factoriesReferencer;
        
        public void SaveGame(bool silentMode = false) {
            var saveData = GetSaveData();

            SaveStorageUtils.Save(saveData);
            if (!silentMode)
                Debug.Log("Game Saved");
        }

        public SaveData GetSaveData() {
            var saveData = new SaveData();
            saveData.currencyAmount = economyDataStore.CurrentTotalAmount.Value;
            saveData.factoryLevels = new List<SaveData.FactoryLevelEntry>();
            foreach (var factory in economyDataStore.ResourceFactories) {
                saveData.SetFactoryLevel(factory.Key, factory.Value.Level);
            }

            return saveData;
        }

        public void LoadGame() {
            var gameLoaded = TryLoadGame();
            if (!gameLoaded) {
                Debug.Log("Game Load [Failed]");
            }
        }

        public bool TryLoadGame() {
            var maybeSaveData = SaveStorageUtils.Load();
            if (maybeSaveData.TryToGetValue(out var saveData)) {
                LoadFromSaveData(saveData);
                return true;
            }
            
            return false;
        }

        public void LoadFromSaveData(SaveData saveData) {
            economyDataStore.CurrentTotalAmount.Value = saveData.currencyAmount;
            foreach (var factoryLevel in saveData.factoryLevels) {
                var factoryData =
                    factoriesReferencer.Factories.FirstOrDefault(x => x.FactoryId == factoryLevel.factoryId);
                if (factoryData == null) {
                    Debug.LogError($"Failed to load factory: {factoryLevel.factoryId}");
                    continue; // TODO make it so that load process will only start when all data is available
                }

                economyDataStore.LoadFactory(factoryData, factoryLevel.level);
            }

            // Debug.Log($"LoadedData: {JsonUtility.ToJson(saveData)}");

            Debug.Log("Game Loaded");
        }

        public void DeleteSave() {
            SaveStorageUtils.DeleteSave();
            Debug.Log("Save deleted");
        }
    }
}