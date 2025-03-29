using Hypnagogia.Utils;
using TMPro;
using UnityEngine;

namespace FattestInc {
    public class TotalAmountView : MonoBehaviour {
        [SerializeField] TMP_Text label;
        [HInject] EconomyDataStore economyDataStore;

        void OnEnable() {
            economyDataStore.CurrentTotalAmount.Bind(ShowTotalAmount);
        }

        void OnDisable() {
            economyDataStore.CurrentTotalAmount.Unbind(ShowTotalAmount);
        }

        void ShowTotalAmount(int value) {
            label.text = $"{value}";
        }
    }
}