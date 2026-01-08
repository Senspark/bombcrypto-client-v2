using Engine.Entities;
using Engine.Manager;

namespace Engine.Strategy.Provider
{
    public interface IProvider
    {
        Entity CreateInstance(IEntityManager manager);
        void Reserve(IEntityManager manager, int capacity);
    }
}