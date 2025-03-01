using Hypnagogia.Utils;
using Zenject;

namespace FattestInc {
    public class ProjectInstaller : MonoInstaller {
        public override void InstallBindings() {
            new SignalsDeclarator().DeclareSignals(Container);

            Container.BindDataStores(transform, makeGroup:false, 
                typeof(EconomyDataStore)
                );
            Container.BindInterfacesAndSelfTo<ScenesLoaderHelper>().AsSingle();
        }
    }
}