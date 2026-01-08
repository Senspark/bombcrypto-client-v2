using Animation;
using App;
using Engine.Components;
using Engine.Entities;
using Engine.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Dialog {
    public class PlayerInHouse : MonoBehaviour {
        [SerializeField]
        private ImageAnimation animation;

        [SerializeField]
        private AnimationResource resource;

        private PlayerType _playerType;
        private PlayerColor _playerColor;
        private HeroRarity _playerRarity;
        private Vector3 _direction;
        private Vector3 _leftBottom;
        private Vector3 _rightTop;

        private void Start() {
            var (x, y) = RandomDirection();
            _direction.x = x;
            _direction.y = y;
            PlayMovingAnimation();
        }

        private void Update() {
            var trans = transform;
            var position = trans.localPosition;
            position += (_direction * 1f);
            trans.localPosition = position;
            CheckToChangeDirection();
        }

        public void SetLimited(Transform leftBottom, Transform rightTop) {
            _leftBottom = leftBottom.localPosition;
            _rightTop = rightTop.localPosition;
            _leftBottom.x += 30;
            _leftBottom.y += 50;
            _rightTop.x -= 30;
            _rightTop.y -= 50;
            SetStartPosition();
        }

        public void ChangeImage(PlayerData player) {
            _playerType = player.playerType;
            _playerColor = player.playercolor;
            _playerRarity = (HeroRarity)player.rare;
        }

        private void CheckToChangeDirection() {
            var position = transform.localPosition;
            if (_direction.x < 0 &&  position.x < _leftBottom.x ||
                _direction.x > 0 && position.x > _rightTop.x ||
                _direction.y < 0 && position.y < _leftBottom.y ||
                _direction.y > 0 && position.y > _rightTop.y) {
                ChooseDirection();
            }
        }

        private void ChooseDirection() {
            var (x, y) = RandomDirection();
            var position = transform.localPosition;
            if (position.x < _leftBottom.x || position.x > _rightTop.x) {
                _direction.x = -_direction.x;
                _direction.y = y;
            } else {
                _direction.x = x;
                _direction.y = -_direction.y;
            }
            PlayMovingAnimation();
        }

        private (float x, float y) RandomDirection() {
            var x = Random.Range(-1f, 1f);
            var y = Random.Range(-1f, 1f);
            if (x < 0.2f && x > -0.2f) {
                x = 0.5f;
            }
            if (y < 0.2f && y > -0.2f) {
                y = 0.5f;
            }
            return (x, y);
        }

        private void PlayMovingAnimation() {
            FaceDirection face;
            var faceLeftRight = FaceDirection.Left;
            var faceUpdown = FaceDirection.Up;

            faceLeftRight = _direction.x switch {
                > 0 => FaceDirection.Right,
                < 0 => FaceDirection.Left,
                _ => faceLeftRight
            };
            faceUpdown = _direction.y switch {
                > 0 => FaceDirection.Up,
                < 0 => FaceDirection.Down,
                _ => faceUpdown
            };

            if (Mathf.Abs(_direction.x) > Mathf.Abs(_direction.y)) {
                face = faceLeftRight;
            } else {
                face = faceUpdown;
            }

            var sprites = resource.GetSpriteMoving(_playerType, _playerColor, face, _playerRarity);
            animation.StartLoop(sprites, face == FaceDirection.Left);
        }

        private void SetStartPosition() {
            var x = Random.Range(_leftBottom.x, _rightTop.x);
            var y = Random.Range(_leftBottom.y, _rightTop.y);
            var trans = transform;
            var currentPos = trans.localPosition;
            var startPos = new Vector3(x, y, currentPos.z);
            trans.localPosition = startPos;
            
            // Lệnh PlayMovingAnimation(); sẽ bắt đầu enable image và run anim...
            // Gọi trong start bị bypass trước do disable khi gọi init sau 2 frames
            // => thêm vào dòng cuối để bắt đầu amin, nếu không thì chỉ đến khi chuyển hướng thì mới có gọi lệnh này.
            PlayMovingAnimation();
        }
    }
}