using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public class AdObserver {
        [CanBeNull]
        public Action OnLoaded { get; set; }

        [CanBeNull]
        public Action<string> OnFailedToLoad { get; set; }
    }

    public interface IAd : IObserverManager<AdObserver>, IDisposable {
        /// <summary>
        /// Checks whether this ad is loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Initializes this ad.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Attempts to load this ad.
        /// </summary>
        /// <returns></returns>
        [MustUseReturnValue]
        [NotNull]
        Task<bool> Load();
    }
}