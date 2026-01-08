using System;

using App;

using Data;

using Senspark;

using Game.Dialog;

using Services;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BaseBLHeroInformation : MonoBehaviour {
        [SerializeField]
        protected Text idText;

        [SerializeField]
        public OverlayTexture effectPremium;

        [SerializeField]
        protected BLTablListAvatar avatar;

        [SerializeField]
        protected BLHeroStat range;

        [SerializeField]
        protected BLHeroStat speed;

        [SerializeField]
        protected BLHeroStat bomb;

        [SerializeField]
        protected BLHeroStat health;

        [SerializeField]
        protected BLHeroStat damage;

        public Action<UIHeroData> OnShowDialogCallback;
        public Action<OrderDataRequest> OnShowDialogOrderCallback;
        public Action OnOrderErrorCallback;

        private UIHeroData _heroData;
        
        private IProductItemManager _productItemManager;

        protected virtual void Awake() {
            gameObject.SetActive(false);
        }

        public virtual int GetInputAmount() {
            return 0;
        }
        public virtual void UpdateHero(UIHeroData data, bool showButton = true) {
            _heroData = data;
            if (data == null) {
                gameObject.SetActive(false);
                idText.text = "--";
                avatar.gameObject.SetActive(false);
                range.SetValue(0, 0);
                speed.SetValue(0, 0);
                bomb.SetValue(0, 0);
                health.SetValue(0, 0);
                damage.SetValue(0, 0);
                return;
            }

            gameObject.SetActive(true);
            idText.text = $"{data.HeroName}";
            avatar.gameObject.SetActive(true);
            avatar.ChangeAvatar(data);
            range.SetValue(data.BombRange.value, data.BombRange.max);
            speed.SetValue((int)data.Speed.value, data.Speed.max);
            bomb.SetValue(data.BombNum.value, data.BombNum.max);
            health.SetValue(data.Health.value, data.Health.max);
            damage.SetValue(data.Damage.value, data.Damage.max);
            if (effectPremium) {
                _productItemManager ??= ServiceLocator.Instance.Resolve<IProductItemManager>();
                var productItem = _productItemManager.GetItem(_heroData.HeroId);
                effectPremium.enabled = false;
                idText.color = productItem.ItemKind == ProductItemKind.Premium
                    ? effectPremium.m_OverlayColor : Color.white;
            }
        }

        public void OnButtonClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            OnShowDialogCallback?.Invoke(_heroData);
        }
    }
}