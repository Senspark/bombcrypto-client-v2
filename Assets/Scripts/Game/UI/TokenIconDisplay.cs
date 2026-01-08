using System;

using App;

using Senspark;

using Server.Models;

using Services.Rewards;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class TokenIconDisplay : MonoBehaviour {
        [SerializeField]
        private Image tokenImg;

        [SerializeField]
        private BlockRewardType type;

        private void Awake() {
            var launchPadManager = ServiceLocator.Instance.Resolve<ILaunchPadManager>();
            var acc = ServiceLocator.Instance.Resolve<IUserAccountManager>().GetRememberedAccount();
            tokenImg.sprite = launchPadManager.GetData(new RewardType(type), NetworkSymbol.Convert(acc.network)).icon;
        }
    }
}