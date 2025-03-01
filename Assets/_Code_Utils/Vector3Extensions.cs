using UnityEngine;

namespace Hypnagogia.Utils {
    public static class Vector3Extensions {
        public static Vector3 Change(this Vector3 org, float? x = null, float? y = null, float? z = null) 
        {
            return new Vector3(x ?? org.x, y ?? org.y, z ?? org.z);
        }
        
        public static Vector3 Move(this Vector3 org, float? x = null, float? y = null, float? z = null) 
        {
            return org + Vector3.zero.Change(x, y, z);
        }
    }
}