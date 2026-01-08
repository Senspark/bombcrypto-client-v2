using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public interface IFullScreenAd : IAd {
        [MustUseReturnValue]
        [NotNull]
        Task<(AdResult result, string message)> Show();
    }
}