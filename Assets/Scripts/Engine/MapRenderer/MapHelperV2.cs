using System;
using System.Collections.Generic;

using App;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Info;

using Engine.Entities;
using Engine.Manager;
using Engine.Utils;

using SuperTiled2Unity;

using UnityEngine;

using MapInfo = Engine.Manager.MapInfo;

namespace Engine.MapRenderer {
    public static class MapHelperV2 {
        public static bool IsUseMapPveV2(int stage, int level) {
#if TestMapPveV2
            return !AppConfig.IsProduction && stage == 1 && level <= 5;
#else
            return false;
#endif
        }

        public static MapData[,] GetMapDataFromStoryMapDetail(IStoryMapDetail details) {
            var col = details.Col;
            var row = details.Row;

            var mapDatas = new MapData[col, row];
            for (var i = 0; i < col; i++) {
                for (var j = 0; j < row; j++) {
                    mapDatas[i, j] = null;
                }
            }
            for (var k = 0; k < details.Positions.Length; k++) {
                var mapData = new MapData(EntityType.Brick, 0, 0);
                mapDatas[details.Positions[k].x, details.Positions[k].y] = mapData;
            }
            return mapDatas;
        }

        public static MapData[,] GetMapDataFromPvpMapDetail(IMapInfo details, MapInfo mapInfo) {
            var mapsData = new MapData[mapInfo.Col, mapInfo.Row];
            for (var i = 0; i < mapInfo.Col; i++) {
                for (var j = 0; j < mapInfo.Row; j++) {
                    mapsData[i, j] = null;
                }
            }
            // TODO: update late
            if (mapInfo.ExtendData == null) {
                foreach (var block in details.Blocks) {
                    if (block.Position.x < 0 || block.Position.x >= mapInfo.Col || block.Position.y < 0 ||
                        block.Position.y >= mapInfo.Row) {
                        continue;
                    }
                    var mapData = new MapData(MapHelper.ConvertToEntityType(block.BlockType), 0, 0);
                    mapsData[block.Position.x, block.Position.y] = mapData;
                }
            } else {
                foreach (var block in mapInfo.ExtendData.Data.blocks) {
                    if (block.type == SuperBlockType.Empty || block.type == SuperBlockType.Hard) {
                        continue;
                    }
                    if (block.type == SuperBlockType.Soft) {
                        var mapData = new MapData(EntityType.Brick, 0, 0);
                        mapsData[block.x, block.y] = mapData;
                        continue;
                    }
                    throw new Exception($"Could not find block type: {block.type}");
                }
            }
            return mapsData;
        }

        public static int GetMatrixHashCode(int x, int y) {
            return x << 16 | y;
        }

        public static Vector2Int GetVector2ByHashCode(int value) {
            return new Vector2Int(value >> 16, value & ((1 << 16) - 1));
        }

        public static IFallingBlockInfo[] CreateDropListFullMap(int col, int row) {
            var startX = 0;
            var startY = 0;
            var endX = col - 1;
            var endY = row - 1;

            var delayDrop = 1000L;
            var delayDelta = 140L;

            var fallList = new List<IFallingBlockInfo>();
            var timestamp = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond);
            IFallingBlockInfo CreateFallingBlock(int i, int j, long timeStamp) {
                return new FallingBlockInfo((int) timeStamp, i, j);
            }
            while (startX <= endX && startY <= endY) {
                for (var i = startX; i <= endX; i++) {
                    delayDrop += delayDelta;
                    var fall = CreateFallingBlock(i, endY, timestamp + delayDrop);
                    fallList.Add(fall);
                }
                endY--;
                for (var i = endY; i >= startY; i--) {
                    delayDrop += delayDelta;
                    var fall = CreateFallingBlock(endX, i, timestamp + delayDrop);
                    fallList.Add(fall);
                }
                endX--;
                for (var i = endX; i >= startX; i--) {
                    delayDrop += delayDelta;
                    var fall = CreateFallingBlock(i, startY, timestamp + delayDrop);
                    fallList.Add(fall);
                }
                startY++;
                for (var i = startY; i <= endY; i++) {
                    delayDrop += delayDelta;
                    var fall = CreateFallingBlock(startX, i, timestamp + delayDrop);
                    fallList.Add(fall);
                }
                startX++;
            }
            return fallList.ToArray();
        }
    }
}