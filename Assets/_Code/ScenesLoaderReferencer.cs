using Eflatun.SceneReference;
using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc {
    public class ScenesLoaderReferencer : HReferencer {
        [SerializeField] SceneReference gameScene = default;

        public SceneReference GameScene => gameScene;
    }
}