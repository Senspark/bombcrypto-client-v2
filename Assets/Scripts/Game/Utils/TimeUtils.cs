using System;
using UnityEditor;

using UnityEngine;

namespace Engine.Utils
{
    public static class TimeUtils
    {
        private static float lastDelta;
        private static float lastTimeSinceStartup;

        public static float GetDelta()
        {
#if UNITY_EDITOR
            var currentTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            if (!Mathf.Approximately(lastTimeSinceStartup, 0f) &&
                !Mathf.Approximately(lastTimeSinceStartup, currentTimeSinceStartup))
            {
                lastDelta = currentTimeSinceStartup - lastTimeSinceStartup;
            }
            lastTimeSinceStartup = currentTimeSinceStartup;
            return lastDelta;
#else // UNITY_EDITOR
            return Time.deltaTime;
#endif // UNITY_EDITOR
        }
    }

    public static class Epoch {

        public static long ToEpochSeconds(this DateTime dt) {
            var offset = new DateTimeOffset(dt.ToUniversalTime()); // == DateTime.UtcNow
            return offset.ToUnixTimeSeconds();
        }
        
        public static long ToEpochMilliseconds(this DateTime dt) {
            var offset = new DateTimeOffset(dt.ToUniversalTime()); // == DateTime.UtcNow
            return offset.ToUnixTimeMilliseconds();
        }
        
        public static DateTime FromEpochMillisecondsToUtc(long milliseconds) {
            var offset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            return offset.UtcDateTime;
        }
        
        public static DateTime FromEpochMillisecondsToLocal(long milliseconds) {
            var offset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            return offset.LocalDateTime;
        }

        public static string GetTimeString(int seconds) {
            var hour = seconds / 3600;
            var minute = (seconds - (hour * 3600)) / 60;
            var second = seconds - (hour * 3600) - (minute * 60);
            return $"{hour:D2}:{minute:D2}:{second:D2}";
        }

        public static string GetTimeStringOneHour(int seconds) {
            var hour = seconds / 3600;
            var minute = (seconds - (hour * 3600)) / 60;
            var second = seconds - (hour * 3600) - (minute * 60);
            return $"{hour:D1}:{minute:D2}:{second:D2}";
        }

        public static string GetTimeStringMinuteSecond(int seconds) {
            var hour = seconds / 3600;
            var minute = (seconds - (hour * 3600)) / 60;
            var second = seconds - (hour * 3600) - (minute * 60);
            return $"{minute:D2}:{second:D2}";
        }

        public static string GetTimeStringDayHourMinute(int minute) {
            var ts = TimeSpan.FromMinutes(minute);
            return $"{ts.Days}D {ts.Hours}H {ts.Minutes}M";
        }
        
        public static string GetTimeStringHourMinuteSecond(double second) {
            var ts = TimeSpan.FromSeconds(second);
            return $"{ts.Hours}H {ts.Minutes}M {ts.Seconds}S";
        }

        public static long GetUnixTimestamp(long timeSpan) {
            var epochTicks = DateTime.UnixEpoch.Ticks;
            return (DateTime.UtcNow.Ticks - epochTicks) / timeSpan;
        }
    }
}