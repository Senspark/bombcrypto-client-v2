using App;
using JetBrains.Annotations;
using Senspark;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OnBoardingStep : MonoBehaviour {
    [SerializeField]
    private Button leftArrow;
    
    [SerializeField]
    private Button rightArrow;
    
    [SerializeField, CanBeNull]
    private Button okStepBtn;

    [SerializeField]
    private GameObject[] pageContent;
    
    [SerializeField]
    private GameObject[] pageHighlight;

    [SerializeField]
    private UnityEvent skipBtnEvent;

    private int _curPage = 0;
    
    private void OnEnable() {
        _curPage = 0;
        UpdatePageContent();
    }
    
    private void UpdatePageContent() {
        if (pageContent.Length == 0) return;
        leftArrow.interactable = _curPage > 0;
        rightArrow.interactable = _curPage < pageContent.Length - 1;
        if (okStepBtn != null) {
            okStepBtn.gameObject.SetActive(_curPage == pageContent.Length - 1);
        }
        foreach (var content in pageContent) {
            content.SetActive(false);
        }
        pageContent[_curPage].SetActive(true);
        foreach (var highlight in pageHighlight) {
            highlight.SetActive(false);
        }
        pageHighlight[_curPage].SetActive(true);
    }

    public void OnLeftArrowBtn() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        _curPage--; 
        UpdatePageContent();
    }
    
    public void OnRightArrowBtn() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        _curPage++;
        UpdatePageContent();
    }

    public void OnPageBtn(int page) {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        _curPage = page;
        UpdatePageContent();
    }

    public void OnSkipBtn() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        skipBtnEvent.Invoke();
    }

    public void OnCloseBtn() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        this.gameObject.SetActive(false);
    }
    
    public void OpenURL(string url) {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        Application.OpenURL(url);
    }
}
