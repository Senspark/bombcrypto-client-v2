using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Senspark;
using Engine.Utils;
using Game.Dialog.BomberLand.BLFrameShop;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLGacha {
    public class BLGachaChest : MonoBehaviour {
        
        [SerializeField]
        private Transform layout;
        
        [SerializeField]
        private Transform[] layout2;

        [SerializeField]
        private Transform[] layout3;

        [SerializeField]
        private Transform[] layout4;
        
        [SerializeField]
        private Transform[] layout5;
        [SerializeField]
        private Transform[] layout6;

        [SerializeField]
        private BLGachaChestReward rewardPrefab;

        [SerializeField]
        private BLShopResource shopResource;
        
        [SerializeField]
        private GameObject content;
        
        [SerializeField]
        private Image iconChest;

        [SerializeField]
        private ImageAnimation imageAnimation;
        
        [SerializeField]
        private Button buttonClaim;
        
        [SerializeField]
        private Button buttonContinue;
        
        [SerializeField]
        private Button buttonSkip;
        
        [SerializeField]
        private Text remainingChestText;
        
        [SerializeField]
        private GameObject remainingChest;
        
        private Action _onClaim;
        private Action _onContinue;
        private Action _onSkip;

        private IProductItemManager _productItemManager;

        private Transform[] _rewardLayout; 
        private int _rewardSlot;
        private List<GachaChestItemData> _rewardList = new ();
        private BLGachaChestReward[] _items;
        private IInputManager _inputManager;
        
        private void Awake() {
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
        }

        public void Initialize(GachaChestItemData[] data, ChestShopType chestType, bool isMultiChest, int remaining) {
            _rewardList = data.ToList();
            _rewardSlot = shopResource.GetChestShop(chestType).slots;
            if (_rewardSlot > 0) {
                _rewardLayout = _rewardSlot switch {
                    2 => layout2,
                    3 => layout3,
                    4 => layout4,
                    5 => layout5,
                    6 => layout6,
                    _ => Array.Empty<Transform>()
                };
                ShowRewardItems(_rewardLayout);
            } else {
                _rewardSlot = data.Length;
                ShowRewardItems(null);
            }
            if (isMultiChest) {
                if (remaining != 0) {
                    remainingChestText.text = remaining.ToString();
                    buttonSkip.gameObject.SetActive(true);
                } else {
                    remainingChest.SetActive(false);
                    buttonSkip.gameObject.SetActive(false);
                }
                buttonContinue.gameObject.SetActive(true);
                buttonClaim.gameObject.SetActive(false);
            }
        }
        
        public void Initialize(GachaChestItemData[] data) {
            _rewardList = data.ToList();
            _rewardSlot = data.Length;
            if (_rewardSlot > 1) {
                _rewardLayout = _rewardSlot switch {
                    2 => layout2,
                    3 => layout3,
                    4 => layout4,
                    5 => layout5,
                    6 => layout6,
                    _ => Array.Empty<Transform>()
                };
                ShowRewardItems(_rewardLayout);
            } else {
                ShowRewardItems(null);
            }
        }

        private void ShowRewardItems(Transform[] layouts) {
            // clear previous items
            if (_items != null) {
                foreach (var item in _items) {
                    Destroy(item.gameObject);
                }
                _items = null;
            }

            var min = Math.Min(_rewardSlot, _rewardList.Count);
            _items = new BLGachaChestReward[min];
            var idx = 0;
            for (var i = 0; i < min ; i++) {
                var it = _rewardList[i];

                var parent = layouts == null ? layout : layouts[i];
                var reward = Instantiate(rewardPrefab, parent);
                
                _items[idx] = reward;
                var des = _productItemManager.GetItem((int)it.ProductId).Name;
                reward.Initialize(it, des, () => {
                    SetSelect(_items, reward);
                });
                reward.UiShowDescription(false);
                idx++;
            }

            //Remove items vừa mới dùng đê show bên trên.
            for (var i = 0; i < min; i++) {
                _rewardList.RemoveAt(0);
            }
        }
        
        public void Initialize(GachaChestItemData[] data, InventoryChestData chestData) {
            var items = new BLGachaChestReward[data.Length];
            var idx = 0;
            var itemInfo = chestData.DropRate.ToDictionary((it => it.ItemId));
            foreach (var it in data) {
                var reward = Instantiate(rewardPrefab, layout);
                items[idx] = reward;
                var des = _productItemManager.GetItem((int)it.ProductId).Name;
                reward.Initialize(it, des, () => {
                    SetSelect(items, reward);
                });
                reward.UiShowDescription(false);
                idx++;
            }
        }

        public void UiAnimation(ChestShopType chestType) {
            if (!imageAnimation) {
                return;
            }
            content.SetActive(false);
            imageAnimation.StartAni(shopResource.GetChestShop(chestType).animationOpen);
            imageAnimation.SetOnDoneAni(() => {
                UiAnimationEndLoop(chestType);
                content.SetActive(true);
            });
        }

        private void UiAnimationEndLoop(ChestShopType chestType) {
            imageAnimation.StartLoop(shopResource.GetChestShop(chestType).animationIdle);
        }
            
        private void SetSelect(BLGachaChestReward[] items, BLGachaChestReward itemSelect) {
            foreach (var item in items) {
                item.UiShowDescription(item == itemSelect);
            }
        }

        public void SetOnClaim(Action onClaim) {
            _onClaim = onClaim;
        }
        
        public void SetOnContinue(Action onContinue) {
            _onContinue = onContinue;
        }
        
        public void SetOnSkip(Action onSkip) {
            _onSkip = onSkip;
        }

        public void OnBtClaim() {
            if (_rewardList.Count > 0) {
                ShowRewardItems(_rewardLayout);
                return;
            }
            _onClaim?.Invoke();
        }
        
        public void OnBtContinue() {
            _onContinue?.Invoke();
        }
        
        public void OnBtnSkip() {
            _onSkip?.Invoke();
        }

        private void Update() {
            if (_inputManager.ReadButton(_inputManager.InputConfig.Enter)
                && buttonClaim.gameObject.activeInHierarchy) {
                OnBtClaim();
                return;
            }
            
            if (_inputManager.ReadButton(_inputManager.InputConfig.Enter)
                && buttonContinue.gameObject.activeInHierarchy) {
                OnBtContinue();
                return;
            }
            
            if (_inputManager.ReadButton(_inputManager.InputConfig.Back)
                && buttonSkip.gameObject.activeInHierarchy) {
                OnBtnSkip();
            }
        }
    }
}