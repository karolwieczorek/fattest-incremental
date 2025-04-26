using System;
using System.Linq;
using Hypnagogia.Utils;

namespace FattestInc.Economy.API {
    public class BuyMultipleInitializationSystem : HSystem {
        [HInject] BuyMultipleDataStore buyMultipleDataStore;
        [HInject] BuyMultipleReferencer buyMultipleReferencer;
        
        protected override void SystemStart() {
            var data = buyMultipleReferencer.BuyMultipleDataList.FirstOrDefault();
            if (data is null)
                throw new NullReferenceException("BuyMultipleData is null");
            buyMultipleDataStore.SetBuyMultipleData(data);
        }
    }
}