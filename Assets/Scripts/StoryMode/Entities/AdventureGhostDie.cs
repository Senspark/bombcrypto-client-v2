using Animation;

using Engine.Components;
using Engine.Entities;
using Engine.Manager;
using Engine.Utils;

using UnityEngine;

namespace StoryMode.Entities {
    public class AdventureGhostDie : Entity {
        [SerializeField]
        private HeroAnimator heroAnimation;

        [SerializeField]
        private GameObject body;
        
        [SerializeField]
        private RuntimeAnimatorController animatorController;

        private Animator _animator;
        private IAnimatorHelper _animatorHelper;
        private float _elapsed;
        private float _duration;
        
        private void Awake() {
            var updater = new Updater()
                .OnUpdate(delta => {
                    if (_animator == null) {
                        return;
                    }
                    if (!_animatorHelper.Enabled) {
                        return;
                    }
                    _animatorHelper.Update(delta);
                    _elapsed += delta;
                    if (_elapsed >= _duration)
                    {
                        EntityManager.PlayerManager.OnAfterPlayerDie();
                        Kill(true);
                    }
                });    
            AddEntityComponent<Updater>(updater);
        }

        public void InitForDie(PlayerType playerType, PlayerColor playerColor) {
            heroAnimation.SetTypeAndColor(playerType, playerColor);
            heroAnimation.PlayDie(PlayAnimation);
        }
        
        private void PlayAnimation() {
            _animator = body.GetComponent<Animator>();
            if (_animator == null) {
                _animator = body.AddComponent<Animator>();
            }
            _animator.runtimeAnimatorController = animatorController;
            _animatorHelper = new AnimatorHelper(_animator);
            _animatorHelper.Enabled = false;
            
            _elapsed = 0;
            _animatorHelper.Play("PlayerGhost");
            _duration = _animatorHelper.GetClipLength("PlayerGhost");
            _animatorHelper.Enabled = true;
        }
    }
}