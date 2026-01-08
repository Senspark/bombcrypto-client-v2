using App;

using Cysharp.Threading.Tasks;

using Engine.Utils;

using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;

namespace Scenes.ShopScene.Scripts {
    public class BLDialogAvatarReward : Dialog {
        [SerializeField]
        private ImageAnimation avatarTR;
        
        [SerializeField]
        private BLGachaRes resource;
        
        private ISoundManager _soundManager;

        private static UniTask<BLDialogAvatarReward> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogAvatarReward>();
        }
        
        public static void ShowInfo(Canvas canvas, int avatarId) {
            Create().ContinueWith(dialog => {
                dialog.SetInfo(avatarId);
                dialog.Show(canvas);
            });
        }

        private async void SetInfo(int avatarId) {
            var sprites = await resource.GetAvatar(avatarId);
            avatarTR.StartAni(sprites);
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            IgnoreOutsideClick = true;
        }

        public void OnBtClaim() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}