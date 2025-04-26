using System;
using UnityEngine;

namespace Utils.PlayerPrefUtils
{
    [Serializable]
    public class PlayerPrefBool : PlayerPref<bool> {
        public override bool Value {
            get { return Get((x, y) => PlayerPrefs.GetInt(x, y ? 1 : 0) != 0); }
            set { Set(value, (x, y) => { PlayerPrefs.SetInt(x, y ? 1 : 0); }); }
        }

        public PlayerPrefBool(string key, bool defaultValue) {
            this.Key = key;
            this.DefaultValue = defaultValue;
        }
    }
}