using System;
using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Config;
using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.User;
using BLPvpMode.Engine.Utility;
using BLPvpMode.Manager;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    public class HeroManagerState : IHeroManagerState {
        [NotNull]
        public static IHeroManagerState DecodeDelta([NotNull] IHeroStateDelta[] delta) {
            return new HeroManagerState(heroes: delta.Associate(it => {
                var baseState = it.Base;
                var positionState = it.Position;
                return (it.Slot, (IHeroState) new HeroState(
                    baseState == null ? null : HeroBaseState.Decode(baseState.State),
                    positionState == null ? null : HeroPositionState.Decode(positionState.State)
                ));
            }));
        }

        public Dictionary<int, IHeroState> Heroes { get; }

        public HeroManagerState(
            [NotNull] Dictionary<int, IHeroState> heroes
        ) {
            Heroes = heroes;
        }

        public IHeroManagerState Apply(IHeroManagerState state) {
            var items = Heroes.ToDictionary(it => it.Key, it => it.Value);
            state.Heroes.ForEach((key, value) => {
                var item = Heroes[key];
                items[key] = new HeroState(
                    value.BaseState ?? item.BaseState,
                    value.PositionState ?? item.PositionState
                );
            });
            return new HeroManagerState(
                heroes: items
            );
        }
    }

    public class DefaultHeroManager : IHeroManager {
        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly Dictionary<int, IHero> _heroes;

        public IHeroManagerState State {
            get {
                var states = _heroes.MapValues((_, hero) => hero.State);
                return new HeroManagerState(states);
            }
        }

        public DefaultHeroManager(
            [NotNull] IParticipantController[] controllers,
            [NotNull] IMatchTeamInfo[] teamInfo,
            [NotNull] IMatchHeroInfo[] heroInfo,
            [NotNull] IHeroManagerState initialState,
            [NotNull] IHeroConfig heroConfig,
            [NotNull] ILogManager logger,
            [NotNull] IBombManager bombManager,
            [NotNull] ITimeManager timeManager,
            [NotNull] IRandom random,
            [NotNull] IHeroListener listener
        ) {
            _logger = logger;
            _heroes = initialState.Heroes.MapValues((slot, state) => {
                var teamId = Array.FindIndex(teamInfo, info => info.Slots.Contains(slot));
                return (IHero) new Hero(
                    slot: slot,
                    teamId: teamId,
                    initialState: state,
                    controller: controllers[slot],
                    config: heroConfig,
                    info: heroInfo[slot],
                    logger: logger,
                    bombManager: bombManager,
                    timeManager: timeManager,
                    random: random,
                    listener: new HeroListener {
                        OnDamaged = listener.OnDamaged,
                        OnHealthChanged = listener.OnHealthChanged,
                        OnItemChanged = listener.OnItemChanged,
                        OnEffectBegan = listener.OnEffectBegan,
                        OnEffectEnded = listener.OnEffectEnded,
                        OnMoved = (hero, position) => {
                            listener.OnMoved(hero, position);
                            OnHeroMoved(hero, position);
                        },
                    }
                );
            });
        }

        public void ApplyState(IHeroManagerState state) {
            state.Heroes.ForEach((slot, heroState) => {
                var hero = _heroes[slot];
                hero.ApplyState(heroState);
            });
        }

        public IHero GetHero(int slot) {
            return _heroes[slot];
        }

        public void DamageBomb(Vector2Int position, int amount) {
            _heroes.ForEach((_, item) => {
                var itemPositionInt = new Vector2Int(
                    Mathf.FloorToInt(item.Position.x),
                    Mathf.FloorToInt(item.Position.y)
                );
                if (itemPositionInt == position) {
                    item.DamageBomb(amount);
                }
            });
        }

        public void DamageFallingBlock(Vector2Int position) {
            _heroes.ForEach((_, item) => {
                var itemPositionInt = new Vector2Int(
                    Mathf.FloorToInt(item.Position.x),
                    Mathf.FloorToInt(item.Position.y)
                );
                if (itemPositionInt == position) {
                    item.DamageFallingBlock();
                }
            });
        }

        private void OnHeroMoved(IHero hero, Vector2 position) {
            var positionInt = new Vector2Int(
                Mathf.FloorToInt(position.x),
                Mathf.FloorToInt(position.y)
            );
            _heroes.ForEach((_, item) => {
                var itemPositionInt = new Vector2Int(
                    Mathf.FloorToInt(item.Position.x),
                    Mathf.FloorToInt(item.Position.y)
                );
                if (itemPositionInt == positionInt) {
                    if (item.TeamId == hero.TeamId) {
                        item.RescuePrison();
                    } else {
                        item.DamagePrison();
                    }
                }
            });
        }

        public void Step(int delta) {
            _heroes.ForEach((_, hero) => { //
                hero.Update(delta);
            });
        }
    }
}