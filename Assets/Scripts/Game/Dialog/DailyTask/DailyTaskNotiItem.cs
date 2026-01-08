using App;

using Cysharp.Threading.Tasks;
using Game.Dialog.BomberLand.BLGacha;
using Scenes.MainMenuScene.Scripts;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Senspark;

public class DailyTaskNotiItem : MonoBehaviour {
    [SerializeField]
    private RectTransform rectTrf;
    
    [SerializeField]
    private CanvasGroup canvasGroup;
    
    [SerializeField]
    private Image taskIcon;
    
    [SerializeField]
    private BLGachaRes resource;

    private Canvas _canvas;

    private const float DURATION = 1f;

    public void SetTaskIcon(Sprite taskIcon, Canvas canvas) {
        _canvas = canvas;
        this.taskIcon.sprite = taskIcon;
    }
    
    public void PlayAnimSlot1() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.ShowNoti);
        var animSeq = DOTween.Sequence();
        animSeq.Append(rectTrf.DOAnchorPosX(rectTrf.sizeDelta.x * -1, DURATION));
        animSeq.AppendInterval(DURATION);
        animSeq.Append(canvasGroup.DOFade(0f, DURATION));
        animSeq.OnKill(() => Destroy(gameObject));
    }

    public void PlayAnimSlot2(float multiple, float range) {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.ShowNoti);
        var animSeq = DOTween.Sequence();
        animSeq.AppendInterval(multiple * DURATION * 2);
        animSeq.Append(rectTrf.DOAnchorPosX(rectTrf.sizeDelta.x * -1, DURATION));
        animSeq.AppendInterval(DURATION);
        animSeq.Append(rectTrf.DOAnchorPosY(range, DURATION));
        animSeq.AppendInterval(DURATION);
        animSeq.Append(canvasGroup.DOFade(0f, DURATION));
        animSeq.OnKill(() => Destroy(gameObject));
    }

    public void OnGoToDailyTaskBtn() {
        DialogDailyTask.Create().ContinueWith((dialog) => { dialog.Show(_canvas); });
    }
}
