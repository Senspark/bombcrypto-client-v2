using System;
using System.Collections.Generic;

namespace Senspark {
    /// <summary>
    /// NOT thread-safe. Assumes all calls happen on a single thread (Unity main
    /// loop). Re-entrant Add/Remove during DispatchEvent is supported via the
    /// _pendingAdds / _pendingRemovals queues below; concurrent access from
    /// multiple threads is not.
    /// </summary>
    public class ObserverManager<T> : IObserverManager<T> {
        private readonly Dictionary<int, T> _observers;
        private int _counter;
        private int _dispatchDepth;
        private List<int> _pendingRemovals;
        private List<KeyValuePair<int, T>> _pendingAdds;

        public ObserverManager() {
            _observers = new Dictionary<int, T>();
            _counter = 0;
        }

        public int AddObserver(T observer) {
            var id = _counter++;
            if (_dispatchDepth > 0) {
                _pendingAdds ??= new List<KeyValuePair<int, T>>();
                _pendingAdds.Add(new KeyValuePair<int, T>(id, observer));
            } else {
                _observers.Add(id, observer);
            }
            return id;
        }

        public bool RemoveObserver(int id) {
            if (_dispatchDepth > 0) {
                _pendingRemovals ??= new List<int>();
                _pendingRemovals.Add(id);
                return _observers.ContainsKey(id) ||
                       (_pendingAdds != null && _pendingAdds.Exists(kv => kv.Key == id));
            }
            return _observers.Remove(id);
        }

        public void DispatchEvent(Action<T> dispatcher) {
            _dispatchDepth++;
            try {
                foreach (var entry in _observers) {
                    dispatcher(entry.Value);
                }
            } finally {
                _dispatchDepth--;
                if (_dispatchDepth == 0) {
                    if (_pendingAdds != null) {
                        foreach (var kv in _pendingAdds) {
                            _observers[kv.Key] = kv.Value;
                        }
                        _pendingAdds.Clear();
                    }
                    if (_pendingRemovals != null) {
                        foreach (var id in _pendingRemovals) {
                            _observers.Remove(id);
                        }
                        _pendingRemovals.Clear();
                    }
                }
            }
        }
    }
}