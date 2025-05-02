using System;
using Hypnagogia.Utils;

namespace FattestInc.Economy.API {
    public class BuyMultipleHelper {
        [HInject] BuyMultipleDataStore buyMultipleDataStore;
        [HInject] EconomyDataStore economyDataStore;

        public void GetAmountForMultiBuy(FactoryLevelsData factoryLevelsData, int level, out int amount, out ulong price) {
            IMultiBuySetting data = buyMultipleDataStore.CurrentBuyMultipleData;
            int amountToBuy = data.Amount;

            switch (data.Type) {
                case MultipleType.Number:
                    GetAmountForMultiBuyForANumber(factoryLevelsData, level, amountToBuy, out amount, out price);
                    return;
                case MultipleType.NumberOrLess:
                    GetAmountOrLessForMultiBuyForANumber(HasEnoughMoney, factoryLevelsData, level,
                        amountToBuy, out amount, out price);
                    return;
                case MultipleType.Max:
                    GetMaxAmountForMultiBuy(HasEnoughMoney, factoryLevelsData, level, out amount, out price);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            bool HasEnoughMoney(ulong cost) {
                return economyDataStore.HasEnoughMoney(cost); 
            }
        }

        public static void GetMaxAmountForMultiBuy(Func<ulong, bool> hasEnoughMoney, IFactoryLevelsData factoryLevelsData, int currentLevel,
            out int amount, out ulong price) {
            var levelsToBuy = factoryLevelsData.GetLastLevel() - currentLevel;
            GetAmountOrLessForMultiBuyForANumber(hasEnoughMoney, factoryLevelsData, currentLevel, levelsToBuy, out amount, out price);
        }
        
        public static void GetAmountOrLessForMultiBuyForANumber(Func<ulong, bool> hasEnoughMoney, IFactoryLevelsData factoryLevelsData, int currentLevel,
            int levelsToBuy, out int amount, out ulong price) {
            ulong priceSum = 0;
            int i = 0;
            var levelsLeft = factoryLevelsData.GetLastLevel() - currentLevel;
            if (levelsLeft <= 0) {
                amount = 0;
                price = 0;
                return;
            }

            levelsLeft = Math.Min(levelsToBuy, levelsLeft);
            for (; i < levelsLeft; i++) {
                var levelPrice = factoryLevelsData.GetCostForLevel(currentLevel + i + 1);
                if (hasEnoughMoney(priceSum + levelPrice))
                    priceSum += levelPrice;
                else
                    break;
            }
            amount = i;
            price = priceSum;
                
            if (amount < 1) {
                amount = 1;
                price = factoryLevelsData.GetCostForLevel(currentLevel + 1);
            }
        }

        public static void GetAmountForMultiBuyForANumber(IFactoryLevelsData factoryLevelsData, int currentLevel, int levelsToBuy,
            out int amount, out ulong price) {
            if (factoryLevelsData.IsLastLevel(currentLevel)) {
                amount = 0;
                price = 0;
                return;
            }

            ulong priceSum = 0;
            var targetLevel = currentLevel + levelsToBuy;
            amount = 0;
            for (int level = currentLevel + 1; level <= targetLevel; level++) {
                var costForLevel = factoryLevelsData.GetCostForLevel(level);
                priceSum += costForLevel;
                // Debug.Log($"level: {level} - {costForLevel} - {priceSum}");
                amount++;
                
                if (factoryLevelsData.IsLastLevel(level))
                    break;
            }

            // amount = levelsToBuy;
            price = priceSum;
        }
    }
}