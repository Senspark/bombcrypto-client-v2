using System.Collections.Generic;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Senspark;

namespace Analytics {
    public interface IAnalyticsBridge {
        Task<bool> Initialize();
        void LogScene(string sceneName);
        void LogEvent(string name);
        void LogEvent(string name, Parameter[] parameters);
        void LogEvent(string name, Dictionary<string, object> parameters);
        void PushGameLevel(int levelNo, string levelMode);
        void PopGameLevel(bool winGame);
        void LogAdRevenue(
            AdNetwork mediationNetwork,
            string monetizationNetwork,
            double revenue,
            [NotNull] string currencyCode,
            AdFormat format,
            [NotNull] string adUnit,
            [CanBeNull] Dictionary<string, object> extraParameters
        );

        void LogIapRevenue(
            string eventName,
            string packageName,
            string orderId,
            double priceValue,
            string currencyIso,
            string receipt
        );
    }

    public interface IAppsFlyerAnalyticsBridge : IAnalyticsBridge {
        void SetWalletAddress(string address);
    }
}