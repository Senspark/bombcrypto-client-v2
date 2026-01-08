using UnityEngine;

namespace Game.Dialog
{
    public class ItemGettingAnim : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        public void PlayGettingAnimation()
        {
            animator.Play("Getting");
        }
    }
}