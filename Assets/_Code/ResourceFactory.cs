using Sirenix.OdinInspector;

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
            this.level = 0;
            this.valuePerTick = 0;
            this.time = 0;
            this.duration = 1f;
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
            if (Progress >= 1f) {
                time = 0;
                value = valuePerTick;
            }
            else {
                value = 0;
            }
        }

        public float NextUpgradePerSecondAmount() {
            return GetValuePerSecond(level + 1);
        }

        public float GetNextLevelDifferencePerSecond() {
            return NextUpgradePerSecondAmount() - GetCurrentValuePerSecond();
        }

        public float GetCurrentValuePerSecond() {
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