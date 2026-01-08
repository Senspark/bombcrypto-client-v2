using System.Collections.Generic;

using App;

using Senspark;

using UnityEngine;

namespace Analytics.Modules {
    public class FirebaseAnalyticsModuleLogin : IAnalyticsModuleLogin {
        private readonly IAnalyticsBridge _analyticsBridge;

        private const string LogProgress = "lg_login_progress";
        private const string LogFirstLogin0 = "conversion_lg_1st_login_{0}";
        private const string LogFirstLoginSuccess0 = "conversion_lg_1st_login_success_{0}";
        private const string LogSecondLoginSuccess0 = "conversion_lg_2nd_login_success_{0}";
        private const string KeyResult = "result";
        private const string KeyType = "type";
        private const string ValueFail = "fail";
        private const string ValueSuccess = "success";

        private readonly Dictionary<ActionType, string> _actionTypes = new() {
            [ActionType.ShowDialogLogin] = "lg_show_dialog_login",
            [ActionType.AutoLogin] = "lg_auto_login",
            [ActionType.ChooseLogin] = "lg_choose_login",
            [ActionType.CreateNewAccountFailed] = "lg_create_account_result",
            [ActionType.CreateNewAccountSuccess] = "lg_create_account_result",
            [ActionType.LoginFailed] = "lg_login_result",
            [ActionType.LoginSuccess] = "lg_login_result",
            [ActionType.EnteringMainMenu] = "lg_entering_menu",
            [ActionType.EnteringTreasureHunt] = "lg_entering_treasure_hunt",
        };

        private readonly Dictionary<LoginType, string> _loginTypes = new() {
            [LoginType.Unknown] = "unknown",
            [LoginType.Guest] = "guest",
            [LoginType.Senspark] = "senspark",
            [LoginType.Facebook] = "facebook",
            [LoginType.Apple] = "apple",
        };
        
        private readonly string _keyTrackedEvent = $"{nameof(FirebaseAnalyticsModuleLogin)}_tracked";
        private readonly List<string> _tracked;

        public FirebaseAnalyticsModuleLogin(bool enableLog, ILogManager logManager) {
            _analyticsBridge = Application.platform switch {
                RuntimePlatform.WebGLPlayer => new WebGLFirebaseAnalyticsBridge(enableLog, AppConfig.FirebaseAppId,
                    AppConfig.FirebaseMeasurementId),
                RuntimePlatform.Android or RuntimePlatform.IPhonePlayer => new MobileFirebaseAnalyticsBridge(logManager,
                    enableLog),
                _ => new NullAnalyticsBridge(logManager, nameof(FirebaseAnalyticsModuleLogin))
            };
            _analyticsBridge.Initialize();
            _tracked = AnalyticsUtils.LoadTrackedEvent(_keyTrackedEvent);
        }

        public void TrackLoadingProgress(int progress, LoginType loginType) {
            Parameter[] parameters = {
                new(KeyResult, progress),
                new(KeyType, _loginTypes[loginType]),
            };
            _analyticsBridge.LogEvent(LogProgress, parameters);
        }

        public void TrackAction(ActionType actionType, LoginType loginType) {
            // Track normal events
            var name = _actionTypes[actionType];
            var parameters = new List<Parameter>();
            var hasParameter = false;
            switch (actionType) {
                case ActionType.ChooseLogin:
                    hasParameter = true;
                    parameters.Add(new Parameter(KeyType, _loginTypes[loginType]));
                    break;
                case ActionType.CreateNewAccountFailed:
                case ActionType.LoginFailed:
                    hasParameter = true;
                    parameters.Add(new Parameter(KeyResult, ValueFail));
                    parameters.Add(new Parameter(KeyType, _loginTypes[loginType]));
                    break;
                case ActionType.CreateNewAccountSuccess:
                case ActionType.LoginSuccess:
                    hasParameter = true;
                    parameters.Add(new Parameter(KeyResult, ValueSuccess));
                    parameters.Add(new Parameter(KeyType, _loginTypes[loginType]));
                    break;
            }
            if (name == null) {
                return;
            }
            if (!hasParameter) {
                _analyticsBridge.LogEvent(name);
            } else {
                _analyticsBridge.LogEvent(name, parameters.ToArray());
            }

            // Track conversions
            switch (actionType) {
                case ActionType.ChooseLogin:
                    TrackFirstTimeLogin(loginType);
                    break;
                case ActionType.LoginSuccess:
                    TrackFirstTimeLoginSuccess(loginType);
                    break;
            }
        }

        private void TrackFirstTimeLogin(LoginType loginType) {
            var name = loginType.ToString().ToLower();
            var first = string.Format(LogFirstLogin0, name);
            if (IsTracked(first)) {
                return;
            }
            _analyticsBridge.LogEvent(first);
            SaveTracked(first);
        }

        private void TrackFirstTimeLoginSuccess(LoginType loginType) {
            var name = loginType.ToString().ToLower();
            var first = string.Format(LogFirstLoginSuccess0, name);
            if (!IsTracked(first)) {
                _analyticsBridge.LogEvent(first);
                SaveTracked(first);
                return;
            }
            
            var second = string.Format(LogSecondLoginSuccess0, name);
            if (IsTracked(second)) {
                return;
            }
            _analyticsBridge.LogEvent(second);
            SaveTracked(second);
        }

        private bool IsTracked(string value) {
            return _tracked.Contains(value);
        }

        private void SaveTracked(string value) {
            _tracked.Add(value);
            AnalyticsUtils.SaveTrackedEvent(_keyTrackedEvent, _tracked);
        }
    }
}