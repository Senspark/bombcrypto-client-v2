using System;
using System.Collections.Generic;

using UnityEngine;

namespace Scenes.MarketplaceScene.Scripts {
    public static class MarketUtils {
        // Define constants for better readability and maintenance
        private const int SecondsPerDay = 86400; // 24*60*60
        private const long MillisecondsPerDay = 86400000L; // 24*60*60*1000
    
        private static readonly Dictionary<int, string> CommonExpirations = new Dictionary<int, string> {
            { 1, "1"},
            { 7, "7" },
            { 30, "30" }
        };
    
        public static string ExpirationToDays(long expiration) {
            // Determine if value is in seconds or milliseconds and convert to days
            int days;
            if (expiration  >= SecondsPerDay * 1000) {
                days = (int)(expiration / MillisecondsPerDay);
            } else if (expiration >= SecondsPerDay) {
                days = (int)(expiration / SecondsPerDay);
            } else {
                Debug.LogWarning("Expiration value is too small to determine if it's in seconds or milliseconds.");
                return "Invalid";
            }

            if (days <= 0) {
                return "forever";
            }
        
            // Return the string representation if it's a common value, otherwise the actual days
            return CommonExpirations.TryGetValue(days, out string result) 
                ? result 
                : days.ToString();
        }
    }
}
