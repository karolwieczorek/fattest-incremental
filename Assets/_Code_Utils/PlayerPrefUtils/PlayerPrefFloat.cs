using System;
using UnityEngine;

namespace Utils.PlayerPrefUtils
{
    [Serializable]
    public class PlayerPrefFloat : PlayerPref<float> {
        public override float Value {
            get => Get(PlayerPrefs.GetFloat);
            set => Set(value, PlayerPrefs.SetFloat);
        }

        public PlayerPrefFloat(string key, float defaultValue) {
            this.Key = key;
            this.DefaultValue = defaultValue;
        }
    }
}