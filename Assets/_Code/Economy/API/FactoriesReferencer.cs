using System.Collections.Generic;
using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc.Economy.API {
    public class FactoriesReferencer : HReferencer {
        const string DataPath = "_Data/Factories";

        // [AssetList(Path = DataPath, AutoPopulate = false)]
        [SerializeField] List<ScriptableObject> factories;

        public IReadOnlyList<IFactoryData> Factories {
            get {
                cachedList.Clear();
                if (factories != null) {
                    foreach (var so in factories) {
                        if (so is IFactoryData data)
                            cachedList.Add(data);
                    }
                }
                return cachedList;
            }
        }

        readonly List<IFactoryData> cachedList = new();
    }
}