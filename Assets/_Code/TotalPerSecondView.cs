using Hypnagogia.Utils;
using TMPro;
using UnityEngine;

namespace FattestInc {
    public class TotalPerSecondView : MonoBehaviour {
        [SerializeField] TMP_Text label;
        [HInject] EconomyDataStore economyDataStore;

        void OnEnable() {
            economyDataStore.CurrentAmountPerSecond.ChangedValue += OnAmountPerSecondChanged;
        }

        void OnDisable() {
            economyDataStore.CurrentAmountPerSecond.ChangedValue -= OnAmountPerSecondChanged;
        }

        void OnAmountPerSecondChanged(float amountPerSecond) {
            // if decimal is bigger than 0 (modulo from number) then just print number else print number with string format F1
            var decimalAmount = amountPerSecond % 1;
            var numberToDraw = decimalAmount > 0 ? $"{amountPerSecond:F1}" : $"{amountPerSecond}";
            label.text = $"{numberToDraw}/sec";
        }
    }
}