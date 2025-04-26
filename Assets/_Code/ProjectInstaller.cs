using FattestInc.Audio;
using FattestInc.Economy.API;
using FattestInc.Windows.General;
using Hypnagogia.Utils;
using Zenject;

namespace FattestInc {
    public class ProjectInstaller : MonoInstaller {
        public override void InstallBindings() {
            new SignalsDeclarator().DeclareSignals(Container);

            Container.BindDataStores(transform, makeGroup:false, 
                typeof(UIDataStore),
                typeof(EconomyDataStore),
                typeof(AudioSettingsDataStore),
                typeof(BuyMultipleDataStore)
                
                );
            
            Container.BindInterfacesAndSelfTo<ScenesLoaderHelper>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveHelper>().AsSingle();
            Container.BindInterfacesAndSelfTo<BuyMultipleHelper>().AsSingle();
            
            Container.BindSystems(
                typeof(AudioSystem),
                typeof(BuyMultipleInitializationSystem)
            );
            // Container.BindSystem<AudioSystem>();
        }
    }
}