using System;
using FattestInc;
using Hypnagogia.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace MiniRealEstate.Buildings {
    public class FactoryView : MonoBehaviour {
        [SerializeField] Button button;
        [SerializeField] Image progressBar;
        [HInject] EconomyDataStore economyDataStore;
        [SerializeField] string factoryId;
        [SerializeField] int amount = 1;

        ResourceFactory factory;

        void OnEnable() {
            factory = economyDataStore.AddOrUpgradeFactory(factoryId, 0);
            button.onClick.AddListener(BuyUpgrade);
        }

        void OnDisable() {
            button.onClick.RemoveListener(BuyUpgrade);
            factory = null;
        }

        void BuyUpgrade() {
            economyDataStore.AddOrUpgradeFactory(factoryId, 1);
        }

        void Update() {
            if (factory == null)
                return;

            progressBar.fillAmount = factory.Progress;
        }
    }
}