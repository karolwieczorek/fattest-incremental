namespace FattestInc {
    public class ResourceFactory {
        int level;
        float time;
        float duration = 1f;

        public int Level => level;

        public ResourceFactory(int level) {
            this.level = level;
        }

        public void Upgrade(int levels) {
            level = Level + levels;
        }

        public void Tick(float deltaTime, out int value) {
            time += deltaTime;
            if (time >= duration) {
                time = 0;
                value = level;
            }

            value = 0;
        }
    }
}