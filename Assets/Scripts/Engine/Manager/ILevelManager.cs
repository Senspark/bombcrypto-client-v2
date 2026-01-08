using System.Collections.Generic;

using App;

using BLPvpMode.Engine;
using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Info;

using Cysharp.Threading.Tasks;

using Engine.Camera;
using Engine.Entities;

using PvpMode.Manager;

using UnityEngine;

using Bomb = Engine.Entities.Bomb;

namespace Engine.Manager {
    public interface ILevelManager {
        public bool IsStoryMode { get; }
        public bool IsPvpMode { get; }
        public bool IsHunterMode { get; }
        public GameModeType GameMode { get; set; }
        public IHeroDetails StoryModeHero { get; set; }
        int CurrentLevel { get; }
        int CurrentStage { get; }
        public bool IsCompleted { get; }
        public ICamera Camera { get; set; }

        void UpdateProcess();
        void CreateBlocksUnderBrick(IBlockInfo[] blocks);
        void CreateItemsUnderBrick(IAdventureItem[] items);
        void SetDoorLocation(Vector2Int location);
        void SetTestingMode(bool isTesting);
        Bomb CreateBomb(Vector3 position);
        UniTask<Entity> CreateEntity(EntityType entityType, Vector2Int tileLocation, bool follow = false);
        void GenerateEnemiesFromDoor(IEnemyDetails[] enemiesFromDoor);
        void LevelCompleted(bool win, IStoryModeEnterDoorResponse reward);
        void EnterDoor();
        void OnUseBooster(BoosterType type);
        void OnPlayerInJail(int slot);
        void OnPlayerEndInJail(int slot);
        void RequestTakeItem(Item item);
        void OnUpdateItem(int slot, ItemType item, int value);
        void OnUpdateHealthUi(int slot, int value);
        void OnBombExploded(HeroId heroId, int bombId, Vector2Int tileLocation, List<Vector2Int> brokenList);
        void CheckEnemiesClear();
        void OnAddEnemy(EnemyType enemyType);
        void OnRemoveEnemy(EnemyType enemyType);
        void EarnGoldFromEnemy(int value, Vector3 from);
        void OnSleepBossCountDown(int value);
        void OnBossWakeup();
    }
}