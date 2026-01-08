using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace BLPvpMode.Engine {
    public class TypeTreeCache<T> {
        private readonly Dictionary<Type, List<Type>> _cache;

        public TypeTreeCache() {
            _cache = new Dictionary<Type, List<Type>>();
        }

        [NotNull]
        public List<Type> GetTree([NotNull] Type type) {
            if (_cache.TryGetValue(type, out var result)) {
                return result;
            }
            var tree = new List<Type>();
            var currentType = type;
            while (currentType != null && currentType != typeof(T)) {
                tree.Add(currentType);
                currentType = currentType.BaseType;
            }
            tree.Reverse();
            _cache.Add(type, tree);
            return tree;
        }
    }
}