namespace Engine.Utils
{
    public interface IObjectPool<T>
    {
        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        T Instantiate();

        /// <summary>
        /// Destroys an active instance.
        /// </summary>
        void Destroy(T instance);

        void Reserve(int capacity);
    }
}