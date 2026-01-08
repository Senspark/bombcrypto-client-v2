using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BLPvpMode.Manager.Api;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using Senspark;

using Services.Server.Handlers;

using Sfs2X.Entities.Data;

using UnityEngine;

using Utils;

namespace App {
    public class DefaultCacheRequestManager : ICacheRequestManager {
        private const int Version = 2;
        private const string CacheRequestKey = "cache_request_key";

        private static readonly (string, int)[] KeyForAllUser = {
            ("version", 2),
            (SFSDefine.SFSCommand.GET_LAUNCH_PAD_DATA, 3600),
            (SFSDefine.SFSCommand.GET_INFORMATION_DATA, 3600),
            (SFSDefine.SFSCommand.GET_START_GAME_CONFIG_V2, 600),
            (SFSDefine.SFSCommand.GET_GACHA_CHEST_SHOP_V2, 600),
            
            (SFSDefine.SFSCommand.GET_GOLD_SHOP_V2, 3600),
            (SFSDefine.SFSCommand.GET_COIN_LEADERBOARD_CONFIG_V2, 3600),
        };

        private static readonly (string, int)[] KeyForOneUser = {
            (SFSDefine.SFSCommand.GET_GEM_SHOP_V2,3600),
            (SFSDefine.SFSCommand.GET_PACK_SHOP_V2, 600),
            (SFSDefine.SFSCommand.GET_PVP_RANKING_V2, 300),
            (SFSDefine.SFSCommand.GET_COIN_RANKING_V2, 300),
            (SFSDefine.SFSCommand.GET_ALL_SEASON_COIN_RANKING_V2, 300),
        };

        private class CacheData {
            [JsonProperty("key")]
            public string RequestKey;

            [JsonProperty("request")]
            public long RequestTime;

            [JsonProperty("expired")]
            public int ExpiredTime;

            [JsonProperty("data")]
            public string DataJson;

            [JsonProperty("parameters")]
            public string Parameters; 

            public CacheData(string requestKey, long request, int expired, string data, string parameters) {
                RequestKey = requestKey;
                RequestTime = request;
                ExpiredTime = expired;
                DataJson = data;
                Parameters = parameters;
            }
        }

        private readonly ILogManager _logManager;
        private readonly ISmartFoxApi _api;
        private Dictionary<string, CacheData> _cache;
        private readonly Dictionary<string, TaskCompletionSource<(long, string)>> _webTasks;

        public DefaultCacheRequestManager(
            ILogManager logManager,
            ISmartFoxApi api
        ) {
            _logManager = logManager;
            _api = api;
            _webTasks = new Dictionary<string, TaskCompletionSource<(long, string)>>();
            Load();
        }

        public void ClearCacheForNewUser() {
            foreach (var (key, _) in KeyForOneUser) {
                if(_cache.ContainsKey(key))
                    _cache[key].DataJson = null;
            }
            Save();
        }

        public void ClearCache(string requestKey) {
            if (_cache.ContainsKey(requestKey)) {
                _cache[requestKey].DataJson = null;
            }
        }
        
        public async Task<(long, string)> GetWebResponse(string cmd, string path) {
            if (!_cache.ContainsKey(cmd)) {
                return await Utils.GetWebResponse(_logManager, path);
            }

            var resultFromCache = GetResultFromCache(cmd, null);
            if (resultFromCache != null) {
                return (200, resultFromCache); //ResponseCode for success = 200
            }
            
            if (!_webTasks.ContainsKey(cmd)) {
                _webTasks.Add(cmd, new TaskCompletionSource<(long, string)>());
                DoTaskGetWeb(cmd, path);
            }
            var (responseCode, result) = await _webTasks[cmd].Task;
            // Chỉ cache kết quả khi get về thành công
            if (responseCode == 200) {
                UpdateData(cmd, null, result);
            }
            return (responseCode, result);
        }

        private void DoTaskGetWeb(string cmd, string path) {
            UniTask.Void(async () => {
                var result = await Utils.GetWebResponse(_logManager, path);
                _webTasks[cmd].SetResult(result);
            });
        }
        
        public async Task<T> ProcessApi<T>(string cmd, ISFSObject data,  IServerHandler<T> serverHandler) {
            if (!_cache.ContainsKey(cmd)) {
                return await _api.Process(serverHandler);
            }
            
            var resultFromCache = GetResultFromCache(cmd, data.ToJson());
            if (resultFromCache != null) {
                return serverHandler switch {
                    IServerHandler<string> => (T) Convert.ChangeType(resultFromCache, typeof(T)),
                    IServerHandler<ISFSObject> => (T) SFSObject.NewFromJsonData(resultFromCache),
                    _ => Parse(serverHandler as LegacyExtensionHandler<T>, cmd, SFSObject.NewFromJsonData(resultFromCache))
                };
            }
            
            var result = await _api.Process(serverHandler);

            // Kiểm tra result có Error ?
            var legacyExtensionHandler = serverHandler as LegacyExtensionHandler<T>;
            var resultError = result switch {
                string str => JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult>(str).Code != 0,
                ISFSObject obj => ServerUtils.HasError(obj),
                _ => legacyExtensionHandler?.ResponseObject == null
            };
            // Chỉ cache kết quả khi kết quả không có Error
            if (!resultError) {
                UpdateData(cmd, data.ToJson(),
                    result switch {
                        string str => str,
                        ISFSObject obj => obj.ToJson(),
                        _ => legacyExtensionHandler?.ResponseObject.ToJson()
                    });
            }
            
            return result;
        }
        
        public async Task<ISFSObject> ProcessApi(string cmd, ISFSObject data, IServerHandler<ISFSObject> serverHandler) {
            if (!_cache.ContainsKey(cmd)) {
                return await _api.Process(serverHandler);
            }

            var resultFromCache = GetResultFromCache(cmd, data.ToJson());
            if (resultFromCache != null) {
                return SFSObject.NewFromJsonData(resultFromCache);
            }

            var result = await _api.Process(serverHandler);

            // Chỉ cache kết quả khi kết quả không có Error
            if (!ServerUtils.HasError(result)) {
                UpdateData(cmd, data.ToJson(),
                    result.ToJson());
            }

            return result;
        }
        
        private T Parse<T>(LegacyExtensionHandler<T> serverHandler, string cmd, ISFSObject data) {
            return serverHandler.ResponseParser(cmd, data);
        }
        
        private string GetResultFromCache(string requestKey, string parameters) {
            if (!_cache.ContainsKey(requestKey)) {
                return null;
            }
            
            // Chỉ trả về số liệu đã catch trùng parameters trước đó 
            if (_cache[requestKey].Parameters != parameters) {
                return null;
            }
            var result = _cache[requestKey];
            var expiredTime = result.RequestTime + result.ExpiredTime;
            var expired = expiredTime < DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
            return expired ? null : result.DataJson;
        }

        private void UpdateData(string requestKey, string parameters, string newData) {
            if (!_cache.ContainsKey(requestKey)) {
                return;
            }
            var cache = _cache[requestKey];
            cache.DataJson = newData;
            cache.Parameters = parameters;
            cache.RequestTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
            _cache[requestKey] = cache;
            Save();
        }

        private void InitData() {
            var keys = KeyForAllUser.Concat(KeyForOneUser);
            _cache = new Dictionary<string, CacheData>();
            foreach (var (key, expired) in keys) {
                _cache[key] = new CacheData(key, 0, expired, null, null);
            }
            Save();
        }

        private void Load() {
            var value = PlayerPrefs.GetString(CacheRequestKey, string.Empty);
            if (string.IsNullOrEmpty(value)) {
                InitData();
                return;
            }
            
            var data = JsonConvert.DeserializeObject<CacheData[]>(value) ?? Array.Empty<CacheData>();
            // Reset các key dành cho 1 user
            var keyForOneUserSet = new HashSet<string>(KeyForOneUser.Select(k => k.Item1));
            _cache = data
                .Where(it => !keyForOneUserSet.Contains(it.RequestKey))
                .ToDictionary(
                    it => it.RequestKey,
                    it => new CacheData(it.RequestKey, it.RequestTime, it.ExpiredTime, it.DataJson, it.Parameters)
                );

            foreach (var (key, expired) in KeyForOneUser) {
                _cache[key] = new CacheData(key, 0, expired, null, null);
            }
            if (!_cache.ContainsKey("version")) {
                InitData();
                return;
            }
            if (_cache["version"].ExpiredTime != Version) {
                InitData();
            }
        }

        private void Save() {
            PlayerPrefs.SetString(CacheRequestKey, JsonConvert.SerializeObject(_cache.Values.Select(it =>
                new CacheData(it.RequestKey, it.RequestTime, it.ExpiredTime, it.DataJson, it.Parameters)
            )));
        }
    }
}