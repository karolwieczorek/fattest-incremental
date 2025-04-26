using Hypnagogia.Utils;

namespace FattestInc.Economy.API {
    public class BuyMultipleHelper {
        [HInject] BuyMultipleDataStore buyMultipleDataStore;
        [HInject] EconomyDataStore economyDataStore;

        public void GetAmountForMultiBuy(FactoryLevelsData factoryLevelsData, int level, out int amount, out ulong price) {
            var data = buyMultipleDataStore.CurrentBuyMultipleData;
            if (data.type is MultipleType.Number) {
                ulong priceSum = 0;
                for (int i = 1; i <= data.number; i++) {
                    priceSum = factoryLevelsData.GetCostForLevel(level + i);
                }
                amount = 1;
                price = priceSum;
            }
            else if (data.type is MultipleType.NumberOrLess) {
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
            } else if (data.type is MultipleType.Max) {
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
            } else {
                amount = 0;
                price = 0;
            }
        }
    }
}