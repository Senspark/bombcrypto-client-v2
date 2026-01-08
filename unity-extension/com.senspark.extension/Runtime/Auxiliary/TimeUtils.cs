using System;

namespace Senspark {
    public static class TimeUtils {
        public static long ConvertDateTimeToEpochSeconds(DateTime dateTime) {
            var offset = new DateTimeOffset(dateTime);
            var epoch = offset.ToUnixTimeSeconds();
            return epoch;
        }
        
        public static long ConvertDateTimeToEpochMilliSeconds(DateTime dateTime) {
            var offset = new DateTimeOffset(dateTime);
            var epoch = offset.ToUnixTimeMilliseconds();
            return epoch;
        }

        public static DateTime ConvertEpochSecondsToLocalDateTime(long epoch) {
            var offset = DateTimeOffset.FromUnixTimeSeconds(epoch);
            var localDateTime = offset.LocalDateTime;
            return localDateTime;
        }
    }
}