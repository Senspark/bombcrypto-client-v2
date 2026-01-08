using App;

using CreativeSpore.SuperTilemapEditor;

using Engine.Entities;
using Engine.Manager;

using UnityEngine;

namespace Engine.MapRenderer {
    public enum IdxLayerForeground {
        Background = 0,
        Border = 1,
        Wall = 2,
        Brick = 3
    }

    public class SteMapRenderer : IMapRenderer {
        private readonly GameModeType _gameModeType;
        private readonly Tileset _tileSet;
        private readonly TilemapGroup _tmForeground;
        private readonly TilemapGroup _tmMidground;
        private readonly TilemapGroup _tmBackground;

        public Transform TransformForeground => _tmForeground.transform;
        private STETilemap LayerBackground { get; }
        private STETilemap LayerBorder { get; }
        private STETilemap LayerWall { get; }
        private STETilemap LayerBrick { get; }

        private Brick _brickPrefab;

        public SteMapRenderer(
            GameModeType gameModeType,
            Tileset tileSet,
            TilemapGroup tmBackground,
            TilemapGroup tmMidground,
            TilemapGroup tmForeground,
            MapInfo mapInfo) {
            _gameModeType = gameModeType;
            _tileSet = tileSet;
            _tmBackground = tmBackground;
            _tmMidground = tmMidground;
            _tmForeground = tmForeground;
            LayerBackground = _tmForeground[(int) IdxLayerForeground.Background];
            LayerBorder = _tmForeground[(int) IdxLayerForeground.Border];
            LayerWall = _tmForeground[(int) IdxLayerForeground.Wall];
            LayerBrick = _tmForeground[(int) IdxLayerForeground.Brick];
            LayerBrick.ChunkSize = 1;
            if (mapInfo.ExtendData != null) {
                // Hide map 
                tmBackground.Tilemaps[0].gameObject.SetActive(false);
                tmForeground.Tilemaps[0].gameObject.SetActive(false); // background
                tmForeground.Tilemaps[1].gameObject.SetActive(false); // border
                tmForeground.Tilemaps[2].gameObject.SetActive(false); // wall
                tmForeground.Tilemaps[3].gameObject.SetActive(false); // brick
            }
        }

        public void Init(TileType[,] map, int col, int row) {
            InitBackground(_tileSet, col, row);
            ShowMidGroundMapModePolygon(col, row);
            FillTileMapForeground(_tileSet, col, row);
            SyncMapData(map, col, row);
            FillBorderLayer(col, row, 2, 1);
        }

        /// <summary>
        /// Fill TileMap: Background
        /// </summary>
        private void InitBackground(Tileset tileSet, int col, int row) {
            var offset = 10;
            var bcol = col + offset * 2;
            var brow = row + offset * 2;
            _tmBackground.transform.localPosition = new Vector3((float) -bcol / 2.0f, (float) -brow / 2.0f, 0);
            var tilemap = _tmBackground[0];
            tilemap.Tileset = tileSet;
            tilemap.SetMapBounds(0, 0, bcol - 1, brow - 1);
            tilemap.OrderInLayer = -1;
            for (var i = 0; i < bcol; i++) {
                for (var j = 0; j < brow; j++) {
                    // if (i >= offset && i < bcol - offset && j >= offset && j < brow - offset) {
                    //     continue;
                    // }
                    if (i < bcol / 2) {
                        if (j > 0) {
                            tilemap.SetTile(i, j, ConvertTileTypeToID(TileType.OutSideLeft));
                        } else {
                            tilemap.SetTile(i, j, ConvertTileTypeToID(TileType.OutSideLeftBottom));
                        }
                    } else {
                        if (j > 0) {
                            tilemap.SetTile(i, j, ConvertTileTypeToID(TileType.OutSideRight));
                        } else {
                            tilemap.SetTile(i, j, ConvertTileTypeToID(TileType.OutSideRightBottom));
                        }
                    }
                }
            }
            tilemap.UpdateMeshImmediate();
        }

        private void FillTileMapForeground(Tileset tileSet, int col, int row) {
            var tileMaps = _tmForeground.Tilemaps;
            var centerPos = new Vector3(-col / 2.0f, -row / 2.0f, 0);
            _tmForeground.transform.localPosition = centerPos;
            foreach (var tileMap in tileMaps) {
                tileMap.Tileset = tileSet;
                tileMap.SetMapBounds(0, 0, col - 1, row - 1);
            }
        }

        public void FillBrick(int i, int j) {
            LayerBrick.SetTile(i, j, ConvertTileTypeToID(TileType.Brick));
            LayerBrick.UpdateMesh();
            ShowLayerBrickChunk(i, j);
        }

        public void RemoveBrick(int i, int j) {
            // LayerBrick.Erase(i, j);
            HideLayerBrickChunk(i, j);
        }

        public void OnAfterWallDropFallDown(int i, int j) {
            // Sau khi đá rơi thì đổi ảnh Brick -> Wall
            // LayerBrick.SetTile(i, j, ConvertTileTypeToID(TileType.Wall));
            // LayerBrick.UpdateMesh();
            HideLayerBrickChunk(i, j);
        }

        private void HideLayerBrickChunk(int i, int j) {
            var chunk = LayerBrick.GetTileChunk(i, j);
            if (chunk) {
                chunk.gameObject.SetActive(false);
            }
        }

        private void ShowLayerBrickChunk(int i, int j) {
            var chunk = LayerBrick.GetTileChunk(i, j);
            if (chunk) {
                chunk.gameObject.SetActive(true);
            }
        }

        private void SyncMapData(TileType[,] map, int col, int row) {
            for (var i = 0; i < col; i++) {
                for (var j = 0; j < row; j++) {
                    LayerBackground.SetTile(i, j, ConvertTileTypeToID(TileType.Background));
                    var tileType = map[i, j];
                    switch (tileType) {
                        case TileType.Background:
                            continue;
                        case TileType.Wall:
                            LayerWall.SetTile(i, j, ConvertTileTypeToID(TileType.Wall));
                            break;
                        case TileType.Brick:
                            LayerBrick.SetTile(i, j, ConvertTileTypeToID(TileType.Brick));
                            break;
                        default:
                            LayerBrick.SetTile(i, j, ConvertTileTypeToID(tileType));
                            break;
                    }
                }
            }
            LayerBackground.UpdateMeshImmediate();
            LayerWall.UpdateMeshImmediate();
            LayerBrick.UpdateMeshImmediate();
        }

        private void FillBorderLayer(int colRaw, int rowRaw, int offsetX, int offsetY) {
            //fill Left and Right
            var col = colRaw + offsetX * 2;
            var row = rowRaw + offsetY * 2;
            SetTileBorder(0, 0, TileType.LeftBottom);
            SetTileBorder(col - 1, 0, TileType.RightBottom);
            for (var j = 1; j < row; j++) {
                SetTileBorder(0, j, TileType.Left);
                SetTileBorder(col - 1, j, TileType.Right);
            }

            //fill four corners
            SetTileBorder(1, 0, TileType.BorderLeftBottom);
            SetTileBorder(col - 2, 0, TileType.BorderRightBottom);
            SetTileBorder(1, row - 1, TileType.BorderLeftTop);
            SetTileBorder(col - 2, row - 1, TileType.BorderRightTop);

            //fill Top and Bottom
            TileType[] tops = { TileType.BorderTop1, TileType.BorderTop2 };
            TileType[] bottoms = { TileType.BorderBottom1, TileType.BorderBottom2 };
            var index = 0;
            for (var i = 2; i < col - 2; i++) {
                SetTileBorder(i, row - 1, tops[index]);
                SetTileBorder(i, 0, bottoms[index]);
                index = index == 0 ? 1 : 0;
            }

            //fill Left and Right
            TileType[] lefts = { TileType.BorderLeft1, TileType.BorderLeft2 };
            TileType[] rights = { TileType.BorderRight1, TileType.BorderRight2 };
            index = 0;
            for (var j = 1; j < row - 1; j++) {
                SetTileBorder(1, j, lefts[index]);
                SetTileBorder(col - 2, j, rights[index]);
                index = index == 0 ? 1 : 0;
            }
            LayerBorder.UpdateMeshImmediate();
            LayerBorder.transform.localPosition = new Vector3((float) -offsetX, (float) -offsetY, 0);
        }

        private void SetTileBorder(int i, int j, TileType tileType) {
            LayerBorder.SetTile(i, j, ConvertTileTypeToID(tileType));
        }

        private void SetTile(IdxLayerForeground idxLayer, int i, int j, TileType tileType,
            int brushId = Tileset.k_BrushId_Default) {
            _tmForeground[(int) idxLayer].SetTile(i, j, ConvertTileTypeToID(tileType), brushId);
        }

        public void BlinkItemBricks(EntityType[,] entityLocations, int col, int row) {
            for (var i = 0; i < col; i++) {
                for (var j = 0; j < row; j++) {
                    var itemType = entityLocations[i, j];
                    if (itemType != EntityType.Unknown && itemType != EntityType.Brick) {
                        SetTile(IdxLayerForeground.Brick, i, j, TileType.Brick, 1);
                    }
                }
            }
            UpdateMesh(IdxLayerForeground.Brick);
        }

        private void UpdateMesh(IdxLayerForeground idxLayer) {
            _tmForeground[(int) idxLayer].UpdateMesh();
        }

        private int ConvertTileTypeToID(TileType tileType) {
            var rand = _gameModeType == GameModeType.TreasureHuntV2;
            return MapHelper.ConvertTileTypeToID(tileType, rand);
        }

        private void ShowMidGroundMapModePolygon(int col, int row) {
            if (!_tmMidground) {
                return;
            }
            var centerPos = new Vector3(-col / 2.0f - 2.0f, -row / 2.0f, 0);
            _tmMidground.transform.localPosition = centerPos;
            _tmForeground.Tilemaps[0].IsVisible = false;
            _tmForeground.Tilemaps[1].IsVisible = false;
        }
    }
}