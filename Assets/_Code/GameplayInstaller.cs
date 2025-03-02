using Hypnagogia.Utils;
using Zenject;

namespace FattestInc {
    public class GameplayInstaller : MonoInstaller {
        public override void InstallBindings() {
            Container.BindSystems(
                typeof(ResourceFactoriesSystem)
                );
        }
    }
}