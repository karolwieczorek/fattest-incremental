using System;
using UnityEngine;

namespace Utils.PlayerPrefUtils
{
    public abstract class PlayerPref<T> where T : IComparable {
        protected string Key;
        protected T DefaultValue;
        protected bool readDone;
        protected T currentValue;
        public abstract T Value { get; set; }

        protected void Set(T value, Action<string, T> setter) {
            if (!value.Equals(currentValue)) {
                currentValue = value;
                setter(Key, value);
                PlayerPrefs.Save();
                readDone = true;
            }
        }

        protected T Get(Func<string, T, T> getter) {
            if (!readDone) {
                currentValue = getter(Key, DefaultValue);
                readDone = true;
            }
            return currentValue;
        }

        public void Unset() {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
            currentValue = DefaultValue;
            readDone = true;
        }

        public static implicit operator T(PlayerPref<T> v) {
            return v.Value;
        }

        public bool IsSet() {
            return PlayerPrefs.HasKey(Key);
        }
    }
}