using System.Collections.Generic;
using FattestInc.Economy.API;
using FattestInc.Progression.API;
using Hypnagogia.Utils;
using UnityEngine;
using Zenject;

namespace FattestInc.UI.Implementation {
    public class FactoriesPanel : MonoBehaviour {
        [SerializeField] FactoryView factoryViewPrefab;
        [SerializeField] GameObject clickerViewPrefab;
        [SerializeField] Transform content;

        [HInject] DiContainer container;
        [HInject] FactoriesReferencer factoriesReferencer;
        [HInject] EconomyDataStore economyDataStore;
        [HInject] UnlockingHelper unlockingHelper;

        readonly List<FactoryView> factoryViews = new();

        void Start() {
            content.DestroyChildren();
            factoryViews.Clear();
            // container.InstantiatePrefab(clickerViewPrefab, content);
            foreach (var factoryLevelsData in factoriesReferencer.Factories) {
                var factoryView = container.InstantiateTypedPrefab(factoryViewPrefab, content);
                factoryView.Init(factoryLevelsData, economyDataStore);
                factoryViews.Add(factoryView);
            }
        }

        void OnEnable() {
            economyDataStore.FactoryUpgradedEvent += OnFactoryUpgraded;
            economyDataStore.CurrentTotalAmount.ChangedValue += OnTotalAmountChanged;
        }

        void OnDisable() {
            economyDataStore.FactoryUpgradedEvent -= OnFactoryUpgraded;
            economyDataStore.CurrentTotalAmount.ChangedValue -= OnTotalAmountChanged;
        }

        void OnFactoryUpgraded() {
            Refresh();
        }

        void OnTotalAmountChanged(ulong value) {
            Refresh();
        }

        void Refresh() {
            foreach (var factoryView in factoryViews) {
                factoryView.RefreshUnlockedState();
            }
        }
    }
}