using System;

using Engine.Entities;
using Engine.Utils;

using UnityEngine;

namespace Engine.Components {
    public class PrisonAnimation : MonoBehaviour {
        [SerializeField]
        private Animator animator;

        private IAnimatorHelper AnimatorHelper;
        private float _elapsed;
        private float _duration;
        private float _delay;
        private bool _isBroken;

        private Action _callback;

        private void Awake() {
            AnimatorHelper = new AnimatorHelper(animator);
            var entity = GetComponent<Entity>();
            entity.GetEntityComponent<Updater>()
                .OnBegin(() => {
                    _elapsed = 0;
                    AnimatorHelper.Enabled = true;
                })
                .OnPause(() => AnimatorHelper.Enabled = false)
                .OnResume(() => AnimatorHelper.Enabled = true)
                .OnUpdate(delta => {
                    if (AnimatorHelper.Enabled) {
                        AnimatorHelper.Update(delta);
                    }
                    if (!_isBroken) {
                        return;
                    }
                    
                    _elapsed += delta;
                    if (!(_elapsed >= _duration)) {
                        return;
                    }
                    
                    _callback?.Invoke();
                    entity.Kill(true);
                });
        }

        public void Init(Action callback) {
            _callback = callback;
        }

        public void DoBroken(Action callback = null) {
            if (callback != null) {
                _callback = callback;
            }
            AnimateBroken();
        }
        
        protected virtual void AnimateBroken() {
            _isBroken = true;
            AnimatorHelper.Play("Broken");
            _duration = AnimatorHelper.GetClipLength("Broken");
        }
    }
}