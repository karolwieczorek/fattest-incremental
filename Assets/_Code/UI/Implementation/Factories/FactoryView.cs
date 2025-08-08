using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FattestInc.Economy.API;
using FattestInc.Progression.API;
using Hypnagogia.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.UI.Implementation.Factories {
    public class FactoryView : MonoBehaviour {
        [SerializeField] Button button;

        [SerializeField] FactoryIconView factoryIconView;
        [SerializeField] Image progressBar;
        [SerializeField] TMP_Text nameLabel;
        [SerializeField] TMP_Text valueLabel;
        [SerializeField] TMP_Text costLabel;
        [SerializeField] TMP_Text nextLevelValueDifferenceLabel;
        [SerializeField] TMP_Text timeLeftA;
        [SerializeField] TMP_Text timeLeftB;

        [SerializeField] GameObject idleContainer;
        [SerializeField] GameObject clickerContainer;
        [SerializeField] TMP_Text clickerButtonLabel;
        [SerializeField] Button clickerButton;

        [SerializeField] FactoryUpgradeButtonView factoryUpgradeButtonView;
        [SerializeField] GameObject unlockedState; 
        [SerializeField] FactoryShowedStateView factoryShowedStateView;

        ResourceFactory factory;
        ResourceFactory Factory {
            get => factory;
            set {
                if (factory == value)
                    return;
                
                if (factory != null)
                    factory.State.Changed -= RefreshUnlockedState;
                factory = value;
                if (factory != null)
                    factory.State.Changed += RefreshUnlockedState;
            }
        }
        IFactoryData factoryLevelsData;

        [HInject] BuyMultipleHelper buyMultipleHelper;
        [HInject] BuyMultipleDataStore buyMultipleDataStore;
        [HInject] UnlockingHelper unlockingHelper;
        [HInject] EconomyDataStore economyDataStore;
        
        public string FactoryId { get; private set; }

        CancellationTokenSource cancellationTokenSource;

        void OnEnable() {
            button.onClick.AddListener(BuyUpgrade);
            clickerButton.onClick.AddListener(ClickerButtonClick);
            factoryUpgradeButtonView.PointerEnter += Refresh;
            factoryUpgradeButtonView.PointerExit += Refresh;
            buyMultipleDataStore.BuyMultipleUpdated += OnBuyMultipleUpdated;
            if (economyDataStore != null)
                economyDataStore.CurrentTotalAmount.Changed += Refresh;

            if (cancellationTokenSource != null) {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            cancellationTokenSource = new CancellationTokenSource();
            RefreshTimeTask(cancellationTokenSource.Token).Forget();
        }

        void OnDisable() {
            button.onClick.RemoveListener(BuyUpgrade);
            clickerButton.onClick.RemoveListener(ClickerButtonClick);
            factoryUpgradeButtonView.PointerEnter -= Refresh;
            factoryUpgradeButtonView.PointerExit -= Refresh;
            buyMultipleDataStore.BuyMultipleUpdated -= OnBuyMultipleUpdated;
            if (economyDataStore != null)
                economyDataStore.CurrentTotalAmount.Changed -= Refresh;
            
            if (cancellationTokenSource != null) {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        void OnBuyMultipleUpdated(BuyMultipleData _) {
            Refresh();
        }

        void BuyUpgrade() {
            if (economyDataStore == null || factoryLevelsData == null) {
                Debug.LogError("Missing economyDataStore or factoryLevelsData");
                var economy = economyDataStore == null ? "null" : economyDataStore.name;
                var factoryData = factoryLevelsData == null ? "null" : (factoryLevelsData as UnityEngine.Object)?.name;
                Debug.Log($"{economy}, {factoryData}");
                return;
            }

            if (factoryLevelsData.IsLastLevel(Factory.Level)) {
                Debug.LogError($"Last level - {Factory.Level}. Cant upgrade further. {factoryLevelsData.FactoryName}", this);
                return;
            }

            buyMultipleHelper.GetAmountForMultiBuy(factoryLevelsData, Factory.Level, out var amount, out var cost);
            // var cost = factoryLevelsData.GetCostForNextLevel(factory.Level);
            if (economyDataStore.TryBuy(cost)) {
                economyDataStore.AddOrUpgradeFactory(factoryLevelsData, amount);
                Refresh();
            } else {
                Debug.Log($"Not enough money to buy upgrade: current: {economyDataStore.CurrentTotalAmount.Value}, cost: {cost}");
            }
        }

        void ClickerButtonClick() {
            economyDataStore.CurrentTotalAmount.Value += (ulong)factoryLevelsData.GetValueForLevel(Factory.Level);
        }

        void Update() {
            if (Factory == null)
                return;

            progressBar.fillAmount = Factory.Progress;
        }

        public void Init(IFactoryData factoryLevelsData) {
            factoryIconView.SetIcon(factoryLevelsData.Icon);
            FactoryId = factoryLevelsData.FactoryId;
            nameLabel.text = factoryLevelsData.FactoryName;
            this.economyDataStore = economyDataStore;
            this.Factory = economyDataStore.GetOrInitFactory(factoryLevelsData);
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
            var hasNextLevel = factoryLevelsData.HasNextLevel(Factory.Level);
            if (factoryUpgradeButtonView.IsHovered && hasNextLevel) {
                buyMultipleHelper.GetAmountForMultiBuy(factoryLevelsData, Factory.Level, out var amount, out var price);
                valueLabel.text = NumbersFormattingUtil.FormatNumber(factoryLevelsData.GetValueForLevel(Factory.Level + amount));
                clickerButtonLabel.text = $"+{factoryLevelsData.GetValueForLevel(Factory.Level + amount)}";
                factoryIconView.SetAmount(Factory.Level + amount);
            }
            else {
                valueLabel.text = NumbersFormattingUtil.FormatNumber(factoryLevelsData.GetValueForLevel(Factory.Level));
                clickerButtonLabel.text = $"+{factoryLevelsData.GetValueForLevel(Factory.Level)}";
                factoryIconView.SetAmount(Factory.Level);
            }

            if (!hasNextLevel) {
                factoryUpgradeButtonView.ApplyMax();
            }
            else {
                buyMultipleHelper.GetAmountForMultiBuy(factoryLevelsData, Factory.Level, out var amount, out var price);

                var costAmount = price;
                bool canAffort = economyDataStore.HasEnoughMoney(costAmount);
                if (canAffort)
                    factoryUpgradeButtonView.ApplyAvailable();
                else 
                    factoryUpgradeButtonView.ApplyUnavailable();
                
                costLabel.text = $"Cost: {NumbersFormattingUtil.FormatNumber(costAmount)}";
                nextLevelValueDifferenceLabel.text = $"Buy {amount}";
            }
        }

        public void RefreshUnlockedState() {
            if (Factory == null) {
                Hide();
                return;
            }
            switch (Factory.State.Value) {
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
                // Debug.Log($"Hide: {FactoryId}", this);
                gameObject.SetActive(false);
            }

            void Unlock() {
                // Debug.Log($"Unlock: {FactoryId}", this);
                factoryShowedStateView.Hide();
                unlockedState.SetActive(true);
                gameObject.SetActive(true);
            }

            void ShowFactory() {
                // Debug.Log($"Show: {FactoryId}", this);
                unlockedState.SetActive(false);
                var unlockingData = unlockingHelper.GetUnlockingData(FactoryId);
                factoryShowedStateView.Show(unlockingData);
                gameObject.SetActive(true);
            }
        }

        async UniTaskVoid RefreshTimeTask(CancellationToken cancellationToken) {
            while (true) {
                await UniTask.WaitForSeconds(0.1f, cancellationToken: cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (Factory != null) {
                    if (Factory.Duration > 10) {
                        var timeSpan = TimeSpan.FromSeconds(Factory.TimeLeft);
                        var timeLabel = $"{timeSpan.TotalMinutes:0}:{timeSpan.Seconds}";
                        timeLeftA.text = timeLabel;
                        timeLeftB.text = timeLabel;
                    }
                    else {
                        // var timeSpan = TimeSpan.FromSeconds(factory.TimeLeft);
                        var timeLabel = $"{Factory.TimeLeft:F1}";
                        timeLeftA.text = timeLabel;
                        timeLeftB.text = timeLabel;
                    }
                }
            }
        }
    }
}