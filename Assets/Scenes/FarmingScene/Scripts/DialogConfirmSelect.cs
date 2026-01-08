using System;

using App;

using Cysharp.Threading.Tasks;

using Engine.Input;
using Engine.Manager;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogConfirmSelect : Dialog {
        [SerializeField]
        private TMP_Text amountText, amountSenText, bcoinFulltext, senFullText;

        [SerializeField]
        private Button btnYes, btnNo;

        private ISoundManager _soundManager;
        private IBlockchainManager _blockchainManager;
        private PlayerData _selectedPlayerData;
        private Action _onYesBtnClicked;
        private IInputManager _inputManager;
        private bool _isClicked;

        public static UniTask<DialogConfirmSelect> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogConfirmSelect>();

        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
        }

        //Khi chưa lấy đc số stake của hero này hiện tạm 0 và ko cho nhấn nút
        public void FirstShow() {
            amountText.text = "0";
            EnableButton(false);
        }

        /// <summary>
        /// Sau khi lấy đc số stake của hero này thì cho phéo nhấn yes hoặc unstake
        /// </summary>
        /// <param name="player">Player Data</param>
        /// <param name="onYesBtnClicked">Event đc gọi nếu player nhấn yes</param>
        public async void SetInfo(PlayerData player, Action onYesBtnClicked) {
            _selectedPlayerData = player;
            //var amountStake = await _blockchainManager.GetStakeFromHeroId(player.heroId.Id);
            var amountStakeBcoin = player.stakeBcoin;
            bcoinFulltext.text = (Math.Floor(amountStakeBcoin * 1e9) / 1e9).ToString("0.#########");
            var amount = amountStakeBcoin.ToString("0.######");
            amountText.text = $"<sprite=0>{amount}";

            senFullText.text = (Math.Floor(player.stakeSen * 1e9) / 1e9).ToString("0.#########");
            amountSenText.text = player.stakeSen.ToString("0.######");

            _onYesBtnClicked = onYesBtnClicked;
            EnableButton(true);
        }

        protected override void OnYesClick() {
            OnBtnYes();
        }

        protected override void OnNoClick() {
            OnBtnNo();
        }

    
        protected override void ExtraCheck() {
            if (_inputManager.ReadButton(ControllerButtonName.X)) {
                if (_isClicked)
                    return;
                _isClicked = true;
                OnBtnUnStake();
            }
        }

        public void OnBtnYes() {
            if (IsHiding()) {
                return;
            }
            _soundManager.PlaySound(Audio.Tap);
            _onYesBtnClicked?.Invoke();
            Hide();
        }

        public void OnBtnNo() {
            if (IsHiding()) {
                return;
            }
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        public void OnBtnUnStake() {
            if (IsHiding()) {
                return;
            }
            _soundManager.PlaySound(Audio.Tap);
            //popup stake cho hero L+
            if (!_selectedPlayerData.IsHeroS && _selectedPlayerData.Shield != null) {
                DialogStakeHeroesPlus.Create().ContinueWith(dialogS => {
                    dialogS.Show(_selectedPlayerData, DialogCanvas, GetCallback());                    
                });
            } else {
                //Popup stake cho hero L và S
                DialogStakeHeroesS.Create().ContinueWith(dialogL => {
                    dialogL.Show(_selectedPlayerData, DialogCanvas, GetCallback());    
                });
            }

            gameObject.SetActive(false);
        }

        private StakeCallback.Callback GetCallback() {
            var callback = new StakeCallback()
                //tắt popup stake hiện lại popup này
                .OnHide(() => { gameObject.SetActive(true); })
                //Sau khi bấm stake hoặc unstake thì đóng popup này luôn
                .OnStakeOrUnStakeComplete(Hide)
                //úntake thành công, update lại ui
                .OnUnStakeComplete(player => {
                    Hide();
                    EventManager<PlayerData>.Dispatcher(StakeEvent.AfterStake, player);

                })
                .OnStakeComplete(player => {
                    Hide();
                    EventManager<PlayerData>.Dispatcher(StakeEvent.AfterStake, player);

                })
                .Create();
            return callback;
        }

        private void EnableButton(bool value) {
            btnNo.interactable = value;
            btnYes.interactable = value;
        }
    }
}