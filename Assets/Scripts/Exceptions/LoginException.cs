using System;

namespace Exceptions {
    public class LoginException : Exception {
        public readonly ErrorType Error;

        public LoginException(ErrorType error, string message) : base(message) {
            Error = error;
        }

        public enum ErrorType {
            WrongVersion,
            CannotReconnect,
            AlreadyLogin,
            Other,
            LoadThMode,
            KickByOtherDevice,
        }
    }
}