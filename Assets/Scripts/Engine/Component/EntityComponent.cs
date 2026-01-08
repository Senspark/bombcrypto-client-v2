using Engine.Entities;
using Engine.Manager;
using UnityEngine;

namespace Engine.Components
{
    public class EntityComponent : MonoBehaviour
    {
        public class IndexTree
        {
            public readonly int[] Indices = { -1, -1, -1 };

            public int this[int i]
            {
                get => Indices[i];
                set => Indices[i] = value;
            }
        }

        private bool cached;
        private Entity cachedEntity;

        public Entity Entity
        {
            get
            {
                if (!cached)
                {
                    cached = true;
                    cachedEntity = GetComponent<Entity>();
                }
                return cachedEntity;
            }
        }

        public IEntityManager EntityManager => Entity.EntityManager;

        public IndexTree Index { get; } = new IndexTree();
    }
}
