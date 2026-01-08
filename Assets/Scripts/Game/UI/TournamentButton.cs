using App;

using DG.Tweening;

using Senspark;

using UnityEngine;

public class TournamentButton : MonoBehaviour {
    private ISoundManager _soundManager;
    private ISceneManager _sceneLoader;
    private IFeatureManager _featureManager;
    private IStoryModeManager _storyModeManager;

    private void Awake() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _sceneLoader = ServiceLocator.Instance.Resolve<ISceneManager>();
        _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
        _storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();

        var enable = _featureManager.IsUsingMetaMask;
        gameObject.SetActive(enable);
    }

    private void OnDestroy() {
        transform.DOKill();
    }

    public void OnTournamentBtnClicked() {
        _soundManager.PlaySound(Audio.Tap);
        _storyModeManager.SetTicketMode(StoryModeTicketType.Tournament);
        _storyModeManager.EnterToLevelMenu();
    }
}