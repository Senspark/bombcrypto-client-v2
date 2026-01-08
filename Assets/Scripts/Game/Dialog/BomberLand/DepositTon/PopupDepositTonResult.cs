using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PopupDepositTonResult : MonoBehaviour {
    [SerializeField]
    private GameObject resultObject, dim, successObject, failObject;
    [SerializeField]
    private RectTransform resultRectTransform;
    private Tween _currentTween;
    

    public void ShowResult(bool isSuccess) {
        resultRectTransform.DOAnchorPosY(-25, 0);
        gameObject.SetActive(true);
        dim.SetActive(true);
        successObject.SetActive(isSuccess);
        failObject.SetActive(!isSuccess);

        // Move up animation
        _currentTween?.Kill();
        _currentTween = resultRectTransform.DOAnchorPosY(25, 0.5f).OnComplete(() => {
            // Auto close after 5 seconds
            Invoke(nameof(CloseResult), 5f);
        });
    }

    public void CloseResult() {
        // Move down animation
        _currentTween?.Kill();
        dim.SetActive(false);
        _currentTween = resultRectTransform.DOAnchorPosY(-25, 0.5f).OnComplete(() => {
            gameObject.SetActive(false);
        });
    }

    private void OnDisable() {
        _currentTween?.Kill();
    }
}