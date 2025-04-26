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
    public class BuyMultipleData {
        public string label;
        [ShowIf(nameof(NotMax))]
        public int number;
        public MultipleType type;

        bool NotMax => type != MultipleType.Max;
    }

    public enum MultipleType { Number, NumberOrLess, Max }
}