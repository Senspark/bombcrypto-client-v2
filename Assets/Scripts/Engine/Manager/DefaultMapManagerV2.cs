using System.Collections.Generic;

using CreativeSpore.SuperTilemapEditor;

using Engine.Components;

using UnityEngine;

using Engine.Utils;
using Engine.Entities;

using System.Linq;
using System;
using System.Security.Cryptography;

using App;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Info;

using Cysharp.Threading.Tasks;

using Engine.MapRenderer;
using Engine.Strategy.Provider;

using PvpMode.Services;

using Senspark;

using Sfs2X.Util;

using SuperTiled2Unity;

using Random = UnityEngine.Random;

namespace Engine.Manager {
    public class DefaultMapManagerV2 : IMapManager {
        private const int R_COL_THMODE = 35;
        private const int R_ROW_THMODE = 17;
        private const float TileSize = 1;
        public int TileIndex { get; }
        public bool IsEmptyBlock { get; set; }
        public int NumberOfBlock { get; set; }

        private readonly int[,] _grid;
        private readonly BFS.BFSNode[,] _nodes;

        private readonly MapInfo _mapInfo;

        private readonly IEntityManager _entityManager;
        private readonly int _col;
        private readonly int _row;
        private readonly EntityType[,] _entityUnderBrick; // for location entity in map;
        private readonly ItemType[,] _itemLocations; // for location items in map;
        private readonly Block[,] _blockLocations; // for location the block in map;
        private readonly bool[,] _aliveBlocks; // Để chứa các Block đã bị Destroyed, optimize kiểm tra block != null
        private readonly bool[,] _emptyLocations; // for get random empty location to create new enemies;
        private readonly TileType[,] _map;
        private bool _hasBlinkBricks;
        private Vector2Int _doorLocation;
        private int _maxWall;

        private List<Vector2Int> _fixedWallLocations;

        // private bool _isWallDropTakeDamage;
        private int _stageCount = 0; // stageCount thay đổi khi có map có sự thay đổi

        private readonly long[,]
            _timePlantBomb; // lưu thời gian lần cuối đặt bomb, để giúp Bot kiểm tra vùng an toàn

        private readonly SteMapRenderer _mapRenderer;
        private readonly SuperMapRender _superMapRender;
        private readonly PoolBrickHelper _poolBrickHelper;
        private readonly PoolWallDropHelper _poolWallDropHelper;
        private readonly IProviderMap _providerMap;
        private readonly IHashEntityLocation _hashEntityLocation;

        private readonly bool[,] _hadBombs;
        private IDictionary<int, bool> _markBreakBrick;

        public DefaultMapManagerV2(
            int tileIndex,
            Tileset tileSet,
            IEntityManager entityManager,
            TilemapGroup tmBackground,
            TilemapGroup tmMidground,
            TilemapGroup tmForeground,
            MapInfo mapInfo) {
            _entityManager = entityManager;
            var gameModeType = mapInfo.GameModeType;
            // Debug.Assert(gameModeType == GameModeType.StoryMode || gameModeType == GameModeType.PvpMode,
            //     "V2 map only support StoryMode, PvpMode");
            _mapInfo = mapInfo;
            // _col = mapInfo.Col;
            // _row = mapInfo.Row;
            _col = mapInfo.Col == 0 ? R_COL_THMODE : mapInfo.Col;
            _row = mapInfo.Row == 0 ? R_ROW_THMODE : mapInfo.Row;
            TileIndex = tileIndex;
            _map = new TileType[_col, _row];
            _emptyLocations = new bool[_col, _row];
            _entityUnderBrick = new EntityType[_col, _row];
            _itemLocations = new ItemType[_col, _row];
            _blockLocations = new Block[_col, _row];
            _aliveBlocks = new bool[_col, _row];
            _grid = new int[_col, _row];
            _nodes = new BFS.BFSNode[_col, _row];
            _doorLocation = new Vector2Int(-1, -1);
            _stageCount = 0;
            _timePlantBomb = new long[_col, _row];
            _hadBombs = new bool[_col, _row];
            _markBreakBrick = new Dictionary<int, bool>();
            _mapRenderer = new SteMapRenderer(
                gameModeType,
                tileSet,
                tmBackground,
                tmMidground,
                tmForeground,
                mapInfo);

            if (mapInfo.ExtendData != null) {
                var levelView = tmBackground.transform.parent;
                _superMapRender = new SuperMapRender(levelView.parent.GetComponentInChildren<SuperMap>());
            }
            _providerMap = new ProviderMap(entityManager);
            _poolBrickHelper = new PoolBrickHelper(entityManager, tileIndex);
            _poolWallDropHelper =
                new PoolWallDropHelper(entityManager, _col, _row, tileIndex,
                    _mapRenderer.TransformForeground);
            _hashEntityLocation = new HashEntityLocation();
            FillBackgroundData();
            FillWallData();
        }

        public int Col => _col;
        public int Row => _row;

        public Vector3 GetTilePosition(int i, int j) {
            var x = (i * TileSize) + 0.5f;
            var y = (j * TileSize) + 0.5f;
            return new Vector3(x, y, 0);
        }

        public Vector3 GetTilePosition(Vector2Int tileLocation) {
            return GetTilePosition(tileLocation.x, tileLocation.y);
        }

        public Vector2Int GetTileLocation(Vector3 position) {
            var i = position.x > 0 ? (int) (position.x / TileSize) : -1 - (int) (-position.x / TileSize);
            var j = position.y > 0 ? (int) (position.y / TileSize) : -1 - (int) (-position.y / TileSize);
            return new Vector2Int(i, j);
        }

        public bool IsStandOnBomb(Vector3 position) {
            var tileLocation = GetTileLocation(position);
            return IsStandOnBomb(tileLocation.x, tileLocation.y);
        }

        public bool IsStandOnBomb(int i, int j) {
            return _map[i, j] == TileType.Bomb;
        }

        public bool IsStuck(Vector3 position, bool throughBrick, bool throughBomb, bool throughWall = false) {
            var tileLocation = GetTileLocation(position);
            var isStuck =
                !IsEmpty(tileLocation.x - 1, tileLocation.y, throughBrick, throughBomb, throughWall) &&
                !IsEmpty(tileLocation.x + 1, tileLocation.y, throughBrick, throughBomb, throughWall) &&
                !IsEmpty(tileLocation.x, tileLocation.y + 1, throughBrick, throughBomb, throughWall) &&
                !IsEmpty(tileLocation.x, tileLocation.y - 1, throughBrick, throughBomb, throughWall);

            return isStuck;
        }

        public bool IsWallOrOutSide(int i, int j) {
            if (i < 0 || i > _col - 1 || j < 0 || j > _row - 1) {
                return false;
            }
            return _map[i, j] == TileType.Wall;
        }

        public bool IsOutOfMap(int i, int j) {
            if (i < 0 || i >= _col || j < 0 || j >= _row) {
                return true;
            }
            return false;
        }

        private bool IsBomb(int i, int j) {
            return _map[i, j] is TileType.Bomb;
        }

        private bool IsWall(int i, int j) {
            return _map[i, j] is TileType.Wall;
        }

        public bool IsEmpty(int i, int j, bool throughBrick = false, bool throughBomb = false,
            bool throughWall = false) {
            if (i < 0 || i > _col - 1 || j < 0 || j > _row - 1) {
                return false;
            }
            var tileType = _map[i, j];
            if (tileType is TileType.Background or TileType.BombExplosion or TileType.Item) {
                return true;
            }
            if (throughBrick) {
                return tileType is TileType.Brick;
            }
            if (throughBomb) {
                return tileType is TileType.Bomb;
            }
            if (throughWall) {
                return tileType is TileType.Wall;
            }
            return false;
        }

        public bool IsEmpty(Vector2Int tileLocation, bool throughBrick = false, bool throughBomb = false,
            bool throughWall = false) {
            return IsEmpty(tileLocation.x, tileLocation.y, throughBrick, throughBomb, throughWall);
        }

        public bool IsBrick(int i, int j) {
            return _map[i, j] == TileType.Brick;
        }

        public bool IsItem(int i, int j) {
            return _map[i, j] is TileType.Item;
        }

        public bool IsMarkBreakBrick(int i, int j) {
            return _markBreakBrick.ContainsKey(MapHelperV2.GetMatrixHashCode(i, j));
        }

        public List<Vector2Int> GetEmptyAround(Vector2Int tileLocation, bool throughBrick, bool throughBomb) {
            var i = tileLocation.x;
            var j = tileLocation.y;

            var emptyList = new List<Vector2Int>();

            //Left
            if (IsEmpty(i - 1, j, throughBrick, throughBomb)) {
                emptyList.Add(Vector2Int.left);
            }

            //Right
            if (IsEmpty(i + 1, j, throughBrick, throughBomb)) {
                emptyList.Add(Vector2Int.right);
            }

            //Up
            if (IsEmpty(i, j + 1, throughBrick, throughBomb)) {
                emptyList.Add(Vector2Int.up);
            }

            //Down
            if (IsEmpty(i, j - 1, throughBrick, throughBomb)) {
                emptyList.Add(Vector2Int.down);
            }

            return emptyList;
        }

        public List<Vector2Int> GetRandomEmptyAround(Vector2Int location, int num, int radius1, int radius2,
            int fromTop = 0) {
            var available = new List<Vector2Int>();

            // Nếu phạm vi trên vượt quá map thì giảm phạm vi trên bằng dy.
            var dy = 0;
            if (location.y + radius2 >= _row - (1 + fromTop)) {
                dy = 1 + fromTop;
            }

            var minX1 = location.x - radius1;
            var maxX1 = location.x + radius1;
            var minY1 = location.y - radius1;
            var maxY1 = location.y + radius1 - dy;

            var minX2 = location.x - radius2;
            var maxX2 = location.x + radius2;
            var minY2 = location.y - radius2;
            var maxY2 = location.y + radius2 - dy;

            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row - (1 + fromTop); j++) {
                    if (((i >= minX2 && i <= minX1) || (i >= maxX1 && i <= maxX2)) &&
                        ((j >= minY2 && j <= minY1) || (j >= maxY1 && j <= maxY2))) {
                        if (_emptyLocations[i, j] == true) {
                            available.Add(new Vector2Int(i, j));
                        }
                    }
                }
            }
            available.Remove(location);

            for (var i = 0; i < available.Count; i++) {
                var random = Random.Range(0, available.Count);
                (available[i], available[random]) = (available[random], available[i]);
            }

            var result = new List<Vector2Int>();
            for (var n = 0; n < num && n < available.Count; n++) {
                result.Add(available[n]);
            }
            return result;
        }

        public Vector2Int GetNearestEmptyVert(Vector3 position, FaceDirection face, bool throughBrick,
            bool throughBomb) {
            var tileLocation = GetTileLocation(position);

            int dx;
            if (face == FaceDirection.Right) {
                dx = 1;
            } else if (face == FaceDirection.Left) {
                dx = -1;
            } else {
                return new Vector2Int(-1, -1);
            }

            var i = tileLocation.x;
            var j = tileLocation.y;

            var jUp = -1;
            var jDown = -1;
            for (var r = j; r < _row && r <= j + 1; r++) {
                if (IsEmpty(i + dx, r, throughBrick, throughBomb)) {
                    jUp = r;
                    break;
                }
            }

            for (var r = j; r >= 0 && r >= j - 1; r--) {
                if (IsEmpty(i + dx, r, throughBrick, throughBomb)) {
                    jDown = r;
                    break;
                }
            }

            if (jUp == -1 && jDown == -1) {
                return new Vector2Int(-1, -1);
            } else {
                if (jUp >= 0 && jDown >= 0) {
                    var positionUp = GetTilePosition(i, jUp);
                    var positionDown = GetTilePosition(i, jDown);

                    var dup = positionUp.y - position.y; //  jUp - j;
                    var ddown = position.y - positionDown.y; //j - jDown;
                    return new Vector2Int(i + dx, dup < ddown ? jUp : jDown);
                } else {
                    return new Vector2Int(i + dx, jUp >= 0 ? jUp : jDown);
                }
            }
        }

        public Vector2Int GetNearestEmptyHori(Vector3 position, FaceDirection face, bool throughBrick,
            bool throughBomb) {
            var tileLocation = GetTileLocation(position);

            int dy;
            if (face == FaceDirection.Down) {
                dy = -1;
            } else if (face == FaceDirection.Up) {
                dy = 1;
            } else {
                return new Vector2Int(-1, -1);
            }

            var i = tileLocation.x;
            var j = tileLocation.y;

            var iLeft = -1;
            var iRight = -1;
            for (var c = i; c < _col && c <= i + 1; c++) {
                if (IsEmpty(c, j + dy, throughBrick, throughBomb)) {
                    iRight = c;
                    break;
                }
            }

            for (var c = i; c >= 0 && c >= i - 1; c--) {
                if (IsEmpty(c, j + dy, throughBrick, throughBomb)) {
                    iLeft = c;
                    break;
                }
            }

            if (iLeft == -1 && iRight == -1) {
                return new Vector2Int(-1, -1);
            } else {
                if (iLeft >= 0 && iRight >= 0) {
                    var positionRight = GetTilePosition(iRight, j);
                    var positionLeft = GetTilePosition(iLeft, j);

                    var dright = positionRight.x - position.x; //  iRight - i;
                    var dleft = position.x - positionLeft.x; // i - iLeft;
                    return new Vector2Int(dright < dleft ? iRight : iLeft, j + dy);
                } else {
                    return new Vector2Int(iRight >= 0 ? iRight : iLeft, j + dy);
                }
            }
        }

        private void FillWallData() {
            if (_entityManager.LevelManager == null) {
                return;
            }
            _fixedWallLocations = new List<Vector2Int>();
            if (_mapInfo.ExtendData == null) {
                for (var i = 1; i < _col - 1; i += 2) {
                    for (var j = 1; j < _row - 1; j += 2) {
                        SetTile(i, j, TileType.Wall);
                        _fixedWallLocations.Add(new Vector2Int(i, j));
                    }
                }
            } else {
                var blocks = _mapInfo.ExtendData.Data.blocks;
                for (var idx = 0; idx < blocks.Count; idx++) {
                    var tile = blocks[idx];
                    if (tile.type == SuperBlockType.Hard) {
                        SetTile(tile.x, tile.y, TileType.Wall);
                        _fixedWallLocations.Add(new Vector2Int(tile.x, tile.y));
                    }
                }
            }
        }

        private void FillBackgroundData() {
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    SetTile(i, j, TileType.Background);
                }
            }
        }

        private bool IsFixedWallLocation(Vector2Int location) {
            return _fixedWallLocations.Contains(location);
        }

        public void SaveMap() {
            if (_entityManager.LevelManager.GameMode == GameModeType.StoryMode ||
                _entityManager.LevelManager.GameMode == GameModeType.PvpMode) {
                return;
            }

            var playStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();

            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    if (TryGetBlock(i, j, out var block)) {
                        playStore.MapDatas[i, j] = new MapData(block.blockType, block.health.GetCurrentHealth(),
                            block.health.GetMaxHealth());
                    } else {
                        playStore.MapDatas[i, j] = null;
                    }
                }
            }
        }

        public void LoadMapV2(MapData[,] mapData) {
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    SetTileWithMapData(i, j, mapData[i, j]);
                }
            }
            _mapRenderer.Init(_map, _col, _row);
            _poolWallDropHelper.Init(_col, _row);
        }

        public async UniTask LoadMap() {
            var playStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            var mapRaw = new TileType[_col, _row];
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    if (IsWall(i, j)) {
                        mapRaw[i, j] = TileType.Wall;
                        continue;
                    }
                    var mapData = playStore.MapDatas[i, j];
                    if (mapData == null) {
                        continue;
                    }
                    //Phải await đợi khởi tạo map xong mới xuống check map rỗng
                    await SetTileWithMapData(i, j, mapData);
                    mapRaw[i, j] = MapHelper.ConvertEntityTypeToTileType(mapData.entityType);
                }
            }
            _mapRenderer.Init(mapRaw, _col, _row);
            if (_entityManager.LevelManager.GameMode != GameModeType.StoryMode &&
                _entityManager.LevelManager.GameMode != GameModeType.PvpMode) {
                CheckBlockIsEmpty();
                GetNumberOfBlock();
            }
        }

        private async UniTask SetTileWithMapData(int i, int j, MapData mapData) {
            if (mapData == null) {
                return;
            }

            var tileType = MapHelper.ConvertEntityTypeToTileType(mapData.entityType);
            _map[i, j] = TileType.Brick;

            //true means this is available empty location.
            _emptyLocations[i, j] = (tileType == TileType.Background);

            _entityUnderBrick[i, j] = mapData.entityType;

            var mode = _entityManager.LevelManager.GameMode;
            if (mode != GameModeType.StoryMode &&
                mode != GameModeType.PvpMode) {
                var entity = await _entityManager.LevelManager.CreateEntity(mapData.entityType, new Vector2Int(i, j));
                if (entity != null) {
                    var block = entity as Block;
                    if (block != null) {
                        block.blockType = mapData.entityType;
                        block.GameModeType = mode;
                        block.health.MaxHealth = mapData.maxHp;
                        block.health.SetCurrentHealth(mapData.hp);

                        _blockLocations[i, j] = block;
                        _aliveBlocks[i, j] = true;
                    }
                }
            }
        }

        private void SetTile(int i, int j, TileType tileType) {
            // SetTilePatternDefault
            _map[i, j] = tileType;

            //true means this is available empty location.
            _emptyLocations[i, j] = tileType == TileType.Background;

            _entityUnderBrick[i, j] = tileType == TileType.Brick ? EntityType.Brick : EntityType.Unknown;
        }

        private void RemoveTile(int i, int j) {
            _map[i, j] = TileType.Background;
            _stageCount++;
        }

        public bool RemoveBrick(int i, int j) {
            if (i < 0 || i > _col - 1 || j < 0 || j > _row - 1) {
                return false;
            }
            if (_map[i, j] == TileType.Background) {
                return false;
            }
            RemoveTile(i, j);
            if (_superMapRender != null) {
                _superMapRender.RemoveBrick(i, j);
            } else {
                _mapRenderer.RemoveBrick(i, j);
            }

            return true;
        }

        public void ForceRemoveBrick(int i, int j) {
            if (i < 0 || i > _col - 1 || j < 0 || j > _row - 1) {
                return;
            }
            RemoveTile(i, j);
            _mapRenderer.RemoveBrick(i, j);
        }

        public ItemType GetItemType(int i, int j) {
            return _itemLocations[i, j];
        }

        public Block GetBlock(int i, int j) {
            return _blockLocations[i, j];
        }

        public bool TryGetBlock(int i, int j, out Block block) {
            if (i < 0 || i > _col - 1 || j < 0 || j > _row - 1) {
                block = null;
                return false;
            }
            if (_entityManager.LevelManager.GameMode == GameModeType.StoryMode ||
                _entityManager.LevelManager.GameMode == GameModeType.PvpMode) {
                block = null;
                return _map[i, j] == TileType.Brick;
            }

            block = _blockLocations[i, j];
            return _aliveBlocks[i, j];
        }

        private void ClearItemUnderBrick(int i, int j) {
            _entityUnderBrick[i, j] = EntityType.Unknown;
        }

        public void ClearBlock(int i, int j) {
            var block = _blockLocations[i, j];
            if (block != null) {
                UnityEngine.Object.Destroy(block.gameObject);
            }
            _blockLocations[i, j] = null;
            _aliveBlocks[i, j] = false;

            CheckBlockIsEmpty();
            GetNumberOfBlock();
        }

        private void CheckBlockIsEmpty() {
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    if (_aliveBlocks[i, j]) {
                        IsEmptyBlock = false;
                        return;
                    }
                }
            }

            IsEmptyBlock = true;
        }

        private int GetNumberOfBlock() {
            var numberOfBlock = 0;
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    if (_aliveBlocks[i, j]) {
                        numberOfBlock++;
                    }
                }
            }
            NumberOfBlock = numberOfBlock;
            return numberOfBlock;
        }

        public void MarkPlayerAddBomb(Vector2Int tileLocation, int explosionLength) {
            if (_map[tileLocation.x, tileLocation.y] == TileType.Background) {
                var timestamp = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond);
                var offset = explosionLength + 1;
                for (var i = Mathf.Max(0, tileLocation.x - offset);
                     i < _col && i < tileLocation.x + offset; i++) {
                    _timePlantBomb[i, tileLocation.y] = timestamp;
                }
                for (var j = Mathf.Max(0, tileLocation.y - offset);
                     j < _row && j < tileLocation.y + offset; j++) {
                    _timePlantBomb[tileLocation.x, j] = timestamp;
                }
            }
        }

        public void AddBomb(Vector2Int tileLocation, int explosionLength, TileType bombType = TileType.Bomb) {
            if (_map[tileLocation.x, tileLocation.y] == TileType.Background) {
                SetTileType(tileLocation.x, tileLocation.y, bombType);
                var timestamp = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond);
                var offset = explosionLength;
                for (var i = Mathf.Max(0, tileLocation.x - offset);
                     i < _col && i < tileLocation.x + offset; i++) {
                    _timePlantBomb[i, tileLocation.y] = timestamp;
                }
                for (var j = Mathf.Max(0, tileLocation.y - offset);
                     j < _row && j < tileLocation.y + offset; j++) {
                    _timePlantBomb[tileLocation.x, j] = timestamp;
                }
            }
        }

        public TileType RemoveBomb(Vector2Int tileLocation) {
            var prevType = _map[tileLocation.x, tileLocation.y];
            if (_map[tileLocation.x, tileLocation.y] == TileType.Bomb) {
                SetTileType(tileLocation.x, tileLocation.y, TileType.Background);
            }
            return prevType;
        }

        public List<Vector2Int> TakeEmptyLocations(int num) {
            //Get empty list...
            var available = new List<Vector2Int>();
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    //free cell near player
                    if (_entityManager.LevelManager.GameMode == GameModeType.StoryMode) {
                        if (i <= 6 && j >= _row - 6) {
                            continue;
                        }
                    }

                    if (_emptyLocations[i, j] == true) {
                        available.Add(new Vector2Int(i, j));
                    }
                }
            }

            //random [num] of empty list
            var result = new List<Vector2Int>();

            if (available.Count > 0) {
                var shuffed = available.OrderBy(x => Guid.NewGuid()).ToList();
                for (var i = 0; i < num && i < shuffed.Count; i++) {
                    var location = shuffed[i];
                    result.Add(location);
                    // không cần thiết remove trong pve tránh:
                    // bug đứng game do_emptyLocations có sử dụng để tìm đường đi
                    // bug cho hero xuất hiện đi xuất hiện lại trong map nhiều lần sẽ _emptyLocations bị false toàn bộ
                    if (_entityManager.LevelManager.GameMode != GameModeType.TreasureHuntV2) { 
                        _emptyLocations[location.x, location.y] = false;
                    }
                }
            }

            return result;
        }

        public Vector2Int TakeLocationsForBoss() {
            // var i = Random.Range(_col / 2, _col - 3);
            // var j = Random.Range(1, _row - 2);
            // return new Vector2Int(i, j);
            //Get empty list...
            var available = new List<Vector2Int>();
            for (var i = _col / 2; i < _col - 3; i++) {
                for (var j = 1; j < _row - 2; j++) {
                    if (_emptyLocations[i, j] == true) {
                        available.Add(new Vector2Int(i, j));
                    }
                }
            }
            var r = UnityEngine.Random.Range(0, available.Count);
            return available[r]; 
        }

        public List<Vector2Int> GetRandomEmptyLocations(int num, int fromTop = 0) {
            //Get empty list...
            var available = new List<Vector2Int>();
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row - fromTop; j++) {
                    if (IsEmpty(i, j)) {
                        available.Add(new Vector2Int(i, j));
                    }
                }
            }

            //random [num] of empty list
            var result = new List<Vector2Int>();
            if (available.Count <= 0) {
                return result;
            }

            var shuffed = available.OrderBy(x => Guid.NewGuid()).ToList();
            for (var i = 0; i < num && i < shuffed.Count; i++) {
                var location = shuffed[i];
                result.Add(location);
            }
            return result;
        }

        public Vector2Int GetNearestEmptyLocation(Vector2Int location) {
            //Get empty list...
            var available = new List<Vector2Int>();
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row - 0; j++) {
                    if (_emptyLocations[i, j] == true) {
                        available.Add(new Vector2Int(i, j));
                    }
                }
            }
            available.Remove(location);

            //ShuffleEmptyLocations
            for (var i = 0; i < available.Count; i++) {
                var random = UnityEngine.Random.Range(0, available.Count);
                (available[i], available[random]) = (available[random], available[i]);
            }
            return available[0];
        }

        public void SetItemUnderBrick(ItemType item, Vector2Int location) {
            _itemLocations[location.x, location.y] = item;
            _entityUnderBrick[location.x, location.y] = EntityType.Item;
        }

        public void SetDoorLocation(Vector2Int location) {
            _entityUnderBrick[location.x, location.y] = EntityType.Door;
            _doorLocation = new Vector2Int(location.x, location.y);
        }

        public Vector2Int GetDoorLocation() {
            return _doorLocation;
        }

        public int[,] GetMapGrid(bool throughBrick, bool throughBomb, bool throughWall) {
            var grid = new int[_col, _row];
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    var isEmpty = IsEmpty(i, j, throughBrick, throughBomb, throughWall);
                    grid[i, j] = isEmpty ? 0 : 1;
                }
            }
            return grid;
        }

        public List<Vector2Int> ShortestPath(Vector2Int source, Vector2Int destination, bool throughBrick,
            bool throughBomb, bool throughWall = false) {
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    var isEmpty = IsEmpty(i, j, throughBrick, throughBomb, throughWall);
                    _grid[i, j] = isEmpty ? 0 : 1;
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

        public int CountNeighbourHardBlock(Vector2Int src) {
            var count = 0;
            var x = src.x;
            var y = src.y;

            if (x - 1 >= 0 && IsWall(x - 1, y)) {
                count++;
            }

            if (x + 1 < _col && IsWall(x + 1, y)) {
                count++;
            }

            if (y - 1 >= 0 && IsWall(x, y - 1)) {
                count++;
            }

            if (y + 1 < _row && IsWall(x, y + 1)) {
                count++;
            }

            return count;
        }

        public IFallingBlockInfo[] CreateDropListFullMap() {
            return MapHelperV2.CreateDropListFullMap(_col, _row);
        }

        public void DropOneWall(int index, int i, int j, float delay) {
            if (IsOutOfMap(i, j)) {
                Debug.LogWarning($"OutOfMap {i} {j}");
                return;
            }
            var localPosition = GetTilePosition(i, j);
            var hDeltaDrop = _row * 1.5f;
            var isHardBlock = _map[i, j] == TileType.Wall;
            var wallDrop =
                _poolWallDropHelper.CreateWallDrop(index, i, j, localPosition, hDeltaDrop, delay, isHardBlock);
            if (!wallDrop) {
                return;
            }
            wallDrop.AniFallDown(OnAfterWallDropFallDown);
        }

        private void OnAfterWallDropFallDown(int i, int j) {
            if (_map[i, j] == TileType.Wall) {
                return;
            }
            _map[i, j] = TileType.Wall;
            _mapRenderer.OnAfterWallDropFallDown(i, j);
            _stageCount++;
        }

        public void WallDropTakeDamage(int i, int j) {
            _poolWallDropHelper.WallDropTakeDamage(i, j);
        }

        //------------------------------------------------------------------
        public void UpdateProcess() {
            BlinkItemBricks();
            _hashEntityLocation.UpdateProcess();
        }

        private void BlinkItemBricks() {
            if (_hasBlinkBricks) {
                return;
            }
            var isBlinkBrick = false;
            if (_entityManager.LevelManager.IsStoryMode) {
                isBlinkBrick = _entityManager.EnemyManager.Count <= 0;
            }
            if (!isBlinkBrick) {
                return;
            }
            _hasBlinkBricks = true;
            _mapRenderer.BlinkItemBricks(_entityUnderBrick, _col, _row);
        }

        public TileType[,] GetTileTypeMap() {
            return _map;
        }

        public void SetTileType(int i, int j, TileType type) {
            if (_aliveBlocks[i, j]) {
                return;
            }

            if (_map[i, j] != TileType.Wall) {
                _map[i, j] = type;
            }
            _stageCount++;
        }

        public void SetHadBomb(Vector2Int location) {
            _hadBombs[location.x, location.y] = true;
        }

        public void RemoveHadBomb(Vector2Int location) {
            _hadBombs[location.x, location.y] = false;
        }

        public bool HadBomb(Vector2Int location) {
            return _hadBombs[location.x, location.y];
        }

        public void FixHeroOutSideMap(IPlayer player) {
            var location = GetTileLocation(player.GetPosition());
            var isOutSide = false;
            if (location.x < 0) {
                location.x = 0;
                isOutSide = true;
            } else if (location.x > _col - 1) {
                location.x = _col - 1;
                isOutSide = true;
            }
            if (location.y < 0) {
                location.y = 0;
                isOutSide = true;
            } else if (location.y > _row - 1) {
                location.y = _row - 1;
                isOutSide = true;
            }
            // Tính toán lại hiện trạng của map để có thê đưa vào vị trí khác. 
            if (isOutSide) {
                Debug.LogWarning("Player is out side map!!!");
                player.SetPosition(GetTilePosition(location));
            }
        }

        public bool CompareMapHash(ByteArray serverHash) {
            var hash = GenerateMapMD5Hash();
            return hash.Bytes.SequenceEqual(serverHash.Bytes);
        }

        public bool IsBigRewardBlock(int x, int y) {
            var entityType = _entityUnderBrick[x, y];
            return entityType == EntityType.BossChest;
        }

        public int GetStageCount() {
            return _stageCount;
        }

        public Entity CreateEntity(EntityType entityType) {
            return _providerMap.CreateEntity(entityType);
        }

        public Entity TryCreateEntityLocation(EntityType entityType, Vector2Int location) {
            var entity = _providerMap.TryCreateEntityLocation(entityType, out var entityLocation);
            if (entityLocation) {
                _hashEntityLocation.AddHashLocation(entityLocation, location);
            }
            return entity;
        }

        private ByteArray GenerateMapMD5Hash() {
            var list = new List<byte>();
            for (var i = 0; i < _col; i++) {
                for (var j = 0; j < _row; j++) {
                    switch (_map[i, j]) {
                        case TileType.Wall:
                            if (!IsFixedWallLocation(new Vector2Int(i, j))) {
                                list.Add(0b01);
                            }
                            break;
                        case TileType.Brick:
                            list.Add(0b01);
                            break;
                        case TileType.Item:
                            list.Add(0b10);
                            break;
                    }
                }
            }
            var md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(list.ToArray());
            return new ByteArray(md5.Hash);
        }

        public void RemoveItem(int i, int j) {
            var location = new Vector2Int(i, j);
            var items = _hashEntityLocation.FindEntitiesAtLocation(location);
            if (items == null) {
                Debug.LogWarning("Not found items");
                // Trường hợp ngoại lệ, khi trả về Xóa Item trước khi tạo ra Item
                // Tạm thời xử lý xóa brick
                BreakBrick(i, j);
                return;
            }
            foreach (var item in items) {
                if (item.Type == EntityType.Door) {
                    return;
                }
                item.Kill(true);
                ClearItemUnderBrick(location.x, location.y);
                _map[location.x, location.y] = TileType.Background;
            }
            _stageCount++;
        }

        public bool LocationIsItem(int i, int j) {
            var location = new Vector2Int(i, j);
            var items = _hashEntityLocation.FindEntitiesAtLocation(location);
            if (items == null) {
                return false;
            }
            var index = 0;
            for (; index < items.Count; index++) {
                var item = items[index];
                if (item.Type == EntityType.Item || item.Type == EntityType.Door) {
                    return true;
                }
            }
            return false;
        }

        public bool BreakBrick(int i, int j) {
            _markBreakBrick[MapHelperV2.GetMatrixHashCode(i, j)] = true;
            var tileType = _map[i, j];
            if (!RemoveBrick(i, j)) {
                return false;
            }
            if (tileType != TileType.Brick && tileType != TileType.NftChest) {
                return false;
            }
            _poolBrickHelper.ShowEntityBreaking(tileType, i, j);
            ShowItemUnderBrick(i, j);
            return true;
        }

        public void ClearMarkBreakBrick() {
            _markBreakBrick.Clear();
        }

        public bool PaintBrick(int i, int j) {
            if (_map[i, j] != TileType.Background) {
                return false;
            }
            _map[i, j] = TileType.Brick;
            _mapRenderer.FillBrick(i, j);
            return true;
        }

        public long TimePlantBomb(int i, int j) {
            return _timePlantBomb[i, j];
        }

        public long TimeWallDrop(int i, int j) {
            return _poolWallDropHelper.LongTimeWallDrop(i, j);
        }

        private void ShowItemUnderBrick(int i, int j) {
            var entityType = _entityUnderBrick[i, j];
            if (entityType is EntityType.Unknown or EntityType.Brick) {
                return;
            }
            ClearItemUnderBrick(i, j);
            _entityManager.LevelManager.CreateEntity(entityType, new Vector2Int(i, j));
        }
    }
}