using System;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Game.UI.FrameTabScroll;
using Scenes.FarmingScene.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLWallet {
    public enum TypeMenuLeftWallet {
        Mine,
        Deposit,
        Withdraw
    };

    public abstract class FrameTabContent : FrameTabContentBase<TypeMenuLeftWallet> {
    };

    public class BLFrameWallet : FrameTabContent {
        [SerializeField]
        private GameObject prefabWalletContent;

        [SerializeField]
        private BLWalletInformation frameWalletInformation;

        [SerializeField]
        private Button buttonDeposit;

        [SerializeField]
        private Button buttonClaim;
        
        private static UniTaskCompletionSource<bool> _waitInitUi = null;
        private Action _onChangeTabMenu;
        private BLWalletSegmentItem _currentSlotSelect = null;

        private TypeMenuLeftWallet _currentApplyUiTab;        
        private BLWalletSegmentItem _firstMine = null;
        private BLWalletSegmentItem _firstDeposit = null;
        private BLWalletSegmentItem _firstWithDraw = null;
        
        public Action OnClickSomeWhere { set; private get; }
        public Action<BLWalletSegmentItem> OnClaim { set; private get; } 
        
        protected void Awake() {
            frameWalletInformation.gameObject.SetActive(false);
            if (buttonDeposit) buttonDeposit.gameObject.SetActive(false);
            if (buttonClaim) buttonClaim.gameObject.SetActive(false);
        }

        public async UniTask<bool> InitUi() {
            _waitInitUi = new UniTaskCompletionSource<bool>();
            var result = await _waitInitUi.Task;
            return result;
        }

        protected override void OnInitUiDone() {
        }

        public void SetOnChangeTabMenu(Action onChangeTabMenu) {
            _onChangeTabMenu = OnChangeTabMenu;
        }

        protected override void OnChangeTabMenu() {
            OnClickSomeWhere?.Invoke();
            
            frameWalletInformation.UiApplyStyle(CurrentTabSelect);
            frameWalletInformation.gameObject.SetActive(false);
            
            if (buttonDeposit != null) {
                buttonDeposit.gameObject.SetActive(CurrentTabSelect == TypeMenuLeftWallet.Deposit);
                buttonDeposit.interactable = _firstDeposit;
            }
            if (buttonClaim != null) {
                buttonClaim.gameObject.SetActive(CurrentTabSelect == TypeMenuLeftWallet.Withdraw);
                buttonClaim.interactable = _firstWithDraw && _firstWithDraw.Balance > 0;
            }
            
            UiUnSelectCurrentSlot();
            _onChangeTabMenu?.Invoke();
        }

        protected override void LateUpdate() {
            base.LateUpdate();
            if (_waitInitUi != null && IsInitSegments) {
                _waitInitUi.TrySetResult(true);;
                _waitInitUi = null;
            }
        }

        private void UiUnSelectCurrentSlot() {
            if (_currentSlotSelect) {
                _currentSlotSelect.UiSetSelect(false);
                _currentSlotSelect = null;
            }
            
            _currentSlotSelect = CurrentTabSelect switch {
                TypeMenuLeftWallet.Mine => _firstMine,
                TypeMenuLeftWallet.Withdraw => _firstWithDraw,
                TypeMenuLeftWallet.Deposit => _firstDeposit,
                _ => _currentSlotSelect
            };
            if (_currentSlotSelect) {
                _currentSlotSelect.OnBtSelect();
            }
        }
        
        public void ApplyUiTab(TypeMenuLeftWallet tab, List<DataWallet> tokensData, List<DataWallet> nftData) {
            _currentApplyUiTab = tab;
            switch (_currentApplyUiTab) {
                case TypeMenuLeftWallet.Deposit:
                    _firstDeposit = null;
                    break;
                case TypeMenuLeftWallet.Withdraw:
                    _firstWithDraw = null;
                    break;
            }
            
            var frameContent = DisContent[tab].frameContent;
            var walletSegmentContent = frameContent.GetComponentInChildren<BLWalletSegmentContent>();
            if (walletSegmentContent == null) {
                walletSegmentContent = Instantiate(prefabWalletContent, frameContent.transform, false)
                    .GetComponent<BLWalletSegmentContent>();   
            }
            var allSlot = new List<BLWalletSegmentItem>();
            if (tokensData != null && tokensData.Count > 0) {
                var slotsToken = walletSegmentContent.InitUiTokens(tokensData.Count);
                ApplyUiTab2(allSlot, slotsToken, tokensData, _currentApplyUiTab);
                allSlot.AddRange(slotsToken);
            } else {
                walletSegmentContent.HideUiSegmentToken();
            }
            if (nftData != null && nftData.Count > 0) {
                var slotsNft = walletSegmentContent.InitUiNFt(nftData.Count);
                ApplyUiTab2(allSlot, slotsNft, nftData, _currentApplyUiTab);
                allSlot.AddRange(slotsNft);
            } else {
                walletSegmentContent.HideUiSegmentNft();
            }
            walletSegmentContent.AutoLayout();
            
            if (tokensData != null && tokensData.Count > 0) {
                OnSlotSelect(allSlot, allSlot[0], tokensData[0]);
            } else if (nftData != null && nftData.Count > 0) {
                OnSlotSelect(allSlot, allSlot[0], nftData[0]);
            }
        }

        private void ApplyUiTab2(List<BLWalletSegmentItem> allSlot, List<BLWalletSegmentItem> slotsToken,
            List<DataWallet> data, TypeMenuLeftWallet tab) {
            for (var idx = 0; idx < slotsToken.Count; idx++) {
                if (idx >= data.Count) {
                    break;
                }
                var slot = slotsToken[idx];
                var info = data[idx];
                slot.ApplyData(info);
                slot.SetItemTab(tab);
                slot.SetOnBtSelect(() => { OnSlotSelect(allSlot, slot, info); });
            }
        }
        
        private void OnSlotSelect(List<BLWalletSegmentItem> allSlot, BLWalletSegmentItem slotSelect, DataWallet info) {
            OnClickSomeWhere?.Invoke();
            foreach (var slot in allSlot) {
                if (slot == slotSelect) {
                    switch (_currentApplyUiTab) {
                        case TypeMenuLeftWallet.Mine:
                            if (!_firstMine) {
                                _firstMine = slot;
                            }
                            break;
                        case TypeMenuLeftWallet.Deposit:
                            if (!_firstDeposit) {
                                _firstDeposit = slot;
                            }
                            break;
                        case TypeMenuLeftWallet.Withdraw:
                            if (!_firstWithDraw) {
                                _firstWithDraw = slot;
                            }
                            break;
                    }
                    slot.UiSetSelect(true);    
                } else {
                    slot.UiSetSelect(false);
                }
            }
            _currentSlotSelect = slotSelect;
            if (CurrentTabSelect == _currentSlotSelect.GetItemTab()) {
                ShowInfo(info);
            }
        }

        private void ShowInfo(DataWallet info) {
            OnClickSomeWhere?.Invoke();
            if (AppConfig.IsTon()) {
                if (buttonDeposit != null) {
                    buttonDeposit.gameObject.SetActive(CurrentTabSelect == TypeMenuLeftWallet.Deposit);
                    buttonDeposit.interactable = _firstDeposit;
                }
                if (buttonClaim != null) {
                    buttonClaim.gameObject.SetActive(CurrentTabSelect == TypeMenuLeftWallet.Withdraw);
                    buttonClaim.interactable = _currentSlotSelect.Balance > 0;
                }
            }
            frameWalletInformation.gameObject.SetActive(true);
            frameWalletInformation.DisplayInfo(info);
        }

        public BLWalletSegmentItem GetCurrentSegment() {
            return _currentSlotSelect;
        }
        
        public void SetOnDeposit(Action<DataWallet> onDeposit) {
            frameWalletInformation.SetOnDeposit(onDeposit);
        }
        
        public void SetOnWithdraw(Action<DataWallet> onWithdraw) {
            frameWalletInformation.SetOnWithdraw(onWithdraw);
        }

        public void OnClaimButtonClicked() {
            OnClaim?.Invoke(_currentSlotSelect);
        }
    }
}