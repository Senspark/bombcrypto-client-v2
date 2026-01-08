using Engine.Components;
using Engine.Entities;

namespace Engine.Components {
    public class EntityComponentV2 : IEntityComponent {
        public IndexTree Index { get; } = new IndexTree();
    }
}