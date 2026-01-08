using System;

using Engine.Entities;
using Engine.Utils;
using UnityEngine;

namespace Engine.Components
{
        public sealed class ExplodeAnimation : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private string animName = "Explode";
        
        private IAnimatorHelper _animatorHelper;
        private float _elapsed;
        private float _duration;

        private Entity _entity;
        
        private void Awake()
        {
            _animatorHelper = new AnimatorHelper(animator);
            _entity = GetComponent<Entity>();

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
                    _entity.Kill(true);
                });
            _entity.AddEntityComponent<Updater>(updater);
        }

        private void AnimateExplode()
        {
            _animatorHelper.Enabled = true;
            _animatorHelper.Play(animName);
            _duration = _animatorHelper.GetClipLength(animName);
        }
    }
}
