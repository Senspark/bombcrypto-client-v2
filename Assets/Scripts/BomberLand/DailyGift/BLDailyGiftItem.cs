using System;
using System.Collections.Generic;

using App;

using Data;

using Game.UI.Animation;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.DailyGift {
    public class BLDailyGiftItem : MonoBehaviour {
        public delegate void Claim();

        public delegate Sprite GetSprite(int itemId, string rewardType);

        [SerializeField]
        private Text sttText;

        [SerializeField]
        private ButtonZoomAndFlash buttonClaim;

        [SerializeField]
        private GameObject claimed;

        [SerializeField]
        private GameObject countdown;

        [SerializeField]
        private Text countdownText;

        [SerializeField]
        private GameObject lockedClaim;

        [SerializeField]
        private Transform layout;

        [SerializeField]
        private Text random;

        [SerializeField]
        private BLDailyGiftReward reward;

        private Claim _claim;
        private DailyRewardData _data;
        private GetSprite _getSprite;
        private IDictionary<string, Sprite> _randomIcons;

        public void Initialize(
            Claim claim,
            GetSprite getSprite,
            IDictionary<string, Sprite> randomIcons
        ) {
            _claim = claim;
            _getSprite = getSprite;
            _randomIcons = randomIcons;
            buttonClaim.SetActive(false);
        }

        public void OnClaimButtonClicked() {
            _claim();
        }

        private void Update() {
            var duration = _data.ClaimTime - DateTime.UtcNow;
            countdown.SetActive(duration.Ticks > 0);
            countdownText.text = TimeUtil.ConvertTimeToString(duration);

            buttonClaim.SetActive(
                !lockedClaim.activeSelf &&
                !claimed.activeSelf &&
                !countdown.activeSelf
            );
        }

        public void SetSttText(int number) {
            sttText.text = $"{number}";
        }

        public void UpdateData(DailyRewardData data) {
            _data = data;
            UpdateStatus(_data.Status);
        }

        private void UpdateStatus(DailyRewardData.DailyRewardStatus status) {
            claimed.SetActive(status == DailyRewardData.DailyRewardStatus.Claimed);
            lockedClaim.SetActive(status == DailyRewardData.DailyRewardStatus.Locked);
        }

        public void UpdateUI() {
            if (_data.Items.Length > 0) {
                foreach (var it in _data.Items) {
                    var instance = Instantiate(reward, layout);
                    instance.UpdateUI(_getSprite(it.Id, it.Type), it.Quantity);
                }
            } else {
                var instance = Instantiate(reward, layout);
                if (_randomIcons.ContainsKey(_data.RandomIcon)) {
                    instance.UpdateUI(_randomIcons[_data.RandomIcon], 0);    
                }
            }
            random.text = $"RANDOM {_data.RandomIcon}";
            random.gameObject.SetActive(_data.Randomize);
        }
    }
}