using FattestInc.Economy.Implementation;
using Hypnagogia.Utils;
using Zenject;

namespace FattestInc {
    public class BootstrapperInstaller : MonoInstaller {
        public override void InstallBindings() {
            Container.BindSystems(
                typeof(ResourceFactoriesInitializationSystem)
            );
        }
    }
}