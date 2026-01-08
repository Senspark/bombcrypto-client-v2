using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark {
    [Service(typeof(IRemoteConfigManager))]
    public interface IRemoteConfigManager {
        /// <summary>
        /// Initializes the service.
        /// </summary>
        [NotNull]
        Task Initialize(float timeOut);
        
        Task<bool> ForceFetch();

        /// <summary>
        /// Gets a bool value.
        /// </summary>
        bool GetBool([NotNull] string key);

        /// <summary>
        /// Gets a long value.
        /// </summary>
        long GetLong([NotNull] string key);

        /// <summary>
        /// Gets a double value.
        /// </summary>
        double GetDouble([NotNull] string key);

        /// <summary>
        /// Gets a string value.
        /// </summary>
        [NotNull]
        string GetString([NotNull] string key);
    }
}