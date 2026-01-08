using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

public class CharacterItemUpgrade : MonoBehaviour
{
    [SerializeField] private GameObject frameChoose;
    [SerializeField] private Avatar avatar;
    [SerializeField] private Text charName;
    [SerializeField] private Text charLv;

    [SerializeField] private GameObject[] rares;

    public System.Action<int> onClickCallback;
    private int myIndex;

    public void OnClicked()
    {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        onClickCallback?.Invoke(myIndex);
    }

    public void SetChoose(bool value)
    {
        frameChoose.SetActive(value);
    }

    public void SetInfo(PlayerData player, int index, System.Action<int> callback)
    {
        myIndex = index;
        onClickCallback = callback;

        avatar.ChangeImage(player);
        charName.text = "" + player.heroId.Id;
        charLv.text = "Lv" + player.level;

        foreach (var rare in rares)
        {
            rare.SetActive(false);
        }
        rares[player.rare].SetActive(true);
    }
}
