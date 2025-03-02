using System.Collections.Generic;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc {
    public class FactoriesReferencer : HReferencer {
        const string DataPath = "_Data/Factories";

        [AssetList(Path = DataPath, AutoPopulate = false)]
        [SerializeField] List<FactoryData> factories;
    }
}