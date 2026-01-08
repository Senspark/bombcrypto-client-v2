using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogNewHouseAirdrop : Dialog {
    [SerializeField] private Text houseType;
    [SerializeField] private Text size;
    [SerializeField] private Text charge;
    [SerializeField] private Text slot;
    [SerializeField] private TextMeshProUGUI title;

    public static UniTask<DialogNewHouseAirdrop> Create() {
        return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogNewHouseAirdrop>();
    }

    public void SetInfo(HouseData data, string titleDialog = "NEW HOUSE") {
        houseType.text = DefaultHouseStoreManager.GetHouseName(data.HouseType);
        size.text = DefaultHouseStoreManager.GetSizeString(data.Size);
        charge.text = "" + data.Charge + "/m";
        slot.text = "" + data.Slot;
        title.text = titleDialog;
    }
        
    public void OnOkBtnClicked() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        Hide();
    }
}
