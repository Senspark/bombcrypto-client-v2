using Engine.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BLPvpMode.UI {
    public class BLHeroReadyCom : MonoBehaviour
    {
        [SerializeField]
        public Avatar avatar;

        [SerializeField]
        public BLBoosterUI boosterDisplay;

        [SerializeField]
        public Text addressText;
        
        [SerializeField]
        public Text ranks;

        [SerializeField]
        public Image rankIcons;
        
        [SerializeField]
        public Text readyText;
        
        [SerializeField]
        public ImageAnimation avatarTR;
    }
}
