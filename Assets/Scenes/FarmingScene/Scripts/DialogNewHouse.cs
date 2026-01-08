using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts
{
    public class DialogNewHouse : Dialog
    {
        [SerializeField] private Text houseType;
        [SerializeField] private Text size;
        [SerializeField] private Text charge;
        [SerializeField] private Text slot;

        public static UniTask<DialogNewHouse> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogNewHouse>();

        }

        public void SetInfo(HouseData data)
        {
            houseType.text = "   " + DefaultHouseStoreManager.GetHouseName(data.HouseType);
            size.text = DefaultHouseStoreManager.GetSizeString(data.Size);
            charge.text = "" + data.Charge + "/m";
            slot.text = "" + data.Slot;

        }
        
        public void OnOkBtnClicked()
        {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }

    }
}
