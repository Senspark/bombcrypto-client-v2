using System.Collections.Generic;

using App;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Info;

using Cysharp.Threading.Tasks;

using Engine.Components;
using Engine.Entities;

using UnityEngine;

using Sfs2X.Util;

namespace Engine.Manager {
    public interface IMapManager {
        UniTask LoadMap();
        void LoadMapV2(MapData[,] mapData);
        void SaveMap();
        int Col { get; }
        int Row { get; }
        Vector3 GetTilePosition(int i, int j);
        Vector3 GetTilePosition(Vector2Int tileLocation);
        Vector2Int GetTileLocation(Vector3 position);

        bool IsStuck(Vector3 position, bool throughBrick, bool throughBomb, bool throughWall = false);
        bool IsStandOnBomb(Vector3 position);
        bool IsStandOnBomb(int i, int j);
        bool IsWallOrOutSide(int i, int j);
        bool IsOutOfMap(int i, int j);
        bool IsEmpty(int i, int j, bool throughBrick = false, bool throughBomb = false, bool throughWall = false);

        bool IsEmpty(Vector2Int tileLocation, bool throughBrick = false, bool throughBomb = false,
            bool throughWall = false);

        bool IsBrick(int i, int j);

        bool IsItem(int i, int j);

        bool IsMarkBreakBrick(int i, int j);

        List<Vector2Int> GetEmptyAround(Vector2Int tileLocation, bool throughBrick, bool throughBomb);
        List<Vector2Int> GetRandomEmptyAround(Vector2Int location, int num, int radius1, int radius2, int fromTop = 0);

        List<Vector2Int> GetRandomEmptyLocations(int num, int fromTop = 0);
        Vector2Int GetNearestEmptyLocation(Vector2Int location);
        Vector2Int GetNearestEmptyVert(Vector3 position, FaceDirection face, bool throughBrick, bool throughBomb);
        Vector2Int GetNearestEmptyHori(Vector3 position, FaceDirection face, bool throughBrick, bool throughBomb);

        void ForceRemoveBrick(int i, int j);
        bool RemoveBrick(int i, int j);
        ItemType GetItemType(int i, int j);
        Block GetBlock(int i, int j);
        bool TryGetBlock(int i, int j, out Block block);
        void ClearBlock(int i, int j);

        // Call trước khi request server
        void MarkPlayerAddBomb(Vector2Int tileLocation, int explosionLength);

        // Call sau khi server response
        void AddBomb(Vector2Int tileLocation, int explosionLength, TileType bombType = TileType.Bomb);

        TileType RemoveBomb(Vector2Int tileLocation);

        List<Vector2Int> TakeEmptyLocations(int num);
        Vector2Int TakeLocationsForBoss();

        void SetItemUnderBrick(ItemType itemType, Vector2Int location);
        Vector2Int GetDoorLocation();
        void SetDoorLocation(Vector2Int location);

        int[,] GetMapGrid(bool throughBrick, bool throughBomb, bool throughWall);

        List<Vector2Int> ShortestPath(Vector2Int Source, Vector2Int Destination, bool throughBrick, bool throughBomb,
            bool throughWall = false);

        int CountNeighbourHardBlock(Vector2Int src);
        IFallingBlockInfo[] CreateDropListFullMap();

        void DropOneWall(int index, int i, int j, float delay);
        void WallDropTakeDamage(int i, int j);

        bool IsEmptyBlock { get; }
        int NumberOfBlock { get; }
        void UpdateProcess();

        int TileIndex { get; }
        TileType[,] GetTileTypeMap();
        void SetTileType(int i, int j, TileType type);

        void SetHadBomb(Vector2Int location);
        void RemoveHadBomb(Vector2Int location);
        bool HadBomb(Vector2Int location);

        void FixHeroOutSideMap(IPlayer player);
        bool BreakBrick(int x, int y);
        void ClearMarkBreakBrick();
        bool PaintBrick(int i, int j);
        void RemoveItem(int x, int y);
        bool LocationIsItem(int x, int y);
        bool CompareMapHash(ByteArray serverHash);

        bool IsBigRewardBlock(int x, int y);
        int GetStageCount();
        Entity CreateEntity(EntityType entityType);
        Entity TryCreateEntityLocation(EntityType entityType, Vector2Int location);
        long TimePlantBomb(int i, int j);
        long TimeWallDrop(int i, int j);
    }
}