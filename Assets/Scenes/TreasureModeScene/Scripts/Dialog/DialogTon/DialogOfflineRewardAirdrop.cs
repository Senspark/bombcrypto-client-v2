using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.PrefabsManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogOfflineRewardAirdrop : Dialog {
        [SerializeField]
        private Text titleTxt;

        [SerializeField]
        private TMP_Text amountTxt;
        
        public static UniTask<DialogOfflineRewardAirdrop> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogOfflineRewardAirdrop>();
        }
        
        public void Show(Canvas canvas, string time, string amount) {
            titleTxt.text = $"{time}H OFFLINE REWARD";
            amountTxt.text = $"<sprite index=0> x{amount}";
            Show(canvas);
        }
        
        public void OnBtnClaim() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }
    }
}