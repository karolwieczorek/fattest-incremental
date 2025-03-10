﻿using FattestInc;
using Hypnagogia.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace MiniRealEstate.Buildings {
    public class EatButton : MonoBehaviour {
        [SerializeField] Button button;
        [HInject] EconomyDataStore economyDataStore;
        [SerializeField] int amount = 1;

        void OnEnable() {
            button.onClick.AddListener(Eat);
        }

        void OnDisable() {
            button.onClick.RemoveListener(Eat);
        }

        void Eat() {
            economyDataStore.CurrentCalories.Value += amount;
        }
    }
}