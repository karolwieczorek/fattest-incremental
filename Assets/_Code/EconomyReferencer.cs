using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc {
    public class EconomyReferencer : HReferencer {
        [SerializeField] ulong startingValue = 3;
        
        public ulong StartingValue => startingValue;
    }
}