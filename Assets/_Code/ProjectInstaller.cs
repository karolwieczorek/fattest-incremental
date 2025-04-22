using FattestInc.Windows.General;
using Hypnagogia.Utils;
using Zenject;

namespace FattestInc {
    public class ProjectInstaller : MonoInstaller {
        public override void InstallBindings() {
            new SignalsDeclarator().DeclareSignals(Container);

            Container.BindDataStores(transform, makeGroup:false, 
                typeof(UIDataStore),
                typeof(EconomyDataStore)
                );
            Container.BindInterfacesAndSelfTo<ScenesLoaderHelper>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveHelper>().AsSingle();
        }
    }
}