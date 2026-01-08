using System;
using Newtonsoft.Json;

namespace Story.Strategy.Level
{
    [Serializable]
    public class LevelStrategy
    {
        [JsonProperty("level")] public int Level { get; }
        [JsonProperty("enemies")] public int[] Enemies { get; }
        [JsonProperty("enemies-num")] public int[] EnemiesNum { get; }
        [JsonProperty("row")] public int Row;
        [JsonProperty("col")] public int Col;
        [JsonProperty("density")] public float Density;
        [JsonProperty("enemies-door-first")] public int[] EnemiesDoorFirst { get; }
        [JsonProperty("enemies-door-first-num")] public int[] EnemiesDoorFirstNum { get; }

        [JsonProperty("enemies-door-then")] public int[] EnemiesDoorThen { get; }
        [JsonProperty("enemies-door-then-num")] public int[] EnemiesDoorThenNum { get; }

        public int GetTotalEnemiesNum() {
            var total = 0;
            for (var i = 0; i < EnemiesNum.Length; i++) {
                total += EnemiesNum[i];
            }
            return total;
        }

        [JsonConstructor]
        public LevelStrategy(int level,
            int[] enemies,
            int[] enemies_num,
            int row,
            int col,
            float density,
            int[] enemies_door_first,
            int[] enemies_door_first_num,
            int[] enemies_door_then,
            int[] enemies_door_then_num)
        {
            Level = level;
            Enemies = enemies;
            EnemiesNum = enemies_num;
            Row = row;
            Col = col;
            Density = density;
            EnemiesDoorFirst = enemies_door_first;
            EnemiesDoorFirstNum = enemies_door_first_num;
            EnemiesDoorThen = enemies_door_then;
            EnemiesDoorThenNum = enemies_door_then_num;
        }
    }
}