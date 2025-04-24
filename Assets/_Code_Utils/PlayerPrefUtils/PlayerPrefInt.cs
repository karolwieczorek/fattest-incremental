using System;
using UnityEngine;

namespace Utils.PlayerPrefUtils
{
    [Serializable]
    public class PlayerPrefInt : PlayerPref<int> {
        public override int Value {
            get => Get(PlayerPrefs.GetInt);
            set => Set(value, PlayerPrefs.SetInt);
        }

        public PlayerPrefInt(string key, int defaultValue = 0) {
            this.Key = key;
            this.DefaultValue = defaultValue;
        }
    }
}