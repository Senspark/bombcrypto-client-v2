using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Senspark.Internal {
    public class UnityAnalyticsBridge : IAnalyticsBridge {
        private static readonly ILogger Logger = Debug.unityLogger;
        private GameLevelData _levelData;

        public UniTask<bool> Initialize() {
            return UniTask.FromResult(true);
        }

        public void LogScreen(string name) {
            Logger.Log(LogType.Log, $"screen={name}");
        }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
            var tokens = new[] { $"[{name}]" };
            if (parameters != null) {
                tokens = tokens
                    .Concat(parameters.Select(item => $"[{item.Key}={item.Value}]"))
                    .ToArray();
            }
            Logger.Log(LogType.Log, $"{string.Join(" ", tokens)}");
        }

        public void PushGameLevel(int levelNo, string levelMode) {
#if UNITY_EDITOR
            if (_levelData != null) {
                throw new Exception("[Senspark] Must call PopGameLevel() to end previous level");
            }
#endif
            _levelData = new GameLevelData(levelNo, levelMode);
            Logger.Log(LogType.Log, $"[Senspark] PushGameLevel: [levelNo={levelNo}] [levelMode={levelMode}]");
        }

        public void PopGameLevel(bool winGame) {
#if UNITY_EDITOR
            if (_levelData == null) {
                throw new Exception("[Senspark] Must call PushGameLevel() before call this function");
            }
#endif
            if (_levelData != null) {
                Logger.Log(LogType.Log,
                    $"[Senspark] PopGameLevel: [levelNo={_levelData.LevelNo}] [levelMode={_levelData.LevelMode}] [winGame={winGame}]");
            }
            _levelData = null;
        }

        public void LogAdRevenue(
            AdNetwork mediationNetwork,
            string monetizationNetwork,
            double revenue,
            string currencyCode,
            AdFormat format,
            string adUnit,
            Dictionary<string, object> extraParameters
        ) {
            Logger.Log(LogType.Log, $"[network={monetizationNetwork}] revenue={revenue} format={format}");
        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue,
            string currencyIso, string receipt) {
            Logger.Log(LogType.Log, $"[eventName={eventName}] revenue={priceValue}");
        }

        public void LogEarnResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "") {
        }

        public void LogSpendResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "") {
        }
    }
}