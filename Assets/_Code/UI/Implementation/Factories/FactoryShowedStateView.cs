using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;
using FattestInc.Progression.API;
using Hypnagogia.Utils;
using TMPro;
using UnityEngine;

namespace FattestInc.UI.Implementation.Factories {
    public class FactoryShowedStateView : MonoBehaviour {
        [SerializeField] TMP_Text requiredAmountPerSecond;
        [SerializeField] FactoryIconView factory1Icon;
        [SerializeField] FactoryIconView factory2Icon;
        [SerializeField] FactoryIconView factory3Icon;

        [HInject] FactoriesReferencer factoriesReferencer;

        public void Hide() {
            gameObject.SetActive(false);
        }

        public void Show(UnlockingFactoriesData.UnlockingData unlockingData) {
            if (unlockingData == null) {
                Debug.LogError("Unlocking data is null.", this);
                return;
            }
            gameObject.SetActive(true);
            requiredAmountPerSecond.gameObject.SetActive(unlockingData.valuePerSecond > 0);
            requiredAmountPerSecond.text = $"{unlockingData.valuePerSecond}/s";

            SetupFactoryRequirement(factory1Icon, unlockingData.factory1Id, unlockingData.factory1Level);
            SetupFactoryRequirement(factory2Icon, unlockingData.factory2Id, unlockingData.factory2Level);
            SetupFactoryRequirement(factory3Icon, unlockingData.factory3Id, unlockingData.factory3Level);

        }

        void SetupFactoryRequirement(FactoryIconView factoryIconView, string factoryId, int factoryLevel) {
            if (factoryId.IsNullOrWhitespace() || factoryLevel == 0) {
                factoryIconView.gameObject.SetActive(false);
                return;
            }

            var factoryLevelsData = factoriesReferencer.Factories.FirstOrDefault(x => x.FactoryId == factoryId);
            if (factoryLevelsData == null) {
                factoryIconView.gameObject.SetActive(false);
                Debug.LogError($"Could not find factory: {factoryId}", this);
                return;
            }

            factoryIconView.SetIcon(factoryLevelsData.Icon);
            factoryIconView.SetAmount(factoryLevel);
            // get factory icon by id
            // get set target level
            // color level red if not yet reached
            factoryIconView.gameObject.SetActive(true);
        }
    }
}