using System;
using App;
using Data;
using Senspark;
using Game.Dialog.BomberLand.BLGacha;
using Game.UI;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Inventory {
    public class BLInventoryItemInformationEquipment : MonoBehaviour {
        private const string DurationFormatYellow = "{0}\n<color=yellow>{1}</color>";
        private const string DurationFormatGreen = "{0}\n<color=#33FF00>{1}</color>";

        [SerializeField]
        private BLTablListAvatar avatar;

        [SerializeField]
        private Text duration;

        [SerializeField]
        private Text equipmentName;
        
        [SerializeField]
        private OverlayTexture effectPremium;
        
        [SerializeField]
        private GameObject premiumFrame;

        [SerializeField]
        private Text equipText;

        public Text EquipText => equipText;

        [SerializeField]
        private Sprite[] buttonSprite;

        [SerializeField]
        private Image buttonImage;

        [SerializeField]
        private bool log;

        [SerializeField]
        private BLGachaRes gachaRes;

        [SerializeField]
        private Transform stats;

        [SerializeField]
        private BLInventoryStat statPrefab;

        private int _instantId;

        private Action<ISkinManager.Skin> _clickEquip;
        private ISkinManager.Skin _data;
        private IItemUseDurationManager _itemUseDurationManager;
        private ILogManager _logManager;
        private TimeTick _timeTick = null;
        private IInputManager _inputManager;

        private void Awake() {
            _itemUseDurationManager ??= ServiceLocator.Instance.Resolve<IItemUseDurationManager>();
            _logManager ??= ServiceLocator.Instance.Resolve<ILogManager>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
        }

        private void Update() {
            if(_inputManager.ReadButton(_inputManager.InputConfig.Enter)) {
                OnButtonEquipClicked();
            }
        }

        private object[] GetDurationParams(ISkinManager.Skin skin) {
            return ToDurationParams(
                skin.Used ? "1 ITEM EXPIRE IN" : "USE DURATION PER ITEM",
                skin.Used ? (skin.Expire - DateTime.UtcNow) :
                skin.ExpirationAfter > 0 ? TimeSpan.FromMilliseconds(skin.ExpirationAfter) :
                _itemUseDurationManager.GetDuration()
            );
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

        public void Initialize(Action<ISkinManager.Skin> clickEquip) {
            _clickEquip = clickEquip;
            _itemUseDurationManager ??= ServiceLocator.Instance.Resolve<IItemUseDurationManager>();
            _logManager ??= ServiceLocator.Instance.Resolve<ILogManager>();
        }

        public void OnButtonEquipClicked() {
            _clickEquip(_data);
        }

        private static object[] ToDurationParams(string text, TimeSpan ts) {
            return new object[] { text, $"{TimeUtil.ConvertTimeToString(ts)}" };
        }

        public void UpdateData(ISkinManager.Skin skin) {
            _data = skin;
            _logManager.Log($"skin id: {skin.SkinId}, skin name: {skin.SkinName}, expire: {skin.Expire}");
            _logManager.Log($"now: {DateTime.UtcNow}");
            UpdateGui();
            _timeTick = new TimeTick(1, UpdateGui);
        }

        private void UpdateGui() {
            var skin = _data;
            avatar.ChangeAvatarByItemId(skin.SkinId);
            if (skin.IsForever) {
                duration.text = "<color=yellow>FOREVER</color>";
            } else {
                var durationParams = GetDurationParams(skin);
                if (log) {
                    var logManager = ServiceLocator.Instance.Resolve<ILogManager>();
                    logManager.Log($"duration params: {string.Join(", ", durationParams)}");
                }
                var durationFormat = skin.Used ? DurationFormatYellow : DurationFormatGreen; 
                duration.text = string.Format(durationFormat, durationParams);
            }
            equipmentName.text = skin.SkinName;
            equipText.text = skin.Equipped ? "UNEQUIP" : "EQUIP";
            buttonImage.sprite = skin.Equipped ? buttonSprite[1] : buttonSprite[0];

            ClearStats();
            if (_data.Stats.Length > 0) {
                stats.gameObject.SetActive(true);
                foreach (var stat in _data.Stats) {
                    var statItem = Instantiate(statPrefab, stats, false);
                    statItem.SetInfo(stat.StatId, stat.Value);
                }
            } else {
                stats.gameObject.SetActive(false);
            }
            
            var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            if (effectPremium) {
                var productItem = productItemManager.GetItem(skin.SkinId);
                effectPremium.enabled = false;
                equipmentName.color = productItem.ItemKind == ProductItemKind.Premium
                    ? effectPremium.m_OverlayColor : Color.white;
            }
            if (premiumFrame) {
                premiumFrame.SetActive(productItemManager.GetItem(skin.SkinId).ItemKind == ProductItemKind.Premium);
            }
        }

        private void ClearStats() {
            foreach (Transform child in stats) {
                Destroy(child.gameObject);
            }
        }

        private void LateUpdate() {
            _timeTick?.Update(Time.deltaTime);
        }
    }
}