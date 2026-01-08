using System;
using App;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClubPromoteItem : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI infoText;
    
    [SerializeField]
    private Image selectFrame;

    private int _bidPriceIndex;
    private Action<int> _onSelect;
    
    public void SetInfo(int index, IClubBidPrice clubBidPrice, Action<int> onSelect) {
        _bidPriceIndex = index;
        _onSelect = onSelect;
        EnableSelectFrame(false);
        infoText.text = $"{GetSuffix(clubBidPrice.PackageId)} - <sprite name=\"ton deposit 1_0\"> {clubBidPrice.Price}";
    }

    private string GetSuffix(int packageId) {
        if (packageId % 10 == 1) {
            return $"{packageId}st";
        }
        if (packageId % 10 == 2)
        {
            return $"{packageId}nd";
        }
        if (packageId % 10 == 3)
        {
            return $"{packageId}rd";
        }
        return $"{packageId}th";
    }

    public void EnableSelectFrame(bool state) {
        if (state) {
            _onSelect?.Invoke(_bidPriceIndex);
        }
        selectFrame.gameObject.SetActive(state);
    }
}
