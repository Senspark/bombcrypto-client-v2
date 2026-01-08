using App;
using Constant;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Engine.Utils;
using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Senspark;
using Services;
using Share.Scripts.PrefabsManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.PvpModeScene.Scripts {
    public class DialogPvpRankUp : Dialog {
        [SerializeField]
        private TextMeshProUGUI rankName;

        [SerializeField]
        private BLGachaRes resource;

        [SerializeField]
        private Image iconRankFrom;

        [SerializeField]
        private Image iconRankTo;

        [SerializeField]
        private CanvasGroup body;

        [SerializeField]
        private ImageAnimation animation;

        [SerializeField]
        private Button buttonOK;

        private ISoundManager _soundManager;
        
        public static UniTask<DialogPvpRankUp> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogPvpRankUp>();
        }

        public async UniTask SetInfo(PvpRankType rankFrom, PvpRankType rankTo) {
            SetAnimation();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            rankName.text = BLProfileCard.GetRankName(rankTo);
            iconRankFrom.sprite = await resource.GetSpriteByPvpRank(rankFrom);
            iconRankTo.sprite = await resource.GetSpriteByPvpRank(rankTo);
        }
        
        private void SetAnimation() {
            buttonOK.interactable = false;
            animation.gameObject.SetActive(false);
            var color = iconRankTo.color;
            color.a = 0;
            iconRankTo.color = color;
            body.alpha = 0;
            var trans = iconRankFrom.transform;
            var pos = trans.localPosition;
            var y = pos.y;
            pos.y = -500;
            trans.localPosition = pos;

            OnWillShow(() => PlayAnimation(y));
        }

        private void PlayAnimation(float y) {
            _soundManager.PlaySound(Audio.RankUp);
            var moveUp = iconRankFrom.transform.DOLocalMoveY(y, 0.5f)
                .SetEase(Ease.InOutSine);
            DOTween.Sequence()
                .AppendInterval(1f)
                .Append(moveUp)
                .OnComplete(DoAnimationUp);
        }

        private async void DoAnimationUp() {
            iconRankFrom.DOFade(0, 1f);
            iconRankTo.DOFade(1, 1f);
            animation.gameObject.SetActive(true);
            animation.SetOnDoneAni(AnimationEnd);
            var ani = await resource.GetAnimation(GachaRankChange.RankUp);
            animation.StartAni(ani.AnimationIdle);
        }

        private void AnimationEnd() {
            animation.gameObject.SetActive(false);
            var fadeIn = body.DOFade(1, 1);
            DOTween.Sequence()
                .Append(fadeIn)
                .AppendCallback(() => { buttonOK.interactable = true; });
        }
    }
}