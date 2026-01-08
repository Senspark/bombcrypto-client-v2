using System;
using System.Collections;
using System.Collections.Generic;

using Engine.Entities;
using Engine.Utils;

using UnityEngine;

namespace Engine.Components {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Updater))]
    public class ExplosionAnimation : MonoBehaviour {
        private static readonly Dictionary<ExplosionPose, string> _dictionary = new() {
            { ExplosionPose.Center, "ExplosionCenter" },
            { ExplosionPose.MidleHori, "ExplosionMidleHori" },
            { ExplosionPose.MidleVert, "ExplosionMidleVert" },
            { ExplosionPose.EndRight, "ExplosionEndRight" },
            { ExplosionPose.EndLeft, "ExplosionEndLeft" },
            { ExplosionPose.EndUp, "ExplosionEndUp" },
            { ExplosionPose.EndDown, "ExplosionEndDown" },
        };

        [SerializeField]
        private Animator animator;

        private IAnimatorHelper animatorHelper;
        private float elapsed;
        private float duration;

        private Action callback;
        private Entity _entity;

        private void Awake() {
            animatorHelper = new AnimatorHelper(animator);
            _entity = GetComponent<Entity>();
            _entity.GetEntityComponent<Updater>()
                .OnBegin(() => { elapsed = 0; })
                .OnPause(() => animatorHelper.Enabled = false)
                .OnResume(() => animatorHelper.Enabled = true)
                .OnUpdate(OnUpdate);
        }

        private void OnEnable() {
            elapsed = 0;
        }

        public void Init(ExplosionPose pose, Action callback) {
            animatorHelper.Enabled = true;

            this.callback = callback;
            animatorHelper.Play(GetAnimateName(pose));
            duration = animatorHelper.GetClipLength(0);
        }

        //--------------------------------
        private void OnUpdate(float delta) {
            if (!animatorHelper.Enabled) {
                return;
            }

            animatorHelper.Update(delta);
            elapsed += delta;
            if (!(elapsed >= duration)) {
                return;
            }

            callback?.Invoke();
            _entity.Kill(true);
        }

        private string GetAnimateName(ExplosionPose pos) {
            return _dictionary[pos];
        }
    }
}