using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

using Newtonsoft.Json;

using UnityEngine;

namespace App {
    public class DefaultNewsManager : INewsManager {
        private readonly IApiManager _apiManager;
        private readonly ILogManager _logManager;

        private const string PLAYER_PREF_HAD_READ = "NEWS_HAD_READ";
        private const string NewsUrlParam = "news";
        private List<NewsMessage> _newsList = new();
        private List<int> _readList;

        private const string AnnouncementsUrlParam = "news?category=announcement";
        private List<AnnouncementsMessage> _announcementsList = new();

        private const int SyncEveryMinute = 5;
        private DateTime _lastSyncTime;
        private SyncState _syncState = SyncState.NotSync;

        public Task<bool> Initialize() {
            LoadReadList();
            return Task.FromResult(true);
        }

        private void LoadReadList() {
            var strList = PlayerPrefs.GetString(PLAYER_PREF_HAD_READ, "");
            if (string.IsNullOrEmpty(strList)) {
                _readList = new List<int>();
            } else {
                _readList = JsonConvert.DeserializeObject<List<int>>(strList);
            }
        }

        public void Destroy() {
        }

        public DefaultNewsManager(ILogManager logManager, IApiManager apiManager) {
            _logManager = logManager;
            _apiManager = apiManager;
        }
        
        public async Task SyncData() {
            if (_syncState == SyncState.Synced) {
                if (CanSync()) {
                    _syncState = SyncState.NotSync;
                }
            }
            if (_syncState == SyncState.NotSync) {
                _syncState = SyncState.Syncing;
                _lastSyncTime = DateTime.Now;
                await SyncNews();
                await SyncAnnouncements();
                _syncState = SyncState.Synced;
            }
            await WebGLTaskDelay.Instance.WaitUtil(() => _syncState == SyncState.Synced);
        }

        private async Task SyncAnnouncements() {
            var url = $"{_apiManager.Domain}{AnnouncementsUrlParam}";
            var (code, res) = await Utils.GetWebResponse(_logManager, url);
            if (!string.IsNullOrWhiteSpace(res)) {
                var announcements = AnnouncementsEvent.Parse(res);
                if (announcements.Code == 0 && announcements.Messages != null) {
                    _announcementsList = announcements.Messages;
                }
            }
        }

        private async Task SyncNews() {
            var url = $"{_apiManager.Domain}{NewsUrlParam}";
            var (code, res) = await Utils.GetWebResponse(_logManager, url);
            if (!string.IsNullOrWhiteSpace(res)) {
                var newsEvent = NewsEvent.Parse(res);
                if (newsEvent.Code == 0) {
                    _newsList = newsEvent.Messages;
                }
            }
        }

        public List<NewsMessage> GetNews() {
            return _newsList;
        }

        public List<AnnouncementsMessage> GetAnnouncements() {
            return _announcementsList;
        }

        public NewsMessage GetNewsEventByIndex(int index) {
            return _newsList[index];
        }

        public void SaveNewsRead(int newsId) {
            if (_readList.Contains(newsId)) {
                return;
            }
            _readList.Add(newsId);
            PlayerPrefs.SetString(PLAYER_PREF_HAD_READ, JsonConvert.SerializeObject(_readList));
        }

        public bool IsHadRead(int newsId) {
            return _readList.Contains(newsId);
        }

        private bool CanSync() {
            if (_lastSyncTime == DateTime.MinValue) {
                _lastSyncTime = DateTime.Now;
                return true;
            }
            return _lastSyncTime.AddMinutes(SyncEveryMinute) < DateTime.Now;
        }
        
        private enum SyncState {
            NotSync, Syncing, Synced
        }
    }

    public class NewsMessage {
        [JsonProperty("Id")]
        public int Id { get; set; }
        [JsonProperty("Date")]
        public long EventDate { get; set; }
        [JsonProperty("Link Image")]
        public string ImageLink { get; set; }
        [JsonProperty("News Title")]
        public string Title { get; set; }
        [JsonProperty("News Content")]
        public string Content { get; set; }
        [JsonProperty("News Link")]
        public string NewsLink { get; set; }
    }
    
    public class NewsEvent {
        [JsonProperty("code")]
        public int Code { get; set; }
        
        [JsonProperty("message")]
        public List<NewsMessage> Messages { get; set; }

        public static NewsEvent Parse(string data) {
            var newsEvent = JsonConvert.DeserializeObject<NewsEvent>(data);
            return newsEvent;
        }
    }
    
    public class AnnouncementsMessage {
        [JsonProperty("Announcements Content")]
        public string Content { get; set; }
        [JsonProperty("Date Start")]
        public long DateStart { get; set; }
        [JsonProperty("Date End")]
        public long DateEnd { get; set; }
        [JsonProperty("Repeat")]
        public int Repeat { get; set; }
    }

    public class AnnouncementsEvent {
        [JsonProperty("code")]
        public int Code { get; set; }
        
        [JsonProperty("message")]
        public List<AnnouncementsMessage> Messages { get; set; }

        public static AnnouncementsEvent Parse(string data) {
            var announcements = JsonConvert.DeserializeObject<AnnouncementsEvent>(data);
            return announcements;
        }
    }
}
