using Hypnagogia.Utils;
using TMPro;
using UnityEngine;

namespace FattestInc {
    public class TotalAmountView : MonoBehaviour {
        [SerializeField] TMP_Text label;
        [HInject] EconomyDataStore economyDataStore;

        void OnEnable() {
            economyDataStore.CurrentTotalAmount.Bind(ShowCalories);
        }

        void OnDisable() {
            economyDataStore.CurrentTotalAmount.Unbind(ShowCalories);
        }

        void ShowCalories(int value) {
            label.text = $"{value}";
        }
    }
}