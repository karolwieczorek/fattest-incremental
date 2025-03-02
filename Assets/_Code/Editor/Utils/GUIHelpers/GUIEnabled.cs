using System;
using UnityEngine;

namespace FattestInc.Utils.GUIHelpers {
    public class GUIEnabled : IDisposable {
        readonly bool prevValue;

        public GUIEnabled(bool enabled) {
            prevValue = GUI.enabled;
            GUI.enabled = enabled;
        }

        public void Dispose() {
            GUI.enabled = prevValue;
        }
    }
}