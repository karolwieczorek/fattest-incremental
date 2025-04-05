using Hypnagogia.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace FattestInc {
    public class ProgressionPanel : MonoBehaviour {
        [SerializeField] ProgressionLevelView progressionLevelViewPrefab;
        [SerializeField] Transform content;

        [HInject] DiContainer container;
        [HInject] EconomyReferencer economyReferencer;

        void Start() {
            content.DestroyChildren();
            var progressionLevels = economyReferencer.ProgressionLevelsData.LevelsList;
            foreach (var levelData in progressionLevels) {
                var view = container.InstantiateTypedPrefab(progressionLevelViewPrefab, content);
                view.Init(levelData);
            }
        }
    }
}