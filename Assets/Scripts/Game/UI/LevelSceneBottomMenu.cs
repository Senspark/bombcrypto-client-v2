using App;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Senspark;
using Game.Dialog;
using Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class LevelSceneBottomMenu : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private Vector2 arrowMinMaxY;
        
        [SerializeField]
        private Vector2 minMaxY;
        
        [SerializeField]
        private Button menuBtn;

        [SerializeField]
        private Button arrowBtn;

        [SerializeField]
        private Image arrow;

        [SerializeField]
        private Image background;

        [SerializeField]
        private RectTransform containerArrow;
        
        [SerializeField]
        private RectTransform container;

        private const float DURATION = 0.3f;
        private bool _isMouseInsideUI;
        private Coroutine _coroutine;
        private LevelScene _levelScene;
        private INetworkConfig _networkConfig;

        private void Awake() {
            _networkConfig = ServiceLocator.Instance.Resolve<INetworkConfig>();
            background.enabled = false;
            menuBtn.onClick.AddListener(Open);
            arrowBtn.onClick.AddListener(Open);
        }

        private void Start() {
            _levelScene = FindObjectOfType<LevelScene>();
            UniTask.Void(async () => {
                await UniTask.Delay(1000);
                arrow.rectTransform.DOKill();
                arrow.rectTransform.DOAnchorPosY(10, 1f).SetLoops(-1, LoopType.Yoyo);
            });
        }

        private void OnDisable() {
            container.DOKill();
            containerArrow.DOKill();
        }

        #region BUTTONS CALLBACKS

        public void OnTwitterButtonClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            CommunityUrl.OpenLink(CommunityUrl.CommunityLink.Twitter, _networkConfig.NetworkType);
        }

        public void OnDiscordButtonClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            CommunityUrl.OpenLink(CommunityUrl.CommunityLink.Discord, _networkConfig.NetworkType);
        }

        public void OnOpenYoutubeButtonClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            CommunityUrl.OpenLink(CommunityUrl.CommunityLink.Youtube, _networkConfig.NetworkType);
        }

        public void OnOpenCommunityDialogButtonClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            DialogCommunityLink.Create().ContinueWith(dialog => {
                dialog.Show(canvasDialog);
            });
        }

        #endregion

        private void Open() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            menuBtn.onClick.RemoveAllListeners();
            arrowBtn.onClick.RemoveAllListeners();

            _levelScene.PauseStatus.SetValue(this, true);
            background.enabled = true;
            container.DOAnchorPosY(minMaxY.y, DURATION);
            containerArrow.DOAnchorPosY(arrowMinMaxY.y, DURATION).OnComplete(AfterOpened);
        }

        private void AfterOpened() {
            arrowBtn.onClick.AddListener(Close);
            arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 180);
        }

        private void Close() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            menuBtn.onClick.RemoveAllListeners();
            arrowBtn.onClick.RemoveAllListeners();

            background.enabled = false;
            container.DOAnchorPosY(minMaxY.x, DURATION);
            containerArrow.DOAnchorPosY(arrowMinMaxY.x, DURATION).OnComplete(AfterClosed);
            _levelScene.PauseStatus.SetValue(this, false);
        }

        private void AfterClosed() {
            menuBtn.onClick.AddListener(Open);
            arrowBtn.onClick.AddListener(Open);
            arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}