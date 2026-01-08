using System.Globalization;
using App;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopItemRock : MonoBehaviour {
        
        [SerializeField]
        public Image icon;
        
        [SerializeField]
        public Text title;
        
        [SerializeField]
        public Text tRock;

        public void SetData(BLShopResource shopResource, IRockPackConfig d) {
            icon.sprite = shopResource.GetImageRock(d.PackageName);
            title.text = ConvertToTitleCase(d.PackageName);
            tRock.text = $"+{d.RockAmount}";
        }
        
        private string ConvertToTitleCase(string input)
        {
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(input.ToLower().Replace("_", " "));
        }
    }
}