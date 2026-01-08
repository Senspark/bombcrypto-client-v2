using System;
using App;
using Cysharp.Threading.Tasks;
using Engine.Manager;
using Game.Dialog;
using Game.UI;
using Senspark;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class CharacterItem : MonoBehaviour {
        public struct CharacterItemCallback {
            public Action<CharacterItem> OnClicked;
            public Action<CharacterItem> OnGoHome;
            public Action<CharacterItem> OnGoWork;
            public Action<CharacterItem> OnGoSleep;
        }
        
        [SerializeField]
        private GameObject frameChoose;

        [SerializeField]
        private Avatar avatar;

        [SerializeField]
        private GrayButton goHome;

        [SerializeField]
        private GrayButton goWork;

        [SerializeField]
        private GrayButton goSleep;

        [SerializeField]
        private EnergyBar energyBar;

        [SerializeField]
        private Animator rarityDiamondAnimator;

        [SerializeField]
        private Image shieldImg, haveStakeImg;

        [SerializeField]
        private Sprite[] shieldIcons;

        [SerializeField]
        private Text charaterIdTxt;
        
        public int ItemIndex { get; private set; }
        public PlayerData PlayerData { get; private set; }

        private const float TIME_UPDATE = 60;
        private CharacterItemCallback _characterItemCallback;
        private ISoundManager _audioManager;
        private IHouseStorageManager _houseStoreManager;
        private IPlayerStorageManager _playerStoreManager;
        
        private float _timeProcess = 0;
        private float _houseCharge;
        private static readonly int Rarity = Animator.StringToHash("Rarity");

        private void Awake() {
            _audioManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _houseStoreManager = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
        }

        private void Update() {
            _timeProcess += Time.deltaTime;
            if (_timeProcess > TIME_UPDATE) {
                _timeProcess = 0;
                SetEnergyProcess(PlayerData);
            }
        }

        public void OnClicked() {
            _audioManager.PlaySound(Audio.Tap);
            _characterItemCallback.OnClicked?.Invoke(this);
            EventManager<PlayerData>.AddUnique(StakeEvent.AfterStake, OnStakeEvent);
        }
        
        //Update lại ui của hero này sau khi stake xong
        private void OnStakeEvent(PlayerData player) {
            PlayerData = player;
            avatar.ChangeImage(player);
            if (haveStakeImg != null)
                haveStakeImg.gameObject.SetActive(ThisHeroHaveAnyStake());
            
            energyBar.UpdateUi(player);

        }

        public void OnGoHomeClicked() {
            if (PlayerData.stage == HeroStage.Home) {
                return;
            }

            _audioManager.PlaySound(Audio.Tap);
            HighLightButton(goHome, false);
            _characterItemCallback.OnGoHome?.Invoke(this);
        }

        public void OnGoWorkClicked() {
            if (PlayerData.stage == HeroStage.Working || PlayerData.hp < 1) {
                return;
            }

            _audioManager.PlaySound(Audio.Tap);
            HighLightButton(goWork, false);
            _characterItemCallback.OnGoWork?.Invoke(this);
        }

        public void OnGoSleepClicked() {
            if (PlayerData.stage == HeroStage.Sleep) {
                return;
            }

            _audioManager.PlaySound(Audio.Tap);
            HighLightButton(goSleep, false);
            _characterItemCallback.OnGoSleep?.Invoke(this);
        }

        public void SetChoose(bool value) {
            frameChoose.SetActive(value);
        }

        public void SetHome(PlayerData player) {
            UpdateButtons(player);
        }

        private void UpdateButtons(PlayerData player) {
            HighLightButton(goWork, player.stage == HeroStage.Working);
            HighLightButton(goSleep, player.stage == HeroStage.Sleep);
            HighLightButton(goHome, player.stage == HeroStage.Home);
            UpdateButtonHome(player.stage == HeroStage.Home);
        }

        private static void HighLightButton(GrayButton button, bool isHighLight) {
            if (button == null) {
                return;
            }
            button.SetEnable(!isHighLight);
            button.SetColor(isHighLight ? GrayButton.ButtonColor.Bright : GrayButton.ButtonColor.Dark);
        }

        private void UpdateButtonHome(bool isHighLight) {
            if (goHome == null) {
                return;
            }
            if (isHighLight) {
                return;
            }

            var houseSlot = _houseStoreManager.GetHouseSlot();
            if (houseSlot != 0) {
                var inHome = _playerStoreManager.GetHomePlayerCount();
                var isFullSlot = false;
                if (inHome > 0) {
                    isFullSlot = inHome >= houseSlot;
                }

                if (isFullSlot) {
                    // Nếu có House mà Full Slot thì Gray + ko cho click
                    goHome.SetEnable(false);
                    goHome.SetColor(GrayButton.ButtonColor.Gray);
                } else {
                    // Nếu có House mà còn Slot thì Dark (tối màu) + cho click
                    goHome.SetEnable(true);
                    goHome.SetColor(GrayButton.ButtonColor.Dark);
                }
            } else {
                // Nếu ko có House thì Gray + ko cho click
                goHome.SetColor(GrayButton.ButtonColor.Gray);
                goHome.SetEnable(false);
            }
        }

        private void SetEnergyProcess(PlayerData player) {
            if (player == null) {
                return;
            }
            var hp = _playerStoreManager.SimulatePlayerHpOverTime(player, _houseCharge);
            var shield = player.Shield;
            var currentAmountShield = 0;
            var maxAmountShield = 0;
            if (shield != null) {
                currentAmountShield = player.Shield.CurrentAmount;
                maxAmountShield = player.Shield.TotalAmount;
            }
            energyBar.SetValue(player.Shield != null, hp, player.maxHp, currentAmountShield, maxAmountShield);
        }

        public void SetInfo(PlayerData player, int itemIndex, CharacterItemCallback callback) {
            UniTask.Void(async () => {
                gameObject.SetActive(false);
                PlayerData = player;
                ItemIndex = itemIndex;
                var activeHouse = _houseStoreManager.GetActiveHouseData();
                if (activeHouse != null) {
                    _houseCharge = activeHouse.Charge;
                }

                _characterItemCallback = callback;

                await avatar.ChangeImage(player);
                gameObject.SetActive(true);
                rarityDiamondAnimator.SetInteger(Rarity, player.rare);
                if (charaterIdTxt) {
                    charaterIdTxt.text = $"{player.heroId.Id}";
                }

                // Bỏ dùng Shield trong bản telegram
                if (AppConfig.IsTon()) {
                    player.Shield = null;
                }
            
                if (player.IsHeroS) {
                    var spr = GetShieldIcon(player);
                    shieldImg.sprite = spr;
                    shieldImg.enabled = spr != null;
                } else {
                    shieldImg.enabled = false;
                }

                if (haveStakeImg != null) {
                    haveStakeImg.gameObject.SetActive(ThisHeroHaveAnyStake());
                }

                SetEnergyProcess(player);
                SetHome(player);
            });
        }

        private bool ThisHeroHaveAnyStake() {
            return  PlayerData.HaveAnyStaked();
        }

        private Sprite GetShieldIcon(PlayerData player) {
            if (player.Shield == null) {
                return null;
            }
            var t = player.Shield.CurrentAmount / (float) player.Shield.TotalAmount;
            var spr = t switch {
                > 0.7f => shieldIcons[3],
                > 0.5f => shieldIcons[2],
                > 0.3f => shieldIcons[1],
                _ => shieldIcons[0]
            };
            return spr;
        }

        public void HideAllButtons() {
            if (goHome == null) {
                return;
            }
            goHome.SetActive(false);
            goWork.SetActive(false);
            goSleep.SetActive(false);
        }
    }
}