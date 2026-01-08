using Engine.Components;
using Engine.Entities;
using Engine.Utils;

using UnityEngine;

namespace PvpMode.Entities {
    public class PvpGhostDie : Entity {

        [SerializeField]
        private Animator animator;
        
        private bool _isOnAfterDie;
        
        private IAnimatorHelper _animatorHelper;
        private float _elapsed;
        private float _duration;
        
        private void Awake() {
            _animatorHelper = new AnimatorHelper(animator);
            _animatorHelper.Enabled = false;
            var updater = new Updater()
                .OnUpdate(delta => {
                    if (_animatorHelper.Enabled)
                    {
                        _animatorHelper.Update(delta);
                        _elapsed += delta;
                        if (_elapsed >= _duration)
                        {
                            if (_isOnAfterDie) {
                                EntityManager.PlayerManager.OnAfterPlayerDie();
                            }
                            Kill(false);
                        }
                    }
                });       
            AddEntityComponent<Updater>(updater);
        }
        
        public void InitForDie(bool isOnAfterDie) {
            _isOnAfterDie = isOnAfterDie;
            PlayAnimation();
        }
        
        private void PlayAnimation() {
            _elapsed = 0;
            _animatorHelper.Play("PlayerPvpGhost");
            _duration = _animatorHelper.GetClipLength("PlayerPvpGhost");
            _animatorHelper.Enabled = true;
        }
    }
}
