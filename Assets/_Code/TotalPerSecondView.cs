using Hypnagogia.Utils;
using TMPro;
using UnityEngine;

namespace FattestInc {
    public class TotalPerSecondView : MonoBehaviour {
        [SerializeField] TMP_Text label;
        [HInject] EconomyDataStore economyDataStore;

        void OnEnable() {
            economyDataStore.FactoryUpgradedEvent += OnFactoryUpgraded;
        }

        void OnDisable() {
            economyDataStore.FactoryUpgradedEvent += OnFactoryUpgraded;
        }

        void OnFactoryUpgraded() {
            var perSecond = 0f;
            foreach (var (_, factory) in economyDataStore.ResourceFactories) {
                perSecond += factory.GetCurrentValuePerSecond();
            }
            // if decimal is bigger than 0 (modulo from number) then just print number else print number with string format F1
            var decimalAmount = perSecond % 1;
            var numberToDraw = decimalAmount > 0 ? $"{perSecond:F1}" : $"{perSecond}";
            label.text = $"{numberToDraw}/sec";
        }
    }
}