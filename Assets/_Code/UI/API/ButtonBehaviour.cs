using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.UI.API {
    public abstract class ButtonBehaviour : MonoBehaviour
    {
        [SerializeField] Button button;

        protected virtual void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("ButtonBehaviour attached to an object without a Button component.");
            }
            else
            {
                button.onClick.AddListener(HandleClick);
            }
        }

        protected virtual void Reset() {
            button = GetComponent<Button>();
        }

        protected virtual void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        void HandleClick()
        {
            OnClick();
        }

        protected abstract void OnClick();
    }
}