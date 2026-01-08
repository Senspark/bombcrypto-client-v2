using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.Networking;

namespace Communicate {
    public static class NetworkHelper {
        public static async UniTask<T> SendGet<T>(string url) {
            try {
                var request = new UnityWebRequest(url);
                request.method = UnityWebRequest.kHttpVerbGET;
                request.downloadHandler = new DownloadHandlerBuffer();
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success) {
                    Debug.LogError($"Error: {request.error}");
                    return default;
                }
                var response = request.downloadHandler.text;
                var wrapper = JsonConvert.DeserializeObject<Wrapper<T>>(response);
                if (!wrapper.Success) {
                    Debug.LogError($"Error: {wrapper.Error}");
                    return default;
                }

                return wrapper.Message ?? default;
            } catch (Exception e) {
                Debug.LogError(e);
                return default;
            }
        }
        
        public static async UniTask<T> SendPost<T>(string url, object postData, Dictionary<string, string> headers = null) {
            try {
                var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                var jsonData = JsonConvert.SerializeObject(postData);
                var bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (headers != null) {
                    foreach (var header in headers) {
                        request.SetRequestHeader(header.Key, header.Value);
                    }
                }

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) {
                    Debug.LogError($"Error: {request.error}");
                    return default;
                }

                var response = request.downloadHandler.text;
                var wrapper = JsonConvert.DeserializeObject<Wrapper<T>>(response);
                if (!wrapper.Success) {
                    Debug.LogError($"Error: {wrapper.Error}");
                    return default;
                }

                return wrapper.Message ?? default;
            } catch (Exception e) {
                Debug.LogError(e);
                return default;
            }
        }

        private record Wrapper<T>(
            [JsonProperty("success")]
            bool Success,
            [JsonProperty("error")]
            string Error,
            [JsonProperty("message")]
            T Message
        )
        {
            public bool Success { get; } = Success;
            public string Error { get; } = Error;
            public T Message { get; } = Message;
        }
    }
}