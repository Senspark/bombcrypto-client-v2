namespace Data {
    public class TRHeroData {
        public int Color { get; }
        public int InstanceId { get; }
        public int ItemId { get; }
        public int HeroId { get; }
        public int Level { get; }
        public bool IsActive { get; }
        public int Quantity { get; }
        public StatData[] Stats { get; }
        public int Status { get; }
        public int GrindCost => 200;
        public int UpgradedSpeed { get; }
        public int UpgradedBomb { get; }
        public int UpgradedRange { get; }
        public int UpgradedHp { get; }
        public int UpgradedDmg { get; }
        public int MaxUpgradedSpeed { get; }
        public int MaxUpgradedBomb { get; }
        public int MaxUpgradedRange { get; }
        public int MaxUpgradedHp { get; }
        public int MaxUpgradedDmg { get; }

        public TRHeroData(
            int color,
            int heroId,
            int instanceId,
            int itemId,
            int level,
            bool isActive,
            int quantity,
            StatData[] stats,
            int status,
            int upgradedSpeed,
            int upgradedBomb,
            int upgradedRange,
            int upgradedHp,
            int upgradedDmg,
            int maxUpgradedSpeed,
            int maxUpgradedBomb,
            int maxUpgradedRange,
            int maxUpgradedHp,
            int maxUpgradedDmg
        ) {
            Color = color;
            HeroId = heroId;
            InstanceId = instanceId;
            ItemId = itemId;
            Level = level;
            IsActive = isActive;
            Quantity = quantity;
            Stats = stats;
            Status = status;
            UpgradedSpeed = upgradedSpeed;
            UpgradedBomb = upgradedBomb;
            UpgradedRange = upgradedRange;
            UpgradedHp = upgradedHp;
            UpgradedDmg = upgradedDmg;
            MaxUpgradedSpeed = maxUpgradedSpeed;
            MaxUpgradedBomb = maxUpgradedBomb;
            MaxUpgradedRange = maxUpgradedRange;
            MaxUpgradedHp = maxUpgradedHp;
            MaxUpgradedDmg = maxUpgradedDmg;
        }
    }
}