using UnityEngine;
using Zenject;

namespace FattestInc.Windows.General {
    public class WindowsInstaller : MonoInstaller {
        [SerializeField] WindowManager windowManager;

        public override void InstallBindings() {
            ProjectContext.Instance.Container.BindInstance(windowManager).AsSingle();
        }
    }
}