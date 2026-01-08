using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using UnityEngine;

public static class HttpClientHelper
{
    private static readonly HttpClient Client = new HttpClient();

    public static async UniTask<T> SendGet<T>(string url)
    {
        try
        {
            var response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var wrapper = JsonConvert.DeserializeObject<Wrapper<T>>(responseBody);
            if (!wrapper.Success)
            {
                Debug.LogError($"Error: {wrapper.Error}");
                return default;
            }

            return wrapper.Message ?? default;
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Request error: {e.Message}");
            return default;
        }
    }

    public static async UniTask<T> SendPost<T>(string url, object postData, Dictionary<string, string> headers = null)
    {
        try
        {
            var jsonData = JsonConvert.SerializeObject(postData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var wrapper = JsonConvert.DeserializeObject<Wrapper<T>>(responseBody);
            if (!wrapper.Success)
            {
                Debug.LogError($"Error: {wrapper.Error}");
                return default;
            }

            return wrapper.Message ?? default;
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Request error: {e.Message}");
            return default;
        }
    }

    private record Wrapper<T>(
        [JsonProperty("success")] bool Success,
        [JsonProperty("error")] string Error,
        [JsonProperty("message")] T Message
    )
    {
        public bool Success { get; } = Success;
        public string Error { get; } = Error;
        public T Message { get; } = Message;
    }
}