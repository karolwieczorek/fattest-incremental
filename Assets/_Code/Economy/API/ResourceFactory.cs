using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc.Economy.API {
    [System.Serializable]
    public class ResourceFactory {
        readonly FactoryType factoryType;
        FactoryState factoryState;
        [ShowInInspector] int level;
        [ShowInInspector] ulong valuePerTick;
        [ShowInInspector] float time;
        [ShowInInspector] float duration = 1f;

        [ShowInInspector]
        public float TimeLeft => duration - time;

        public float Duration => duration;

        [ShowInInspector] public float Progress {
            get {
                if (Type == FactoryType.Clicker)
                    return 1f;
                return duration > 0 ? time / duration : 0f;
            }
        }

        public int Level => level;

        public FactoryType Type => factoryType;
        public FactoryState State => factoryState;

        public ResourceFactory(FactoryType factoryType) {
            this.factoryType = factoryType;
            level = 0;
            valuePerTick = 0;
            time = 0;
            duration = 1f;
        }
        
        public void Upgrade(int level, ulong valuePerTick, float duration) {
            this.level = level;
            this.valuePerTick = valuePerTick;
            this.duration = duration;
        }

        public void Tick(float deltaTime, out ulong value) {
            if (level <= 0 || Type == FactoryType.Clicker) {
                value = 0;
                return;
            }

            time += deltaTime;

            int ticksCompleted = 0;
            if (duration > 0f) {
                ticksCompleted = Mathf.FloorToInt(time / duration);

                if (ticksCompleted > 0)
                    time -= ticksCompleted * duration;
            }

            value = ((ulong)ticksCompleted) * valuePerTick;
        }

        public float NextUpgradePerSecondAmount() {
            return GetValuePerSecond(level + 1);
        }

        public float GetNextLevelDifferencePerSecond() {
            return NextUpgradePerSecondAmount() - GetCurrentValuePerSecond();
        }

        public float GetCurrentValuePerSecond() {
            if (factoryType == FactoryType.Clicker)
                return 0;
            return GetValuePerSecond(level);
        }
        
        public float GetValuePerSecond(int factoryLevel) {
            if (factoryLevel <= 0) {
                return 0;
            }
            return valuePerTick / duration;
        }

        public void Unlock() {
            factoryState = FactoryState.Unlocked;
        }

        public void Show() {
            factoryState = FactoryState.Shown;
        }
    }
}