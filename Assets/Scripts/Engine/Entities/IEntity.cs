using System;
using System.Collections.Generic;

using CodeStage.AntiCheat.ObscuredTypes;

using Engine.Components;
using Engine.Manager;

using JetBrains.Annotations;

namespace Engine.Entities {
    public enum EntityType {
        Unknown,
        Brick,
        Wall,
        Bomb,
        BombExplosion,
        BomberMan,
        normalBlock,
        jailHouse,
        woodenChest,
        silverChest,
        goldenChest,
        diamondChest,
        legendChest,
        keyChest,
        Door,
        Enemy,
        Boss,
        BcoinDiamondChest,
        Item,
        BossChest,
        NftChest,
        Prison,
        WallDrop,
        Fire
    }
    public enum PlayerType {
        Block,
        BomberMan,
        Knight,
        Man,
        Vampire,
        Witch,
        Doge,
        Pepe,
        Ninja,
        King,
        PilotRabit,
        Meo2,
        Monkey,
        Pilot,
        BlackCat,
        Tiger,
        PugDog,
        SailorMoon,
        PepeClown,
        FrogGentlemen,
        Dragoon,
        Ghost,
        Pumpkin,
        Werewolves,
        FootballFrog,
        FootballKnight,
        FootballMan,
        FootballVampire,
        FootballWitch,
        FootballDoge,
        FootballPepe,
        FootballNinja,
        Poo = 111,
        GKu,
        PinkyToon,
        Stickman,
        Monitor,
        Dragon,
        Santa,
        Miner,
        Calico,
        Kuroneko,
        GoldenKat,
        MrDear,
        TLion,
        Frog,
        DogeTr,
        KingTr,
        Cupid,
        BGuy,
        PinkyBear,
        PinkyNeko,
        Dragoon2,
        FatTiger,
        Hesman
    }

    public enum PlayerColor {
        Blue,
        Green,
        Red,
        White,
        Yellow,
        HeroTr,
        Skin
    }

    public enum SkinChestType {
        Bomb = 1,
        Trail,
        Avatar, //WING
        Hero,
        Booster,
        Misc,
        Reward,
        Explosion = 8, // FIRE
        BoxReward
    }

    public enum EnemyType {
        //Stage 1
        LeadSoldier,
        Godzilla,
        RoboToy,
        BigTank,

        //Stage 2
        BabyCandy,
        CookiesBig,
        CreamGuardian,
        CandyKing,

        //Stage 3
        BabyLog,
        BabyMushroom,
        BabyRockyGuardian,
        BigRockyLord,

        //Stage 4
        BabyMummy,
        DwarfAnubis,
        BabyBlackCat,
        BeetlesKing,

        //Stage 5
        BugMachine,
        AutoBots,
        CatPatrol,
        DeceptionsHeadQuater,

        //Stage 6
        BabyPirates,
        BladerPirates,
        VicePirates,
        LordPirates,

        //Stage 7
        BabyDumplings,
        MonsterEaiter,
        BeerEaiter,
        DumplingsMaster,

        //Stage 8
        Sheepdog,
        PigRaker,
        BabyHoe,
        PumpkinLord,
        BabyPumpkin,

        //Stage 9
        BabyDaruma,
        HedgehogCrazy,
        CraftyCat,
        JesterKing,
        GhostCraftyCat
    }

    public enum ItemType {
        BombUp,
        FireUp,
        Boots,
        Armor,
        Kick,
        NftChest,
        SkullHead,
        BronzeChest,
        GoldX1,
        GoldX5,
        SilverChest,
        GoldChest,
        PlatinumChest
    }

    public enum ChestSkin {
        StarBeam = 25,
        CoolPuffy = 2,
        Rainbow,
        AngelWing,
        Cape,
        DevilWing,
        CandyBall
    }

    public enum PlayerAbility {
        TreasureHunter, // +5dmg khi pha ruong
        JailBreaker, //+5dmg khi pha tu
        PierceBlock, //Bomb no xuyen block
        SaveBattery, //+30% ti le khong giam nang luong khi dat bomb
        FastCharge, //+5 the luc/phut khi nghi ngoi
        BombPass, //Di xuyen bomb
        BlockPass, //Di xuyen block
        BossHunter, //+20% dmg gay len Boss
        CreepHunter, //+10% dmg gay len Creep
        Shield //Kich hoat shield moi 60s
    }

    public class IndexTree {
        public readonly int[] Indices = { -1, -1, -1, -1 };

        public int this[int i] {
            get => Indices[i];
            set => Indices[i] = value;
        }
    }

    public class SkinChestTypeAttribute : Attribute {
        public SkinChestType Value { get; }

        public SkinChestTypeAttribute(SkinChestType value) {
            Value = value;
        }
    }

    public class ComponentContainer {
        private readonly Dictionary<Type, IEntityComponent> _components;

        public ComponentContainer() {
            _components = new Dictionary<Type, IEntityComponent>();
        }

        public void AddComponent<T>(IEntityComponent component) where T : IEntityComponent {
            _components[typeof(T)] = component;
        }

        [CanBeNull]
        public T GetComponent<T>() where T : IEntityComponent {
            if (_components.ContainsKey(typeof(T))) {
                return (T) _components[typeof(T)];
            }
            return default;
        }
    }

    public interface IEntity {
        EntityType Type { get; set; }
        IEntityManager EntityManager { get; set; }
        ObscuredBool IsAlive { get; }
        ObscuredBool Immortal { get; }
        void DeActive();
        bool Resurrect();
        bool Kill(bool trigger);
        void AddEntityComponent<T>(IEntityComponent component) where T : IEntityComponent;
        T GetEntityComponent<T>() where T : IEntityComponent;
    }
}