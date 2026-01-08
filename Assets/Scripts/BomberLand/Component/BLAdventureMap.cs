using System;

using Animation;

using App;

using DG.Tweening;

using UnityEngine;

namespace BomberLand.Component {
    public class BLAdventureMap : MonoBehaviour {
        [SerializeField]
        private int myStage;

        [SerializeField]
        private UnityEngine.UI.Button[] levelButtons;

        [SerializeField]
        private WaveInMenu wave;

        [SerializeField]
        private Transform hero;

        [SerializeField]
        private PlayerInMenu avatar;

        [SerializeField]
        private float distance = 600;

        private int _currentStage;

        public Action<int, int> OnLevelButtonCallback { set; get; }

        private void Awake() {
            wave.PlayAnimation(myStage);
            hero.gameObject.SetActive(false);
        }

        public void Initialize(int maxStage, int maxLevel, int stage, int level, PlayerData trHero) {
            _currentStage = 1;
            SetHeroPosition(stage, level);

            if (trHero != null) {
                avatar.ChangeImage(trHero);
                avatar.SetAnimation();
            }

            if (myStage > maxStage) {
                SetAllButton(false);
                return;
            }
            if (myStage < maxStage) {
                SetAllButton(true);
                return;
            }

            for (var i = 0; i <= maxLevel + 1 && i < levelButtons.Length; i++) {
                levelButtons[i].interactable = true;
            }
            for (var i = maxLevel + 1; i < levelButtons.Length; i++) {
                levelButtons[i].interactable = false;
            }
        }

        public void ChangeHero(PlayerData trHero) {
            if (trHero != null) {
                avatar.ChangeImage(trHero);
                avatar.SetAnimation();
            }
        }
        
        public void SetHeroPosition(int stage, int level) {
            if (myStage != stage) {
                hero.gameObject.SetActive(false);
                return;
            }

            hero.gameObject.SetActive(true);
            avatar.SetAnimation();
            hero.localPosition = levelButtons[level - 1].transform.localPosition;
        }

        private void SetAllButton(bool value) {
            foreach (var button in levelButtons) {
                button.interactable = value;
            }
        }

        public void OnLevelButtonClicked(int levelIndex) {
            OnLevelButtonCallback?.Invoke(myStage, levelIndex);
        }

        public void MoveTo(int stage, int level, float duration, Action callback) {
            if (stage == _currentStage) {
                callback?.Invoke();
                return;
            }

            var moveDistance = (_currentStage - stage) * distance;
            if (moveDistance < 0) {
                if (myStage <= _currentStage + 2) {
                    gameObject.SetActive(true);
                }
            } else {
                if (myStage >= _currentStage - 2) {
                    gameObject.SetActive(true);
                }
            }
            _currentStage = stage;
            var move = transform.DOLocalMoveX(moveDistance, duration).SetRelative(true);
            DOTween.Sequence()
                .Append(move)
                .AppendCallback(() => OnAfterMove(callback));
        }

        private void OnAfterMove(Action callback) {
            var index = (myStage - _currentStage) + 1;
            if (index is < -1 or > 3) {
                gameObject.SetActive(false);
            }
            callback?.Invoke();
        }
    }
}