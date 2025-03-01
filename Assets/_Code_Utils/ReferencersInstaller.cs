using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Hypnagogia.Utils {
    public class ReferencersInstaller : ScriptableObjectInstaller {
        const string ReferencersPath = "_Game/_Data/Referencers";

        // [AssetList(Path = ReferencersPath, AutoPopulate = true)]
        [SerializeField] List<HReferencer> referencers;

        public override void InstallBindings() {
            foreach (var hReferencer in referencers) {
                Container.Bind(hReferencer.GetType()).FromInstance(hReferencer);
            }
        }
    }
}