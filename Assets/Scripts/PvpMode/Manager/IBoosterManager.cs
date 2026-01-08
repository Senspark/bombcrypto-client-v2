using System;

using Data;

using Senspark;

using Sfs2X.Entities.Data;

namespace PvpMode.Manager {
    public enum BoosterType {
        Unknown,
        Shield, // pve & pvp - active
        RankGuardian, // pvp - passive
        FullRankGuardian, // pvp - passive
        CupBonus, // pvp - passive
        FullCupBonus, // pvp - passive
        ComboDaily, // not use
        Key, // pve & pvp - active
        BombAddOne, // pve - passive
        RangeAddOne, // pve - passive
        SpeedAddOne // pve - passive
    }

    public interface IBooster {
        BoosterType Type { get; }
        bool IsCombo { get; }
        IBooster[] ComboItems { get; }
        int Price { get; }
        int Quantity { get; }
        bool Locked { get; }
        DateTime UnlockTime { get; }
    }

    [Service(nameof(IBoosterManager))]
    public interface IBoosterManager : IService {
        public delegate void BoosterChanged(IBooster[] boosters);
        event BoosterChanged EventBoosterChanged;
        void SetShopBoosters(IBooster[] boosters);
        IBooster GetShopBooster(BoosterType type);
        void SetUserBoosters(IBooster[] boosters);
        IBooster GetUserBooster(BoosterType type);

        void SetBoosterData(BoosterData[] boosters);
        BoosterData GetDataBooster(BoosterType type);
    }
}