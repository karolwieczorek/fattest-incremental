using FattestInc.Economy.API;
using FattestInc.Economy.Implementation;
using FattestInc.Progression.API;
using Hypnagogia.Utils;
using Zenject;

namespace FattestInc {
    public class GameplayInstaller : MonoInstaller {
        public override void InstallBindings() {
            Container.BindSystems(
                typeof(ResourceFactoriesSystem),
                typeof(EconomySystem),
                typeof(SaveSystem)
                );
            Container.BindInterfacesAndSelfTo<ResourceFactoriesHelper>().AsSingle();
            Container.BindInterfacesAndSelfTo<UnlockingHelper>().AsSingle();
        }
    }
}