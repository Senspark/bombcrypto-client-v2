using System;

using Engine.Entities;
using Engine.Utils;
using UnityEngine;

namespace Engine.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Updater))]
    public class BrokenAnimation : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        protected IAnimatorHelper animatorHelper;
        private float elapsed;
        protected float duration;
        private float delay;

        private Action callback;

        private void Awake()
        {
            animatorHelper = new AnimatorHelper(animator);
            var entity = GetComponent<Entity>();

            entity.GetEntityComponent<Updater>()
                .OnBegin(() =>
                {
                    elapsed = 0;
                    animatorHelper.Enabled = false;
                })
                .OnPause(() => animatorHelper.Enabled = false)
                .OnResume(() => animatorHelper.Enabled = true)
                .OnUpdate(delta => {

                    if (animatorHelper.Enabled)
                    {
                        animatorHelper.Update(delta);
                        elapsed += delta;
                        if (elapsed >= duration)
                        {
                            callback?.Invoke();
                            entity.Kill(true);
                        }
                    }
                    else
                    {
                        elapsed += delta;
                        if (elapsed >= delay)
                        {
                            elapsed = 0;
                            AnimateBroken();
                        }
                    }
                });
        }

        public void Init(Action callback, float delay = 0)
        {
            this.callback = callback;
            this.delay = delay;
            if (delay == 0)
            {
                AnimateBroken();
            }
        }

        protected virtual void AnimateBroken()
        {
            animatorHelper.Enabled = true;
            animatorHelper.Play("Broken");
            duration = animatorHelper.GetClipLength("Broken");
        }
    }
}
