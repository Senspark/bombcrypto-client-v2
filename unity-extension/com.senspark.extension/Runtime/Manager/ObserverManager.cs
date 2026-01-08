using System;
using System.Collections.Generic;

namespace Senspark {
    public class ObserverManager<T> : IObserverManager<T> {
        private readonly Dictionary<int, T> _observers;
        private int _counter;

        public ObserverManager() {
            _observers = new Dictionary<int, T>();
            _counter = 0;
        }

        public int AddObserver(T observer) {
            var id = _counter++;
            _observers.Add(id, observer);
            return id;
        }

        public bool RemoveObserver(int id) {
            return _observers.Remove(id);
        }

        public void DispatchEvent(Action<T> dispatcher) {
            foreach (var entry in _observers) {
                dispatcher(entry.Value);
            }
        }
    }
}