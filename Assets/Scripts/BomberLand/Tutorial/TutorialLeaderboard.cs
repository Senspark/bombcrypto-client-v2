using UnityEngine;

namespace BomberLand.Tutorial {
    public class TutorialLeaderboard : MonoBehaviour {
        [SerializeField]
        private GameObject frameRank;

        public GameObject FrameRank => frameRank;
        public void Initialized() { }
    }
}