using App;

using DG.Tweening;

using Engine.ScriptableObject;

using Senspark;

using UnityEngine;

namespace Engine.Entities {
    public class WallDrop : Entity {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private SpriteRenderer spriteShadow;

        [SerializeField]
        private BLHardDropRes resource;
        
        private ISoundManager _audioManager;
        public int Id { get; set; }
        public Vector2Int Location { get; private set; }

        private float _delayDrop;
        private bool _isHardBlock;
        private System.Action<int, int> _onFinishCallback;

        private void Awake() {
            _audioManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            spriteRenderer.enabled = false;
            spriteShadow.enabled = false;
        }

        public void SetSpriteAndInfo(int tileIndex, int i, int j, float hDelta, float delayDrop, bool isHardBlock) {
            spriteRenderer.sprite = resource.GetSprite(tileIndex);
            spriteRenderer.transform.localPosition = new Vector3(0, hDelta, 0);
            Location = new Vector2Int(i, j);
            _delayDrop = delayDrop;
            _isHardBlock = isHardBlock;
        }

        public void AniFallDown(System.Action<int, int> callback) {
            var duration = 1;
            // Hiện shadow khi không phải rơi tại HardBlock
            if (_isHardBlock) {
                spriteShadow.enabled = false;
            } else {
                spriteShadow.enabled = true;
                var tf = spriteShadow.transform;
                var sizeRaw = tf.localScale;
                // spriteShadow.transform.localScale = new Vector3(sizeRaw.x * 0.6f, sizeRaw.y * 0.6f, 0);
                tf.localScale = Vector3.zero;
                spriteShadow.transform
                    .DOScale(Vector3.zero, 0.05f)
                    .SetDelay(_delayDrop - 0.05f)
                    .OnComplete(() => {
                        spriteShadow.transform.localScale = new Vector3(sizeRaw.x * 0.6f, sizeRaw.y * 0.6f, 0);
                        tf.DOScale(sizeRaw, 0.6f).SetDelay(0.5f);
                    });
            }
            spriteRenderer.enabled = true;
            _onFinishCallback = callback;
            spriteRenderer.transform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.Linear).SetDelay(_delayDrop)
                .SetUpdate(UpdateType.Normal, false)
                .OnComplete(OnFinish);
        }

        public void JumpToEnd() {
            DOTween.Kill(transform, true);
            OnFinish();
        }

        public void StopFallDown() {
            DOTween.Kill(transform);
            spriteRenderer.enabled = false;
            spriteShadow.enabled = false;
        }

        private void OnFinish() {
            _onFinishCallback?.Invoke(Location.x, Location.y);
            DropFinish();
        }

        private void DropFinish() {
            _audioManager.PlaySound(Audio.BlockDropDown);
            // spriteRenderer.enabled = false;
            spriteShadow.enabled = false;
        }
    }
}