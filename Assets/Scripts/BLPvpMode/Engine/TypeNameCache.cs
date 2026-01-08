using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace BLPvpMode.Engine {
    public class TypeNameCache {
        private readonly Dictionary<Type, string> _cache;

        public TypeNameCache() {
            _cache = new Dictionary<Type, string>();
        }

        [NotNull]
        public string GetName<T>() {
            return GetName(typeof(T));
        }

        [NotNull]
        public string GetName([NotNull] Type type) {
            if (_cache.TryGetValue(type, out var result)) {
                return result;
            }
            var name = type.Name;
            _cache.Add(type, name);
            return name;
        }
    }
}