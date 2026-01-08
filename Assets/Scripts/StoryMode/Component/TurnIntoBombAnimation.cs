using System;

using Engine.Entities;
using Engine.Utils;
using UnityEngine;

namespace Engine.Components
{
    public sealed class TurnIntoBombAnimation : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        private IAnimatorHelper _animatorHelper;
        private float _elapsed;
        private float _duration;

        private System.Action _onEndCallback;
        private readonly string animName = "TurnIntoBomb";

        private void Awake()
        {
            _animatorHelper = new AnimatorHelper(animator);
            var entity = GetComponent<Entity>();
            var updater = new Updater()
                .OnBegin(AnimateExplode)
                .OnPause(() => _animatorHelper.Enabled = false)
                .OnResume(() => _animatorHelper.Enabled = true)
                .OnUpdate(delta => {
                    if (!_animatorHelper.Enabled) {
                        return;
                    }
                    _animatorHelper.Update(delta);
                    _elapsed += delta;
                    if (!(_elapsed >= _duration)) {
                        return;
                    }
                    _onEndCallback.Invoke();
                    entity.Kill(true);
                });
            entity.AddEntityComponent<Updater>(updater);
        }

        public void SetCalback(System.Action callback) {
            _onEndCallback = callback;
        }
        
        private void AnimateExplode()
        {
            _animatorHelper.Enabled = true;
            _animatorHelper.Play(animName);
            _duration = _animatorHelper.GetClipLength(animName);
        }
    }
}