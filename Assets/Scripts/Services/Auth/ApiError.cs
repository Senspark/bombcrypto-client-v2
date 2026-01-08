using System;

using Newtonsoft.Json;

namespace App {
    public class ApiError {
        public readonly int StatusCode;
        public readonly string Message;

        private ApiError(int statusCode, string message) {
            StatusCode = statusCode;
            Message = message;
        }

        public static ApiError Parse(string json) {
            if (TryParseCase1(json, out var e1)) {
                return new ApiError(e1.statusCode, e1.message[0]);
            }
            if (TryParseCase2(json, out var e2)) {
                return new ApiError(e2.statusCode, e2.message.message);
            }
            return new ApiError(0, string.Empty);
        }
            
        private static bool TryParseCase1(string json, out ErrorCase1 err) {
            try {
                err = JsonConvert.DeserializeObject<ErrorCase1>(json);
                return true;
            } catch (Exception _) {
                err = null;
                return false;
            }
        }
            
        private static bool TryParseCase2(string json, out ErrorCase2 err) {
            try {
                err = JsonConvert.DeserializeObject<ErrorCase2>(json);
                return true;
            } catch (Exception _) {
                err = null;
                return false;
            }
        }
            
        [Serializable]
        private class ErrorCase1
        {
            public int statusCode;
            public string[] message;
        }

        [Serializable]
        private class ErrorCase2
        {
            public int statusCode;
            public ErrorDetail message;
        }

        [Serializable]
        private class ErrorDetail {
            public string message;
        }
    }
}