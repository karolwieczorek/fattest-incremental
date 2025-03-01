using System;
using JetBrains.Annotations;
using Zenject;

namespace Hypnagogia.Utils {
    [PublicAPI]
    public abstract class HSystem : IInitializable, IDisposable {
        protected virtual void SystemStart() { }
        protected virtual void SystemStop() { }
        
        public void Initialize() {
            SystemStart();
        }

        public void Dispose() {
            SystemStop();
        }
    }
}