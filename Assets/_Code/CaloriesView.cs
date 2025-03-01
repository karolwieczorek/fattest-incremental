using FattestInc;
using Hypnagogia.Utils;
using TMPro;
using UnityEngine;

namespace MiniRealEstate.Buildings {
    public class CaloriesView : MonoBehaviour {
        [SerializeField] TMP_Text label;
        [HInject] EconomyDataStore economyDataStore;

        void OnEnable() {
            economyDataStore.CurrentCalories.Bind(ShowCalories);
        }

        void OnDisable() {
            economyDataStore.CurrentCalories.Unbind(ShowCalories);
        }

        void ShowCalories(int value) {
            label.text = $"{value} kcal";
        }
    }
}