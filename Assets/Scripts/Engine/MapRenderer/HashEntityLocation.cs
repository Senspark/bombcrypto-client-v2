using System.Collections.Generic;

using Engine.Entities;

using UnityEngine;

namespace Engine.MapRenderer {
    public interface IHashEntityLocation {
        void AddHashLocation(EntityLocation entity, Vector2Int location);
        IList<EntityLocation> FindEntitiesAtLocation(Vector2Int location);
        void UpdateProcess();
    }

    public class HashEntityLocation : IHashEntityLocation {
        private static int GetHashCodeLocation(Vector2Int location) {
            return location.x % 16 << 16 | location.y % 16;
        }

        private readonly IDictionary<int, IList<EntityLocation>> _hashEntityLocation;

        public HashEntityLocation() {
            _hashEntityLocation = new Dictionary<int, IList<EntityLocation>>();
        }

        public void AddHashLocation(EntityLocation entity, Vector2Int location) {
            entity.HashLocation = GetHashCodeLocation(location);
            if (!_hashEntityLocation.ContainsKey(entity.HashLocation)) {
                _hashEntityLocation[entity.HashLocation] = new List<EntityLocation>();
            }
            _hashEntityLocation[entity.HashLocation].Add(entity);
        }

        public void UpdateProcess() {
            foreach (var entityList in _hashEntityLocation.Values) {
                for (var idx = entityList.Count - 1; idx >= 0; idx--) {
                    if (!entityList[idx].IsAlive) {
                        entityList.RemoveAt(idx);
                    }
                }
            }
        }

        public IList<EntityLocation> FindEntitiesAtLocation(Vector2Int location) {
            var hash = GetHashCodeLocation(location);
            return _hashEntityLocation.ContainsKey(hash) ? _hashEntityLocation[hash] : null;
        }
    }
}