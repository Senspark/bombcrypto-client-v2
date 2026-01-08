using System;
using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

using UnityEngine.Assertions;

namespace BLPvpMode.Engine {
    public class ListCache {
        private readonly Type _type;
        private readonly IList _items;
        private readonly Dictionary<object, int> _indices;

        public ListCache(Type type) {
            _type = type;
            _items = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
            _indices = new Dictionary<object, int>();
        }

        [NotNull]
        public IList GetItems() {
            return _items;
        }

        public void AddItem([NotNull] object item) {
            // Testing.
            Assert.IsNotNull(item);
            Assert.IsTrue(item.GetType() == _type);
            Assert.IsTrue(!_indices.ContainsKey(item));

            // Add item.
            var index = _items.Count;
            _indices[item] = index;
            _items.Add(item);
        }

        public void RemoveItem([NotNull] object item) {
            // Testing.
            Assert.IsNotNull(item);
            Assert.IsTrue(_indices.ContainsKey(item));

            var index = _indices[item];
            Assert.IsTrue(_items[index] == item);

            // Remove item.
            _indices.Remove(item);
            var size = _items.Count;
            if (index < size) {
                _items[index] = _items[size - 1];
                _indices[_items[index]] = index;
            }
            _items.RemoveAt(size - 1);
        }
    }
}