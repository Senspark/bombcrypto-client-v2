using Engine.Utils;
using Game.Dialog.BomberLand.BLGacha;
using PvpMode.Services;
using Senspark;
using PvpMode.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.Dialogs {
    public class LeaderBoardItem : MonoBehaviour {
        [SerializeField]
        private Image medal;

        [SerializeField]
        private Text rankText;

        [SerializeField]
        private Text topRankText;

        [SerializeField]
        private Text addressText;

        [SerializeField]
        private Text pointText;

        [SerializeField]
        private Text winRateText;

        [SerializeField]
        private Text matchText;

        [SerializeField]
        private Text gemQuantity;
        
        [SerializeField]
        private ImageAnimation avatar;
        
        [SerializeField]
        private BLGachaRes resource;

        [SerializeField]
        private Button button;

        private int _userId;
        private string _userName;
        private IPvpRankingItemResult _rankItem;
        
        private ILogManager _logManager;

        public void SetInfo(IPvpRankingItemResult rank) {
            _rankItem = rank;
            SetInfo(rank.UserId, rank.RankNumber, rank.Name, rank.UserName, rank.Point, rank.Rewards[0].Quantity, rank.Avatar);
        }
        
        private async void SetInfo(int userId, int rank, string uname, string address, int point, int gem, int avatarId) {
            _userId = userId;
            _userName = uname ?? address;
            rankText.text = $"{rank}";
            if (rank > 3) {
                medal.gameObject.SetActive(false);
                topRankText.gameObject.SetActive(false);
                rankText.gameObject.SetActive(true);
            }
            addressText.text = Ellipsis.EllipsisAddress(App.Utils.GetShortenName(_userName));
            pointText.text = "" + point;
            gemQuantity.text = $"{gem}";
            
            var sprites = await resource.GetAvatar(avatarId);
            avatar.StartAni(sprites);
        }

        public void SetCurrentInfo(IPvpRankingItemResult item) {
            _rankItem = item;
            SetCurrentInfo(
                item.UserId,
                item.RankNumber,
                item.Name,
                item.UserName,
                item.WinMatch,
                item.TotalMatch,
                item.Point,
                item.Rewards[0].Quantity,
                item.Avatar
            );
        }
        
        private async void SetCurrentInfo(
            int userId,
            int rank,
            string uname,
            string address,
            int winMatch,
            int totalMatch,
            int point,
            int gem,
            int avatarId
        ) {
            _userId = userId;
            _userName = uname ?? address;
            _logManager ??= ServiceLocator.Instance.Resolve<ILogManager>();
            _logManager.Log($"rank: {rank}, uname: {uname}, address: {address}, gem: {gem}");
            var percent = 0f;
            if (totalMatch > 0) {
                percent = winMatch * 100f / totalMatch;
            }
            rankText.text = $"{rank}";
            addressText.text =  Ellipsis.EllipsisAddress(_userName);
            winRateText.text = $"{percent:0.00}%";
            matchText.text = $"{totalMatch}";
            pointText.text = $"{point}";
            gemQuantity.text = $"{gem}";
            _logManager.Log("end");
            
            var sprites = await resource.GetAvatar(avatarId);
            avatar.StartAni(sprites);
        }

        public void SetButtonClicked(System.Action<int, string, IPvpRankingItemResult> callback) {
            button.onClick.AddListener(()=> {
                callback?.Invoke(_userId, _userName, _rankItem);
            });
        }

        public void SetButtonClicked(System.Action currentCallback) {
            button.onClick.AddListener(()=> {
                currentCallback?.Invoke();
            });
        }
    }
}