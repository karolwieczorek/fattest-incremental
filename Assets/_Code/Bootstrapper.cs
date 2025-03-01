using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc
{
    public class Bootstrapper : MonoBehaviour {
        [HInject] ScenesLoaderHelper scenesLoaderHelper;

        void Start() {
            scenesLoaderHelper.LoadGameScene();
        }
    }
}
