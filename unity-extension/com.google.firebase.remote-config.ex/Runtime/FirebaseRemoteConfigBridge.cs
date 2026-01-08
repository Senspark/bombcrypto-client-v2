#if !UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#if !UNITY_WEBGL
using Firebase.Extensions;
using Firebase.RemoteConfig;
#endif

using JetBrains.Annotations;

namespace Senspark.Internal {
    internal class FirebaseRemoteConfigBridge : IRemoteConfigBridge {
#if !UNITY_WEBGL        
        [NotNull]
        private readonly Dictionary<string, object> _defaults;

        [CanBeNull]
        private FirebaseRemoteConfig _instance;

        private readonly bool _isRealTime;
        private DateTime _lastActivateTime;
        private const string KTag = "[Senspark][RemoteConfig]";
#endif
        
        public FirebaseRemoteConfigBridge(Dictionary<string, object> defaults, bool isRealTime) {
#if !UNITY_WEBGL            
            _defaults = defaults;
            _isRealTime = isRealTime;
#endif
        }

        public async Task<bool> Initialize() {
#if !UNITY_WEBGL            
            var status = await FirebaseInitializer.Initialize();
            if (!status) {
                return false;
            }
            
            var instance = FirebaseRemoteConfig.DefaultInstance;
            
            // Inittialize bị dừng tại dòng lệnh await này không đi tiếp ...
            // Sẽ kiếm tra lại trong trường hợp chọn _isRealTime = false;
            //await ApplyFetchedData(instance);
            
            await instance.SetDefaultsAsync(_defaults);
            _instance = instance;
            if (_isRealTime) {
                instance.OnConfigUpdateListener += OnRealTimeConfigUpdateListener;
            }
            await instance.FetchAsync();
            // mặc định data fetch được sẽ apply khi app khởi động lại
            if (_isRealTime) {
                await ApplyFetchedData(instance);
            }
#endif
            return true;
        }

        public async Task<bool> ForceFetch() {
#if !UNITY_WEBGL            
            if (_instance == null) {
                FastLog.Error($"{KTag} Must initialize first.");
                return false;
            }
            await _instance.FetchAsync(TimeSpan.Zero);
            var info = _instance.Info;
            if (info.LastFetchStatus != LastFetchStatus.Success) {
                FastLog.Error($"{KTag} Fetch failed. {info.LastFetchFailureReason}");
                return false;
            }
            await _instance.ActivateAsync();
#endif
            return true;
        }

        public bool GetBool(string key) {
#if !UNITY_WEBGL            
            if (_instance == null) {
                throw new ArgumentNullException();
            }
            return _instance.GetValue(key).BooleanValue;
#else
            return false;
#endif
        }

        public long GetLong(string key) {
#if !UNITY_WEBGL            
            if (_instance == null) {
                throw new ArgumentNullException();
            }
            return _instance.GetValue(key).LongValue;
#else
            return 0;
#endif
        }

        public double GetDouble(string key) {
#if !UNITY_WEBGL            
            if (_instance == null) {
                throw new ArgumentNullException();
            }
            return _instance.GetValue(key).DoubleValue;
#else
            return 0;
#endif

        }

        public string GetString(string key) {
#if UNITY_ANDROID || UNITY_IOS            
            if (_instance == null) {
                throw new ArgumentNullException();
            }
            return _instance.GetValue(key).StringValue;
#else
            return string.Empty;
#endif
        }
        
#if !UNITY_WEBGL        
        private async Task ApplyFetchedData(FirebaseRemoteConfig instance) {
            var info = instance.Info;
            if (info.LastFetchStatus == LastFetchStatus.Success && info.FetchTime > _lastActivateTime) {
                _lastActivateTime = instance.Info.FetchTime;
                await instance.ActivateAsync();
                FastLog.Info($"{KTag} Remote data loaded and ready. Last fetch time {_lastActivateTime}");
            }
        }

        private void OnRealTimeConfigUpdateListener(object sender, ConfigUpdateEventArgs args) {
            if (args.Error != RemoteConfigError.None) {
                FastLog.Error($"{KTag} Error occurred while listening: {args.Error}");
                return;
            }

            FastLog.Info($"{KTag} Updated keys: " + string.Join(", ", args.UpdatedKeys));

            _instance?.ActivateAsync().ContinueWithOnMainThread(t => {
                FastLog.Info($"{KTag} New remote data has been fetched.");
            });
        }
#endif
    }
}
#endif