using Hypnagogia.Utils;
using UnityEngine;
using Zenject;

namespace FattestInc {
    public class FactoriesPanel : MonoBehaviour {
        [SerializeField] FactoryView factoryViewPrefab;
        [SerializeField] GameObject clickerViewPrefab;
        [SerializeField] Transform content;

        [HInject] DiContainer container;
        [HInject] FactoriesReferencer factoriesReferencer;
        [HInject] EconomyDataStore economyDataStore;

        void Start() {
            content.DestroyChildren();
            container.InstantiatePrefab(clickerViewPrefab, content);
            foreach (var factoryLevelsData in factoriesReferencer.Factories) {
                var factoryView = Instantiate(factoryViewPrefab, content);
                factoryView.Init(factoryLevelsData, economyDataStore);
            }
        }
    }
}