using BLPvpMode.Engine.Config;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.Manager;
using BLPvpMode.Engine.User;
using BLPvpMode.Engine.Utility;
using BLPvpMode.Manager;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;

namespace BLPvpMode.Engine.Data {
    public class Match : IMatch {
        public IMatchState State => new MatchState(
            HeroManager.State,
            BombManager.State,
            MapManager.State
        );

        public IHeroManager HeroManager { get; }
        public IBombManager BombManager { get; }
        public IMapManager MapManager { get; }

        public Match(
            [NotNull] IParticipantController[] controllers,
            [NotNull] IMatchTeamInfo[] teamInfo,
            [NotNull] IMatchHeroInfo[] heroInfo,
            [NotNull] IMapInfo mapInfo,
            [NotNull] IHeroConfig heroConfig,
            [NotNull] IMatchState initialState,
            [NotNull] ILogManager logger,
            [NotNull] ITimeManager timeManager,
            [NotNull] IRandom random,
            [CanBeNull] IHeroListener heroListener,
            [CanBeNull] IBombListener bombListener,
            [CanBeNull] IMapListener mapListener
        ) {
            MapManager = DefaultMapManager.CreateMap(
                info: mapInfo,
                logger: logger,
                timeManager: timeManager,
                random: random,
                listener: new MapListener {
                    OnAdded = (block, reason) => mapListener?.OnAdded(block, reason),
                    OnRemoved = (block, reason) => mapListener?.OnRemoved(block, reason),
                }
            );
            BombManager = new DefaultBombManager(
                initialState: initialState.BombState,
                logger: logger,
                mapManager: MapManager,
                timeManager: timeManager,
                listener: new BombListener {
                    OnAdded = (bomb, reason) => bombListener?.OnAdded(bomb, reason),
                    OnRemoved = (bomb, reason) => bombListener?.OnRemoved(bomb, reason),
                    OnExploded = (bomb, ranges) => bombListener?.OnExploded(bomb, ranges),
                    OnDamaged = (position, amount) => {
                        bombListener?.OnDamaged(position, amount);
                        OnBombDamaged(position, amount);
                    },
                }
            );
            HeroManager = new DefaultHeroManager(
                controllers: controllers,
                teamInfo: teamInfo,
                heroInfo: heroInfo,
                initialState: initialState.HeroState,
                logger: logger,
                heroConfig: heroConfig,
                bombManager: BombManager,
                timeManager: timeManager,
                random: random,
                listener: new HeroListener {
                    OnDamaged = (hero, amount, source) => heroListener?.OnDamaged(hero, amount, source),
                    OnHealthChanged =
                        (hero, amount, oldAmount) => heroListener?.OnHealthChanged(hero, amount, oldAmount),
                    OnItemChanged =
                        (hero, item, amount, oldAmount) => heroListener?.OnItemChanged(hero, item, amount, oldAmount),
                    OnEffectBegan =
                        (hero, effect, reason, duration) => heroListener?.OnEffectBegan(hero, effect, reason, duration),
                    OnEffectEnded = (hero, effect, reason) => heroListener?.OnEffectEnded(hero, effect, reason),
                    OnMoved = (hero, position) => heroListener?.OnMoved(hero, position),
                }
            );
        }

        public void ApplyState(IMatchState state) {
            HeroManager.ApplyState(state.HeroState);
            BombManager.ApplyState(state.BombState);
            MapManager.ApplyState(state.MapState);
        }

        private void OnBombDamaged(Vector2Int position, int amount) {
            HeroManager.DamageBomb(position, amount);
        }

        public void Step(int delta) {
            HeroManager.Step(delta);
            BombManager.Step(delta);
        }
    }
}