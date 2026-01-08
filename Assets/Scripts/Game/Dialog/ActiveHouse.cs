using App;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Game.Dialog
{
    public class ActiveHouse : MonoBehaviour
    {
        [SerializeField] private Transform LeftBottom;
        [SerializeField] private Transform RightTop;

        [SerializeField] private PlayerInHouse[] players;

        [SerializeField] private Image houseImage;
        [SerializeField]
        private AssetReference houseSpriteRef;
        private Sprite _houseSprite;

        public async UniTask SetActive(bool value)
        {
            if (value && _houseSprite == null) {
                _houseSprite = await AddressableLoader.LoadAsset<Sprite>(houseSpriteRef);
                houseImage.sprite = _houseSprite;
            }
            gameObject.SetActive(value);
        }

        public int GetPlayerCount()
        {
            return players.Length;
        }

        public void SetPlayerActive(int index, bool value)
        {
            players[index].gameObject.SetActive(value);
            players[index].SetLimited(LeftBottom, RightTop);
        }

        public void ChangeImagePlayer(int index, PlayerData playerData)
        {
            players[index].ChangeImage(playerData);
        }

    }
}