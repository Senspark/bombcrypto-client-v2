using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace BLPvpMode.Engine {
    public class TypeListCache {
        private readonly TypeNameCache _nameCache;
        private readonly Dictionary<string, ListCache> _caches;

        public TypeListCache() {
            _nameCache = new TypeNameCache();
            _caches = new Dictionary<string, ListCache>();
        }

        [NotNull]
        public List<T> GetItems<T>() {
            var cache = GetCache(typeof(T));
            return (List<T>) cache.GetItems();
        }

        public void AddItem<T>(T item) {
            var cache = GetCache(typeof(T));
            cache.AddItem(item);
        }

        public void AddItem(Type type, object item) {
            var cache = GetCache(type);
            cache.AddItem(item);
        }

        public void RemoveItem<T>(T item) {
            var cache = GetCache(typeof(T));
            cache.RemoveItem(item);
        }

        public void RemoveItem(Type type, object item) {
            var cache = GetCache(type);
            cache.RemoveItem(item);
        }

        [NotNull]
        private ListCache GetCache([NotNull] Type type) {
            var key = _nameCache.GetName(type);
            if (_caches.TryGetValue(key, out var result)) {
                return result;
            }
            var cache = new ListCache(type);
            _caches.Add(key, cache);
            return cache;
        }
    }
}