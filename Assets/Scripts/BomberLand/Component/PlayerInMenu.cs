using Animation;

using App;

using Engine.Components;
using Engine.Entities;
using Engine.Utils;

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

        public void SetAnimation() {
            var sprites = resource.GetSpriteMoving(_playerType, _playerColor, FaceDirection.Down);
            imageAnimation.StartLoop(sprites);
        }

        public void SetIdle() {
            var sprites = resource.GetSpriteIdle(_playerType, _playerColor, FaceDirection.Down);
            imageAnimation.StartLoop(sprites);
        }
    }
}