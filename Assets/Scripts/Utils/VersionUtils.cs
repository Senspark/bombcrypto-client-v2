using System;

namespace Utils {
    public static class VersionUtils {
        public static int Parse(DateTime dateTime) {
            return dateTime.Hour + dateTime.Day * 100+ dateTime.Month * 10000 + dateTime.Year % 100 * 1000000;
        }
    }
}