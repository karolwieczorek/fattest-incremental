using Eflatun.SceneReference;
using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc {
    public class ScenesLoaderReferencer : HReferencer {
        [SerializeField] SceneReference bootstrapperScene = default;
        [SerializeField] SceneReference gameScene = default;

        public SceneReference BootstrapperScene => bootstrapperScene;
        public SceneReference GameScene => gameScene;
    }
}