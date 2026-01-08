using App;

using Engine.Entities;

using UnityEngine;

namespace Animation {
    public class BrokenAnimator : EntityAnimator {

        private GameModeType _gameModeType;
        private bool _isBlock;
        private int _tileIndex;
        private BrokenType _blockType;

        public BrokenType ConvertFrom(EntityType type, int tileIndex) {
            if (AppConfig.IsTon() && type == EntityType.normalBlock) {
                // Nếu map polygon thì auto chuyển thành map1, do ko có anim nổ tương ứng bên polgon
                var isPolygon = tileIndex % 2 == 1;
                if (isPolygon)
                    return BrokenType.map1;
                tileIndex /= 2;
            }

            if (type == EntityType.normalBlock) {
                return tileIndex switch {
                    0 => BrokenType.map0,
                    1 => BrokenType.map1,
                    2 => BrokenType.map2,
                    _=> BrokenType.map0,
                };
            } else {
                var brokenType = (int) type;
                return (BrokenType) brokenType;
            }
        }
        
        public void Initialized(GameModeType gameModeType, int tileIndex) {
           _gameModeType = gameModeType;
            _tileIndex = tileIndex;
            _isBlock = false;
        }

        public void Initialized(GameModeType gameModeType, BrokenType blockType) {
            _gameModeType = gameModeType;
            _blockType = blockType;
            _isBlock = true;
        }

        public void CopyAnimatorFrom(BrokenAnimator animator) {
            _gameModeType = animator._gameModeType;
            _tileIndex = animator._tileIndex;
            _blockType = animator._blockType;
            _isBlock = animator._isBlock;
        }
        
        public void PlayBroken(System.Action callback) {
            if (_isBlock) {
                PlayBrokenBlock(callback);
                return;
            }
            PlayBrokenTile(callback);
        }
        
        private void PlayBrokenTile(System.Action callback) {
            var sprites = animationResource.GetSpriteBrickTile(_gameModeType, _tileIndex);
            bodyAnimation.StartAnimation(sprites, callback);
        }

        private void PlayBrokenBlock(System.Action callback) {
            var sprites = animationResource.GetSpriteBrickBlock(_gameModeType, _blockType);
            bodyAnimation.StartAnimation(sprites, callback);
        }
    }
}