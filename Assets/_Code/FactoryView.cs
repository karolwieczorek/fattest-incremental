using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc {
    public class FactoryView : MonoBehaviour {
        [SerializeField] Button button;
        [SerializeField] Image progressBar;
        [SerializeField] TMP_Text amountLabel;
        [SerializeField] TMP_Text valueLabel;
        [SerializeField] TMP_Text costLabel;

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
            var economy = economyDataStore == null ? "null" : economyDataStore.name;
            var factory = factoryLevelsData == null ? "null" : factoryLevelsData.name;
            Debug.Log($"{economy}, {factory}");
            economyDataStore.AddOrUpgradeFactory(factoryLevelsData.FactoryId, 1);
            Refresh();
        }

        void Update() {
            if (factory == null)
                return;

            progressBar.fillAmount = factory.Progress;
        }

        public void Init(FactoryLevelsData factoryLevelsData, EconomyDataStore economyDataStore) {
            this.economyDataStore = economyDataStore;
            this.factory = economyDataStore.AddOrUpgradeFactory(factoryLevelsData.FactoryId, 0);
            this.factoryLevelsData = factoryLevelsData;
            Refresh();
        }

        void Refresh() {
            amountLabel.text = $"Amount: {factory.Level}";
            valueLabel.text = factoryLevelsData.GetValueForLevel(factory.Level).ToString();
            costLabel.text = factoryLevelsData.GetCostForNextLevel(factory.Level).ToString();
        }
    }
}