using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data;
using Game.Dialog;
using Game.Dialog.BomberLand.BLFrameShop;
using Game.Dialog.BomberLand.BLGacha;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.MainMenuScene.Scripts {
    public class BLDialogGachaChest : Dialog {
        private IInputManager _inputManager;

        private static UniTask<BLDialogGachaChest> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogGachaChest>();
        }
        
        public static async UniTask<BLDialogGachaChest> CreateByPrefab(Canvas canvas, GachaChestItemData[] data, GachaChestShopData chestData) {
            var dialog = await BLDialogGachaChest.Create();
            dialog.Initialize(canvas, data, chestData);
            return dialog;
        }
        
        public static async UniTask<BLDialogGachaChest> CreateFromChestInventory(GachaChestItemData[] data, InventoryChestData chestData) {
            var dialog = await BLDialogGachaChest.Create();            
            dialog.InitializeChestInventory(data, chestData);
            return dialog;
        }
        
        public static async UniTask<BLDialogGachaChest> CreateFromChestDailyTask(GachaChestItemData[] data) {
            var dialog = await BLDialogGachaChest.Create();            
            dialog.InitializeChestDailyTask(data);
            return dialog;
        }
        
        [SerializeField]
        private BLGachaChest chestFi;

        [SerializeField]
        private BLGachaChest chestTr;
        
        [SerializeField]
        private Button claimChestBtn;
        
        [SerializeField]
        private Button claimChestTrBtn;

        private BLGachaChest _chest;
        
        private Dictionary<int, List<GachaChestItemData>> _chestReward;
        private int _currentChestIndex = 0;
        private Canvas _canvas;
        
        protected override void Awake() {
            base.Awake();
            IgnoreOutsideClick = true;
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
        }
        
        private void Update() {
            if (_inputManager.ReadButton(_inputManager.InputConfig.Enter)) {
                if (claimChestBtn.gameObject.activeInHierarchy) {
                    chestFi.OnBtClaim();
                }
                if (claimChestTrBtn.gameObject.activeInHierarchy) {
                    chestTr.OnBtClaim();
                }
            }
        }
        
        private void Initialize(Canvas canvas, GachaChestItemData[] data, GachaChestShopData chestData) {
            _canvas = canvas;
            // chia reward thành từng chest một
            _chestReward = new Dictionary<int, List<GachaChestItemData>>();
            var chestIndex = 0;
            var listItem = new List<GachaChestItemData>();
            foreach (var t in data) {
                listItem.Add(t);
                if (listItem.Count == chestData.ItemQuantity) {
                    _chestReward[chestIndex] = listItem;
                    listItem = new List<GachaChestItemData>();
                    chestIndex++;
                }
            }

            var chestType = chestData.ChestType;
            chestFi.gameObject.SetActive(false);
            chestTr.gameObject.SetActive(true);
            _chest = chestTr;
            OpenChest(chestType);
        }

        private void OpenChest(ChestShopType chestType) {
            if (_chestReward.Count > 1) {
                if (_currentChestIndex >= _chestReward.Count) {
                    Hide();
                    ShowDialogGachaMultiChestReward();
                    return;
                }
                var remainingChest = _chestReward.Count - (_currentChestIndex + 1);
                _chest.Initialize(_chestReward[_currentChestIndex].ToArray(), chestType, true, remainingChest);
                _chest.UiAnimation(chestType);
                _chest.SetOnContinue(() => {
                    OpenChest(chestType);
                });
                _chest.SetOnSkip(() => {
                    Hide();
                    ShowDialogGachaMultiChestReward();
                });
                _currentChestIndex++;
                return;
            }
            _chest.Initialize(_chestReward[_currentChestIndex].ToArray(), chestType, false, 0);
            _chest.UiAnimation(chestType);
            
            _chest.SetOnClaim(Hide);
        }

        private void ShowDialogGachaMultiChestReward() {
            BLDialogGachaMultiReward.Create().ContinueWith(dialog => {
                dialog.Init(_chestReward,Hide);
                dialog.Show(_canvas);
            });
        }

        private void InitializeChestInventory(GachaChestItemData[] data, InventoryChestData chestData) {
            chestFi.gameObject.SetActive(false);
            chestTr.gameObject.SetActive(true);
            _chest = chestTr;
            _chest.Initialize(data, (ChestShopType)chestData.ChestType, false, 0);
            _chest.UiAnimation((ChestShopType)chestData.ChestType);
            _chest.SetOnClaim(() => {
                Hide();
            });
        }

        public void InitializeChestDailyTask(GachaChestItemData[] data) {
            chestFi.gameObject.SetActive(true);
            chestTr.gameObject.SetActive(false);
            _chest = chestFi;
            _chest.Initialize(data);
            _chest.UiAnimation(ChestShopType.DailyTaskChest);
            _chest.SetOnClaim(() => {
                Hide();
            });
        }

        protected override void OnYesClick() {
            // Do nothing
        }

        protected override void OnNoClick() {
            // Do nothing
        }
    }
}