using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc {
    public class FactoryView : MonoBehaviour {
        [SerializeField] Button button;
        [SerializeField] Image icon;
        [SerializeField] Image progressBar;
        [SerializeField] TMP_Text nameLabel;
        [SerializeField] TMP_Text amountLabel;
        [SerializeField] TMP_Text valueLabel;
        [SerializeField] TMP_Text costLabel;
        [SerializeField] TMP_Text nextLevelValueDifferenceLabel;

        [SerializeField] GameObject idleContainer;
        [SerializeField] GameObject clickerContainer;
        [SerializeField] TMP_Text clickerButtonLabel;
        [SerializeField] Button clickerButton;

        ResourceFactory factory;
        FactoryLevelsData factoryLevelsData;
        EconomyDataStore economyDataStore;

        void OnEnable() {
            button.onClick.AddListener(BuyUpgrade);
            clickerButton.onClick.AddListener(ClickerButtonClick);
        }

        void OnDisable() {
            button.onClick.RemoveListener(BuyUpgrade);
            clickerButton.onClick.RemoveListener(ClickerButtonClick);
            factory = null;
        }

        void BuyUpgrade() {
            if (economyDataStore == null || factoryLevelsData == null) {
                Debug.LogError("Missing economyDataStore or factoryLevelsData");
                var economy = economyDataStore == null ? "null" : economyDataStore.name;
                var factoryData = factoryLevelsData == null ? "null" : factoryLevelsData.name;
                Debug.Log($"{economy}, {factoryData}");
                return;
            }

            var cost = factoryLevelsData.GetCostForNextLevel(factory.Level);
            if (economyDataStore.TryBuy(cost)) {
                economyDataStore.AddOrUpgradeFactory(factoryLevelsData, 1);
                Refresh();
            } else {
                Debug.Log($"Not enough money to buy upgrade: current: {economyDataStore.CurrentTotalAmount.Value}, cost: {cost}");
            }
        }

        void ClickerButtonClick() {
            economyDataStore.CurrentTotalAmount.Value += (ulong)factoryLevelsData.GetValueForLevel(factory.Level);
        }

        void Update() {
            if (factory == null)
                return;

            progressBar.fillAmount = factory.Progress;
        }

        public void Init(FactoryLevelsData factoryLevelsData, EconomyDataStore economyDataStore) {
            icon.sprite = factoryLevelsData.Icon;
            nameLabel.text = factoryLevelsData.FactoryName;
            this.economyDataStore = economyDataStore;
            this.factory = economyDataStore.AddOrUpgradeFactory(factoryLevelsData, 0);
            this.factoryLevelsData = factoryLevelsData;
            // factory type idle 
            idleContainer.gameObject.SetActive(factoryLevelsData.FactoryType == FactoryType.Idle);
            clickerContainer.gameObject.SetActive(factoryLevelsData.FactoryType == FactoryType.Clicker);
            // idle state
            // factory type clicker 
            // clicker state
            Refresh();
        }

        void Refresh() {
            amountLabel.text = $"{factory.Level}";
            // Debug.Log($"Value for level {factory.Level} = {factoryLevelsData.GetValueForLevel(factory.Level)}");
            valueLabel.text = factoryLevelsData.GetValueForLevel(factory.Level).ToString();
            var costAmount = factoryLevelsData.GetCostForNextLevel(factory.Level);
            costLabel.text = $"Cost: {costAmount}";
            var valueForNextLevel = factoryLevelsData.GetValueDifferenceForNextLevel(factory.Level).ToString();
            var addMode = factoryLevelsData.FactoryType == FactoryType.Idle ? "tick" : "click";
            nextLevelValueDifferenceLabel.text = $"+{valueForNextLevel}/{addMode}";
            clickerButtonLabel.text = $"+{factoryLevelsData.GetValueForLevel(factory.Level)}";
        }
    }
}