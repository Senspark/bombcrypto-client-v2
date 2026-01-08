using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Engine.Utils;

using System;

using Engine.Entities;
using Engine.Collision;
using Engine.Activity;

using UnityEngine.Assertions;

using Engine.Components;

using Utils;

namespace Engine.Manager {
    public class DefaultEntityManager : IEntityManager {
        private PhysicsScene2D _physics;
        private readonly TypeCache typeCache = new TypeCache();
        private readonly Dictionary<string, IList> cacheComponents = new Dictionary<string, IList>();

        private readonly Action<Entity, Entity, Vector2> collisionBegin;
        private readonly Action<Entity, Entity> collisionEnd;

        private readonly List<ICollisionListener> _listeners = new List<ICollisionListener>();
        private readonly List<IActivity> _activities = new List<IActivity>();

        public ILevelManager LevelManager { get; set; }
        public IMapManager MapManager { get; set; }
        public IPlayerManager PlayerManager { get; set; }
        public IEnemyManager EnemyManager { get; set; }
        public IStatsManager StatsManager { get; set; }
        public IExplodeEventManager ExplodeEventManager { get; set; }
        public ITakeDamageEventManager TakeDamageEventManager { get; set; }
        public IPoolManager PoolManager { get; }
        public GameObject View { get; }

        public List<Entity> Entities { get; }

        private readonly List<(Entity Entity, bool Trigger)> _toBeDestroyEntities = new List<(Entity, bool)>();
        private readonly List<(Entity Entity, bool Trigger)> _pendingEntities = new List<(Entity, bool)>();

        private readonly bool _cache;
        private bool _destroying;
        private float _timer;

        private int _layer;

        public DefaultEntityManager(GameObject view,
            PhysicsScene2D physics,
            bool cache, int layer = 0) {
            PoolManager = new DefaultPoolManager();
            View = view;
            _physics = physics;

            _cache = cache;
            _layer = layer;

            _destroying = false;
            Entities = new List<Entity>();

            collisionBegin = (entity, otherEntity, position) => {
                for (var i = _listeners.Count - 1; i >= 0; --i) {
                    _listeners[i].OnCollisionEntered(entity, otherEntity, position, this);
                }
            };
            collisionEnd = (entity, otherEntity) => {
                for (var i = _listeners.Count - 1; i >= 0; --i) {
                    _listeners[i].OnCollisionExited(entity, otherEntity, this);
                }
            };
        }

        public bool AddEntity(Entity entity) {
            if (_layer > 0) {
                entity.gameObject.SetLayer(_layer);
            }

            var index = entity.Index[0];
            if (index != -1) {
                Assert.IsTrue(false);
                return false;
            }

            // Assign entity manager.
            Assert.IsTrue(entity.EntityManager == null);
            entity.EntityManager = this;

            // Cache
            if (_cache) {
                AddEntityToCache(entity);
            }

            // Assign Collision Listener
            var detector = entity.GetComponent<CollisionDetector>();
            if (detector != null) {
                detector.SetTriggerEntered(collisionBegin);
                detector.SetTriggerExited(collisionEnd);
                detector.SetCollisionEntered(collisionBegin);
                detector.SetCollisionExited(collisionEnd);
            }

            var updater = entity.GetEntityComponent<Updater>();
            updater?.Begin();

            entity.Index[0] = Entities.Count;
            Entities.Add(entity);
            return true;
        }

        public bool RemoveEntity(Entity entity) {
            var index = entity.Index[0];
            if (Entities[index] != entity) {
                Assert.IsTrue(false);
                return false;
            }

            entity.Index[0] = -1;
            if (index < Entities.Count - 1) {
                Entities[index] = Entities[Entities.Count - 1];
                Entities[index].Index[0] = index;
            }
            Entities.RemoveAt(Entities.Count - 1);

            var updater = entity.GetEntityComponent<Updater>();
            if (updater != null) {
                updater.End();
            }

            var detector = entity.GetComponent<CollisionDetector>();
            if (detector != null) {
                detector.SetTriggerEntered(null);
                detector.SetTriggerExited(null);
                detector.SetCollisionEntered(null);
                detector.SetCollisionExited(null);
            }

            if (_cache) {
                RemoveEntityFromCache(entity);
            }

            // Unassign entity manager.
            Assert.IsTrue(entity.EntityManager == this);
            entity.EntityManager = null;
            return true;
        }

        private void AddEntityToCache(Entity entity) {
            AddEntityComponentToCache<Updater>(entity);

            var tree = typeCache.GetEntityTree(entity.GetType());
            var treeSize = tree.Count;
            for (var i = 0; i < treeSize; ++i) {
                var type = tree[i];
                var key = typeCache.GetName(type);
                if (!cacheComponents.ContainsKey(key)) {
                    cacheComponents.Add(key, Activator.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList);
                }
                var cached = cacheComponents[key];
                Assert.IsTrue(entity.Index[i + 1] == -1);
                entity.Index[i + 1] = cached.Count;
                cached.Add(entity);
            }
        }

        private void AddEntityComponentToCache<T>(Entity entity) where T : IEntityComponent {
            var component = entity.GetEntityComponent<T>();
            if (component != null) {
                AddEntityComponentToCache(component);
            }
        }

        private void AddEntityComponentToCache(IEntityComponent component) {
            var type = component.GetType();
            var key = typeCache.GetName(type);
            if (!cacheComponents.ContainsKey(key)) {
                cacheComponents.Add(key, Activator.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList);
            }
            var cached = cacheComponents[key];
            Assert.IsTrue(component.Index[0] == -1);
            component.Index[0] = cached.Count;
            cached.Add(component);
        }

        private void RemoveEntityFromCache(Entity entity) {
            RemoveEntityComponentFromCache<Updater>(entity);

            var tree = typeCache.GetEntityTree(entity.GetType());
            var treeSize = tree.Count;
            for (var i = 0; i < treeSize; ++i) {
                var type = tree[i];
                var key = typeCache.GetName(type);
                Assert.IsTrue(cacheComponents.ContainsKey(key));
                var cached = cacheComponents[key];
                var index = entity.Index[i + 1];
                Assert.IsTrue((Entity) cached[index] == entity);
                entity.Index[i + 1] = -1;
                if (index < cached.Count - 1) {
                    cached[index] = cached[cached.Count - 1];
                    ((Entity) cached[index]).Index[i + 1] = index;
                }
                cached.RemoveAt(cached.Count - 1);
            }
        }

        private void RemoveEntityComponentFromCache<T>(Entity entity) where T : IEntityComponent {
            var component = entity.GetEntityComponent<T>();
            if (component != null) {
                RemoveEntityComponentFromCache(component);
            }
        }

        private void RemoveEntityComponentFromCache(IEntityComponent component) {
            var key = typeCache.GetName(component.GetType());
            Assert.IsTrue(cacheComponents.ContainsKey(key));
            var cached = cacheComponents[key];
            var index = component.Index[0];
            Assert.IsTrue((IEntityComponent) cached[index] == component);
            component.Index[0] = -1;
            if (index < cached.Count - 1) {
                cached[index] = cached[cached.Count - 1];
                ((IEntityComponent) cached[index]).Index[0] = index;
            }
            cached.RemoveAt(cached.Count - 1);
        }

        private List<T> FindFromCache<T>() {
            var key = typeCache.GetName(typeof(T));
            if (cacheComponents.ContainsKey(key)) {
                return cacheComponents[key] as List<T>;
            }
            return new List<T>();
        }

        private List<T> FindNonCache<T>() {
            var components = new List<T>();
            foreach (var item in Entities) {
                components.AddRange(item.GetComponents<T>());
            }
            return components;
        }

        private List<T> Find<T>() {
            if (_cache) {
                var results = FindFromCache<T>();
                return results;
            }
            return FindNonCache<T>();
        }

        public List<T> FindEntities<T>() where T : Entity {
            return Find<T>();
        }

        public List<T> FindComponents<T>() where T : IEntityComponent {
            return Find<T>();
        }

        public void AddListener(ICollisionListener listener) {
            _listeners.Add(listener);
        }

        public void AddActivity(IActivity activity) {
            _activities.Add(activity);
        }

        public void Step(float delta) {
            ExplodeEventManager.UpdateProcess(delta);
            TakeDamageEventManager.UpdateProcess();
            LevelManager.UpdateProcess();
            MapManager.UpdateProcess();

            // Fix: pvp không gọi physic
            if (!LevelManager.IsPvpMode) {
                PhysicSimulate(delta);
            }

            for (var i = _activities.Count - 1; i >= 0; --i) {
                var activity = _activities[i];
                activity.ProcessUpdate(delta);
            }
        }

        private void PhysicSimulate(float delta) {
            _timer += delta;
            while (_timer >= Time.fixedDeltaTime) {
                _timer -= Time.fixedDeltaTime;
                _physics.Simulate(Time.fixedDeltaTime);
            }
        }

        public void MarkDestroy(Entity entity, bool trigger) {
            if (_destroying) {
                _pendingEntities.Add((entity, trigger));
            } else {
                _toBeDestroyEntities.Add((entity, trigger));
            }
        }

        public void ProcessDestroy() {
            if (_pendingEntities.Count > 0) {
                _toBeDestroyEntities.AddRange(_pendingEntities);
                _pendingEntities.Clear();
            }
            if (_toBeDestroyEntities.Count == 0) {
                return;
            }
            _destroying = true;
            for (var i = _toBeDestroyEntities.Count - 1; i >= 0; --i) {
                var item = _toBeDestroyEntities[i];
                var entity = item.Entity;
                Assert.IsFalse(entity.IsAlive);

                var trigger = item.Trigger;
                if (trigger) {
                    var triggers = entity.GetComponents<KillTrigger>();
                    for (var j = triggers.Length - 1; j >= 0; --j) {
                        triggers[j].Trigger();
                    }
                }

                RemoveEntity(entity);

                var poolable = entity.GetComponent<Poolable>();
                if (poolable == null) {
                    UnityEngine.Object.Destroy(entity.gameObject);
                } else {
                    PoolManager.Destroy(entity);
                }
            }
            _toBeDestroyEntities.Clear();
            _destroying = false;
        }
    }
}