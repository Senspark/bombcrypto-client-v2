using Engine.Entities;

namespace Engine.Strategy.Provider {
    public interface IProviderMap {
        Entity TryCreateEntityLocation(EntityType type, out EntityLocation entityLocation);
        Entity CreateEntity(EntityType type);
    }
}