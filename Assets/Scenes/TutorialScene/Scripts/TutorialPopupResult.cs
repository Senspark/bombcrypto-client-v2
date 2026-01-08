using System.Threading.Tasks;

using App;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Tutorial {
    public class TutorialPopupResult : MonoBehaviour {
        [SerializeField]
        private GameObject rankIcon;

        [SerializeField]
        private GameObject btNext;

        [SerializeField]
        private Text lbRank;

        [SerializeField]
        private GameObject boosterBonus;

        public GameObject RankIcon => rankIcon;
        public GameObject BtNext => btNext;

        public GameObject BoosterBonus => boosterBonus;

        private System.Action _onNextCallback;

        private void Awake() {
            btNext.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }

        public void Initialized(System.Action nextCallback) {
            _onNextCallback = nextCallback;
        }

        public void OnNextButtonClicked() {
            _onNextCallback?.Invoke();
        }

        public Task<bool> WaitClose(ISoundManager soundManager) {
            var task = new TaskCompletionSource<bool>();
            var bt = btNext.GetComponent<UnityEngine.UI.Button>();
            bt.interactable = true;
            _onNextCallback = () => {
                soundManager.PlaySound(Audio.Tap);
                bt.interactable = false;
                task.SetResult(true);
            };
            return task.Task;
        }

        public void SetRank(int rank) {
            lbRank.text = $"{rank}";
        }
    }
}