using System.Collections.Generic;

using Engine.Utils;

using UnityEngine;

namespace BomberLand.Shop {
    public class ExplodeAnimation : MonoBehaviour {
        [SerializeField]
        private ImageAnimation bomb;
        
        [SerializeField]
        private ImageAnimation center;

        [SerializeField]
        private ImageAnimation left;

        [SerializeField]
        private ImageAnimation right;

        [SerializeField]
        private ImageAnimation up;

        [SerializeField]
        private ImageAnimation down;

        [SerializeField]
        private Sprite emptySprite;

        [SerializeField]
        private float loopDelay = 3;
        
        private float _timeProgress = 0;
        private Sprite[] _centerSprites;
        private Sprite[] _endSprites;
        
        private bool _isLooping;

        private void Update() {
            if (_isLooping) {
                _timeProgress += Time.deltaTime;
                if (_timeProgress >= loopDelay) {
                    _timeProgress = 0;
                    StartAnimation();
                }
            }
        }

        public void StartLoop(Sprite[] sprites) {
            var centerSprites = new List<Sprite>();
            var endSprites = new List<Sprite>();
            for (var i = 0; i < sprites.Length; i++) {
                if (i < 4) {
                    centerSprites.Add(sprites[i]);
                } else if (i >= sprites.Length - 4) {
                    endSprites.Add(sprites[i]);
                }
            }
            _centerSprites = centerSprites.ToArray();
            _endSprites = endSprites.ToArray();
            
            center.SetOnDoneAni(() => {
                center.SetImageSprite(emptySprite);
                bomb.gameObject.SetActive(true);
            });
            left.SetOnDoneAni(() => {
                left.SetImageSprite(emptySprite);
            });
            right.SetOnDoneAni(() => {
                right.SetImageSprite(emptySprite);
            });
            up.SetOnDoneAni(() => {
                up.SetImageSprite(emptySprite);
            });
            down.SetOnDoneAni(() => {
                down.SetImageSprite(emptySprite);
            });
            
            StartAnimation();
            _isLooping = true;
        }
        
        private void StartAnimation() {
            bomb.gameObject.SetActive(false);
            center.StartAni(_centerSprites);
            left.StartAni(_endSprites);
            right.StartAni(_endSprites);
            up.StartAni(_endSprites);
            down.StartAni(_endSprites);
        }
    }
}