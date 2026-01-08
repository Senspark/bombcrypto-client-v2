using System;
using App;
using Data;
using Game.Dialog;

using Scenes.MarketplaceScene.Scripts;

using Senspark;
using Services;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BLInventoryOnSellInformation : BaseBLItemInformation {
        [SerializeField]
        private GameObject premiumFrame;
        
        [SerializeField] private Text durationText;
        [SerializeField] private GameObject duration;
        
        private Action<ItemData> _onCancel;

        private Action<ItemData> _onEdit;

        public override void UpdateItem(ItemData data, bool showButton = true) {
            base.UpdateItem(data, showButton);
            UpdateDuration(data);
            var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            premiumFrame.SetActive(productItemManager.GetItem(data.ItemId).ItemKind == ProductItemKind.Premium);
        }

        public void SetOnCancel(Action<ItemData> onCancel) {
            _onCancel = onCancel;
        }

        public void SetOnEdit(Action<ItemData> onEdit) {
            _onEdit = onEdit;
        }

        public void OnBtnCancelClick() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            _onCancel?.Invoke(ItemData);
        }

        public void OnBtnEditClick() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            _onEdit?.Invoke(ItemData);
        }
        
        private void UpdateDuration(ItemData data) {
            if (durationText == null) {
                return;
            }
            
            if (data.ExpirationAfter == 0) {
                duration.SetActive(false);
                // durationText.text = "<color=yellow>FOREVER</color>";
            } else {
                duration.SetActive(true);
                durationText.text =
                    $"DURATION <color=#33FF00>{MarketUtils.ExpirationToDays(data.ExpirationAfter)} DAYS</color> AFTER EQUIPPED";
            }
        }
    }
}