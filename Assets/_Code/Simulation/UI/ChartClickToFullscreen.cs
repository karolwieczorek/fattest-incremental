using FattestInc.Simulation.Implementation;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace FattestInc.Simulation.UI {
    public sealed class ChartClickToFullscreen : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] RectTransform target; // default: this rect

        ChartFullscreenService _service;

        [Inject]
        public void Construct(ChartFullscreenService service) => _service = service;

        void Reset() => target = GetComponent<RectTransform>();
        void Awake() { if (target == null) target = GetComponent<RectTransform>(); }

        public void OnPointerClick(PointerEventData eventData)
        {
            _service.Toggle(target);
        }
    }
}