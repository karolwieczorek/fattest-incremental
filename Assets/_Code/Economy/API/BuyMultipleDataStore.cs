using System;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;

namespace FattestInc.Economy.API {
    public class BuyMultipleDataStore : HDataStore {
        [ShowInInspector]
        public BuyMultipleData CurrentBuyMultipleData { get; private set; }
        public event Action<BuyMultipleData> BuyMultipleUpdated;

        public void SetBuyMultipleData(BuyMultipleData data) {
            CurrentBuyMultipleData = data;
            BuyMultipleUpdated?.Invoke(data);
        }
    }
}