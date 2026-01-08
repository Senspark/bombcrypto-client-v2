using Engine.Entities;
using Engine.Utils;

using UnityEngine;

namespace Engine.Components {
    public class ItemAnimation : MonoBehaviour {
        [SerializeField]
        private Animator animator;

        private IAnimatorHelper _animatorHelper;

        private void Awake() {
            _animatorHelper = new AnimatorHelper(animator);
            var entity = GetComponent<Entity>();
            entity.GetEntityComponent<Updater>()
                .OnBegin(AnimateBlink)
                .OnPause(() => _animatorHelper.Enabled = false)
                .OnResume(() => _animatorHelper.Enabled = true)
                .OnUpdate(delta => _animatorHelper.Update(delta));
        }

        private void AnimateBlink() {
            _animatorHelper.Enabled = true;
            _animatorHelper.Play("Appear");
        }
    }
}