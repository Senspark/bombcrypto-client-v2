using UnityEngine;

public class AbilityItem : MonoBehaviour
{
    [SerializeField] private GameObject tip;

    private void Start()
    {
        tip.SetActive(false);
    }

    public void ShowTip()
    {
        tip.SetActive(true);
    }

    public void HideTip()
    {
        tip.SetActive(false);
    }
}
