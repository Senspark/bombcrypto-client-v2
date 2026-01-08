using System;
using System.Collections;
using System.Collections.Generic;
using Engine.Entities;
using Newtonsoft.Json;
using UnityEngine;

namespace Story.Strategy.Provider
{
    [Serializable]
    public class EnemyCreator
    {
        public EnemyType Type { get; }
        public int Health { get; }
        public int Damage { get; }
        public float SpeedMove { get; }
        public int Follow { get; }
        public int Score { get; }
        public EntityType[] Through { get; }

        public static EnemyType ConvertToEnemyType(int type)
        {
            var dictionary = new Dictionary<int, EnemyType> {
                { 1, EnemyType.LeadSoldier },
                { 2, EnemyType.Godzilla },
                { 3, EnemyType.RoboToy },
                { 4, EnemyType.BigTank },
                { 5, EnemyType.BabyCandy },
                { 6, EnemyType.CookiesBig },
                { 7, EnemyType.CreamGuardian },
                { 8, EnemyType.CandyKing },
                { 9, EnemyType.BabyLog },
                { 10, EnemyType.BabyMushroom },
                { 11, EnemyType.BabyRockyGuardian },
                { 12, EnemyType.BigRockyLord },
                { 13, EnemyType.BabyMummy },
                { 14, EnemyType.DwarfAnubis },
                { 15, EnemyType.BabyBlackCat },
                { 16, EnemyType.BeetlesKing },
                { 17, EnemyType.BugMachine },
                { 18, EnemyType.AutoBots },
                { 19, EnemyType.CatPatrol },
                { 20, EnemyType.DeceptionsHeadQuater },
                { 21, EnemyType.BabyPirates },
                { 22, EnemyType.BladerPirates },
                { 23, EnemyType.VicePirates },
                { 24, EnemyType.LordPirates },
                { 25, EnemyType.BabyDumplings },
                { 26, EnemyType.MonsterEaiter },
                { 27, EnemyType.BeerEaiter },
                { 28, EnemyType.DumplingsMaster },
            };
            return dictionary[type];
        }

        [JsonConstructor]
        public EnemyCreator(string type,
                              int health,
                              int damage,
                              float speed_move,
                              int follow, 
                              int score,
                              string[] through)
        {
            Health = health;
            Damage = damage;
            SpeedMove = speed_move;
            Follow = follow;
            Score = score;

            var dictionary = new Dictionary<string, EnemyType> {
                { "LeadSoldier", EnemyType.LeadSoldier },
                { "Godzilla", EnemyType.Godzilla },
                { "RoboToy", EnemyType.RoboToy },
                { "BigTank", EnemyType.BigTank },
                { "BabyCandy", EnemyType.BabyCandy },
                { "CookiesBig", EnemyType.CookiesBig },
                { "CreamGuardian", EnemyType.CreamGuardian },
                { "CandyKing", EnemyType.CandyKing },
                { "BabyLog", EnemyType.BabyLog },
                { "BabyMushroom", EnemyType.BabyMushroom },
                { "BabyRockyGuardian", EnemyType.BabyRockyGuardian },
                { "BigRockyLord", EnemyType.BigRockyLord },
                { "BabyMummy", EnemyType.BabyMummy },
                { "DwarfAnubis", EnemyType.DwarfAnubis },
                { "BabyBlackCat", EnemyType.BabyBlackCat },
                { "BeetlesKing", EnemyType.BeetlesKing },
                { "BugMachine", EnemyType.BugMachine },
                { "AutoBots", EnemyType.AutoBots },
                { "CatPatrol", EnemyType.CatPatrol },
                { "DeceptionsHeadQuater", EnemyType.DeceptionsHeadQuater },
                { "BabyPirates", EnemyType.BabyPirates },
                { "BladerPirates", EnemyType.BladerPirates },
                { "VicePirates", EnemyType.VicePirates },
                { "LordPirates", EnemyType.LordPirates },
                { "BabyDumplings", EnemyType.BabyDumplings },
                { "MonsterEaiter", EnemyType.MonsterEaiter },
                { "BeerEaiter", EnemyType.BeerEaiter },
                { "DumplingsMaster", EnemyType.DumplingsMaster },
            };

            if (dictionary.ContainsKey(type)) {
                Type = dictionary[type];
            } else {
                Type = EnemyType.Godzilla;
            }

            var entityThrough = new Dictionary<string, EntityType> {
                { "Brick", EntityType.Brick },
                { "Bomb", EntityType.Bomb }
            };
            Through = new EntityType[through.Length];
            for (var i = 0; i<through.Length; i++)
            {
                Through[i] = entityThrough[through[i]];
            }

        }
    }
}
