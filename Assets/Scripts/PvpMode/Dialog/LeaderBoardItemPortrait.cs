using System;
using Senspark;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.Dialogs {
    public class LeaderBoardItemPortrait : MonoBehaviour {
        [SerializeField]
        private Image medal;

        [SerializeField]
        private Text rankText;

        [SerializeField]
        private Text topRankText;

        [SerializeField]
        private Text addressText;

        [SerializeField]
        private TMP_Text pointText;

        private ILogManager _logManager;
        private Action<long> _action;
        private long _itemId;
        private bool _isCurrentClub;
        
        private const int NAME_LENGTH_LIMIT = 16;
        
        public void SetInfo(int rank, string uname, float point) {
            if (rank > 3) {
                medal.gameObject.SetActive(false);
                topRankText.gameObject.SetActive(false);
                rankText.gameObject.SetActive(true);
                rankText.text = $"{rank}";
            }
            
            if (App.AppConfig.IsWebAirdrop() && uname != null) {
                //DevHoang: Add new airdrop
                if (uname.EndsWith("ron") || 
                    uname.EndsWith("bas") ||
                    uname.EndsWith("vic")) {
                    uname = uname.Substring(0, uname.Length - 3);
                }
            }
            
            addressText.text = string.IsNullOrEmpty(uname) ? "--" : GetShortenName(uname);
            pointText.text = point > 0 ? $"{point:#,0.####}" : "--";
        }

        public void SetCurrentInfo(
            int rank,
            string uname,
            float point
        ) {
            _logManager ??= ServiceLocator.Instance.Resolve<ILogManager>();
            _logManager.Log($"rank: {rank}, uname: {uname}");
            rankText.text = rank > 0 ? $"{rank}" : "--";
            
            if (App.AppConfig.IsWebAirdrop() && uname != null) {
                //DevHoang: Add new airdrop
                if (uname.EndsWith("ron") || 
                    uname.EndsWith("bas") ||
                    uname.EndsWith("vic")) {
                    uname = uname.Substring(0, uname.Length - 3);
                }
            }
            
            addressText.text = string.IsNullOrEmpty(uname) ? "--" : GetShortenName(uname);
            pointText.text = point > 0 ? $"{point:#,0.####}" : "--";
            _logManager.Log("end");
        }

        private string GetShortenName(string name) {
            var friendlyName = TonAddressConverter.ToFriendlyAddress(name);
            if (name.Length > NAME_LENGTH_LIMIT) {
                var start = friendlyName.Substring(0, 5);
                var end = friendlyName.Substring(friendlyName.Length - 4);
                var shortenName = $"{start}...{end}";
                return shortenName;
            }
            return friendlyName;
        }

        public void SetItemId(long itemId) {
            _itemId = itemId;
        }

        public void SetItemButton(Action<long> action) {
            _action = action;
        }

        public void OnItemButton() {
            _action?.Invoke(_itemId);
        }
    }
}