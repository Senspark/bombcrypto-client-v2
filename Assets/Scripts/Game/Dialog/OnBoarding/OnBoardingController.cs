using System.Collections;
using App;
using Senspark;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TutorialStep {
    NotInTutorial = -1,
    InTutorial,
    PopupStartTutorial,
    BuyHeroGuide,
    BuyHero,
    DoneBuyHero,
    StakeHeroGuide,
    StakeHero,
    DoneStakeHero,
    RepairShieldGuide,
    RepairShield,
    DoneRepairShield,
    PopupFinishTutorial,
    DoneTutorial
}

public enum TutorialClaimed {
    // Send confirm get reward to server
    None,
    BuyHero,
    StakeHero,
    RepairShield
}

public class OnBoardingController : MonoBehaviour {
    [SerializeField]
    private GameObject onBoardingBtn;
    
    [SerializeField]
    private GameObject tooltip;
    
    [SerializeField]
    private GameObject skipConfirmPopup;
    
    [SerializeField]
    private OnBoardingReward onBoardingReward;
    
    [SerializeField]
    private GameObject stepBuyHeroBtn;
    
    [SerializeField]
    private GameObject stepStakeHeroBtn;
    
    [SerializeField]
    private GameObject stepRepairShieldBtn;
    
    [SerializeField]
    private OnBoardingStep onBoardingStart;
    
    [SerializeField]
    private OnBoardingStep onBoardingBuyHero;
    
    [SerializeField]
    private OnBoardingStep onBoardingStakeHero;
    
    [SerializeField]
    private OnBoardingStep onBoardingRepairShield;
    
    [SerializeField]
    private OnBoardingStep onBoardingFinish;
    

    private TutorialClaimed _curClaimed = TutorialClaimed.None;
    private bool _isChangeStep = false;
    private bool _isTooltipOpen = false;
    private Coroutine _delayTooltip;
    private ObserverHandle _handle;
    private IOnBoardingManager _onBoardingManager;
    
    private async void Awake() {
        _onBoardingManager = ServiceLocator.Instance.Resolve<IOnBoardingManager>();
        skipConfirmPopup.SetActive(false);
        UpdateEventTrigger();
        await _onBoardingManager.InitConfig();
        if (_onBoardingManager.CurrentStep > TutorialStep.InTutorial) {
            SetTutorialStep();
        } else {
            if (IsUserInTutorial()) {
                SetNextStep();
            } else {
                SetupOnBoardingStep(TutorialStep.NotInTutorial);
                SetupTooltip();
                return;
            }
        }
        _handle = new ObserverHandle();
        _handle.AddObserver(_onBoardingManager, new OnBoardingObserver() {
            updateOnBoarding = UpdateOnBoarding,
            refreshOnBoarding = RefreshOnBoarding
        });
    }
    
    private void OnDestroy() {
        _handle.Dispose();
    }

    private void UpdateEventTrigger() {
        var pointerEnter = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener(OnPointerEnter);

        var pointerExit = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener(OnPointerExit);
        
        var pointerClick = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerClick
        };
        pointerClick.callback.AddListener(OnPointerClick);
        
        var ev = onBoardingBtn.GetComponent<EventTrigger>();
        if (AppConfig.IsMobile()) {
            ev.triggers.Add(pointerClick);
        } else {
            ev.triggers.Add(pointerEnter);
            ev.triggers.Add(pointerExit);
        }
    }
    
    private void OnPointerEnter(BaseEventData data) {
        tooltip.SetActive(true);
        _isTooltipOpen = true;
    }

    private void OnPointerExit(BaseEventData data) {
        tooltip.SetActive(false);
        _isTooltipOpen = false;
    }
    
    private void OnPointerClick(BaseEventData data) {
        if (_isTooltipOpen) {
            tooltip.SetActive(false);
            _isTooltipOpen = false;
        } else {
            tooltip.SetActive(true);
            _isTooltipOpen = true;
        }
    }

    private bool IsUserInTutorial() {
        var storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
        return storageManager.NewUser;
    }

    private void UpdateOnBoarding(TutorialStep tutorialStep) {
        if ((int)tutorialStep - (int)_onBoardingManager.CurrentStep == 1) {
            _onBoardingManager.NextCurrentStep(_curClaimed);
            _isChangeStep = true;
        }
    }

    private void RefreshOnBoarding() {
        if (_isChangeStep) {
            SetTutorialStep();
            _isChangeStep = false;
        }
    }
    
    private void SetTutorialStep() {
        SetupOnBoardingStep(_onBoardingManager.CurrentStep);
        SetupTooltip();
    }

    private void SetupOnBoardingStep(TutorialStep tutorialStep) {
        onBoardingStart.gameObject.SetActive(false);
        onBoardingFinish.gameObject.SetActive(false);
        onBoardingBuyHero.gameObject.SetActive(false);
        onBoardingStakeHero.gameObject.SetActive(false);
        onBoardingRepairShield.gameObject.SetActive(false);
        onBoardingReward.gameObject.SetActive(false);
        _curClaimed = TutorialClaimed.None;
        switch (tutorialStep) {
            case TutorialStep.PopupStartTutorial:
                onBoardingStart.gameObject.SetActive(true);
                break;
            case TutorialStep.BuyHeroGuide:
                onBoardingBuyHero.gameObject.SetActive(true);
                break;
            case TutorialStep.DoneBuyHero:
                _curClaimed = TutorialClaimed.BuyHero;
                onBoardingReward.InitReward($"Buy BHero", _onBoardingManager.RewardConfig[TutorialClaimed.BuyHero]);
                break;
            case TutorialStep.StakeHeroGuide:
                onBoardingStakeHero.gameObject.SetActive(true);
                break;
            case TutorialStep.DoneStakeHero:
                _curClaimed = TutorialClaimed.StakeHero;
                onBoardingReward.InitReward($"Stake BHero", _onBoardingManager.RewardConfig[TutorialClaimed.StakeHero]);
                break;
            case TutorialStep.RepairShieldGuide:
                onBoardingRepairShield.gameObject.SetActive(true);
                break;
            case TutorialStep.DoneRepairShield:
                _curClaimed = TutorialClaimed.RepairShield;
                onBoardingReward.InitReward($"Repair Shield", _onBoardingManager.RewardConfig[TutorialClaimed.RepairShield]);
                break;
            case TutorialStep.PopupFinishTutorial:
                onBoardingFinish.gameObject.SetActive(true);
                break;
        }
    }

    private void SetupTooltip() {
        onBoardingBtn.SetActive(_onBoardingManager.CurrentStep > TutorialStep.PopupStartTutorial && _onBoardingManager.CurrentStep < TutorialStep.PopupFinishTutorial);
        tooltip.SetActive(false);
        stepBuyHeroBtn.SetActive(false);
        stepStakeHeroBtn.SetActive(false);
        stepRepairShieldBtn.SetActive(false);
        if (_onBoardingManager.CurrentStep < TutorialStep.DoneBuyHero) {
            stepBuyHeroBtn.SetActive(true);
        } else if (_onBoardingManager.CurrentStep < TutorialStep.DoneStakeHero) {
            stepStakeHeroBtn.SetActive(true);
        } else if (_onBoardingManager.CurrentStep < TutorialStep.DoneRepairShield) {
            stepRepairShieldBtn.SetActive(true);
        }
    }
    
    public void EnableTooltip() {
        tooltip.SetActive(true);
        if (_delayTooltip != null) {
            StopCoroutine(_delayTooltip);
            _delayTooltip = null;
        }
    }
    
    public void DisableTooltip() {
        _delayTooltip = StartCoroutine(DisableToolTip());
    }

    private IEnumerator DisableToolTip() {
        yield return new WaitForSeconds(0.1f);
        tooltip.SetActive(false);
    }

    public void EnableOnBoardingStep() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        switch (_onBoardingManager.CurrentStep) {
            case TutorialStep.BuyHero:
                onBoardingBuyHero.gameObject.SetActive(true);
                break;
            case TutorialStep.StakeHero:
                onBoardingStakeHero.gameObject.SetActive(true);
                break;
            case TutorialStep.RepairShield:
                onBoardingRepairShield.gameObject.SetActive(true);
                break;
        }
    }

    public void OnGuideBtn() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        SetNextStep();
    }
    
    public void OnSkipBtn() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        skipConfirmPopup.SetActive(true);
    }

    public void OnNoBtn() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        skipConfirmPopup.SetActive(false);
    }
    
    public void OnYesBtn() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        skipConfirmPopup.SetActive(false);
        _onBoardingManager.SetCurrentStep(TutorialStep.DoneTutorial);
        SetTutorialStep();
    }
    
    public void OnOkRewardBtn() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        SetNextStep();
        onBoardingReward.gameObject.SetActive(false);
    }

    public void OnCloseStepPopup() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        if (_onBoardingManager.CurrentStep != TutorialStep.BuyHero
            && _onBoardingManager.CurrentStep != TutorialStep.StakeHero
            && _onBoardingManager.CurrentStep != TutorialStep.RepairShield) {
            SetNextStep();
        }
    }

    private void SetNextStep() {
        _onBoardingManager.NextCurrentStep(_curClaimed);
        SetTutorialStep();
    }
}
