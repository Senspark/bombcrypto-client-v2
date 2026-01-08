using System;

using App;

using Engine.Components;

using UnityEngine;
using UnityEngine.Assertions;
using DG.Tweening;

using UnityEngine.PlayerLoop;

using Utils;

namespace StoryMode.Entities
{
    public class GhostDie : Engine.Entities.Entity {

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private bool isPlayer;

        private int ExplosionLength { set; get; } = 3;
        private float TimeToExplode { set; get; } = 3.0f;
        private bool _isExitFromDoor;
        private HeroId _id;
        private float _damage;
        private int _bombSkin;
        private int _explosionSkin;

        private void Awake() {
            AddEntityComponent<Updater>(new Updater());
        }

        public void InitForDie(Sprite sprite, bool isExit = false) {
            spriteRenderer.sprite = sprite;
            _isExitFromDoor = isExit;
            var blink = GetComponent<BlinkEffect>();
            Assert.IsNotNull(blink);
            blink.StartBlink(DimToDie);
        }

        public void InitForTurnToBomb(HeroId id, int bombSkin, int explosionSkin, float damage) {
            _id = id;
            _bombSkin = bombSkin;
            _explosionSkin = explosionSkin;
            _damage = damage;
            GetComponent<TurnIntoBombAnimation>().SetCalback(OnTurnToBomb);
        }
        
        private void DimToDie() {
            var fade = spriteRenderer.DOFade(0, 2);
            DOTween.Sequence()
                .Append(fade)
                .AppendCallback(() => {
                    if (isPlayer) {
                        if (_isExitFromDoor) {
                            //Không gọi trong KillTrigger nữa mà sẽ gọi sau khi delay 4s trước đó.
                            //EntityManager.PlayerManager.OnAfterPlayerEnterTheDoor();
                        } else {
                            EntityManager.PlayerManager.OnAfterPlayerDie();
                        }
                    }
                    Kill(false);
                });
        }

        private void OnTurnToBomb() {
            var bomb = EntityManager.LevelManager.CreateBomb(transform.localPosition);
            bomb.Init(0, _id, _bombSkin, _explosionSkin,_damage, 0, ExplosionLength, TimeToExplode, false, null, true);
        }
    }
}