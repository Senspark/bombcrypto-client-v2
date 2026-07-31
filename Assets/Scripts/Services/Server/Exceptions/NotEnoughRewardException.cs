using System;

namespace Services.Server.Exceptions {
    // ec 1019. Raised inside Postgres, so its text is flattened to "Handler server error" on the way out
    // and only the code survives — callers must supply their own copy rather than showing Message.
    public class NotEnoughRewardException : Exception {
        public NotEnoughRewardException(string message) : base(message) {
        }
    }
}
