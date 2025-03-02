using System;
using UnityEngine;

namespace FattestInc.Utils.GUIHelpers {
    public class GUIColor : IDisposable {
        readonly Color defaultColor;

        public GUIColor(Color color) {
            defaultColor = GUI.color;
            GUI.color = color;
        }

        public void Dispose() {
            GUI.color = defaultColor;
        }
    }
}