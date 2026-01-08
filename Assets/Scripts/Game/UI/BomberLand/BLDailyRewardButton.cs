using System;
using System.Threading.Tasks;

using Data;

using Engine.Utils;

using Senspark;

using Services;

using UnityEngine;

namespace Game.UI {
    public class BLDailyRewardButton : MonoBehaviour {
        [SerializeField]
        private AnimationZoom redDot;

        private IDailyRewardManager _dailyRewardManager;

        private void Awake() {
            _dailyRewardManager = ServiceLocator.Instance.Resolve<IDailyRewardManager>();
        }

        private void OnDestroy() {
            redDot = null;
        }

        public async Task CheckRedDot() {
            await _dailyRewardManager.UpdateDataAsync();
            // redDot is null => did destroy
            if(!redDot) {
                return;
            }
            UpdateRedDot();
        }

        private void UpdateRedDot() {
            var hasClaimable = false;
            foreach (var dailyReward in _dailyRewardManager) {
                if (dailyReward.Status is not (DailyRewardData.DailyRewardStatus.Countdown
                    or DailyRewardData.DailyRewardStatus.None)) {
                    continue;
                }
                var duration = dailyReward.ClaimTime - DateTime.UtcNow;
                hasClaimable = duration.Ticks <= 0;
                break;
            }
            if (hasClaimable) {
                redDot.gameObject.SetActive(true);
                redDot.Play();
            } else {
                redDot.gameObject.SetActive(false);
            }
        }
    }
}