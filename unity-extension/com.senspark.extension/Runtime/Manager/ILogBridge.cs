using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public interface ILogBridge {
        /// <summary>
        /// Initializes this bridge.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        Task<bool> Initialize();

        /// <summary>
        /// Logs a message.
        /// </summary>
        void Log([NotNull] string message);
    }
}