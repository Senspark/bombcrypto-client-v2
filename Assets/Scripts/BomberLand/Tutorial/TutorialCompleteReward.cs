using System.Threading.Tasks;

using App;

using UnityEngine;

namespace BomberLand.Tutorial {
    public class TutorialReward {
        public int ItemId { get; }
        public string Footer { get; }

        public TutorialReward(int itemId, string footer) {
            ItemId = itemId;
            Footer = footer;
        }
    }

    public class TutorialCompleteReward : MonoBehaviour {
        [SerializeField]
        private Transform rewardContainer;

        [SerializeField]
        private TutorialRewardItem prefabReward;

        private System.Action _onClaimCallback;

        public void Initialized(TutorialReward[] rewards, System.Action claimCallback) {
            _onClaimCallback = claimCallback;
            foreach (var reward in rewards) {
                CreateReward(reward);
            }
        }

        private void CreateReward(TutorialReward reward) {
            var item = Instantiate(prefabReward, rewardContainer);
            item.SetInfo(reward);
        }

        public void OnClaimButtonClicked() {
            _onClaimCallback?.Invoke();
        }

        public Task<bool> CloseAsync(ISoundManager soundManager) {
            var task = new TaskCompletionSource<bool>();
            _onClaimCallback = () => {
                soundManager.PlaySound(Audio.Tap);
                Destroy(gameObject);
                task.SetResult(true);
            };
            return task.Task;
        }
    }
}