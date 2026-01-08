using Newtonsoft.Json;

namespace Utils {
    public struct EcEsSendExtensionRequestResult {
        [JsonProperty("ec")]
        public int Code;

        [JsonProperty("es")]
        public string Message;
    }
    
    public struct EcEsSendExtensionRequestResult<T> {
        [JsonProperty("ec")]
        public int Code;

        [JsonProperty("data")]
        public T Data;

        [JsonProperty("es")]
        public string Message;
    }
}