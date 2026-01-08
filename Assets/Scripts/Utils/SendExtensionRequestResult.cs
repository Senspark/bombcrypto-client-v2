using Newtonsoft.Json;

namespace Utils {
    public struct SendExtensionRequestResult {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;
    }
}

namespace Utils {
    public struct SendExtensionRequestResult<T> {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("data")]
        public T Data;

        [JsonProperty("message")]
        public string Message;
    }
}