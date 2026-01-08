using System.Collections.Generic;

using Engine.Activity;
using Engine.Collision;
using Engine.Components;
using Engine.Entities;

using UnityEngine;

namespace Engine.Manager {
    public interface IEntityManager {
        ILevelManager LevelManager { get; }
        IMapManager MapManager { get; }
        IPlayerManager PlayerManager { get; }
        IEnemyManager EnemyManager { get; }
        IStatsManager StatsManager { get; }
        IExplodeEventManager ExplodeEventManager { get; set; }
        ITakeDamageEventManager TakeDamageEventManager { get; }
        IPoolManager PoolManager { get; }

        GameObject View { get; }

        List<Entity> Entities { get; }
        bool AddEntity(Entity entity);
        bool RemoveEntity(Entity entity);

        void MarkDestroy(Entity entity, bool trigger);
        void ProcessDestroy();

        List<T> FindEntities<T>() where T : Entity;
        List<T> FindComponents<T>() where T : IEntityComponent;

        void AddListener(ICollisionListener listener);
        void AddActivity(IActivity activity);

        void Step(float delta);
    }
}