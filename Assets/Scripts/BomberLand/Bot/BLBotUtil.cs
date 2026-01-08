using System;
using System.Collections.Generic;

using Engine.Manager;
using Engine.MapRenderer;
using Engine.Utils;

using UnityEngine;

using Random = UnityEngine.Random;

namespace BomberLand.Bot {
    public interface IBLBotUtil {
        List<Vector2Int> PathToNearestBrick(Vector2Int src);
        List<Vector2Int> PathToNearestSafeZone(Vector2Int src, ref bool isStandOnDangerous);
        List<Vector2Int> PathToNearestItem(Vector2Int src);
        List<Vector2Int> FindBombFarthest(Vector2Int src);
        bool CheckPathIsSafeWithNewBomb(List<Vector2Int> paths);
        List<Vector2Int> PathToNearestEnemy(Vector2Int from, Vector2Int to);
    }

    public class BLBotUtil : IBLBotUtil {
        private readonly IMapManager _mapManager;
        private readonly int _col;
        private readonly int _row;
        private readonly int[,] _grid;
        private readonly BFS.BFSNode[,] _nodes;

        public BLBotUtil(IMapManager mapManager) {
            _mapManager = mapManager;
            _col = _mapManager.Col;
            _row = _mapManager.Row;
            _grid = new int[_col, _row];
            _nodes = new BFS.BFSNode[_col, _row];
        }

        public List<Vector2Int> PathToNearestBrick(Vector2Int src) {
            IDictionary<int, bool> dangerZone = new Dictionary<int, bool>();
            GetDangerZone(ref dangerZone);
            if (dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(src.x, src.y))) {
                // User stand on dangerZone
                return null;
            }
            var neighbours = FloodFill.FindNeighboursNoCheck(_col, _row, src);
            foreach (var it in neighbours) {
                if (dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(it.x, it.y))) {
                    // User stand near dangerZone
                    return null;
                }
            }
            var ran = new List<List<Vector2Int>>();
            var grid = GetMapGrid();
            var map = _mapManager.GetTileTypeMap();
            const int offset = 4;
            for (var i = Mathf.Max(0, src.x - offset); i < _col && i < src.x + offset; i++) {
                for (var j = Mathf.Max(0, src.y - offset); j < _row && j < src.y + offset; j++) {
                    if (map[i, j] is not TileType.Brick) {
                        continue;
                    }
                    foreach (var it in FloodFill.FindNeighbours(grid, new Vector2Int(i, j))) {
                        if (dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(it.x, it.y))) {
                            continue;
                        }
                        var r = ShortestPath(src, it);
                        if (r.Count <= 0) {
                            continue;
                        }
                        if (!CheckPathIsSafe(r, dangerZone)) {
                            continue;
                        }
                        ran.Add(r);
                    }
                }
            }
            return ran.Count <= 0 ? null : ran[Random.Range(0, ran.Count)];
        }

        public List<Vector2Int> PathToNearestSafeZone(Vector2Int src, ref bool isStandOnDangerous) {
            IDictionary<int, bool> dangerZone = new Dictionary<int, bool>();
            GetDangerZone(ref dangerZone);
            isStandOnDangerous = dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(src.x, src.y));
            if (isStandOnDangerous) {
                // User stand on dangerZone
                // Tìm những vị trí xung quanh Dangerous có thể di chuyển 
                var ran = new List<List<Vector2Int>>();
                var neighbourMove = 0;
                var timestamp = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond);
                foreach (var v in dangerZone.Keys) {
                    var v2 = MapHelperV2.GetVector2ByHashCode(v);
                    if (v2 == src) {
                        continue;
                    }
                    var grid = new int[_col, _row];
                    for (var i = 0; i < _col; i++) {
                        for (var j = 0; j < _row; j++) {
                            var isCanMove = IsEmpty(i, j)
                                            && dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(i, j))
                                            && !IsWillExplode(i, j, timestamp);
                            grid[i, j] = isCanMove ? 0 : 1;
                        }
                    }
                    grid[src.x, src.y] = 0;
                    var p = ShortestPathStepFast(grid, src, v2);
                    if (p.Count == 0) {
                        continue;
                    }
                    var neighbours = FloodFill.FindNeighboursNoCheck(_col, _row, new Vector2Int(v2.x, v2.y));
                    bool IsCanMove(Vector2Int pos) {
                        if (!IsEmpty(pos.x, pos.y)) {
                            return false;
                        }
                        if (dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(pos.x, pos.y))) {
                            return false;
                        }
                        return true;
                    }
                    foreach (var it in neighbours) {
                        if (!IsCanMove(it)) {
                            continue;
                        }
                        // Ưu tiên di chuyển tới vùng có nhiều khoảng trống
                        var nm = 0;
                        foreach (var p1 in FloodFill.FindNeighboursNoCheck(_col, _row, it)) {
                            if (!IsCanMove(p1)) {
                                continue;
                            }
                            foreach (var p2 in FloodFill.FindNeighboursNoCheck(_col, _row, p1)) {
                                if (!IsCanMove(p2)) {
                                    continue;
                                }
                                nm++;
                            }
                        }
                        if (nm > neighbourMove) {
                            neighbourMove = nm;
                            ran = new List<List<Vector2Int>>();
                        }
                        var r = new List<Vector2Int> { it };
                        r.AddRange(p);
                        ran.Add(r);
                    }
                }
                if (ran.Count > 0) {
                    ran.Sort((a, b) => a.Count - b.Count);
                    return ran[Random.Range(0, (int) (0.5f * ran.Count))];
                }
                // Set lại vị trí hiện tại user là safe
                dangerZone.Remove(MapHelperV2.GetMatrixHashCode(src.x, src.y));
            }
            // find pathMove
            {
                var offset = 4;
                var ranAll = new List<List<Vector2Int>>();
                var ranCanMove = new List<List<Vector2Int>>();
                for (var i = Mathf.Max(0, src.x - offset); i < _col && i < src.x + offset; i++) {
                    for (var j = Mathf.Max(0, src.y - offset); j < _row && j < src.y + offset; j++) {
                        if (i == src.x && j == src.y) {
                            continue;
                        }
                        if (!IsEmpty(i, j) || dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(i, j))) {
                            continue;
                        }
                        var r = ShortestPath(src, new Vector2Int(i, j));
                        if (r.Count <= 0) {
                            continue;
                        }
                        ranAll.Add(r);
                        if (CheckPathIsSafe(r, dangerZone)) {
                            ranCanMove.Add(r);
                        }
                    }
                }
                if (ranCanMove.Count > 0) {
                    // Bot tìm được đường an toàn
                    return ranCanMove[Random.Range(0, ranCanMove.Count)];
                }

                if (ranAll.Count > 0) {
                    // Bot random vào những vùng có di chuyển tới
                    return ranAll[Random.Range(0, ranCanMove.Count)];
                }

                var timestamp = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond);
                if (IsWillExplode(src.x, src.y, timestamp)) {
                    var neighbours = FloodFill.FindNeighboursNoCheck(_col, _row, src);
                    foreach (var p in neighbours) {
                        if (!IsEmpty(p.x, p.y) || IsWillExplode(p.x, p.y, timestamp)) {
                            continue;
                        }
                        ranAll.Add(new List<Vector2Int>() { p });
                    }
                    if (ranAll.Count > 0) {
                        // Bot random vào vị trí di chuyển được
                        return ranAll[Random.Range(0, ranAll.Count)];
                    }
                    // Bot stuck
                }
            }
            return null;
        }

        public List<Vector2Int> PathToNearestItem(Vector2Int src) {
            IDictionary<int, bool> dangerZone = new Dictionary<int, bool>();
            GetDangerZone(ref dangerZone);
            if (dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(src.x, src.y))) {
                // User Stand on Bomb
                return null;
            }
            var ran = new List<List<Vector2Int>>();
            const int offset = 4;
            var map = _mapManager.GetTileTypeMap();
            for (var i = Mathf.Max(0, src.x - offset); i < _col && i < src.x + offset; i++) {
                for (var j = Mathf.Max(0, src.y - offset); j < _row && j < src.y + offset; j++) {
                    var isItemPositive = map[i, j] == TileType.Item;
                    if (!isItemPositive) {
                        continue;
                    }
                    if (dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(i, j))) {
                        continue;
                    }
                    var r = ShortestPath(src, new Vector2Int(i, j));
                    if (r.Count <= 0) {
                        continue;
                    }
                    if (!CheckPathIsSafe(r, dangerZone)) {
                        continue;
                    }
                    ran.Add(r);
                }
            }
            return ran.Count <= 0 ? null : ran[Random.Range(0, ran.Count)];
        }

        public List<Vector2Int> FindBombFarthest(Vector2Int src) {
            IDictionary<int, bool> dangerZone = new Dictionary<int, bool>();
            GetDangerZone(ref dangerZone);
            if (dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(src.x, src.y))) {
                // User Stand on Bomb
                return null;
            }
            var ran = new List<List<Vector2Int>>();
            const int offset = 4;
            var check = 1;
            for (var i = Mathf.Max(0, src.x - offset); i < _col && i < src.x + offset; i++) {
                for (var j = Mathf.Max(0, src.y - offset); j < _row && j < src.y + offset; j++) {
                    if (i == src.x && j == src.y) {
                        continue;
                    }
                    if (!dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(i, j))) {
                        continue;
                    }
                    var r = ShortestPath(src, new Vector2Int(i, j));
                    if (r.Count <= 0) {
                        continue;
                    }
                    if (r.Count > check) {
                        check = r.Count;
                        ran.Clear();
                    }
                    ran.Add(r);
                }
            }
            return ran.Count <= 0 ? null : ran[Random.Range(0, ran.Count)];
        }

        public bool CheckPathIsSafeWithNewBomb(List<Vector2Int> paths) {
            var timestamp = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond);
            for (var idx = 0; idx < paths.Count; idx++) {
                var check = paths[idx];
                if (IsHasBomb(check.x, check.y, timestamp) ||
                    IsHasWallDrop(check.x, check.y, timestamp)) {
                    return false;
                }
            }
            return true;
        }

        public List<Vector2Int> PathToNearestEnemy(Vector2Int from, Vector2Int to) {
            var ran = new List<List<Vector2Int>>();
            IDictionary<int, bool> dangerZone = new Dictionary<int, bool>();
            GetDangerZone(ref dangerZone);
            if (dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(from.x, from.y))) {
                // User stand on dangerZone
                return null;
            }
            const int offset = 2;
            for (var i = Mathf.Max(0, to.x - offset); i < _col && i < to.x + offset; i++) {
                for (var j = Mathf.Max(0, to.y - offset); j < _row && j < to.y + offset; j++) {
                    if (!IsEmpty(i, j) || dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(i, j))) {
                        continue;
                    }
                    var r = ShortestPath(from, new Vector2Int(i, j));
                    if (r.Count <= 0) {
                        continue;
                    }
                    if (!CheckPathIsSafe(r, dangerZone)) {
                        continue;
                    }
                    ran.Add(r);
                }
            }
            return ran.Count <= 0 ? null : ran[Random.Range(0, ran.Count)];
        }

        private int[,] GetMapGrid() {
            var grid = new int[_col, _row];
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    var isEmpty = IsEmpty(i, j);
                    grid[i, j] = isEmpty ? 0 : 1;
                }
            }
            return grid;
        }

        private bool IsHasWallDrop(int i, int j, long timestamp) {
            var timeWallDrop = _mapManager.TimeWallDrop(i, j);
            return timeWallDrop > 0 && timestamp >= timeWallDrop - 2000;
        }

        private bool IsHasBomb(int i, int j, long timestamp) {
            var timeExplode = _mapManager.TimePlantBomb(i, j) + 3000;
            return timestamp <= timeExplode + 200;
        }

        private bool IsWillExplode(int i, int j, long timestamp) {
            var timeExplode = _mapManager.TimePlantBomb(i, j) + 3000;
            var isExplode = timestamp >= timeExplode - 200 && timestamp <= timeExplode + 200;
            return isExplode;
        }

        private bool IsEmpty(int i, int j) {
            return _mapManager.IsEmpty(i, j);
        }

        private void GetDangerZone(ref IDictionary<int, bool> cells) {
            var timestamp = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond);
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    if (IsHasWallDrop(i, j, timestamp)
                        || IsHasBomb(i, j, timestamp)) {
                        cells[MapHelperV2.GetMatrixHashCode(i, j)] = true;
                    }
                }
            }
        }

        private List<Vector2Int> ShortestPath(Vector2Int source, Vector2Int destination) {
            var timestamp = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond);
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    if (i == source.x && j == source.y) {
                        // set default source is can move
                        _grid[i, j] = 0;
                        continue;
                    }
                    var isEmpty = IsEmpty(i, j);
                    var isDropWall = IsHasWallDrop(i, j, timestamp);
                    _grid[i, j] = isEmpty && !isDropWall && !IsHasBomb(i, j, timestamp) ? 0 : 1;
                }
            }
            return ShortestPathStepFast(_grid, source, destination);
        }

        private List<Vector2Int> ShortestPathStepFast(int[,] grid, Vector2Int source, Vector2Int destination) {
            var result = BFS.ShortestPathBinaryMatrix(grid, _nodes, source, destination);
            var path = new List<Vector2Int>();
            if (result != null) {
                var location = result.location;
                while (location != BFS.NullLocation) {
                    path.Add(location);
                    location = _nodes[location.x, location.y].prevLocation;
                }
            }
            return path;
        }

        private bool CheckPathIsSafe(List<Vector2Int> paths, IDictionary<int, bool> dangerZone) {
            if (dangerZone == null) {
                dangerZone = new Dictionary<int, bool>();
                GetDangerZone(ref dangerZone);
            }
            for (var idx = 0; idx < paths.Count - 1; idx++) {
                var from = paths[idx];
                var to = paths[idx + 1];
                if (from.x == to.x) {
                    for (var y = Mathf.Min(from.y, to.y); y <= Mathf.Max(from.y, to.y); y++) {
                        if (dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(from.x, y))) {
                            return false;
                        }
                    }
                } else {
                    for (var x = Mathf.Min(from.x, to.x); x <= Mathf.Max(from.x, to.x); x++) {
                        if (dangerZone.ContainsKey(MapHelperV2.GetMatrixHashCode(x, from.y))) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}