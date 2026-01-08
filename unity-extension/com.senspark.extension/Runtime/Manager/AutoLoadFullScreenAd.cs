using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public class AutoLoadFullScreenAd : AutoLoadAd, IFullScreenAd {
        [NotNull]
        private readonly IFullScreenAd _ad;

        public AutoLoadFullScreenAd([NotNull] IFullScreenAd ad) : base(ad) {
            _ad = ad;
        }

        public async Task<(AdResult result, string message)> Show() {
            try {
                return await _ad.Show();
            } finally {
                _ = Load();
            }
        }
    }
}