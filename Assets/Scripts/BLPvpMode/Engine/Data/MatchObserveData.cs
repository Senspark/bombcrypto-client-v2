using BLPvpMode.Engine.Delta;

using JetBrains.Annotations;

using Sfs2X.Entities.Data;

namespace BLPvpMode.Engine.Data {
    public class MatchObserveData : IMatchObserveData {
        public static IMatchObserveData FastParse(ISFSObject data) {
            var id = data.GetInt("id");
            var timestamp = data.GetLong("timestamp");
            var matchId = data.GetUtfString("match_id");
            var heroesArray = data.GetSFSArray("heroes");
            var heroes = new IHeroStateDelta[heroesArray.Count];
            for (var i = 0; i < heroesArray.Count; ++i) {
                var item = heroesArray.GetSFSObject(i);
                heroes[i] = HeroStateDelta.Parse(item);
            }
            var bombsArray = data.GetSFSArray("bombs");
            var bombs = new IBombStateDelta[bombsArray.Count];
            for (var i = 0; i < bombsArray.Count; ++i) {
                var item = bombsArray.GetSFSObject(i);
                bombs[i] = BombStateDelta.Parse(item);
            }
            var blocksArray = data.GetSFSArray("blocks");
            var blocks = new IBlockStateDelta[blocksArray.Count];
            for (var i = 0; i < blocksArray.Count; ++i) {
                var item = blocksArray.GetSFSObject(i);
                blocks[i] = BlockStateDelta.Parse(item);
            }
            return new MatchObserveData(
                id: id, //
                timestamp: timestamp,
                matchId: matchId,
                heroDelta: heroes,
                bombDelta: bombs,
                blockDelta: blocks
            );
        }

        public int Id { get; }
        public long Timestamp { get; }
        public string MatchId { get; }
        public IHeroStateDelta[] HeroDelta { get; }
        public IBombStateDelta[] BombDelta { get; }
        public IBlockStateDelta[] BlockDelta { get; }

        public MatchObserveData(
            int id,
            long timestamp,
            [NotNull] string matchId,
            IHeroStateDelta[] heroDelta,
            IBombStateDelta[] bombDelta,
            IBlockStateDelta[] blockDelta
        ) {
            Id = id;
            Timestamp = timestamp;
            MatchId = matchId;
            HeroDelta = heroDelta;
            BombDelta = bombDelta;
            BlockDelta = blockDelta;
        }
    }
}