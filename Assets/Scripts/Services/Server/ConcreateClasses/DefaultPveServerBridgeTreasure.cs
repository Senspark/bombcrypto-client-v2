using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CustomSmartFox.SolCommands;

using Server.Models;

using Sfs2X.Entities.Data;

using UnityEngine;

using RewardType = Constant.RewardType;

namespace App {
    // Server-driven treasure mode: START/STOP/PAUSE/RESUME_TREASURE_MODE + TREASURE_EVENTS parsing.
    public partial class DefaultPveServerBridge {
        private const string TreasureDatasKey = "datas_pve_v2";
        private const string TreasureTilesetKey = "tileset_pve_v2";

        public async Task<TreasureSnapshot> StartTreasureMode(bool paused = false) {
            var data = new SFSObject();
            if (paused) {
                data.PutBool("paused", true);
            }
            var response = await _serverDispatcher.SendCmd(new CmdStartTreasureMode(data));
            var snapshot = ParseTreasureSnapshot(response);
            _bHeroManager.SetMapDetails(snapshot.Map);
            _bHeroManager.LoadMap(GameModeType.TreasureHuntV2);
            _logManager.Log(
                $"[TREASURE] start seq={snapshot.Seq} heroes={snapshot.Heroes.Count} bombs={snapshot.Bombs.Count} awaitingNewMap={snapshot.AwaitingNewMap}");
            return snapshot;
        }

        public async Task StopTreasureMode() {
            await _serverDispatcher.SendCmd(new CmdStopTreasureMode(new SFSObject()));
        }

        public async Task PauseTreasureMode(bool paused) {
            if (paused) {
                await _serverDispatcher.SendCmd(new CmdPauseTreasureMode(new SFSObject()));
            } else {
                await _serverDispatcher.SendCmd(new CmdResumeTreasureMode(new SFSObject()));
            }
        }

        public static TreasureSnapshot ParseTreasureSnapshot(ISFSObject data) {
            var snapshot = new TreasureSnapshot {
                Map = new MapDetails(ReadInt(data, TreasureTilesetKey), data.GetUtfString(TreasureDatasKey) ?? "[]"),
                Heroes = ParseHeroPositions(data.GetSFSArray("heroes")),
                Seq = ReadLong(data, "seq"),
                ServerTime = ReadLong(data, "server_time", ReadLong(data, "at")),
                FuseMs = ReadLong(data, "fuse_ms"),
                AwaitingNewMap = data.GetBool("awaiting_new_map"),
                ResumeAt = ReadLong(data, "resume_at"),
            };
            var bombs = data.GetSFSArray("bombs");
            if (bombs != null) {
                foreach (ISFSObject b in bombs) {
                    snapshot.Bombs.Add(new TreasureBombInfo {
                        HeroId = ReadInt(b, SFSDefine.SFSField.Id),
                        Num = ReadInt(b, "num"),
                        Cell = new Vector2Int(ReadInt(b, "i"), ReadInt(b, "j")),
                        PlantedAt = ReadLong(b, "planted_at"),
                        ExplodeAt = ReadLong(b, "explode_at"),
                    });
                }
            }
            var dangerous = data.GetSFSArray("dangerous");
            if (dangerous != null) {
                foreach (ISFSObject d in dangerous) {
                    snapshot.DangerousData.Add(new PveHeroDangerous(d));
                }
            }
            if (data.ContainsKey("is_trial")) {
                snapshot.IsTrial = data.GetBool("is_trial") ? TrialState.TrialBegin : TrialState.TrialEnd;
            }
            return snapshot;
        }

        public static List<TreasureEvent> ParseTreasureEvents(ISFSObject data) {
            var result = new List<TreasureEvent>();
            var events = data.GetSFSArray("events");
            if (events == null) {
                return result;
            }
            foreach (ISFSObject e in events) {
                result.Add(ParseTreasureEvent(e));
            }
            return result;
        }

        private static TreasureEvent ParseTreasureEvent(ISFSObject e) {
            var rawType = e.GetUtfString("type") ?? "";
            var ev = new TreasureEvent {
                Seq = ReadLong(e, "seq"),
                RawType = rawType,
                Type = rawType switch {
                    "MOVE" => TreasureEventType.Move,
                    "PLANT" => TreasureEventType.Plant,
                    "EXPLODE" => TreasureEventType.Explode,
                    "HERO_JOIN" => TreasureEventType.HeroJoin,
                    "HERO_LEAVE" => TreasureEventType.HeroLeave,
                    "NEW_MAP" => TreasureEventType.NewMap,
                    "RESYNC" => TreasureEventType.Resync,
                    _ => TreasureEventType.Unknown,
                },
                At = ReadLong(e, "at"),
                Reason = e.GetUtfString("reason"),
            };
            switch (ev.Type) {
                case TreasureEventType.Move:
                    ev.Hero = ParseHeroPosition(e);
                    ev.StepMs = ReadLong(e, "step_ms");
                    ev.RoamUntil = ReadLong(e, "roam_until", -1);
                    ev.Path = new List<Vector2Int>();
                    var path = e.GetSFSArray("path");
                    if (path != null) {
                        foreach (ISFSObject cell in path) {
                            ev.Path.Add(new Vector2Int(ReadInt(cell, "i"), ReadInt(cell, "j")));
                        }
                    }
                    break;
                case TreasureEventType.Plant:
                    ev.Hero = ParseHeroPosition(e);
                    ev.Num = ReadInt(e, "num");
                    ev.PlantedAt = ReadLong(e, "planted_at", ev.At);
                    ev.ExplodeAt = ReadLong(e, "explode_at", ev.At);
                    break;
                case TreasureEventType.Explode:
                    ev.Hero = ParseHeroPosition(e);
                    ev.Num = ReadInt(e, "num");
                    ev.PlantedAt = ReadLong(e, "planted_at");
                    ev.Explode = new TreasureExplodeData(e, ev.Hero);
                    break;
                case TreasureEventType.HeroJoin:
                case TreasureEventType.HeroLeave:
                    ev.Hero = ParseHeroPosition(e);
                    break;
                case TreasureEventType.NewMap:
                    ev.Map = new MapDetails(ReadInt(e, TreasureTilesetKey), e.GetUtfString(TreasureDatasKey) ?? "[]");
                    ev.Heroes = ParseHeroPositions(e.GetSFSArray("heroes"));
                    ev.ResumeAt = ReadLong(e, "resume_at", ev.At);
                    break;
                case TreasureEventType.Resync:
                    ev.Snapshot = ParseTreasureSnapshot(e);
                    break;
            }
            return ev;
        }

        private static List<TreasureHeroPosition> ParseHeroPositions(ISFSArray array) {
            var result = new List<TreasureHeroPosition>();
            if (array == null) {
                return result;
            }
            foreach (ISFSObject h in array) {
                result.Add(ParseHeroPosition(h));
            }
            return result;
        }

        private static TreasureHeroPosition ParseHeroPosition(ISFSObject data) {
            var heroType = data.ContainsKey(SFSDefine.SFSField.AccountType)
                ? ReadInt(data, SFSDefine.SFSField.AccountType, -1)
                : ReadInt(data, SFSDefine.SFSField.HeroType, -1);
            return new TreasureHeroPosition {
                Id = ReadInt(data, SFSDefine.SFSField.Id),
                HeroType = heroType,
                Cell = new Vector2Int(ReadInt(data, "i"), ReadInt(data, "j")),
            };
        }

        // Pushes are JSON: small numbers decode as Int32, large ones as Int64, missing keys may throw.
        private static long ReadLong(ISFSObject data, string key, long defaultValue = 0) {
            if (!data.ContainsKey(key)) {
                return defaultValue;
            }
            var value = data.GetData(key)?.Data;
            return value == null ? defaultValue : Convert.ToInt64(value);
        }

        private static int ReadInt(ISFSObject data, string key, int defaultValue = 0) {
            return (int) ReadLong(data, key, defaultValue);
        }

        private class TreasureExplodeData : ITreasureExplode {
            public HeroId HeroId { get; }
            public int BombNo { get; }
            public Vector2Int Cell { get; }
            public int Energy { get; }
            public List<IPveBlockData> DestroyedBlocks { get; }
            public IPveHeroDangerous Dangerous { get; }
            public TrialState IsTrial { get; }
            public List<RewardType> AttendPools { get; }
            public bool HasEnergy { get; }
            public bool MapNowEmpty { get; }
            public long PlantedAt { get; }

            public TreasureExplodeData(ISFSObject data, TreasureHeroPosition hero) {
                HeroId = new HeroId(hero.Id, (HeroAccountType) Math.Max(0, hero.HeroType));
                BombNo = ReadInt(data, "num");
                Cell = hero.Cell;
                HasEnergy = data.ContainsKey(SFSDefine.SFSField.Enegy);
                Energy = ReadInt(data, SFSDefine.SFSField.Enegy);
                MapNowEmpty = data.GetBool("map_now_empty");
                PlantedAt = ReadLong(data, "planted_at");
                DestroyedBlocks = new List<IPveBlockData>();
                var blocks = data.GetSFSArray(SFSDefine.SFSField.BLocks);
                if (blocks != null) {
                    foreach (ISFSObject b in blocks) {
                        DestroyedBlocks.Add(new TreasureBlockData(b));
                    }
                }
                var pools = data.ContainsKey(SFSDefine.SFSField.AttendPools)
                    ? data.GetIntArray(SFSDefine.SFSField.AttendPools)
                    : null;
                AttendPools = pools != null ? pools.Select(item => (RewardType) item).ToList() : new List<RewardType>();
                var dangerousType = (PveDangerousType) ReadInt(data, "is_dangerous");
                Dangerous = new PveHeroDangerous(HeroId, HeroStage.Working, dangerousType);
                if (data.ContainsKey("is_trial")) {
                    IsTrial = data.GetBool("is_trial") ? TrialState.TrialBegin : TrialState.TrialEnd;
                }
            }
        }

        private class TreasureBlockData : IPveBlockData {
            public Vector2Int Coord { get; }
            public int Type { get; }
            public int Hp { get; }
            public int MaxHp { get; }
            public List<ITokenReward> Rewards { get; }

            public TreasureBlockData(ISFSObject data) {
                Coord = new Vector2Int(ReadInt(data, "i"), ReadInt(data, "j"));
                Type = ReadInt(data, "type");
                Hp = ReadInt(data, "hp");
                MaxHp = ReadInt(data, "maxHp");
                Rewards = new List<ITokenReward>();
                var rewards = data.GetSFSArray("rewards");
                if (rewards != null) {
                    foreach (ISFSObject r in rewards) {
                        Rewards.Add(new TokenReward(r));
                    }
                }
            }
        }
    }
}
