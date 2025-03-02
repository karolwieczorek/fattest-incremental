using System;
using UnityEngine;

namespace FattestInc.Utils.GUIHelpers {
    public class GUIBackgroundColor : IDisposable {
        readonly Color defaultColor;

        public GUIBackgroundColor(Color color) {
            defaultColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
        }

        public void Dispose() {
            GUI.backgroundColor = defaultColor;
        }
    }
}