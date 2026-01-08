using System;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using Data;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;

namespace Scenes.MainMenuScene.Scripts {
    public class BLDialogGachaMultiReward : Dialog {
        [SerializeField]
        private Transform container;

        [SerializeField]
        private MultiChestRewardItem itemPrefab;

        private ISoundManager _soundManager;
        private Action _callback;

        public static UniTask<BLDialogGachaMultiReward> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogGachaMultiReward>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            IgnoreOutsideClick = true;
        }

        public void Init(Dictionary<int, List<GachaChestItemData>> chestReward, Action callback) {
            _callback = callback;
            foreach (var it in chestReward) {
                var reward = Instantiate(itemPrefab, container, false);
                reward.Initialize(it.Key, it.Value);
                if (it.Key == chestReward.Count - 1) {
                    reward.DisableDotLine();
                }
            }
        }

        protected override void OnYesClick() {
            OnClaimClicked();
        }

        protected override void OnNoClick() {
            // Do nothing
        }

        public void OnClaimClicked() {
            Hide();
            _callback?.Invoke();
        }
    }
}