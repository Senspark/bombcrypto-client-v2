using System;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using TMPro;
using UnityEngine;

namespace Game.Dialog {
    public class DialogConfirmReactiveHouse : Dialog {
        [SerializeField]
        private TMP_Text houseName, amount;

        private ISoundManager _soundManager;
        private IChestRewardManager _chestRewardManager;
        private IUserTonManager _userTonManager;
        private IHouseStorageManager _houseStorageManager;
        private IUserSolanaManager _userSolanaManager;
        
        private HouseData _thisHouse;
        private Action _onReactiveHouse;
        private bool _isClicked;


        public static UniTask<DialogConfirmReactiveHouse> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogConfirmReactiveHouse>();
        }

        protected override void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _userTonManager = ServiceLocator.Instance.Resolve<IServerManager>().UserTonManager;
            _houseStorageManager = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
            _userSolanaManager = ServiceLocator.Instance.Resolve<IServerManager>().UserSolanaManager;

            base.Awake();
        }

        public void SetInfo(HouseData house, int index, Action onReactiveHouse) {
            _onReactiveHouse = onReactiveHouse;
            houseName.text = $"Unlocking {DefaultHouseStoreManager.GetHouseName(house.HouseType)}";
            amount.text = Mathf.CeilToInt((float)(house.Price * 0.5)).ToString();
            _thisHouse = house;
        }

        public void OnBtnCancel() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }


        protected override void OnYesClick() {
            if (_isClicked)
                return;
            _isClicked = true;
            OnBtnOk();
        }

        public async void OnBtnOk() {
            _soundManager.PlaySound(Audio.Tap);
            if (!CanReactiveHouse()) {
                var dialog = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.BCoinDeposited);
                dialog.Show(DialogCanvas);
                _isClicked = false;
                return;
            }

            //ReactiveHouse
            if (AppConfig.IsTon()) {
                ReactiveHouseTon();
            } else {
                ReactiveHouseSol();
            }
        }

        private async void ReactiveHouseTon() {
            var result = await _userTonManager.ReactiveHouse(_thisHouse.id);
            if (!result) {
                DialogError.ShowError(DialogCanvas, "Reactive House Failed", () => { _isClicked = false; });
                return;
            }
            _houseStorageManager.UpdateLockedHouse(_thisHouse);

            var dialogNew = await DialogNewHouseAirdrop.Create();
            var house = DefaultHouseStoreManager.GetHouseInfo((int)_thisHouse.HouseType);
            house.id = _thisHouse.id;
            house.genID = _thisHouse.genID;
            house.isActive = _thisHouse.isActive;
            house.Charge = _thisHouse.Charge;
            house.Slot = _thisHouse.Slot;

            _houseStorageManager.UpdateHouse(house);
            dialogNew.SetInfo(house, "HOUSE UNLOCKED");
            dialogNew.Show(DialogCanvas);
            _onReactiveHouse?.Invoke();
            Hide();
        }

        private async void ReactiveHouseSol() {
            var result = await _userSolanaManager.ReactiveHouseSol(_thisHouse.id);
            if (!result) {
                DialogError.ShowError(DialogCanvas, "Reactive House Failed");
                return;
            }
            _houseStorageManager.UpdateLockedHouse(_thisHouse);
            
            var dialogNew = await DialogNewHouseAirdrop.Create();
            var house = DefaultHouseStoreManager.GetHouseInfo((int)_thisHouse.HouseType);
            house.id = _thisHouse.id;
            house.genID = _thisHouse.genID;
            house.isActive = _thisHouse.isActive;
            house.Charge = _thisHouse.Charge;
            house.Slot = _thisHouse.Slot;
                
            _houseStorageManager.UpdateHouse(house);
            dialogNew.SetInfo(house, "HOUSE UNLOCKED");
            dialogNew.Show(DialogCanvas);
            _onReactiveHouse?.Invoke();
            Hide();
        }
        
        private bool CanReactiveHouse() {
            if (!float.TryParse(amount.text, out var price))
                return false;

            var balance = _chestRewardManager.GetChestReward(BlockRewardType.BCoinDeposited);
            if (balance >= price) {
                return true;
            }
            return false;
        }
    }
}