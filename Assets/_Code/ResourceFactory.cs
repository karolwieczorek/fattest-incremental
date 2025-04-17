using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc {
    [System.Serializable]
    public class ResourceFactory {
        readonly FactoryType factoryType;
        [ShowInInspector] int level;
        [ShowInInspector] int valuePerTick;
        [ShowInInspector] float time;
        [ShowInInspector] float duration = 1f;

        [ShowInInspector] public float Progress {
            get {
                if (Type == FactoryType.Clicker)
                    return 1f;
                return duration > 0 ? time / duration : 0f;
            }
        }

        public int Level => level;

        public FactoryType Type => factoryType;

        public ResourceFactory(FactoryType factoryType) {
            this.factoryType = factoryType;
            level = 0;
            valuePerTick = 0;
            time = 0;
            duration = 1f;
        }
        
        public void Upgrade(int level, int valuePerTick, float duration) {
            this.level = level;
            this.valuePerTick = valuePerTick;
            this.duration = duration;
        }

        public void Tick(float deltaTime, out int value) {
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

            value = ticksCompleted * valuePerTick;
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
    }
}