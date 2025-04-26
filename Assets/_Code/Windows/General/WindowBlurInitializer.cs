using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc.Windows.General {
    public class WindowBlurInitializer : MonoBehaviour {
        public GameObject windowBlur;

        [HInject] UIDataStore uiDataStore;

        void Awake() {
            uiDataStore.windowBlur = windowBlur;
        }
    }
}