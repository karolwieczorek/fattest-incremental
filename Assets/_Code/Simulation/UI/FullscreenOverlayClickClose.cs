using FattestInc.Simulation.Implementation;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace FattestInc.Simulation.UI {
    public sealed class FullscreenOverlayClickClose : MonoBehaviour, IPointerClickHandler
    {
        ChartFullscreenService _service;

        [Inject]
        public void Construct(ChartFullscreenService service) => _service = service;

        public void OnPointerClick(PointerEventData eventData)
        {
            _service.ExitFullscreen();
        }
    }
}