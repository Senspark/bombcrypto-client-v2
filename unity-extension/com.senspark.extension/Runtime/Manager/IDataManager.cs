using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark {
    [Service(typeof(IDataManager))]
    public interface IDataManager {
        /// <summary>
        /// Initializes this manager.
        /// </summary>
        Task Initialize();

        /// <summary>
        /// Gets a value by key.
        /// </summary>
        /// <param name="key">The desired key</param>
        /// <param name="defaultValue">The default value returned if the key doesn't exist</param>
        /// <typeparam name="T">The type of value</typeparam>
        [NotNull]
        T Get<T>([NotNull] string key, [NotNull] T defaultValue);

        /// <summary>
        /// Sets value by key.
        /// </summary>
        /// <param name="key">The desired key</param>
        /// <param name="value">The desired value</param>
        /// <typeparam name="T">The type of value</typeparam>
        void Set<T>([NotNull] string key, [NotNull] T value);
    }
}