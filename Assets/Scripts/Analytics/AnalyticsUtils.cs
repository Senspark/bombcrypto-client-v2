using System;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;

using UnityEngine;

namespace Analytics {
    public static class AnalyticsUtils {
        private const string DayFormat = "yyyy/MM/dd";

        public static int GetRetentionDays() {
            const string retentionKey = "Analytics_Retention_FirstLoginDay";
            var firstDate = DateTime.Today;
            if (!PlayerPrefs.HasKey(retentionKey)) {
                PlayerPrefs.SetString(retentionKey, firstDate.ToString(DayFormat));
                PlayerPrefs.Save();
            } else {
                var k = PlayerPrefs.GetString(retentionKey);
                firstDate = ParseDateFromString(k);
            }
            var days = (int) (DateTime.Today - firstDate).TotalDays;
            return days;
        }

        public static int GetPvpMatchPlay() {
            const string keyMatchCount = "Analytics_PvpMatchCount";
            var matchCount = 0;
            if (PlayerPrefs.HasKey(keyMatchCount)) {
                matchCount = PlayerPrefs.GetInt(keyMatchCount);
            }
            PlayerPrefs.SetInt(keyMatchCount, ++matchCount);
            PlayerPrefs.Save();
            return matchCount;
        }
        
        public static List<string> LoadTrackedEvent(string prefKey) {
            List<string> tracked = null;
            try {
                var trackedData = PlayerPrefs.GetString(prefKey);
                tracked = JsonConvert.DeserializeObject<List<string>>(trackedData);
            } catch (Exception _) {
                // do nothing
            } finally {
                tracked ??= new List<string>();
            }
            return tracked;
        }

        public static void SaveTrackedEvent(string prefKey, List<string> data) {
            PlayerPrefs.SetString(prefKey, JsonConvert.SerializeObject(data));
            PlayerPrefs.Save();
        }

        public static bool CanTrackActiveUserToday(string prefKey) {
            var today = DateTime.Today;
            if (PlayerPrefs.HasKey(prefKey)) {
                var d = PlayerPrefs.GetString(prefKey);
                var trackedDay = ParseDateFromString(d);
                if (trackedDay == today) {
                    return false;
                }
            }
            PlayerPrefs.SetString(prefKey, today.ToString(DayFormat));
            PlayerPrefs.Save();
            return true;
        }

        private static DateTime ParseDateFromString(string str) {
            return DateTime.TryParseExact(str, DayFormat, null, DateTimeStyles.None, out var dt) ? dt : DateTime.MinValue;
        }
    }
}