using UnityEngine;
using Zenject;

namespace FattestInc.Simulation.Implementation {
    public sealed class ChartFullscreenService : ITickable
    {
        readonly GameObject _overlayRoot;
        readonly RectTransform _host;

        RectTransform _current;
        Transform _originalParent;
        int _originalSiblingIndex;
        Vector2 _originalAnchorMin, _originalAnchorMax, _originalPivot, _originalSizeDelta, _originalAnchoredPos;

        public ChartFullscreenService(GameObject overlayRoot, RectTransform host)
        {
            _overlayRoot = overlayRoot;
            _host = host;

            if (_overlayRoot != null)
                _overlayRoot.SetActive(false);
        }

        public bool IsFullscreen(RectTransform target) => _current == target;

        public void Toggle(RectTransform target)
        {
            if (target == null) return;

            if (_current == target) ExitFullscreen();
            else EnterFullscreen(target);
        }

        public void EnterFullscreen(RectTransform target)
        {
            if (_overlayRoot == null || _host == null)
            {
                Debug.LogError("ChartFullscreenService: overlayRoot/host not provided.");
                return;
            }

            if (_current != null)
                ExitFullscreen();

            _current = target;

            // Save original layout
            _originalParent = target.parent;
            _originalSiblingIndex = target.GetSiblingIndex();
            _originalAnchorMin = target.anchorMin;
            _originalAnchorMax = target.anchorMax;
            _originalPivot = target.pivot;
            _originalSizeDelta = target.sizeDelta;
            _originalAnchoredPos = target.anchoredPosition;

            _overlayRoot.SetActive(true);

            // Reparent & stretch
            target.SetParent(_host, worldPositionStays: false);
            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.one;
            target.pivot = new Vector2(0.5f, 0.5f);
            target.anchoredPosition = Vector2.zero;
            target.sizeDelta = Vector2.zero;
        }

        public void ExitFullscreen()
        {
            if (_current == null) return;

            _current.SetParent(_originalParent, worldPositionStays: false);
            _current.SetSiblingIndex(_originalSiblingIndex);

            _current.anchorMin = _originalAnchorMin;
            _current.anchorMax = _originalAnchorMax;
            _current.pivot = _originalPivot;
            _current.sizeDelta = _originalSizeDelta;
            _current.anchoredPosition = _originalAnchoredPos;

            _current = null;

            if (_overlayRoot != null)
                _overlayRoot.SetActive(false);
        }

        // Esc to close
        public void Tick()
        {
            if (_current != null && Input.GetKeyDown(KeyCode.Escape))
                ExitFullscreen();
        }
    }
}
