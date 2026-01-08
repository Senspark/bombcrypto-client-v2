using System.Collections.Generic;

using BLPvpMode.Engine.Entity;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public interface IEntityManager {
        [NotNull]
        List<IEntity> Entities { get; }

        [NotNull]
        T GetManager<T>() where T : IManager;

        void AddManager<T>([NotNull] T manager) where T : IManager;

        [NotNull]
        List<T> FindEntities<T>() where T : IEntity;

        [NotNull]
        List<T> FindComponents<T>() where T : IEntityComponent;

        void AddEntity([NotNull] IEntity entity);
        void RemoveEntity([NotNull] IEntity entity);

        void AddActivity([NotNull] IActivity activity);

        void ProcessUpdate(float delta);
    }
}