using System;
using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc.Economy.API {
    public class BuyMultipleHelper {
        [HInject] BuyMultipleDataStore buyMultipleDataStore;
        [HInject] EconomyDataStore economyDataStore;

        public void GetAmountForMultiBuy(FactoryLevelsData factoryLevelsData, int level, out int amount, out ulong price) {
            var data = buyMultipleDataStore.CurrentBuyMultipleData;

            switch (data.type) {
                case MultipleType.Number:
                    GetAmountForMultiBuyForANumber(factoryLevelsData, level, data.number, out amount, out price);
                    return;
                case MultipleType.NumberOrLess:
                    break;
                case MultipleType.Max:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // if (data.type is MultipleType.Number) {
            //     GetAmountForMultiBuyForANumber(factoryLevelsData, level, data.number, out amount, out price);
            //     return;
            // }

            if (data.type is MultipleType.NumberOrLess) {
                ulong priceSum = 0;
                int i = 0;
                for (; i <= data.number; i++) {
                    var levelPrice = factoryLevelsData.GetCostForLevel(level + i + 1);
                    if (economyDataStore.HasEnoughMoney(priceSum + levelPrice))
                        priceSum += levelPrice;
                    else
                        break;
                }
                amount = i;
                price = priceSum;
                
                if (amount < 1) {
                    amount = 1;
                    price = factoryLevelsData.GetCostForLevel(level + 1);
                }

                return;
            }

            if (data.type is MultipleType.Max) {
                ulong priceSum = 0;
                int i = 0;
                var levelsLeft = factoryLevelsData.GetLastLevel() - level;
                if (levelsLeft <= 0) {
                    amount = 0;
                    price = 0;
                }
                for (; i <= levelsLeft; i++) {
                    var levelPrice = factoryLevelsData.GetCostForLevel(level + i + 1);
                    if (economyDataStore.HasEnoughMoney(priceSum + levelPrice))
                        priceSum += levelPrice;
                    else
                        break;
                }
                amount = i;
                price = priceSum;
                
                if (amount < 1) {
                    amount = 1;
                    price = factoryLevelsData.GetCostForLevel(level + 1);
                }
                return;
            }
            
            amount = 0;
            price = 0;
            throw new ArgumentOutOfRangeException();
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
                Debug.Log($"level: {level} - {costForLevel} - {priceSum}");
                amount++;
                
                if (factoryLevelsData.IsLastLevel(level))
                    break;
            }

            // amount = levelsToBuy;
            price = priceSum;
        }
    }
}