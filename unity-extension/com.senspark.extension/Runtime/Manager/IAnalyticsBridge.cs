using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public interface IAnalyticsBridge {
        [NotNull]
        UniTask<bool> Initialize();

        /// <summary>
        /// Logs a screen view.
        /// </summary>
        void LogScreen([CanBeNull] string name);

        /// <summary>
        /// Logs an event.
        /// </summary>
        /// <param name="name">Event name.</param>
        /// <param name="parameters">Optional event parameters.</param>
        void LogEvent([NotNull] string name, [CanBeNull] Dictionary<string, object> parameters);

        /// <summary>
        /// Ghi nhận bắt đầu vào một màn chơi.
        /// </summary>
        /// <param name="levelNo">Level số mấy?</param>
        /// <param name="levelMode">Mode chơi, hoặc độ khó</param>
        void PushGameLevel(int levelNo, string levelMode);

        /// <summary>
        /// Ghi nhận đã kết thúc màn chơi (Thắng, thua hoặc thoát)
        /// </summary>
        /// <param name="winGame">Set true nếu Thắng</param>
        void PopGameLevel(bool winGame);

        /// <summary>
        /// Logs an ad revenue event.
        /// </summary>
        void LogAdRevenue(
            AdNetwork mediationNetwork,
            string monetizationNetwork,
            double revenue,
            [NotNull] string currencyCode,
            AdFormat format,
            [NotNull] string adUnit,
            [CanBeNull] Dictionary<string, object> extraParameters
        );

        /// <summary>
        /// Logs an in app purchase revenue
        /// </summary>
        void LogIapRevenue(
            string eventName,
            string packageName,
            string orderId,
            double priceValue,
            string currencyIso,
            string receipt
        );

        void LogEarnResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "");

        void LogSpendResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "");
    }

    public class GameLevelData {
        public readonly int LevelNo;
        public readonly string LevelMode;

        public GameLevelData(int levelNo, string levelMode) {
            LevelNo = levelNo;
            LevelMode = levelMode;
        }
    }
}