using System;
using UnityEngine;

namespace Utils.PlayerPrefUtils
{
    [Serializable]
    public class PlayerPrefString : PlayerPref<string> {
        public override string Value {
            get => Get(PlayerPrefs.GetString);
            set => Set(value, PlayerPrefs.SetString);
        }

        public PlayerPrefString(string key, string defaultValue) {
            this.Key = key;
            this.DefaultValue = defaultValue;
        }
    }
}