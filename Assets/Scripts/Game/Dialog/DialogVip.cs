using System;
using System.Collections.Generic;
using System.Linq;

using App;

using Cysharp.Threading.Tasks;

using Game.Manager;

using Senspark;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogVip : Dialog {
        [SerializeField]
        private List<Sprite> backgroundSprites;

        [SerializeField]
        private List<Color> textColor;
        
        [SerializeField]
        private List<RewardIcon> rewardIcons;

        [SerializeField]
        private List<VipItemBlock> items;

        [SerializeField]
        private VipItemBlock claimPopup;

        [SerializeField]
        private Button claimBtn;

        [SerializeField]
        private Image shadow;

        [SerializeField]
        private Text inventoryShieldAmount;

        [SerializeField]
        private Text inventoryConquestCardAmount;
        
        private ISoundManager _soundManager;
        private IServerManager _serverManager;

        public static DialogVip Create() {
            var prefab = Resources.Load<DialogVip>("Prefabs/Dialog/DialogVip");
            return Instantiate(prefab);
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();

            claimPopup.gameObject.SetActive(false);
        }

        private void Start() {
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                var result = await _serverManager.General.UserStakeVip();
                InitData(result);
                waiting.End();
            });
        }

        public void ShowClaimPopup() {
            _soundManager.PlaySound(Audio.Tap);
            claimPopup.gameObject.SetActive(true);
        }

        public void HideClaimPopup() {
            _soundManager.PlaySound(Audio.Tap);
            claimPopup.gameObject.SetActive(false);
        }

        private void ClaimReward(VipRewardType type) {
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var result = await _serverManager.General.UserClaimStakeVip(type);
                    InitData(result);
                } catch (Exception e) {
                    DialogOK.ShowError(DialogCanvas, e.Message);
                }
                waiting.End();
            });
        }

        private void InitData(IVipStakeResponse response) {
            var rewards = response.Rewards;
            
            for (var i = 0; i < items.Count; i++) {
                items[i].SetData(rewardIcons, backgroundSprites[i], textColor[i], rewards[i], false, null);
            }

            var currentVip = rewards.Find(e => e.IsCurrentVip);
            if (currentVip != null) {
                var index = currentVip.VipLevel - 1;
                claimPopup.SetData(rewardIcons, backgroundSprites[index], textColor[index], currentVip, true, ClaimReward);
                
                claimBtn.gameObject.SetActive(true);
                shadow.gameObject.SetActive(true);

                var x = items[index].transform.position.x;
                var btnPos = claimBtn.transform.position;
                btnPos.x = x;
                claimBtn.transform.position = btnPos;

                var shadowPos = shadow.transform.position;
                shadowPos.x = x;
                shadow.transform.position = shadowPos;
            } else {
                claimPopup.gameObject.SetActive(false);
                shadow.gameObject.SetActive(false);
                claimBtn.gameObject.SetActive(false);
            }

            var shield = response.Inventory.FirstOrDefault(e => e.Type == VipRewardType.Shield);
            if (shield != null) {
                inventoryShieldAmount.text = shield.Quantity.ToString();
            }
            var conquestCard = response.Inventory.FirstOrDefault(e => e.Type == VipRewardType.ConquestCard);
            if (conquestCard != null) {
                inventoryConquestCardAmount.text = conquestCard.Quantity.ToString();
            }
        }

        [Serializable]
        public class RewardIcon {
            public VipRewardType type;
            public Sprite sprite;
        }
    }
}