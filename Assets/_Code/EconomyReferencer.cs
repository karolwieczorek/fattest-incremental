using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc {
    public class EconomyReferencer : HReferencer {
        [SerializeField] ulong startingValue = 3;

        [SerializeField] ProgressionLevelsData progressionLevelsData;
        
        public ulong StartingValue => startingValue;

        public ProgressionLevelsData ProgressionLevelsData => progressionLevelsData;
    }
}