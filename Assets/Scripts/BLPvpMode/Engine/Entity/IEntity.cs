using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Entity {
    public interface IEntity {
        [CanBeNull]
        IEntityManager EntityManager { get; }

        bool IsAlive { get; }

        [CanBeNull]
        T GetComponent<T>() where T : IEntityComponent;

        void Kill();
        void Begin([NotNull] IEntityManager entityManager);
        void Update(int delta);
        void End();
    }
}