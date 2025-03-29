using Hypnagogia.Utils;
using TMPro;
using UnityEngine;

namespace FattestInc {
    public class TotalPerSecondView : MonoBehaviour {
        [SerializeField] TMP_Text label;
        [HInject] EconomyDataStore economyDataStore;

        void OnEnable() {
            Debug.Log("Subscribed");
            economyDataStore.FactoryUpgradedEvent += OnFactoryUpgraded;
        }

        void OnDisable() {
            economyDataStore.FactoryUpgradedEvent += OnFactoryUpgraded;
        }

        void OnFactoryUpgraded() {
            var perSecond = 0;
            foreach (var (_, factory) in economyDataStore.ResourceFactories) {
                perSecond += factory.GetCurrentValuePerSecond();
            }
            label.text = $"{perSecond}/sec";
        }
    }
}