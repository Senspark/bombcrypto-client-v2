using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using TMPro;
using Platform = Data.Platform;

namespace Game.Dialog {
    public class DialogReferralTask : Dialog {
        [SerializeField]
        private TextMeshProUGUI descText;

        private ISoundManager _soundManager;
        private ObserverHandle _handle;

        public static UniTask<DialogReferralTask> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogReferralTask>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public void UpdateUI(int minClaimReferral, int timePayOutReferral) {
            var timePayOut = $"{timePayOutReferral}";
            descText.SetText($"Invite friends to earn Star Cores\n" +
                             $"Star Cores earn from Referral will be account into your <color=#13EE00>Balance</color> " +
                             $"and <color=#EE8100>Ranking</color>\nStar Cores earn from Referral will be pay out every {timePayOut}h\n" +
                             $"Minimum claim is <color=#13EE00>{minClaimReferral}</color> Star Cores");
        }

        protected override void OnDestroy() {
            if (_handle != null) {
                _handle.Dispose();
            }
            base.OnDestroy();
        }

        public void OnCloseBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}