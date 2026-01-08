using System;

using App;

using Sfs2X.Entities.Data;

namespace Exceptions {
    public class BanException : Exception {
        public readonly int BanCode;
        public readonly long ExpireAt;

        public BanException(int banCode, string expireAt, string message) : base(message) {
            BanCode = banCode;
            long.TryParse(expireAt, out var expireAtLong);
            ExpireAt = expireAtLong;
        }
    }
}