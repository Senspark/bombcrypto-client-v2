using System.Collections.Generic;

using BLPvpMode.Engine.Info;
using BLPvpMode.GameView;

using Newtonsoft.Json;

using UnityEngine;

namespace BLPvpMode.Test {
    public class TestFallingBlock : MonoBehaviour {
        private class TestPvpFallingBlock : IFallingBlockInfo {
            public int Timestamp { get; set; }
            public Vector2Int Position { get; set; }

            [JsonProperty("x")]
            public int X { get; set; }

            [JsonProperty("y")]
            public int Y { get; set; }
        }

        [SerializeField]
        private string jsonBlock;

        [Button]
        public void TestAddFallingWall() {
            var list = JsonConvert.DeserializeObject<TestPvpFallingBlock[]>(jsonBlock);

            var delta = -list[0].Timestamp + 1000;

            var prevTime = list[0].Timestamp;

            foreach (var iter in list) {
                iter.Position = new Vector2Int(iter.X, iter.Y);
                iter.Timestamp += delta;
                var diff = iter.Timestamp - prevTime;
                prevTime = iter.Timestamp;
            }

            GetComponentInChildren<BLevelViewPvp>().AddFallingWall(list);
        }

        [Button]
        public void TestDropWall() {
            GetComponentInChildren<BLevelViewPvp>().DropWall();
        }

        public IFallingBlockInfo[] GetFallingWall() {
            var future = 10;
            var duration = 0;

            var fallList = new List<IFallingBlockInfo>();
            for (var i = 0; i < 16; i++) {
                duration += 1;
                var j = 12;
                var fall = new TestPvpFallingBlock {
                    Timestamp = future + duration,
                    Position = new Vector2Int(i + 2, j + 1),
                };
                fallList.Add(fall);
            }

            for (var j = 11; j >= 0; j--) {
                duration += 1;
                var i = 16;
                var fall = new TestPvpFallingBlock {
                    Timestamp = future + duration,
                    Position = new Vector2Int(i + 2, j + 1),
                };
                fallList.Add(fall);
            }

            for (var i = 17; i >= 0; i--) {
                duration += 1;
                var j = 0;
                var fall = new TestPvpFallingBlock {
                    Timestamp = future + duration,
                    Position = new Vector2Int(i + 2, j + 1),
                };
                fallList.Add(fall);
            }

            return fallList.ToArray();
        }
    }
}