using System;
using App;
using Senspark;
using TMPro;
using Ton.Leaderboard;
using UnityEngine;
using UnityEngine.UI;

public class ClubJoinItem : MonoBehaviour {
    [SerializeField]
    private Image rankIcon;
    
    [SerializeField]
    private Text nameText;
    
    [SerializeField]
    private TextMeshProUGUI minedText;
    
    [SerializeField]
    private AirdropRankTypeResource resource;

    private Action<long> _onClubInfo;
    private long _clubId;
    
    public void SetInfo(IClubRank clubRank, AirdropRankType rankType, Action<long> onClubInfo) {
        _clubId = clubRank.ClubId;
        rankIcon.sprite = resource.GetAirdropRankTypeIcon(rankType);
        nameText.text = $"{clubRank.Name}";
        minedText.text = clubRank.PointCurrentSeason > 0 ? $"{clubRank.PointCurrentSeason:#,0.####}" : "--";
        _onClubInfo = onClubInfo;
    }
    
    public void OnClubInfo() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        _onClubInfo?.Invoke(_clubId);
    }
}
