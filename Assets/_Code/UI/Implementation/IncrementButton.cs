using FattestInc.Economy.API;
using Hypnagogia.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.UI.Implementation {
    public class IncrementButton : MonoBehaviour {
        [SerializeField] Button button;
        [HInject] EconomyDataStore economyDataStore;
        [SerializeField] int amount = 1;

        void OnEnable() {
            button.onClick.AddListener(Increment);
        }

        void OnDisable() {
            button.onClick.RemoveListener(Increment);
        }

        void Increment() {
            economyDataStore.CurrentTotalAmount.Value += (ulong)amount;
        }
    }
}