using Newtonsoft.Json;

namespace Senspark.Iap.Validators {
    public class ValidateResult {
        public readonly bool valid;
        public readonly bool isTest;
        public readonly string errorMessage;

        [JsonConstructor]
        public ValidateResult(
            [JsonProperty("valid")] bool valid,
            [JsonProperty("isTest")] bool isTest,
            [JsonProperty("error")] string error
        ) {
            this.valid = valid;
            this.isTest = isTest;
            errorMessage = error;
        }
    }
}