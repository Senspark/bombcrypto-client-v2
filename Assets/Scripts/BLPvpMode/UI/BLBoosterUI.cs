using System;

using BLPvpMode.Data;

using Game.Dialog;

using PvpMode.Manager;
using PvpMode.UI;

using Scenes.MarketplaceScene.Scripts;

using Senspark;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.EventSystems;

namespace BLPvpMode.UI {
    public class BLBoosterUI : MonoBehaviour {
        [SerializeField]
        private bool showTip = true;
        
        [SerializeField]
        private BoosterButton[] buttons;

        private Canvas _canvasDialog;

        private IBoosterManager _boosterManager;

        private BLBoosterUI _boosterDisplay;

        private BoosterStatus _boosterStatus;
        public BoosterStatus BoosterStatus => _boosterStatus;

        private System.Action<BoosterType, Action> _onNotificationToMarket;
        
        private void Awake() {
            _boosterManager = ServiceLocator.Instance.Resolve<IBoosterManager>();
            _boosterStatus = new BoosterStatus(buttons.Length);

            var pointerEnter = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener(OnPointerEnter);

            var pointerExit = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerExit
            };
            pointerExit.callback.AddListener(OnPointerExit);

            var pointerClick = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerClick
            };
            pointerClick.callback.AddListener(OnPointerClick);
            
            var pointerDown = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerDown
            };
            pointerDown.callback.AddListener(OnPointerDown);
            
            var pointerUp = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerUp
            };
            pointerUp.callback.AddListener(OnPointerUp);

            for (var i = 0; i < buttons.Length; i++) {
                var button = buttons[i];
                
                var ev = button.GetComponent<EventTrigger>();
                if (showTip) {
                    ev.triggers.Add(pointerEnter);
                    ev.triggers.Add(pointerExit);
                    ev.triggers.Add(pointerDown);
                    ev.triggers.Add(pointerUp);
                }
                if (button.IsDisable) {
                    continue;
                }
                ev.triggers.Add(pointerClick);
                
                
                button.SetCallback(OnNotificationToMarket, OnUpdateChoose);
                SetChoose(i, false);
                UpdateQuantity(button);
            }
        }

        public void Initialize(Canvas canvasDialog, BoosterButton.ChooseBoosterDelegate chooseBooster, BLBoosterUI display = null) {
            _canvasDialog = canvasDialog;
            if (display != null) {
                _boosterDisplay = display;
                _boosterDisplay.HideAll();
            }
            foreach (var button in buttons) {
                UpdateQuantity(button);
                button.ChooseBooster = chooseBooster;
            }
            SetDefaultChooseShieldAndKey();
        }

        public void Initialize(BoosterButton.ChooseBoosterDelegate chooseBooster,
            System.Action<BoosterType, Action> onNotificationToMarket) {
            foreach (var button in buttons) {
                UpdateQuantity(button);
                button.ChooseBooster = chooseBooster;
            }
            _onNotificationToMarket = onNotificationToMarket;
            SetDefaultChooseShieldAndKey();
        }
        
        private void SetDefaultChooseShieldAndKey() {
            for (var i = 0; i < buttons.Length; i++) {
                var button = buttons[i];
                if (button.Quantity == 0) {
                    continue;
                }

                var boosterType = button.GetBoosterType();
                if (boosterType is BoosterType.Shield or BoosterType.Key) {
                    SetChoose(i, true);
                }
            }
        }        
        
        public void UpdateQuantity() {
            foreach (var button in buttons) {
                UpdateQuantity(button);
            }
        }

        private void UpdateQuantity(BoosterButton button) {
            var booster = _boosterManager.GetDataBooster(button.GetBoosterType());
            button.SetQuantity(booster?.Quantity ?? 0);
            button.SetTipDescription(booster?.Description ?? "");
       }

        private void OnNotificationToMarket(BoosterType boosterType, Action callback) {
            if (_canvasDialog != null) {
                DialogNotificationBoosterToMarket.ShowOn(_canvasDialog, boosterType , callback);
                return;
            }
            _onNotificationToMarket?.Invoke(boosterType, callback);
        }
        
        private void OnUpdateChoose(int index, bool value) {
            Debug.Log($"{index}:{value}");
            var type = value ? buttons[index].GetBoosterType() : BoosterType.Unknown;
            var quantity = value ? buttons[index].Quantity : 0;
            _boosterStatus.UpdatedItem(index, type, quantity);
            
            if (!value) {
                HideDisplay(buttons[index]);
                RemoveChooseBooster(buttons[index].GetBoosterType());
                return;
            }

            ShowDisplay(buttons[index]);
            var others = new[] {1, 0, 3, 2, -1, -1, -1, -1, -1};
            var other = others[index];
            if (other >= 0) {
                buttons[other].OnForceToFalse();
                HideDisplay(buttons[other]);
            }
        }

        private void ShowDisplay(BoosterButton button) {
            if (_boosterDisplay != null) {
                _boosterDisplay.ShowBooster(button.GetBoosterType());
            }
        }

        private void HideDisplay(BoosterButton button) {
            if (_boosterDisplay != null) {
                _boosterDisplay.HideBooster(button.GetBoosterType());
            }
        }

        private void SetChoose(int index, bool value) {
            buttons[index].SetChoose(value);
            var type = value ? buttons[index].GetBoosterType() : BoosterType.Unknown;
            var quantity = value ? buttons[index].Quantity : 0;
            _boosterStatus.UpdatedItem(index, type, quantity);
        }

        public bool IsChooseBooster(BoosterType type) {
            return _boosterStatus.IsChooseBooster(type);
        }

        public int GetQuantity(BoosterType type) {
            return _boosterStatus.GetQuantity(type);
        }
        
        public void RemoveChooseBooster(BoosterType type) {
            _boosterStatus.RemoveChooseBooster(type);
        }

        public void HideAll() {
            for (var i = 0; i < buttons.Length; i++) {
                buttons[i].gameObject.SetActive(false);
            }
        }

        public void HideBooster(BoosterType type) {
            for (var i = 0; i < buttons.Length; i++) {
                if (buttons[i].GetBoosterType() == type) {
                    buttons[i].gameObject.SetActive(false);
                }
            }
        }

        public void ShowBooster(BoosterType type) {
            for (var i = 0; i < buttons.Length; i++) {
                if (buttons[i].GetBoosterType() == type) {
                    buttons[i].gameObject.SetActive(true);
                }
            }
        }

        private void OnPointerEvent(BaseEventData data, EventTriggerType eventType) {
            var pointerEventData = (PointerEventData) data;
            if (pointerEventData == null) {
                return;
            }
            var boosterButton = eventType switch {
                EventTriggerType.PointerEnter => pointerEventData.pointerEnter.GetComponent<BoosterButton>(),
                EventTriggerType.PointerExit => pointerEventData.pointerEnter.GetComponent<BoosterButton>(),
                EventTriggerType.PointerClick => pointerEventData.pointerEnter.GetComponent<BoosterButton>(),
                EventTriggerType.PointerDown => pointerEventData.pointerEnter.GetComponent<BoosterButton>(),
                EventTriggerType.PointerUp => pointerEventData.pointerEnter.GetComponent<BoosterButton>(),
                _ => null
            };
            if (boosterButton == null) {
                return;
            }
            switch (eventType) {
                case EventTriggerType.PointerEnter:
                    for (var i = 0; i < buttons.Length; i++) {
                        var bt = buttons[i];
                        if(!bt.gameObject.activeSelf) {
                            continue;
                        }
                        if (bt != boosterButton) {
                            bt.HideTip();
                        }
                    }
                    boosterButton.ShowTip();
                    break;
                case EventTriggerType.PointerExit:
                    // boosterButton.HideTip();
                    break;
                case EventTriggerType.PointerClick:
                    boosterButton.OnButtonClicked();
                    break;
                case EventTriggerType.PointerDown:
                    boosterButton.OnTouchDown();
                    break;
                case EventTriggerType.PointerUp:
                    boosterButton.OnTouchUp();
                    break;
            }
        }

        private void OnPointerEnter(BaseEventData data) {
            OnPointerEvent(data, EventTriggerType.PointerEnter);
        }

        private void OnPointerExit(BaseEventData data) {
            OnPointerEvent(data, EventTriggerType.PointerExit);
        }

        private void OnPointerClick(BaseEventData data) {
            OnPointerEvent(data, EventTriggerType.PointerClick);
        }
        
        private void OnPointerDown(BaseEventData data) {
            OnPointerEvent(data, EventTriggerType.PointerDown);
        }
        
        private void OnPointerUp(BaseEventData data) {
            OnPointerEvent(data, EventTriggerType.PointerUp);
        }

        public int[] GetSelectedBoosterIds() {
            return _boosterStatus.GetSelectedBoosterIds();
        }

        private void SetChoose(BoosterType type, bool value) {
            for (var i = 0; i < buttons.Length; i++) {
                var button = buttons[i];
                if (button.Quantity == 0) {
                    continue;
                }
                var boosterType = button.GetBoosterType();
                if (boosterType == type) {
                    SetChoose(i, value);
                }
            }
        }

        public void SetSelectedBooster(int[] ids) {
            foreach (var iter in ids) {
                SetChoose(DefaultBoosterManager.ConvertFromId(iter), true);
            }
        }
    }
}