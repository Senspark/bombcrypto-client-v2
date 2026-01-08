using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark {
    [Service(typeof(ILogManager))]
    public interface ILogManager {
        /// <summary>
        /// Initializes this service.
        /// </summary>
        [NotNull]
        Task Initialize(float timeOut);

        /// <summary>
        /// Logs a message.
        /// </summary>
        void Log(
            [NotNull] string message = "",
            [CallerMemberName] [NotNull] string memberName = "",
            [CallerFilePath] [NotNull] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);
    }
}