using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc {
    public class FactoriesPanel : MonoBehaviour {
        [SerializeField] FactoryView factoryViewPrefab;
        [SerializeField] Transform content;

        [HInject] FactoriesReferencer factoriesReferencer;
        [HInject] EconomyDataStore economyDataStore;

        void Start() {
            content.DestroyChildren();
            foreach (var factoryLevelsData in factoriesReferencer.Factories) {
                var factoryView = Instantiate(factoryViewPrefab, content);
                factoryView.Init(factoryLevelsData, economyDataStore);
            }
        }
    }
}