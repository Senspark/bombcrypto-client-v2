using System;

namespace Services.Server.Exceptions {
    public class PassCodeException : Exception {
        public PassCodeException() : base("Invalid Passcode") {
        }
    }
}