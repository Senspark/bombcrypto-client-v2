using System.Collections.Generic;

namespace Utils {
    public class Map<T1, T2> {
        private readonly Dictionary<T1, T2> _forward = new();
        private readonly Dictionary<T2, T1> _reverse = new();

        public Map() {
            Forward = new Indexer<T1, T2>(_forward);
            Reverse = new Indexer<T2, T1>(_reverse);
        }

        public Map(IDictionary<T1, T2> dictionary) {
            foreach (var (k, v) in dictionary) {
                _forward[k] = v;
                _reverse[v] = k;
            }
            Forward = new Indexer<T1, T2>(_forward);
            Reverse = new Indexer<T2, T1>(_reverse);
        }

        public class Indexer<T3, T4> {
            private readonly Dictionary<T3, T4> _dictionary;

            public Indexer(Dictionary<T3, T4> dictionary) {
                _dictionary = dictionary;
            }

            public T4 this[T3 index] {
                get => _dictionary[index];
                set => _dictionary[index] = value;
            }
        }

        public void Add(T1 t1, T2 t2) {
            _forward.Add(t1, t2);
            _reverse.Add(t2, t1);
        }

        public Indexer<T1, T2> Forward { get; private set; }
        public Indexer<T2, T1> Reverse { get; private set; }
    }
}