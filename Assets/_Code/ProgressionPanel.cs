using System.Collections.Generic;
using Hypnagogia.Utils;
using UnityEngine;
using Zenject;

namespace FattestInc {
    public class ProgressionPanel : MonoBehaviour {
        [SerializeField] ProgressionLevelView progressionLevelViewPrefab;
        [SerializeField] Transform content;

        [HInject] DiContainer container;
        [HInject] EconomyReferencer economyReferencer;
        [HInject] EconomyDataStore economyDataStore;

        readonly List<ProgressionLevelView> viewsList = new();

        void OnEnable() {
            economyDataStore.CurrentAmountPerSecond.ChangedValue += OnCurrentAmountChanged;
        }

        void OnDisable() {
            economyDataStore.CurrentAmountPerSecond.ChangedValue -= OnCurrentAmountChanged;
        }

        void OnCurrentAmountChanged(float value) {
            Debug.Log($"Current amount per second: {value}");
            RefreshCurrentView(value);
        }

        void RefreshCurrentView(float value) {
            ProgressionLevelView currentView = null;
            foreach (var view in viewsList) {
                if (value >= view.Value)
                    view.SetCompleted();
                else if (currentView == null) {
                    currentView = view;
                    view.SetCurrent();
                    Debug.Log($"Current view: {view.name}", view);
                }
                else {
                    view.SetFuture();
                }
            }
        }

        void Start() {
            content.DestroyChildren();
            var progressionLevels = economyReferencer.ProgressionLevelsData.LevelsList;
            foreach (var levelData in progressionLevels) {
                var view = container.InstantiateTypedPrefab(progressionLevelViewPrefab, content);
                view.Init(levelData);
                viewsList.Add(view);
            }
            RefreshCurrentView(economyDataStore.CurrentAmountPerSecond.Value);
        }
    }
}