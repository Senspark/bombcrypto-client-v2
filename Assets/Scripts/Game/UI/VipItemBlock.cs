using System;
using System.Collections.Generic;

using App;

using DG.Tweening;

using Game.Dialog;

using UnityEngine;
using UnityEngine.UI;

public class VipItemBlock : MonoBehaviour {
    [SerializeField]
    private Image block;

    [SerializeField]
    private float jumpYPos;
    
    [SerializeField]
    private Text vipTxt;

    [SerializeField]
    private Text stakeTxt;

    [SerializeField]
    private bool isPopup;

    [SerializeField]
    private List<VipItemDisplay> itemDisplays;

    private Vector2 _defaultPosition;
    private Tween _tween;

    public void SetData(
        List<DialogVip.RewardIcon> icons,
        Sprite background,
        Color color,
        IVipInfo data,
        bool showTotalAmount,
        Action<VipRewardType> onClaimClicked) {
        vipTxt.text = $"VIP {data.VipLevel}";
        stakeTxt.text = $"STAKE {data.StakeAmount}";
        // stakeTxt.color = color;
        
        _tween?.Kill();
        if (_defaultPosition == Vector2.zero) {
            _defaultPosition = block.rectTransform.anchoredPosition;
        }
        if (isPopup) {
            block.sprite = background;
        } else {
            if (data.IsCurrentVip) {
                var pos = _defaultPosition;
                pos.y += jumpYPos;
                _tween = block.rectTransform.DOAnchorPosY(pos.y, 1).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            } else {
                block.rectTransform.anchoredPosition = _defaultPosition;
            }
        }

        for (var i = 0; i < itemDisplays.Count; i++) {
            var item = itemDisplays[i];
            if (i >= data.Rewards.Count) {
                item.gameObject.SetActive(false);
                continue;
            }
            var reward = data.Rewards[data.Rewards.Count - 1 - i];
            var spr = icons.Find(e => e.type == reward.Type).sprite;
            item.SetData(spr, reward, showTotalAmount);
            item.SetCallback(onClaimClicked);
            item.gameObject.SetActive(true);
        }
    }
}