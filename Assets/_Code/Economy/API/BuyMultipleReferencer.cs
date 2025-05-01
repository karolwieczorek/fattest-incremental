using System.Collections.Generic;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc.Economy.API {
    public class BuyMultipleReferencer : HReferencer {
        [SerializeField] List<BuyMultipleData> buyMultipleDataList = new();

        public IReadOnlyList<BuyMultipleData> BuyMultipleDataList => buyMultipleDataList;
    }

    [System.Serializable]
    public class BuyMultipleData : IMultiBuySetting {
        public string label;
        [ShowIf(nameof(NotMax))]
        public int number;
        public MultipleType type;

        bool NotMax => type != MultipleType.Max;

        public int Amount => number;
        public MultipleType Type => type;
    }

    public interface IMultiBuySetting {
        int Amount { get; }
        MultipleType Type { get; }
    }

    public enum MultipleType { Number, NumberOrLess, Max }
}