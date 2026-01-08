using System.Collections.Generic;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;

using Engine.Entities;
using Engine.Manager;

using JetBrains.Annotations;

using PvpMode.Entities;
using PvpMode.Services;

using SuperTiled2Unity;

using UnityEngine;

using IEntityManager = Engine.Manager.IEntityManager;
using IMapManager = Engine.Manager.IMapManager;

namespace BLPvpMode.GameView {
    public interface ILevelViewPvp {
        /// <summary>
        /// Khởi gán LevelView
        /// </summary>
        /// <param name="heroes"> Danh sách Hero </param>
        /// <param name="mapDetail"> Chi tiết map </param>
        /// <param name="matchData"></param>
        /// <param name="pvpModeCallback"> callback to levelScene</param>
        /// <param name="layer"> layer của Map parent  </param>
        /// <param name="mapData"> SuperMapData, force load map data theo design </param> 
        void Initialize(
            IMatchHeroInfo[] heroes,
            IMapInfo mapDetail,
            IMatchData matchData,
            PvpModeCallback pvpModeCallback,
            int layer,
            SuperMapData mapData
        );

        /// <summary>
        /// Cập nhật chỉ số ban đầu của hero trên levelScene
        /// </summary>
        /// <param name="slot"> chỉ số của hero theo slot </param>
        void UpdateState(int slot);

        /// <summary>
        ///  Hiển thị health bar trên đầu đối với hero khác slot
        /// </summary>
        /// <param name="slot"></param>
        void ShowHealthBarOnPlayerNotSlot(int slot);

        /// <summary>
        /// Play kill effect trên Hero khác slot 
        /// </summary>
        /// <param name="slot"></param>
        void PlayKillEffectOnOther(int slot);

        /// <summary>
        /// Update health bar của hero
        /// </summary>
        /// <param name="slot"> update cho hero theo slot </param>
        /// <param name="value"> update health theo value </param>
        void UpdateHealthBar(int slot, int value);

        /// <summary>
        /// Set bất tử 3s cho hero
        /// </summary>
        /// <param name="slot"> áp dụng cho hero theo slot </param>
        void SetImmortal(int slot);

        /// <summary>
        /// Lấy danh sách pvp heroes
        /// </summary>
        /// <returns></returns>
        List<PlayerPvp> GetPvpHeroes();

        /// <summary>
        /// Lấy pvp hero by slot
        /// </summary>
        /// <returns></returns>
        PlayerPvp GetPvpHeroBySlot(int slot);

        /// <summary>
        /// Kill Hero 
        /// </summary>
        /// <param name="slot"> kill hero theo slot </param>
        /// <param name="source"> hero bị giết bởi </param>
        void KillHero(int slot, HeroDamageSource source);

        /// <summary>
        /// Hủy shield
        /// </summary>
        /// <param name="slot"> hero theo slot bị hủy</param>
        void HeroShieldBroken(int slot);

        /// <summary>
        /// Ép nổ quả bomb
        /// </summary>
        void ExplodeBomb(int slot, int id, [NotNull] Dictionary<Direction, int> ranges);

        /// <summary>
        /// Gán shield cho hero
        /// </summary>
        /// <param name="slot"> gán cho hero theo slot</param>
        void SetShieldToPlayer(int slot);

        /// <summary>
        /// Phá ngục
        /// </summary>
        /// <param name="slot"> phá nguc cho hero theo slot</param>
        void JailBreak(int slot);

        /// <summary>
        /// gán item nhặt được cho hero
        /// </summary>
        /// <param name="playSound"> play the sound when set item </param>
        /// <param name="slot"> gán cho hero theo slot </param>
        /// <param name="item"> loại item được gán </param>
        /// <param name="value"> nếu value > 0 thì hiện số thưởng bay lên </param>
        void AddItemToPlayer(bool playSound, int slot, HeroItem item, int value);

        /// <summary>
        /// gán hiệu ứng skull head cho hero
        /// </summary>
        /// <param name="playSound"> play the sound when set item </param>
        /// <param name="slot"> gán cho hero theo slot </param>
        /// <param name="effect"> kiểu hiệu ứng của skull head </param>
        /// <param name="duration"> thời gian hiệu lực của skull head </param>
        void SetSkullHeadToPlayer(bool playSound, int slot, HeroEffect effect, int duration);

        /// <summary>
        /// Adds a specified item at the specified position.
        /// </summary>
        void CreateItem(Vector2Int position, ItemType type);

        /// <summary>
        /// Remove block at the specified position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        bool RemoveBlock(Vector2Int position);

        void RemoveBomb(int slot, int id);

        /// <summary>
        /// Hủy item 
        /// </summary>
        /// <param name="location"> hủy item tại vị trí location </param>
        void RemoveItem(Vector2Int location);

        /// <summary>
        /// hero đặt bomb
        /// </summary>
        /// <param name="slot"> hero theo slot </param>
        /// <param name="id"> index của bomb sẽ đặt </param>
        /// <param name="range"> chiều dài lửa của bomb </param>
        /// <param name="position"> vị trí đặt bomb </param>
        void SpawnBomb(int slot, int id, int range, Vector2Int position);

        /// <summary>
        /// Bắt sự kiện đá rơi (kiểu mới - rơi từng đợt)
        /// </summary>
        /// <param name="blocks">danh sách vị trí và thời điểm đá rơi</param>
        public void AddFallingWall(IFallingBlockInfo[] blocks);

        /// <summary>
        /// Lấy kiểu tile trên map
        /// </summary>
        /// <param name="x"> tại location x </param>
        /// <param name="y"> tại location y </param>
        /// <returns></returns>
        public TileType GetTileType(int x, int y);

        /// <summary>
        /// Lấy loại item trên map
        /// </summary>
        /// <param name="x"> tại location x </param>
        /// <param name="y"> tại location y </param>
        /// <returns></returns>
        ItemType GetItemType(int x, int y);

        /// <summary>
        /// Update theo delta
        /// </summary>
        /// <param name="delta"></param>
        public void Step(float delta);

        /// Di chuyển hero theo hướng
        /// </summary>
        /// <param name="slot"> hero theo slot </param>
        /// <param name="direction"> hướng di chuyên </param>
        public void ProcessMovement(int slot, Vector2 direction);

        /// <summary>
        /// update hero (thực hiện các hành vị tự động nếu có)
        /// </summary>
        /// <param name="slot"> hero theo slot </param>
        public void ProcessUpdatePlayer(int slot);

        /// <summary>
        /// Bắt hero vào tù
        /// </summary>
        /// <param name="slot"> nhốt hero theo slot </param>
        public void SetPlayerInJail(int slot);

        /// <summary>
        /// Kiểm tra hero có trong tù.
        /// </summary>
        /// <param name="slot"> kiểm tra hero theo slot </param>
        /// <returns></returns>
        bool PlayerIsInJail(int slot);

        /// <summary>
        /// Kiểm tra và lấy bombid hero sẽ đật
        /// </summary>
        bool CheckSpawnPvpBomb(int slot);

        /// <summary>
        /// Shake camera
        /// </summary>
        void ShakeCamera();

        Vector3 GetTilePosition(int x, int y);
        GameObject StartLocation => null;

        IEntityManager EntityManager => null;
        IMapManager MapManager => null;
    }
}