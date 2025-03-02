using FattestInc;
using Hypnagogia.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace MiniRealEstate.Buildings {
    public class FactoryView : MonoBehaviour {
        [SerializeField] Button button;
        [HInject] EconomyDataStore economyDataStore;
        [SerializeField] string factoryId;
        [SerializeField] int amount = 1;

        void OnEnable() {
            button.onClick.AddListener(BuyUpgrade);
        }

        void OnDisable() {
            button.onClick.RemoveListener(BuyUpgrade);
        }

        void BuyUpgrade() {
            economyDataStore.AddOrUpgradeFactory(factoryId, 1);
        }
    }
}