using UnityEngine;

namespace FattestInc.Windows.General
{
    public abstract class WindowBase : MonoBehaviour, IWindowProtectedApi
    {
        public virtual bool IsVisible => gameObject.activeSelf;
        
        protected virtual void BeforeShowInitialize()
        {
            
        }
        
        void IWindowProtectedApi.WindowApiShow() {
            ProtectedShow();
        }

        protected void ProtectedShow() {
            BeforeShowInitialize();
            gameObject.SetActive(true);
        }

        void IWindowProtectedApi.WindowApiHide() {
            ProtectedHide();
        }

        protected void ProtectedHide() {
            gameObject.SetActive(false);
        }
    }

    public class WindowParameters
    {
        
    }

    public interface ISimpleWindowOpen : IWindowProtectedApi
    {
        void Open()
        {
            WindowApiShow();
        }
    }

    public interface ISimpleWindowClose : IWindowProtectedApi
    {
        void Close()
        {
            WindowApiHide();
        }
    }

    public interface IWindowProtectedApi
    {
        void WindowApiShow();
        void WindowApiHide();
    }

    public abstract class SimpleWindow : WindowBase, ISimpleWindowOpen, ISimpleWindowClose {
        
    }
}