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

        ResourceFactory factory;
        FactoryLevelsData factoryLevelsData;
        EconomyDataStore economyDataStore;

        void OnEnable() {
            button.onClick.AddListener(BuyUpgrade);
        }

        void OnDisable() {
            button.onClick.RemoveListener(BuyUpgrade);
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
            Refresh();
        }

        void Refresh() {
            amountLabel.text = $"{factory.Level}";
            Debug.Log($"Value for level {factory.Level} = {factoryLevelsData.GetValueForLevel(factory.Level)}");
            valueLabel.text = factoryLevelsData.GetValueForLevel(factory.Level).ToString();
            var costAmount = factoryLevelsData.GetCostForNextLevel(factory.Level);
            costLabel.text = $"Cost: {costAmount}";
            var valueForNextLevel = factoryLevelsData.GetValueDifferenceForNextLevel(factory.Level).ToString();
            nextLevelValueDifferenceLabel.text = $"+{valueForNextLevel}/tick";
        }
    }
}