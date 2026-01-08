using System;

using Analytics;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Dialog.BomberLand;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

namespace Scenes.StoryModeScene.Scripts {
    public class DialogRating : Dialog {
        public const int Default = 0;
        public const int Showed = 1;
        public const string Status = nameof(DialogRating) + "Status";
        private Action _continuation;
        private int _value = 4;

        private IAnalytics _analytics;
        
        public static UniTask<DialogRating> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogRating>();
        }

        protected override void Awake() {
            base.Awake();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
        }

        public void OnButtonCloseClicked() {
            OnDidHide(_continuation);
            Hide();
        }

        public void OnButtonSendClicked() {
            _analytics.TrackRate(_value);
            if (_value < 4) {
                DialogFeedback.Create().ContinueWith(feedback => {
                    feedback.OnDidHide(() => ShowThankYou(_continuation));
                    feedback.Show(DialogCanvas);
                });
            } else {
                App.Utils.GoToStore();
                OnDidHide(() => ShowThankYou(_continuation));
            }
            Hide();
        }

        public void OnClickedOutside() {
            OnDidHide(_continuation);
        }

        public void OnCompleted(Action continuation) {
            _continuation = continuation;
        }

        public void OnValueChanged(int value) {
            _value = value;
        }

        private void ShowThankYou(Action continuation) {
            DialogOK.ShowInfo(DialogCanvas, "Thank you", continuation);
        }
    }
}