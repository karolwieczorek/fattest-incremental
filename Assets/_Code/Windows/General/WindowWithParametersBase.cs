namespace FattestInc.Windows.General
{
    public abstract class WindowWithParametersBase<T> : WindowBase, ISimpleWindowOpen, ISimpleWindowClose, IWindowParametersInitialization<T> where T : WindowParameters
    {
        protected abstract void Initialize(T parametersContainer);
        
        void IWindowParametersInitialization<T>.InitializeWithParameters(T parametersContainer)
        {
            Initialize(parametersContainer);
        }
    }

    public interface IWindowParametersInitialization<in T> where T : WindowParameters
    {
        void InitializeWithParameters(T parametersContainer);
    }
}