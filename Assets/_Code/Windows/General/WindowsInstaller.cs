using System;
using UnityEngine;
using Zenject;

namespace FattestInc.Windows.General {
    public class WindowsInstaller : MonoInstaller {
        [SerializeField] WindowManager windowManager;

        public override void InstallBindings() {
            // ProjectContext.Instance.Container.BindInstance(windowManager).AsSingle();
            Container.BindInstance(windowManager).AsSingle();
        }

        // void OnDestroy() {
        //     ProjectContext.Instance.Container.Unbind(windowManager.GetType());
        // }
    }
}