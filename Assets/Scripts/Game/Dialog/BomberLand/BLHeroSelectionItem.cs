using App;

using Data;

using Senspark;

using Services;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BLHeroSelectionItem : MonoBehaviour {
        [SerializeField]
        private Text heroName;

        [SerializeField]
        public OverlayTexture effectPremium;

        [SerializeField]
        private Avatar icon;

        [SerializeField]
        private Text eSelected;

        [SerializeField]
        private Image highLight;

        private int _heroId;
        private PlayerData _hero;
        private System.Action<int, bool> _onClickCallback;

        public int Index { get; private set; }
        public HeroId HeroId { get; private set; }

        private IProductItemManager _productItemManager;

        public void SetInfo(int index, PlayerData data, int quantity, System.Action<int, bool> callback) {
            _productItemManager ??= ServiceLocator.Instance.Resolve<IProductItemManager>();

            var productItem = _productItemManager.GetItem(data.itemId);
            heroName.text = productItem.Name;
            HeroId = data.heroId;
            Index = index;
            _hero = data;
            _onClickCallback = callback;
            icon.ChangeImage(data);

            if (effectPremium) {
                effectPremium.enabled = false;
                heroName.color = productItem.ItemKind == ProductItemKind.Premium
                    ? effectPremium.m_OverlayColor : Color.white;
            }
        }

        public void OnClicked() {
            _onClickCallback?.Invoke(Index, true);
        }

        public void SetActive(bool value) {
            highLight.gameObject.SetActive(value);
        }

        public void SetSelected(bool value) {
            eSelected.gameObject.SetActive(value);
        }
    }
}