using Cysharp.Threading.Tasks;
using Game.UI;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Share.Scripts.Dialog {
    public class DialogWaiting : Game.Dialog.Dialog {
        [SerializeField]
        private Image blockUi;
        
        [SerializeField]
        private WaitingPanel waitingPanel;
        
        [SerializeField]
        private AnimationPanel animationPanel;

        public static UniTask<DialogWaiting> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogWaiting>();
        }

        public void BlockUi(Canvas canvas) {
            if (!blockUi) {
                return;
            }
            if (canvas == null) {
                return;
            }
            blockUi.transform.SetParent(canvas.transform, false);
        }

        public override void Hide() {
            if (blockUi) {
                Destroy(blockUi.gameObject);
            }
            base.Hide();
        }
        public override void HideImmediately() {
            if (blockUi) {
                Destroy(blockUi.gameObject);
            }
            base.HideImmediately();
        }

        public void ChangeText(string str) {
            waitingPanel.ChangeText(str);
        }

        public void ShowLoadingAnim() {
            waitingPanel.gameObject.SetActive(false);
            animationPanel.gameObject.SetActive(true);
        }

        protected override void OnNoClick() {
            // Do nothing
        }

        protected override void OnYesClick() {
            // Do nothing
        }
    }
}