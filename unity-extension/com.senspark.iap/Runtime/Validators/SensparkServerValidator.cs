using System;
using System.Text;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Senspark.Security;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;

namespace Senspark.Iap.Validators {
    public class SensparkServerValidator : IServerValidator {
        private const string URL_IAP = "https://api.bombcrypto.io/gateway/api/verify-purchase-product-v2";
        private const string URL_SUBS = "https://api.bombcrypto.io/gateway/api/verify-purchase-subscription";
        private const string URL_GOOGLE = "https://google.com/";
        private const int CodeSuccess = 200;
        private const int CodeFailure = 400;
        private const int CodeTimeOut = -1;

        private const string JWT =
            "552D55626870494862567477374834654373694343764767676D376A5F484D69444732514C6470503062662E395A444F3463544F3455544E32456A4F69515859704A434C694D53656C354864504245522D426C627468565831455353794E7A49553155507941464E6B426E565A4A694F696B585A724A79652E394A435658706B493649436335526E497349694E31497A55494A694F69634762684A7965";

        public SensparkServerValidator() {
            // Keep this to avoid IL2CPP stripping
            _ = new ApiIapResponse(0, new ApiIapResponse.Message(false, false, null));
            _ = new ApiSubsResponse(0, new ApiSubsResponse.Message(null, 0, 0));
        }

        public async UniTask<ValidateResult> Validate(
            string productId,
            PurchaseToken purchaseToken,
            string appPackageName,
            ProductType productType
        ) {
            if (string.IsNullOrWhiteSpace(productId) || string.IsNullOrWhiteSpace(purchaseToken.Token)) {
                return new ValidateResult(false, false, "Invalid data");
            }

            var hasInternet = await ValidateInternetConnection();
            if (!hasInternet) {
                return new ValidateResult(false, false, "No internet connection");
            }

            var body = new JObject {
                { "redirect", "google-play" },
                { "productId", productId },
                { "token", purchaseToken.Token },
                { "packageName", appPackageName },
            }.ToString();
            FastLog.Info($"[Senspark] {Application.identifier} {productId} {purchaseToken}");
            var jwt = Encryption.DeObfuscate(JWT);
            if (productType == ProductType.Subscription) {
                return await ValidateSubs(body, jwt);
            }
            return await ValidateIap(body, jwt);
        }

        private static async UniTask<bool> ValidateInternetConnection() {
            var (resCode, _) = await GetRequest(URL_GOOGLE, null, 10f);
            return resCode == CodeSuccess;
        }

        private static async UniTask<ValidateResult> ValidateIap(string body, string jwt) {
            try {
                var (resCode, resStr) = await PostRequest(URL_IAP, body, jwt);
                if (resCode != CodeSuccess) {
                    return ConsiderWhatHappenIfFail(resCode, resStr);
                }

                var sv2 = JsonConvert.DeserializeObject<ApiIapResponse>(resStr);
                return new ValidateResult(sv2.msg.isPurchased, sv2.msg.isTest, string.Empty);
            } catch (Exception e) {
                FastLog.Error(e.Message);
                return new ValidateResult(false, false, string.Empty);
            }
        }

        private static async UniTask<ValidateResult> ValidateSubs(string body, string jwt) {
            try {
                var (resCode, resStr) = await PostRequest(URL_SUBS, body, jwt);
                if (resCode != CodeSuccess) {
                    return ConsiderWhatHappenIfFail(resCode, resStr);
                }

                var sv2 = JsonConvert.DeserializeObject<ApiSubsResponse>(resStr);
                var validState = sv2.msg.state is ApiSubsResponse.State.Active or ApiSubsResponse.State.Pending;
                return new ValidateResult(validState, false, string.Empty);
            } catch (Exception e) {
                FastLog.Error(e.Message);
                return new ValidateResult(false, false, string.Empty);
            }
        }

        private static ValidateResult ConsiderWhatHappenIfFail(long responseCode, string responseMessage) {
            return responseCode == CodeTimeOut
                ?
                // Vì mục đích chính của việc server validate chỉ là chống cheat
                // Cho nên nếu timeout thì thôi tạm chấp nhận cho pass
                new ValidateResult(true, true, responseMessage)
                : new ValidateResult(false, false, responseMessage);
        }

        private static async UniTask<(long, string)> GetRequest(string url, string jwt, float timeout = 10f) {
            using var req = UnityWebRequest.Get(url);
            req.timeout = (int) (timeout * 1000);
            req.SetRequestHeader("Authorization", $"Bearer {jwt}");
            var res = await AwaitWebResponse(req, timeout);
            return res;
        }

        private static async UniTask<(long, string)> PostRequest(string url, string jsonBody, string jwt,
            float timeout = 10f) {
            var req = new UnityWebRequest(url, "POST");
            req.timeout = (int) (timeout * 1000);
            var rawData = Encoding.UTF8.GetBytes(jsonBody);
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {jwt}");
            req.uploadHandler = new UploadHandlerRaw(rawData);
            req.downloadHandler = new DownloadHandlerBuffer();
            var res = await AwaitWebResponse(req, timeout);
            return res;
        }

        private static async Task<(long, string)> AwaitWebResponse(UnityWebRequest req, float timeout) {
            var startTime = Time.time;
            try {
                await req.SendWebRequest();
                var endTime = Time.time;
                var isSuccess = req.result == UnityWebRequest.Result.Success;
                var isTimeOut = endTime - startTime >= timeout;
                return (isSuccess, isTimeOut) switch {
                    (false, true) => (CodeTimeOut, "Time out"),
                    _ => (req.responseCode, req.downloadHandler.text),
                };
            } catch (UnityWebRequestException e) {
                return (e.ResponseCode, e.Error);
            } catch (Exception e) {
                FastLog.Error(e.Message);
                return (CodeFailure, "Unknown error");
            }
        }
    }

    public class ApiSubsResponse {
        public readonly int code;
        public readonly Message msg;

        [JsonConstructor]
        public ApiSubsResponse(
            [JsonProperty("statusCode")] int code,
            [JsonProperty("message")] Message msg
        ) {
            this.code = code;
            this.msg = msg;
        }

        public class Message {
            public readonly State state;
            public readonly DateTime startTime;
            public readonly DateTime endTime;

            [JsonConstructor]
            public Message(
                [JsonProperty("purchased")] string state,
                [JsonProperty("is_test")] long startTime,
                [JsonProperty("region")] long endTime
            ) {
                this.state = state switch {
                    "pending" => State.Pending,
                    "active" => State.Active,
                    "expired" => State.Expired,
                    _ => State.Invalid,
                };
                this.startTime = TimeUtils.ConvertEpochSecondsToLocalDateTime(startTime);
                this.endTime = TimeUtils.ConvertEpochSecondsToLocalDateTime(endTime);
            }
        }

        public enum State {
            Invalid,
            Pending,
            Active,
            Expired
        }
    }

    public class ApiIapResponse {
        public readonly int code;
        public readonly Message msg;

        [JsonConstructor]
        public ApiIapResponse(
            [JsonProperty("statusCode")] int code,
            [JsonProperty("message")] Message msg
        ) {
            this.code = code;
            this.msg = msg;
        }

        public class Message {
            public readonly bool isPurchased;
            public readonly bool isTest;
            public readonly string region;

            [JsonConstructor]
            public Message(
                [JsonProperty("purchased")] bool isPurchased,
                [JsonProperty("is_test")] bool isTest,
                [JsonProperty("region")] string region
            ) {
                this.isPurchased = isPurchased;
                this.isTest = isTest;
                this.region = region;
            }
        }
    }
}