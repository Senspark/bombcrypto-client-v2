using System.Collections.Generic;

using App;

using Constant;

using Cysharp.Threading.Tasks;

using Engine.Entities;

using Senspark;

using UnityEngine;

namespace Engine.Manager {
    public class TakeDamageEvent{
        public int IdxPlayer { get; }
        public int KillerId { get; }
        public Entity Dealer { get; }
        public Vector2Int LastPlayerLocation { get; }
        public TakeDamageEvent(int idxPlayer, int killerId, Entity dealer, Vector2Int lastPlayerLocation) {
            IdxPlayer = idxPlayer;
            KillerId = killerId;
            Dealer = dealer;
            LastPlayerLocation = lastPlayerLocation;
        }
    }
    public class DefaultTakeDamageEventManager : ITakeDamageEventManager
    {
        protected readonly IEntityManager EntityManager;
        private readonly List<TakeDamageEvent> _takeDamageList;
        public DefaultTakeDamageEventManager(IEntityManager entityManager) {
            EntityManager = entityManager;
            _takeDamageList = new List<TakeDamageEvent>();
        }
        public void PushEvent(int idxPlayer, int killerId, Entity dealer, Vector2Int lastPlayerLocation) {
            _takeDamageList.Add(new TakeDamageEvent(idxPlayer, killerId, dealer, lastPlayerLocation));
        }
        
        private TakeDamageEvent PopEvent() {
            if (_takeDamageList.Count == 0) {
                return null;
            }
            var takeDamageEvent = _takeDamageList[0];
            _takeDamageList.RemoveAt(0);
            return takeDamageEvent;
        }

        public void UpdateProcess() {
           ProcessEvents(PopEvent());
        }

        private void ProcessEvents(TakeDamageEvent takeDamageEvent) {
            if(takeDamageEvent == null) {
                return;
            }
            var entityManager = EntityManager;
            var playerTakeDmg = entityManager.PlayerManager.GetPlayerBySlot(takeDamageEvent.IdxPlayer);
            if (!GameConstant.AdventureRequestServer) {
                playerTakeDmg.PlayAnimTakeDamage();
            }
            UniTask.Void(async () => {
                var storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
                var hero = await storyModeManager.HeroTakeDamage(playerTakeDmg.HeroId, takeDamageEvent.KillerId);
                if (hero.Id == playerTakeDmg.HeroId.Id) {
                    entityManager.PlayerManager.SaveHeroTakeDamageInfo(hero.AllowRevive, hero.AllowReviveByAds, hero.ReviveGemAmount, takeDamageEvent.LastPlayerLocation);
                    playerTakeDmg.TakeDamage(hero.StoryHp, takeDamageEvent.Dealer);
                }
            });
        }
        
        /*
        private void RequestTakeDamage(int ownerId, Entity dealer) {
            if (!GameConstant.AdventureRequestServer) {
                _hasAnimation.PlayTakeDamage();
            }
            EE.Utils.NoAwait(async () => {
                _soundManager.PlaySound(Audio.HeroTakeDamage);
                var entityManager = EntityManager;
                var hero = await _storyModeManager.HeroTakeDamage(HeroId, ownerId);
                if (hero.Id == HeroId.Id) {
                    entityManager.PlayerManager.SaveHeroTakeDamageInfo(hero.AllowRevive, hero.AllowReviveByAds, hero.ReviveGemAmount);
                    TakeDamage(hero.StoryHp, dealer);
                }
            });
        }
        */
    }
}
