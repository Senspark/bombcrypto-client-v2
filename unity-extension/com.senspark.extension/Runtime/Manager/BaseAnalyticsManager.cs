using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public class BaseAnalyticsManager : IAnalyticsManager {
        [NotNull]
        private readonly IAnalyticsBridge _bridge;

        [NotNull]
        private readonly Queue<Action> _pendingActions;

        [NotNull]
        private readonly Dictionary<string, object> _parameters;

        [NotNull]
        private readonly Stack<string> _screens;

        [CanBeNull]
        private Task _initializer;

        private bool? _initialized;

        public BaseAnalyticsManager([NotNull] IAnalyticsBridge bridge) {
            _bridge = bridge;
            _pendingActions = new Queue<Action>();
            _parameters = new Dictionary<string, object>();
            _screens = new Stack<string>();
        }

        public Task Initialize(float timeOut) => _initializer ??= InitializeImpl(timeOut);

        [NotNull]
        private async Task InitializeImpl(float timeOut) {
            await Task.WhenAny(((Func<Task>) (async () => {
                var result = await _bridge.Initialize();
                _initialized = result;
                if (result) {
                    while (_pendingActions.Count > 0) {
                        var action = _pendingActions.Dequeue();
                        action();
                    }
                }
            }))(), ((Func<Task>) (async () => { //
                await Task.Delay((int) (timeOut * 1000));
            }))());
        }

        public void PushScreen(string screenName) {
            _screens.Push(screenName);
            if (_initialized == null) {
                _pendingActions.Enqueue(() => _bridge.LogScreen(screenName));
                return;
            }
            if (!_initialized.Value) {
                return;
            }
            _bridge.LogScreen(screenName);
        }

        public bool PopScreen() {
            if (_screens.Count == 0) {
                return false;
            }
            _screens.Pop();
            var screen = _screens.Count == 0 ? null : _screens.Peek();
            if (_initialized == null) {
                _pendingActions.Enqueue(() => _bridge.LogScreen(screen));
                return true;
            }
            if (!_initialized.Value) {
                return true;
            }
            _bridge.LogScreen(screen);
            return true;
        }

        public void PushParameter(string key, object value) {
            _parameters[key] = value;
        }

        public bool PopParameter(string key) {
            return _parameters.Remove(key);
        }

        public void LogEvent(string name) {
            LogEvent(name, new Dictionary<string, object>());
        }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
            foreach (var (key, value) in _parameters) {
                parameters.TryAdd(key, value);
            }
            if (_screens.Count > 0) {
                parameters.TryAdd("screen", _screens.Peek());
            }
            if (_initialized == null) {
                _pendingActions.Enqueue(() => _bridge.LogEvent(name, parameters));
                return;
            }
            if (!_initialized.Value) {
                return;
            }
            _bridge.LogEvent(name, parameters);
        }

        public void LogEvent(IAnalyticsEvent analyticsEvent) {
            var type = analyticsEvent.GetType();
            var parameters = new Dictionary<string, object>();
            var infos = type.GetMembers(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.GetField |
                BindingFlags.GetProperty);
            foreach (var info in infos) {
                var attribute = Attribute.GetCustomAttribute(info, typeof(AnalyticsParameterAttribute)) as
                    AnalyticsParameterAttribute;
                if (attribute == null) {
                    continue;
                }
                var name = attribute.Name ?? info.Name;
                var value = info switch {
                    FieldInfo item => item.GetValue(analyticsEvent),
                    PropertyInfo item => item.GetValue(analyticsEvent),
                    _ => throw new ArgumentOutOfRangeException(),
                };
                parameters[name] = value;
            }
            LogEvent(analyticsEvent.Name, parameters);
        }

        public void PushGameLevel(int levelNo, string levelMode) {
            if (_initialized == null) {
                _pendingActions.Enqueue(DoAction);
                return;
            }
            if (!_initialized.Value) {
                return;
            }
            DoAction();
            return;
            void DoAction() {
                _bridge.PushGameLevel(levelNo, levelMode);
            }
        }

        public void PopGameLevel(bool winGame) {
            if (_initialized == null) {
                _pendingActions.Enqueue(DoAction);
                return;
            }
            if (!_initialized.Value) {
                return;
            }
            DoAction();
            return;
            void DoAction() {
                _bridge.PopGameLevel(winGame);
            }
        }

        public void LogAdRevenue(
            AdNetwork mediationNetwork,
            string monetizationNetwork,
            double revenue,
            string currencyCode,
            AdFormat format,
            string adUnit
        ) {
            var parameters = _parameters.ToDictionary(it => it.Key, it => it.Value);
            if (_initialized == null) {
                _pendingActions.Enqueue(Action);
                return;
            }
            if (!_initialized.Value) {
                return;
            }
            Action();
            return;
            void Action() {
                _bridge.LogAdRevenue(
                    mediationNetwork,
                    monetizationNetwork,
                    revenue,
                    currencyCode,
                    format,
                    adUnit,
                    parameters
                );
            }
        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue,
            string currencyIso, string receipt) {
            if (_initialized == null) {
                _pendingActions.Enqueue(DoAction);
                return;
            }
            if (!_initialized.Value) {
                return;
            }
            DoAction();
            return;
            void DoAction() {
                _bridge.LogIapRevenue(eventName, packageName, orderId, priceValue, currencyIso, receipt);
            }
        }

        public void LogEarnResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "") {
            if (_initialized == null) {
                _pendingActions.Enqueue(DoAction);
                return;
            }
            if (!_initialized.Value) {
                return;
            }
            DoAction();
            return;
            void DoAction() {
                _bridge.LogEarnResource(playMode, level, resourceName, amount, balance, item, itemType, booster);
            }
        }

        public void LogSpendResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "") {
            if (_initialized == null) {
                _pendingActions.Enqueue(DoAction);
                return;
            }
            if (!_initialized.Value) {
                return;
            }
            DoAction();
            return;
            void DoAction() {
                _bridge.LogSpendResource(playMode, level, resourceName, amount, balance, item, itemType, booster);
            }
        }
    }
}