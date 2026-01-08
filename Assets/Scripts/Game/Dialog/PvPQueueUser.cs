using System;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.Dialog {
    public class PvPQueueUser : MonoBehaviour {
        [FormerlySerializedAs("_joiningTimeText")]
        [SerializeField]
        private Text joiningTimeText;
        
        [FormerlySerializedAs("_pointText")]
        [SerializeField]
        private Text pointText;

        [FormerlySerializedAs("_walletAddressText")]
        [SerializeField]
        private Text walletAddressText;

        public void UpdateUI(TimeSpan joiningTime, int point, string walletAddress) {
            joiningTimeText.text = $"{joiningTime.Hours}:{joiningTime.Minutes}:{joiningTime.Seconds}";
            pointText.text = $"{point}";
            walletAddressText.text =
                $"{walletAddress[..4]}...{walletAddress[^4..]}";
        }
    }
}