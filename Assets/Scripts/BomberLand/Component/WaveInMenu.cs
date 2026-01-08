using Engine.Utils;

using UnityEngine;

namespace Animation {
    public class WaveInMenu : MonoBehaviour {
        [SerializeField]
        private int myStage;

        [SerializeField]
        private ImageAnimation imageAnimation;

        [SerializeField]
        private WaveRes resource;

        public void PlayAnimation(int mapIndex) {
            var sprites = resource.GetSprite((WaveType) mapIndex);
            imageAnimation.StartLoop(sprites);
        }
    }
}