using UnityEngine;
using Zenject;

namespace FattestInc.Simulation.Implementation {
    public sealed class ChartFullscreenInstaller : MonoInstaller
    {
        [Header("Overlay refs")]
        [SerializeField] GameObject overlayRoot;   // ChartFullscreenOverlay (disabled by default)
        [SerializeField] RectTransform host;       // ChartFullscreenOverlay/Host (stretched)

        public override void InstallBindings()
        {
            // Bind the service as a single instance
            Container.BindInterfacesAndSelfTo<ChartFullscreenService>()
                .AsSingle()
                .WithArguments(overlayRoot, host);

            // If your click-close component is on the overlay, Zenject will inject it automatically
            // as long as it’s under the same context. Otherwise:
            // Container.QueueForInject(overlayRoot);
        }
    }
}