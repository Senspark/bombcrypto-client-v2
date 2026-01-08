using System;

namespace Services.Server.Exceptions {
    public class ErrorCodeException : Exception {
        public int ErrorCode;
        
        public ErrorCodeException(int errorCode, string message) : base(message) {
            ErrorCode = errorCode;
        }
    }
}