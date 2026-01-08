using System.Collections.Generic;
using System.Linq;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;

namespace Scenes.StoryModeScene.Scripts {
    public class BLDialogLuckyWheelReward : Dialog, ILuckyWheelReward {
        public static async UniTask<ILuckyWheelReward> Create() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogLuckyWheelReward>();
        }

        public static UniTask<DialogRewardTutorial> CreateDialogTutorialClaim() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogRewardTutorial>();
        }
        
        public static UniTask<DialogLuckyWheelReward2X6> CreateDialogTutorialClaimLarge() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogLuckyWheelReward2X6>();
        }

        [SerializeField]
        private BLDialogLuckyWheelRewardItem item;

        [SerializeField]
        private Transform layout;

        [SerializeField]
        private BLGachaRes gachaRes;

        private ISoundManager _soundManager;

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            IgnoreOutsideClick = true;
        }

        public void OnBtClaim() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        public void UpdateUI(IEnumerable<(int, int)> items) {
            // UpdateUI(items.Select(it => {
            //     var (itemId, quantity) = it;
            //     return (gachaRes.GetSpriteByItemId(itemId), quantity);
            // }));
            UniTask.Void(async () => {
                var updatedItems = await UniTask.WhenAll(items.Select(async it => {
                    var (itemId, quantity) = it;
                    var sprite = await gachaRes.GetSpriteByItemId(itemId);
                    return (sprite, quantity);
                }).ToArray());
                UpdateUI(updatedItems);
            });
        }

        public void UpdateUI(IEnumerable<(Sprite, int)> items) {
            foreach (var (sprite, quantity) in items) {
                var instance = Instantiate(item, layout);
                instance.UpdateUI(sprite, $"x{quantity}");
            }
        }

        public async void UpdateUI(IEnumerable<IBonusRewardAdventureV2Item> items) {
            foreach (var it in items) {
                var instance = Instantiate(item, layout);
                var sprite = await gachaRes.GetSpriteByItemId(it.ItemId);
                instance.UpdateUI(sprite, $"x{it.Quantity}");
            }
        }

        public async void UpdateUI(int itemId, int quantity) {
            var instance = Instantiate(item, layout);
            var sprite = await gachaRes.GetSpriteByItemId(itemId);
            instance.UpdateUI(sprite, $"x{quantity}");
        }
    }
}