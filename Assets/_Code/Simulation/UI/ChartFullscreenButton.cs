using FattestInc.Simulation.Implementation;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FattestInc.Simulation.UI {
    public sealed class ChartFullscreenButton : MonoBehaviour
    {
        [SerializeField] private RectTransform target; // chart root to fullscreen
        [SerializeField] private Button button;

        [Inject] ChartFullscreenService _service;

        void Reset()
        {
            button = GetComponent<Button>();
            // If you put the button as a child of the chart root, this is a good default:
            target = transform.parent as RectTransform;
        }

        void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (target == null)
            {
                // fallback: nearest RectTransform up the hierarchy
                target = GetComponentInParent<RectTransform>();
            }

            button.onClick.AddListener(OnClick);
        }

        void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }

        void OnClick()
        {
            if (_service == null || target == null) return;
            _service.Toggle(target);
        }
    }
}