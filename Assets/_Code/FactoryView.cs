using System;
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

        [SerializeField] FactoryUpgradeButtonView factoryUpgradeButtonView;

        ResourceFactory factory;
        FactoryLevelsData factoryLevelsData;
        EconomyDataStore economyDataStore;
        
        public string FactoryId { get; private set; }

        void OnEnable() {
            button.onClick.AddListener(BuyUpgrade);
            clickerButton.onClick.AddListener(ClickerButtonClick);
            factoryUpgradeButtonView.PointerEnter += Refresh;
            factoryUpgradeButtonView.PointerExit += Refresh;
            if (economyDataStore != null)
                economyDataStore.CurrentTotalAmount.Changed += Refresh;
        }

        void OnDisable() {
            button.onClick.RemoveListener(BuyUpgrade);
            clickerButton.onClick.RemoveListener(ClickerButtonClick);
            factoryUpgradeButtonView.PointerEnter -= Refresh;
            factoryUpgradeButtonView.PointerExit -= Refresh;
            if (economyDataStore != null)
                economyDataStore.CurrentTotalAmount.Changed -= Refresh;
        }

        void BuyUpgrade() {
            if (economyDataStore == null || factoryLevelsData == null) {
                Debug.LogError("Missing economyDataStore or factoryLevelsData");
                var economy = economyDataStore == null ? "null" : economyDataStore.name;
                var factoryData = factoryLevelsData == null ? "null" : factoryLevelsData.name;
                Debug.Log($"{economy}, {factoryData}");
                return;
            }

            if (factoryLevelsData.IsLastLevel(factory.Level)) {
                Debug.LogError($"Last level - {factory.Level}. Cant upgrade further. {factoryLevelsData.FactoryName}", factoryLevelsData);
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
            FactoryId = factoryLevelsData.FactoryId;
            nameLabel.text = factoryLevelsData.FactoryName;
            this.economyDataStore = economyDataStore;
            this.factory = economyDataStore.AddOrUpgradeFactory(factoryLevelsData, 0);
            this.factoryLevelsData = factoryLevelsData;
            // factory type idle 
            idleContainer.gameObject.SetActive(factoryLevelsData.FactoryType == FactoryType.Idle);
            clickerContainer.gameObject.SetActive(factoryLevelsData.FactoryType == FactoryType.Clicker);
            if (economyDataStore != null)
                economyDataStore.CurrentTotalAmount.Changed += Refresh;
            // idle state
            // factory type clicker 
            // clicker state
            Refresh();
        }

        void Refresh() {
            var hasNextLevel = factoryLevelsData.HasNextLevel(factory.Level);
            if (factoryUpgradeButtonView.IsHovered && hasNextLevel) {
                valueLabel.text = factoryLevelsData.GetValueForLevel(factory.Level + 1).ToString();
                clickerButtonLabel.text = $"+{factoryLevelsData.GetValueForLevel(factory.Level + 1)}";
                amountLabel.text = $"{factory.Level + 1}";
            }
            else {
                valueLabel.text = factoryLevelsData.GetValueForLevel(factory.Level).ToString();
                clickerButtonLabel.text = $"+{factoryLevelsData.GetValueForLevel(factory.Level)}";
                amountLabel.text = $"{factory.Level}";
            }
            
            if (!hasNextLevel) {
                factoryUpgradeButtonView.ApplyMax();
            }
            else {
                var costAmount = factoryLevelsData.GetCostForNextLevel(factory.Level);
                bool canAffort = economyDataStore.HasEnoughMoney(costAmount);
                if (canAffort)
                    factoryUpgradeButtonView.ApplyAvailable();
                else 
                    factoryUpgradeButtonView.ApplyUnavailable();
                
                costLabel.text = $"Cost: {costAmount}";
                nextLevelValueDifferenceLabel.text = $"Buy 1";
            }
        }

        public void RefreshUnlockedState() {
            if (factory == null) {
                Hide();
                return;
            }
            switch (factory.State) {
                case FactoryState.Hidden:
                    Hide();
                    break;
                case FactoryState.Shown:
                    ShowFactory();
                    break;
                case FactoryState.Unlocked:
                    Unlock();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;

            void Hide() {
                gameObject.SetActive(false);
            }

            void Unlock() {
                Debug.Log($"Unlock: {FactoryId}", this);
                gameObject.SetActive(true);
            }

            void ShowFactory() {
                Debug.Log($"Show: {FactoryId}", this);
                gameObject.SetActive(true);
            }
        }
    }
}