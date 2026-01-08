using System.Collections.Generic;

using Newtonsoft.Json;

using UnityEngine;

namespace PvpSchedule {
    public static class TournamentUtils {
        private const string JoinedListKey = "Joined_Match_List";

        public static List<string> LoadJoinedList() {
            var listData = PlayerPrefs.GetString(JoinedListKey, "");
            return string.IsNullOrEmpty(listData) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(listData);
        }
        
        public static void InsertToJoinedList(string matchId) {
            var list = LoadJoinedList();
            if (list.Contains(matchId)) {
                return;
            }
            list.Add(matchId);
            PlayerPrefs.SetString(JoinedListKey, JsonConvert.SerializeObject(list));
            PlayerPrefs.Save();
        }
    }
}