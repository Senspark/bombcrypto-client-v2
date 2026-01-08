using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

using UnityEngine;

using Castle.Core.Internal;

using Senspark;

public class JsonTaskLoader {
    public List<TaskTonDataJson> Tasks { get; private set; } = new();
    public List<CategoryTonDataJson> Categories { get; private set; } = new();

    private readonly ILogManager _logManager;

    //Default path dùng cho trường hợp server ko trả về link
    private readonly string _defaultDataPath =
        "https://game.bombcrypto.io/tasks_data/data/data.json";

    private readonly string _defaultIconPath = "https://game.bombcrypto.io/tasks_data/icon";

    private readonly string _dataPath;
    private string _iconPath;

    public JsonTaskLoader(string path, ILogManager logManager) {
        _logManager = logManager;
        #if UNITY_EDITOR
        _dataPath = path;
        #else
        if (path != null && App.Utils.IsUrl(path)) {
            _dataPath = path;
        } else {
            _dataPath = path == null ? _defaultDataPath : Path.Combine(Application.streamingAssetsPath, path);
        }
#endif
    }

    public async Task LoadJson() {
        try {
            if (!IsValidUrl(_dataPath))
                return;

            if (App.Utils.IsUrl(_dataPath)) {
                await LoadWithUrl();
            } else {
                await LoadWithStreamingAsset();
            }
            //Kiểm tra xem icon path có đc cập nhật chưa
            if (!IsValidUrl(_iconPath)) {
                _iconPath = _defaultIconPath;
            }
        } catch (Exception e) {
            _logManager.Log($"CLIENT: Error loading JSON files {_dataPath}: {e.Message}");
        }
    }

    private async Task LoadWithStreamingAsset() {

#if UNITY_EDITOR
        var resourceRequest = Resources.LoadAsync<TextAsset>(_dataPath);
        await resourceRequest;

        if (resourceRequest.asset is TextAsset textAsset) {
            var jsonContent = textAsset.text;
            var jsonData = JsonConvert.DeserializeObject<TaskJsonData>(jsonContent);

            if (jsonData != null) {
                Tasks.AddRange(jsonData.Tasks);
                Categories.AddRange(jsonData.Categories);
                _iconPath = jsonData.IconBaseUrl;
            }
        }
        #else
        
        string jsonContent = await File.ReadAllTextAsync(_dataPath);
        var jsonData = JsonConvert.DeserializeObject<TaskJsonData>(jsonContent);
        
        if (jsonData != null) {
            Tasks.AddRange(jsonData.Tasks);
            Categories.AddRange(jsonData.Categories);
            _iconPath = jsonData.IconBaseUrl;
        }
#endif

    }

    private async Task LoadWithUrl() {
        var jsonContent = await App.Utils.GetTextFile(_logManager, _dataPath);
        var jsonData = JsonConvert.DeserializeObject<TaskJsonData>(jsonContent);
        if (jsonData != null) {
            Tasks.AddRange(jsonData.Tasks);
            Categories.AddRange(jsonData.Categories);
            _iconPath = jsonData.IconBaseUrl;
        }
    }

    public async Task<Sprite> LoadIcon(string iconPath) {
        if (!IsValidUrl(iconPath)) {
            return null;
        }
        var path = Path.Combine(_iconPath, iconPath);

        if (!IsValidUrl(path)) {
            return null;
        }
        try {
            var loadImageTask = App.Utils.LoadImageFromPath(path);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(3));

            var completedTask = await Task.WhenAny(loadImageTask, timeoutTask);

            if (completedTask == timeoutTask) {
                _logManager.Log($"CLIENT: Operation timed out when load icon {iconPath} - url = {path}");
                return null;
            }
            var icon = await loadImageTask;
            return icon.TryCompress(true);
        } catch (Exception e) {
            _logManager.Log($"CLIENT: Error loading icon {iconPath} - url = {path}: {e.Message}");
            return null;
        }
    }

    private bool IsValidUrl(string url) {
        return !url.IsNullOrEmpty();
    }

    private class TaskJsonData {
        [JsonProperty("tasks")]
        public List<TaskTonDataJson> Tasks { get; set; }

        [JsonProperty("categories")]
        public List<CategoryTonDataJson> Categories { get; set; }

        [JsonProperty("icon_base_url")]
        public string IconBaseUrl { get; set; } = "";
    }
}

public static class SpriteExtensions {
    public static Sprite TryCompress(this Sprite sprite, bool highQuality) {
        if (sprite.texture.width % 4 == 0 && sprite.texture.height % 4 == 0) {
            sprite.texture.Compress(highQuality);
            return sprite;
        }
        return sprite;
    }
}