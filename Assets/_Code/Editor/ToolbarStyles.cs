using UnityEngine;

namespace FattestInc {
    public static class ToolbarStyles {
        public static GUIStyle Command => "Command";

        public static GUIStyle DropDown => "DropDown";

        public static Rect GetThinArea(Rect pos) => new(pos.x, 2f, pos.width, 18f);

        public static Rect GetThickArea(Rect pos) => new(pos.x, 0f, pos.width, 24f);
    }
}