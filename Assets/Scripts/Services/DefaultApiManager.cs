using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Data;

using Senspark;

using Exceptions;

using Game.UI;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PvpSchedule.Models;

using UnityEngine;
using UnityEngine.Networking;

using Application = UnityEngine.Device.Application;

namespace App {
    public class DefaultApiManager : IApiManager {
        private const string GET_COIN_BALANCE = "coin_balance?address=";
        private const string GET_CCU = "ccu";
        private const string GET_PVP_ROOM_LIST = "pvp-matching-2/tournament/room/status";
        private const string GET_PVP_MATCHES = "pvp-matching-2/tournament/status";
        private const string GET_MY_MATCHES = "pvp-matching-2/tournament/my-matches";

        private readonly string ApiHost;
        private readonly string ApiTestHost;
        private const string BASE_API_TEST_HOST_LOCAL = "http://localhost:8101/";

        public string Domain => _isProduction ? ApiHost : ApiTestHost;

        private readonly bool _isProduction;
        private readonly ILogManager _logManager;
        private readonly string _network;

        public DefaultApiManager(INetworkConfig networkConfig, ILogManager logManager, bool isProduction) {
            _logManager = logManager;
            _isProduction = isProduction;
            _network = networkConfig.NetworkName;
            if (AppConfig.IsTournament()) {
                ApiHost = AppConfig.TournamentBaseApiHost;
                ApiTestHost = Application.isEditor ? BASE_API_TEST_HOST_LOCAL : AppConfig.BaseApiTestHost;
            } else {
                if (networkConfig.NetworkType == NetworkType.Binance) {
                    ApiHost = AppConfig.BaseApiHost;
                    ApiTestHost = AppConfig.BaseApiTestHost;
                } else {
                    ApiHost = $"{AppConfig.BaseApiHost}{networkConfig.NetworkName}/";
                    ApiTestHost = $"{AppConfig.BaseApiTestHost}{networkConfig.NetworkName}/";
                }
            }
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task<double> GetCoinBalance(string walletAddress) {
            var url = GetHost(Domain, GET_COIN_BALANCE, walletAddress);

            var (code, res) = await Utils.GetWebResponse(_logManager, url);
            var message = "Could not get BCoin Balance";
            if (!string.IsNullOrEmpty(res)) {
                var data = JObject.Parse(res);
                if (data["code"] != null && data["message"] != null) {
                    if (data["code"].Value<int>() == 0) {
                        var result = data["message"].Value<double>();
                        return result;
                    }
                    message = data["message"].Value<string>();
                }
            }
            throw new Exception(message);
        }

        /// <exception cref="NoInternetException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<bool> CheckServerTime() {
            long longTime = -1;
            try {
                longTime = await RequestServerUnixTime();
            } catch (Exception e) {
                // ignore
            }
            // if (longTime < 0) {
            //     throw new NoInternetException();
            // }

            var unixTime = DateTimeOffset.FromUnixTimeMilliseconds(longTime).DateTime;
            var serverTime = TimeZoneInfo.ConvertTimeFromUtc(unixTime, TimeZoneInfo.Local);
            var clientTime = DateTime.Now;
            var difference = DateTime.Now - serverTime;

            _logManager.Log(
                $"ServerTime: {serverTime}, ClientTime: {clientTime}, Difference: {difference.TotalSeconds}");

            if (Math.Abs(difference.TotalMinutes) > 10) {
                throw new Exception("Error logging in, your computer time is not correct");
            }

            return true;
        }

        [Obsolete("This method is not used anymore")]
        public async Task<long> RequestServerUnixTime() {
            // var url = GetHost(BASE_API_HOST, GET_TIME);
            //
            // using var request = UnityWebRequest.Get(url);
            // request.timeout = 3;
            // await request.SendWebRequest();
            //
            // if (request.result == UnityWebRequest.Result.Success) {
            //     var obj = JObject.Parse(request.downloadHandler.text);
            //     request.Dispose();
            //     if (obj["code"] != null && obj["code"].Value<int>() == 0 && obj["message"] != null) {
            //         return obj["message"].Value<long>();
            //     }
            // }
            //
            // request.Dispose();
            return -1;
        }

        public async Task<(int, int)> GetCcu() {
            var url = GetHost(Domain, GET_CCU);

            var (code, res) = await Utils.GetWebResponse(_logManager, url);
            if (!string.IsNullOrWhiteSpace(res)) {
                var obj = JObject.Parse(res);
                var msg = (JObject)obj["message"];
                if (msg?["ccu"] != null && msg["maxCcu"] != null) {
                    return (msg["ccu"].Value<int>(), msg["maxCcu"].Value<int>());
                }
            }
            return (0, 0);
        }

        private static string GetHost(string host, string command, string param = null) {
            return param == null
                ? $"{host}{command}"
                : $"{host}{command}{param}";
        }

        public async Task<List<IPvpRoomInfo>> GetPvpRoomList() {
            var url = GetHost(Domain, GET_PVP_ROOM_LIST);
            var (code, response) = await Utils.GetWebResponse(_logManager, url);
            var infoList = new List<IPvpRoomInfo>();
            if (string.IsNullOrWhiteSpace(response)) {
                return infoList;
            }
            var obj = JObject.Parse(response);
            var msg = (JObject)obj["message"];
            var details = (JArray)msg["details"];
            foreach (var item in details) {
                var zone = (JObject)item;
                var rooms = (JArray)zone["rooms"];
                foreach (var item2 in rooms) {
                    var info = JsonConvert.DeserializeObject<PvpRoomInfo>(item2.ToString());
                    infoList.Add(info);
                }
            }
            return infoList;
        }

        public async Task<List<IPvpMatchSchedule>> GetPvpMatches() {
            var url = GetHost(Domain, GET_PVP_MATCHES);
            var (code, response) = await Utils.GetWebResponse(_logManager, url);
            var matchList = new List<IPvpMatchSchedule>();
            if (string.IsNullOrWhiteSpace(response)) {
                return matchList;
            }
            var obj = JObject.Parse(response);
            var message = (JObject)obj["message"];
            var matches = (JArray)message["matches"];
            foreach (var item in matches) {
                var match = JsonConvert.DeserializeObject<PvpMatchSchedule>(item.ToString());
                matchList.Add(match);
            }
            return matchList;
        }
        
        public async Task<List<string>> GetMyMatches(string userName) {
            var url = GetHost(Domain, GET_MY_MATCHES);
            var body = new JObject(){
                {"username", userName}
            };

            var (code, response) = await Utils.PostWebResponse(_logManager, url, body.ToString());
            var matchList = new List<string>();
            if (string.IsNullOrWhiteSpace(response)) {
                return matchList;
            }
            var obj = JObject.Parse(response);

            var message = (JObject)obj["message"];
            var matches = (JArray)message["my_match"];
            foreach (var item in matches) {
                var match = JsonConvert.DeserializeObject<string>(item.ToString());
                matchList.Add(match);
            }
            return matchList;
        }
    }
}