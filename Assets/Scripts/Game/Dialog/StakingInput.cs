using System.Globalization;

using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class StakingInput : MonoBehaviour {
        public delegate bool CanStake(float value);
        public delegate void Stake(bool max, BlockRewardType rewardType, float value);

        [SerializeField]
        private BlockRewardType blockRewardType;

        [SerializeField]
        private Button button;

        [SerializeField]
        private InputField inputField;

        private CanStake _canStake;
        private IChestRewardManager _chestRewardManager;
        private bool _max;
        private Stake _stake;
        private float _value;

        private void Awake() {
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            button.interactable = false;
            inputField.onValueChanged.AddListener(OnValueChanged);
        }

        public void Initialize(CanStake canStake, Stake stake) {
            _canStake = canStake;
            _stake = stake;
        }

        public void OnButtonMaxClicked() {
            _value = _chestRewardManager.GetChestReward(blockRewardType);
            inputField.SetTextWithoutNotify($"{_value}");
            _max = true;
            UpdateButton();
        }

        public void OnButtonStakeClicked() {
            _stake(_max, blockRewardType, _value);
        }

        private void OnValueChanged(string value) {
            _max = false;
            if (value == string.Empty) {
                _value = 0;
            } else {
                if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var iValue)) {
                    _value = iValue;
                } else {
                    inputField.SetTextWithoutNotify($"{_value}");
                }
            }
            UpdateButton();
        }

        private void UpdateButton() {
            button.interactable = _canStake(_value) &&
                                  (_max || _value <= _chestRewardManager.GetChestReward(blockRewardType));
        }
    }
}