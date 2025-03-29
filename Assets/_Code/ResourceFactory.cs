using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc {
    [System.Serializable]
    public class ResourceFactory {
        [ShowInInspector] int level;
        [ShowInInspector] float time;
        [ShowInInspector] float duration = 1f;

        [ShowInInspector] public float Progress => duration > 0 ? time / duration : 0f;

        public int Level => level;

        public ResourceFactory(int level) {
            this.level = level;
        }

        public void Upgrade(int levels) {
            level = Level + levels;
        }

        public void Tick(float deltaTime, out int value) {
            if (level <= 0) {
                value = 0;
                return;
            }
            
            time += deltaTime;
            if (Progress >= 1f) {
                time = 0;
                value = level;
            }
            else {
                value = 0;
            }
        }

        public int GetCurrentValuePerSecond() {
            if (level <= 0) {
                return 0;
            }
            Debug.Log($"Level: {level}, Duration: {duration}");
            return (int)(level / duration);
        }
    }
}