using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using UnityEngine;

[Service(nameof(IOnBoardingManager))]
public interface IOnBoardingManager : IService, IObserverManager<OnBoardingObserver> {
    TutorialStep CurrentStep { get; }
    Dictionary<TutorialClaimed, float> RewardConfig { get; }
    UniTask InitConfig();
    void InitCurrentStep(int currentStep);
    void InitRewardConfig(Dictionary<TutorialClaimed, float> rewardConfig);
    void SetCurrentStep(TutorialStep value);
    void NextCurrentStep(TutorialClaimed tutorialClaimed);
    void UpdateOnBoarding(TutorialStep tutorialStep);
    void RefreshOnBoarding();
}

public class OnBoardingObserver {
    public Action<TutorialStep> updateOnBoarding;
    public Action refreshOnBoarding;
}

public class OnBoardingManager : ObserverManager<OnBoardingObserver>, IOnBoardingManager {
    public TutorialStep CurrentStep { get; set; }
    public Dictionary<TutorialClaimed, float> RewardConfig { get; set; }

    public async UniTask InitConfig() {
        var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        var logManager = ServiceLocator.Instance.Resolve<ILogManager>();
        try {
            await serverManager.General.GetOnBoardingConfig();
        } catch (Exception e) {
            logManager.Log(e.Message);
        }
    }

    public void InitCurrentStep(int currentStep) {
        CurrentStep = (TutorialStep)currentStep;
    }

    public void InitRewardConfig(Dictionary<TutorialClaimed, float> rewardConfig) {
        RewardConfig = rewardConfig;
    }
    
    public void SetCurrentStep(TutorialStep value) {
        CurrentStep = value;
        var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        serverManager.General.UpdateUserOnBoarding((int)CurrentStep);
    }
    
    public void NextCurrentStep(TutorialClaimed tutorialClaimed) {
        CurrentStep++;
        var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        serverManager.General.UpdateUserOnBoarding((int)CurrentStep, (int)tutorialClaimed);
    }
    
    public void UpdateOnBoarding(TutorialStep tutorialStep) {
        DispatchEvent(e => e.updateOnBoarding?.Invoke(tutorialStep));
    }
    
    public void RefreshOnBoarding() {
        DispatchEvent(e => e.refreshOnBoarding?.Invoke());
    }

    public Task<bool> Initialize() {
        return Task.FromResult(true);
    }

    public void Destroy() {
    }
}
