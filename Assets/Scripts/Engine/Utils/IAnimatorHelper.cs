using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Engine.Utils
{
    public interface IAnimatorHelper
    {
        bool Enabled { get; set; }
        void Update(float delta);
        void Play(string stateName);
        bool IsPlaying();
        bool IsPlaying(string stateName);
        float GetClipLength(int index);
        float GetClipLength(string name);
        bool ClipAvailable(string name);
    }

    public class AnimatorHelper : IAnimatorHelper
    {
        private readonly Animator animator;
        private readonly AnimationClip[] _clips;
        private readonly List<string> _clipNames;
        private readonly HashSet<string> _clipNameSet;

        public bool Enabled { get; set; }

        public AnimatorHelper(Animator animator)
        {
            this.animator = animator;
            _clips = this.animator.runtimeAnimatorController.animationClips;
            _clipNames = _clips.Select(item => item.name).ToList();
            _clipNameSet = _clipNames.ToHashSet();
            animator.speed = 0;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            // animator.speed = 1;
            // animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.CullCompletely;
        }

        public void Update(float delta)
        {
            if (!Enabled)
            {
                return;
            }
            if (!animator.isActiveAndEnabled)
            {
                return;
            }
            animator.speed = 1;
            animator.Update(delta);
            animator.speed = 0;
        }

        public void Play(string stateName)
        {
            if (!animator.isActiveAndEnabled)
            {
                return;
            }
            if (animator.GetCurrentAnimatorStateInfo(0).loop)
            {
                animator.Play(stateName);
            }
            else
            {
                animator.Play(stateName, -1, 0f);
            }
        }

        public bool IsPlaying()
        {
            // https://answers.unity.com/questions/362629/how-can-i-check-if-an-animation-is-being-played-or.html
            return animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
        }

        public bool IsPlaying(string stateName)
        {
            return IsPlaying() && animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
        }

        public float GetClipLength(int index)
        {
            // https://answers.unity.com/questions/692593/get-animation-clip-length-using-animator.html
            return _clips[index].length;
        }

        public float GetClipLength(string name) {
            for (var i = 0; i < _clips.Length; i++) {
                if (_clipNames[i] == name) {
                    return _clips[i].length;
                }
            }
            return 0;
        }

        public bool ClipAvailable(string name) {
            return _clipNameSet.Contains(name);
        }
    }

    public class MultiAnimatorHelper : IAnimatorHelper
    {
        private readonly IAnimatorHelper helper;
        private readonly IAnimatorHelper[] subHelpers;

        public bool Enabled
        {
            get => helper.Enabled;
            set
            {
                helper.Enabled = value;
                foreach (var item in subHelpers)
                {
                    item.Enabled = value;
                }
            }
        }

        public MultiAnimatorHelper(Animator animator, params Animator[] subAnimators)
        {
            helper = new AnimatorHelper(animator);
            subHelpers = subAnimators.Select(item => new AnimatorHelper(item) as IAnimatorHelper).ToArray();
        }

        public float GetClipLength(int index)
        {
            return helper.GetClipLength(index);
        }

        public float GetClipLength(string name)
        {
            return helper.GetClipLength(name);
        }

        public bool ClipAvailable(string name)
        {
            return helper.ClipAvailable(name);
        }

        public bool IsPlaying()
        {
            return helper.IsPlaying();
        }

        public bool IsPlaying(string stateName)
        {
            return helper.IsPlaying(stateName);
        }

        public void Play(string stateName)
        {
            helper.Play(stateName);
            foreach (var item in subHelpers)
            {
                item.Play(stateName);
            }
        }

        public void Update(float delta)
        {
            helper.Update(delta);
            foreach (var item in subHelpers)
            {
                item.Update(delta);
            }
        }
    }
}