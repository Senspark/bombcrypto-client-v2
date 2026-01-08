using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Castle.Core.Internal;
using Senspark;

public class DailyTaskLoader {
    public class TaskDataJson {
        [JsonProperty("task_id")]
        public int taskId { get; set; }

        [JsonProperty("task_desc")]
        public string taskDesc { get; set; }
        
        [JsonProperty("task_max_progress")]
        public int taskMaxProgress { get; set; }
    }
    
    public class DailyTaskJson {
        [JsonProperty("chest")]
        public int[] chest { get; set; }

        [JsonProperty("tasks")]
        public TaskDataJson[] tasks { get; set; }
    }

    public List<int> Chest { get; set; } = new();
    public List<TaskDataJson> Tasks { get; set; } = new();

    private readonly ILogManager _logManager;

    private string _dataUrl;

    public DailyTaskLoader(ILogManager logManager, string dataUrl) {
        _logManager = logManager;
        _dataUrl = dataUrl;
    }

    public async Task LoadJson() {
        if (!IsValidUrl(_dataUrl))
            return;
        try {
            if (App.Utils.IsUrl(_dataUrl)) {
                await LoadWithUrl(_dataUrl);
            } else {
                await LoadWithStreamingAsset();
            }
        } catch (Exception e) {
            _logManager.Log($"CLIENT: Error loading JSON files {_dataUrl}: {e.Message}");
        }
    }

    private async Task LoadWithStreamingAsset() {
    }

    private async Task LoadWithUrl(string url) {
        var jsonContent = await App.Utils.GetTextFile(_logManager, url);
        var jsonData = JsonConvert.DeserializeObject<DailyTaskJson>(jsonContent);
        if (jsonData != null) {
            Chest.AddRange(jsonData.chest);
            Tasks.AddRange(jsonData.tasks);
        }
    }

    private bool IsValidUrl(string url) {
        return !url.IsNullOrEmpty();
    }
}