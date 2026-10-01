using System.Collections.Generic;

using UnityEngine;

namespace App {
    // Server-driven treasure mode wire models (docs/treasure-server-driven-client-guide.md).
    public enum TreasureEventType {
        Unknown,
        Move,
        Plant,
        Explode,
        HeroJoin,
        HeroLeave,
        NewMap,
        Resync,
    }

    public class TreasureHeroPosition {
        public int Id { get; set; }

        // -1 when the server no longer knows the hero.
        public int HeroType { get; set; }
        public Vector2Int Cell { get; set; }
    }

    public class TreasureBombInfo {
        public int HeroId { get; set; }
        public int Num { get; set; }
        public Vector2Int Cell { get; set; }
        public long PlantedAt { get; set; }
        public long ExplodeAt { get; set; }
    }

    public interface ITreasureExplode : IPveExplodeResponse {
        // Absent when the hero is no longer credited (asleep, sent home, ...).
        bool HasEnergy { get; }
        bool MapNowEmpty { get; }
        long PlantedAt { get; }
    }

    // START_TREASURE_MODE response, also the body of a RESYNC event.
    public class TreasureSnapshot : IStartPveResponse {
        public IMapDetails Map { get; set; }
        public List<TreasureHeroPosition> Heroes { get; set; } = new();
        public List<TreasureBombInfo> Bombs { get; set; } = new();
        public long Seq { get; set; }
        public long ServerTime { get; set; }
        public long FuseMs { get; set; }
        public bool AwaitingNewMap { get; set; }
        public long ResumeAt { get; set; }
        public List<IPveHeroDangerous> DangerousData { get; set; } = new();
        public TrialState IsTrial { get; set; }
    }

    public class TreasureEvent {
        public long Seq { get; set; }
        public TreasureEventType Type { get; set; }
        public string RawType { get; set; }
        public long At { get; set; }

        // MOVE / PLANT / EXPLODE / HERO_JOIN / HERO_LEAVE
        public TreasureHeroPosition Hero { get; set; }

        // MOVE
        public List<Vector2Int> Path { get; set; }
        public long StepMs { get; set; }

        // PLANT / EXPLODE
        public int Num { get; set; }
        public long PlantedAt { get; set; }
        public long ExplodeAt { get; set; }
        public ITreasureExplode Explode { get; set; }

        // HERO_LEAVE / RESYNC
        public string Reason { get; set; }

        // NEW_MAP
        public IMapDetails Map { get; set; }
        public List<TreasureHeroPosition> Heroes { get; set; }
        public long ResumeAt { get; set; }

        // RESYNC
        public TreasureSnapshot Snapshot { get; set; }
    }
}
