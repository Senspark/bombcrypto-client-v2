using System;
using System.Collections.Generic;

using BLPvpMode.Engine.Entity;

using JetBrains.Annotations;

using UnityEngine.Assertions;

namespace BLPvpMode.Engine.Manager {
    public class DefaultEntityManager : IEntityManager {
        private readonly Dictionary<string, IManager> _managers;
        private readonly ManagerNameCache _nameCache;
        private readonly TypeTreeCache<IEntity> _treeCache;
        private readonly TypeListCache _listCache;
        private readonly float _maxTimeStep;
        private readonly float _minTimeStep;
        private readonly Queue<IEntity> _toBeRemovedEntities;
        private readonly List<IActivity> _activities;
        private int _entityLocker;
        private float _accumulatedTime;

        public List<IEntity> Entities => _listCache.GetItems<IEntity>();

        public DefaultEntityManager(
            float maxTimeStep,
            float minTimeStep) {
            _managers = new Dictionary<string, IManager>();
            _nameCache = new ManagerNameCache();
            _treeCache = new TypeTreeCache<IEntity>();
            _listCache = new TypeListCache();
            _maxTimeStep = maxTimeStep;
            _minTimeStep = minTimeStep;
            _toBeRemovedEntities = new Queue<IEntity>();
            _activities = new List<IActivity>();
            _entityLocker = 0;
            _accumulatedTime = 0;
        }

        public T GetManager<T>() where T : IManager {
            return Resolve<T>();
        }

        public void AddManager<T>(T manager) where T : IManager {
            Provide(manager);
        }

        private void Provide(IManager manager) {
            var type = manager.GetType();
            var name = _nameCache.GetName(type);
            Assert.IsTrue(!_managers.ContainsKey(name));
            _managers.Add(name, manager);
        }

        private T Resolve<T>() where T : IManager {
            var name = _nameCache.GetName<T>();
            if (_managers.TryGetValue(name, out var item)) {
                if (item is T service) {
                    return service;
                }
            }
            throw new Exception($"Cannot find the requested manager: {name}");
        }

        public List<T> FindEntities<T>() where T : IEntity {
            return _listCache.GetItems<T>();
        }

        public List<T> FindComponents<T>() where T : IEntityComponent {
            return _listCache.GetItems<T>();
        }

        [Button]
        public void AddEntity(IEntity entity) {
            Assert.IsTrue(entity.IsAlive);
            AddEntityInstantly(entity);
        }

        [Button]
        public void RemoveEntity(IEntity entity) {
            _toBeRemovedEntities.Enqueue(entity);
            ProcessEntities();
        }

        private void AddEntityInstantly([NotNull] IEntity entity) {
            // Assign entity manager.
            Assert.IsTrue(entity.EntityManager == this);
            
            // FIXME.
            // entity.EntityManager = this;

            // Add to cache.
            AddEntityComponentToCache<UpdateComponent>(entity);
            var tree = _treeCache.GetTree(entity.GetType());
            foreach (var type in tree) {
                _listCache.AddItem(type, entity);
            }

            // Initialize components.
            var updater = entity.GetComponent<UpdateComponent>();
            updater?.Begin();
        }

        private void AddEntityComponentToCache<T>([NotNull] IEntity entity) where T : IEntityComponent {
            var component = entity.GetComponent<T>();
            if (component == null) {
                return;
            }
            _listCache.AddItem(component);
        }

        private void RemoveEntityInstantly([NotNull] IEntity entity) {
            // Un-initialize components.
            var updater = entity.GetComponent<UpdateComponent>();
            updater?.End();

            // Remove from cache.
            RemoveEntityComponentFromCache<UpdateComponent>(entity);
            var tree = _treeCache.GetTree(entity.GetType());
            foreach (var type in tree) {
                _listCache.RemoveItem(type, entity);
            }

            // Un-assign entity manager.
            Assert.IsTrue(entity.EntityManager == this);
            
            // FIXME.
            // entity.EntityManager = null;
        }

        private void RemoveEntityComponentFromCache<T>([NotNull] IEntity entity) where T : IEntityComponent {
            var component = entity.GetComponent<T>();
            if (component == null) {
                return;
            }
            _listCache.RemoveItem(component);
        }

        private void ProcessEntities() {
            while (true) {
                if (_toBeRemovedEntities.Count == 0) {
                    return;
                }
                if (_entityLocker > 0) {
                    return;
                }
                var entity = _toBeRemovedEntities.Dequeue();
                ++_entityLocker;
                Assert.IsFalse(entity.IsAlive);
                RemoveEntityInstantly(entity);
                --_entityLocker;
            }
        }

        public void AddActivity(IActivity activity) {
            _activities.Add(activity);
        }

        public void ProcessUpdate(float delta) {
            _accumulatedTime += delta;
            while (_accumulatedTime >= _maxTimeStep) {
                _accumulatedTime -= _maxTimeStep;
                ProcessUpdateInternal(_maxTimeStep);
            }
            if (_accumulatedTime >= _minTimeStep) {
                ProcessUpdateInternal(_accumulatedTime);
                _accumulatedTime = 0;
            }
        }

        private void ProcessUpdateInternal(float delta) {
            foreach (var activity in _activities) {
                ++_entityLocker;
                activity.ProcessUpdate(delta);
                --_entityLocker;
                ProcessEntities();
            }
        }
    }
}