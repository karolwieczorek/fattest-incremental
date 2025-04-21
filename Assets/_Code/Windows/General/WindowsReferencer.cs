using System.Collections.Generic;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc.Windows.General {
    public class WindowsReferencer : HReferencer {
        const string Path = "Prefabs/Windows";

        [AssetList(Path = Path, AutoPopulate = true)] 
        [SerializeField] List<WindowBase> windowsPrefabs;

        public List<WindowBase> WindowsPrefabs => windowsPrefabs;
    }
}