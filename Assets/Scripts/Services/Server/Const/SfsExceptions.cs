using System;

using Services.Server.Exceptions;

public static partial class SFSDefine {
    public static class SfsExceptions {
        public static Exception GetException(int ec, string msg) {
            return ec switch {
                1019 => new NotEnoughRewardException(msg),
                9999 => new PassCodeException(),
                _ => new Exception(msg)
            };
        }
    }
}