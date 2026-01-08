using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AnalyticsParameterAttribute : Attribute {
        [CanBeNull]
        public string Name { get; }

        public AnalyticsParameterAttribute([CanBeNull] string name = null) {
            Name = name;
        }
    }

    public interface IAnalyticsEvent {
        /// <summary>
        /// Event name.
        /// </summary>
        [NotNull]
        string Name { get; }
    }

    [Service(typeof(IAnalyticsManager))]
    public interface IAnalyticsManager {
        /// <summary>
        /// Initializes the service.
        /// </summary>
        [NotNull]
        Task Initialize(float timeOut);

        /// <summary>
        /// Pushes a screen.
        /// </summary>
        void PushScreen([NotNull] string screenName);

        /// <summary>
        /// Pops the last screen.
        /// </summary>
        bool PopScreen();

        /// <summary>
        /// Pushes a tracking parameter.
        /// </summary>
        void PushParameter([NotNull] string key, [NotNull] object value);

        /// <summary>
        /// Pops an existed tracking parameter.
        /// </summary>
        /// <param name="key"></param>
        bool PopParameter([NotNull] string key);

        /// <summary>
        /// Logs an event.
        /// </summary>
        void LogEvent([NotNull] string name);

        /// <summary>
        /// Logs an event with additional parameters.
        /// </summary>
        void LogEvent([NotNull] string name, [NotNull] Dictionary<string, object> parameters);

        /// <summary>
        /// Logs an event with the customized event.
        /// </summary>
        void LogEvent([NotNull] IAnalyticsEvent analyticsEvent);

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
            [NotNull] string monetizationNetwork,
            double revenue,
            [NotNull] string currencyCode,
            AdFormat format,
            [NotNull] string adUnit
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
}