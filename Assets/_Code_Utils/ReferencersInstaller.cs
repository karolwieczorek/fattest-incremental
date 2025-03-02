using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Hypnagogia.Utils {
    public class ReferencersInstaller : ScriptableObjectInstaller {
        const string ReferencersPath = "_Data/Referencers";

        [AssetList(Path = ReferencersPath, AutoPopulate = true)]
        [SerializeField] List<HReferencer> referencers;

        public override void InstallBindings() {
            foreach (var hReferencer in referencers) {
                Container.Bind(hReferencer.GetType()).FromInstance(hReferencer);
            }
        }
    }
}