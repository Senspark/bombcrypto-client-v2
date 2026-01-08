using Engine.Entities;

namespace Engine.Manager
{
    public interface IPoolManager
    {
        /// <summary>
        /// Instantiate the specified prefab.
        /// </summary>
        /// <returns>The instantiate.</returns>
        /// <param name="prefab">Prefab.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        T Instantiate<T>(T prefab) where T : Entity;

        /// <summary>
        /// Destroy the specified instance.
        /// </summary>
        /// <param name="instance">Instance.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        void Destroy<T>(T instance) where T : Entity;

        /// <summary>
        /// Reserve the specified prefab
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="capacity"></param>
        void Reserve<T>(T prefab, int capacity) where T : Entity;

    }
}