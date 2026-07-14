using Animation;

using App;

using Engine.Components;
using Engine.Entities;
using Engine.Utils;

using Senspark;

using UnityEngine;

namespace BomberLand.Component {
    public class PlayerInMenu : MonoBehaviour {
        [SerializeField]
        private ImageAnimation imageAnimation;

        [SerializeField]
        private AnimationResource resource;

        private PlayerType _playerType;
        private PlayerColor _playerColor;

        public void ChangeImage(PlayerData player) {
            _playerType = player.playerType;
            _playerColor = player.playercolor;
        }

        public void ChangeImage(PlayerType playerType, PlayerColor playerColor) {
            _playerType = playerType;
            _playerColor = playerColor;
        }

        // Thân hero qua IHeroSpriteLoader (path-load) thay cho dict AnimationResource.
        // (Loader phục vụ idle/move từ cùng clip Down nên SetAnimation/SetIdle dùng chung.)
        public async void SetAnimation() {
            var loader = ServiceLocator.Instance.Resolve<IHeroSpriteLoader>();
            var sprites = await loader.LoadClip(_playerType, _playerColor, FaceDirection.Down);
            if (!imageAnimation) {
                return;
            }
            imageAnimation.StartLoop(sprites);
        }

        public async void SetIdle() {
            var loader = ServiceLocator.Instance.Resolve<IHeroSpriteLoader>();
            var sprites = await loader.LoadClip(_playerType, _playerColor, FaceDirection.Down);
            if (!imageAnimation) {
                return;
            }
            imageAnimation.StartLoop(sprites);
        }
    }
}