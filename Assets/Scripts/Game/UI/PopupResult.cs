
using UnityEngine;
public class PopupResult : MonoBehaviour {
    // [SerializeField]
    // private GameObject successPanel, failPanel;
    //
    // [SerializeField, CanBeNull, Optional]
    // private TMP_Text heroId, level, shield;
    //
    // [SerializeField, CanBeNull, Optional]
    // private Text rareText;
    //
    // [SerializeField, CanBeNull, Optional]
    // private Avatar icon;
    //
    // [SerializeField, CanBeNull, Optional]
    // private DisplayRarity rarity;
    //
    // private Action _callbackHide;
    // private bool _isSuccess;
    //
    // public void Show(bool isSuccess, HeroDataSuccess heroData = null, Action callbackHide = null) {
    //     _callbackHide = callbackHide;
    //     _isSuccess = isSuccess;
    //
    //     if (heroData != null) {
    //         if (icon) icon.ChangeImage(heroData.PlayerData);
    //         if (heroId) heroId.text = heroData.HeroId;
    //         if (level) level.text = "Lv " + heroData.Level;
    //         if (shield) shield.text = heroData.CurrentShield + "/" + heroData.TotalShield;
    //         if (rarity) rarity.Show(heroData.PlayerData.rare);
    //         if (rareText) rareText.text = rareText.text.ToUpper();
    //     }
    //
    //     gameObject.SetActive(true);
    //     successPanel.SetActive(isSuccess);
    //     failPanel.SetActive(!isSuccess);
    // }
    //
    // public void Hide() {
    //     _callbackHide?.Invoke();
    //     gameObject.SetActive(false);
    // }
}