using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace Senspark {
    public class ObserverHandle : IDisposable {
        [NotNull]
        private readonly List<Action> _rollbacks;

        public ObserverHandle() {
            _rollbacks = new List<Action>();
        }

        [NotNull]
        public ObserverHandle AddObserver<T>(
            [NotNull] IObserverManager<T> manager,
            [NotNull] T observer
        ) {
            var id = manager.AddObserver(observer);
            _rollbacks.Add(() => manager.RemoveObserver(id));
            return this;
        }

        public void Dispose() {
            foreach (var item in _rollbacks) {
                item();
            }
            _rollbacks.Clear();
        }
    }
}