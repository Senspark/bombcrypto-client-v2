using System.Collections.Generic;

using Data;

using Game.Dialog.BomberLand.BLGacha;

using Senspark;

using Services;

using UnityEngine;
using UnityEngine.UI;

public class MultiChestRewardItem : MonoBehaviour {
    [SerializeField]
    private BLGachaMultiChestReward itemPrefab;

    [SerializeField]
    private Text chest;

    [SerializeField]
    private Transform itemContainer;

    [SerializeField]
    private GameObject dotLine;

    private List<BLGachaMultiChestReward> _itemRewards;
    private IProductItemManager _productItemManager;

    private void Awake() {
        _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
    }

    public void Initialize(int indexChest, List<GachaChestItemData> chestReward) {
        chest.text = $"Chest {indexChest + 1}";
        _itemRewards = new List<BLGachaMultiChestReward>();
        foreach (var it in chestReward) {
            var reward = Instantiate(itemPrefab, itemContainer, false);
            reward.gameObject.SetActive(true);
            _itemRewards.Add(reward);
            var des = _productItemManager.GetItem((int)it.ProductId).Name;
            reward.Initialize(it, des);
            reward.UiShowDescription(false);
        }
    }

    private void SetSelect(List<BLGachaChestReward> items, BLGachaChestReward itemSelect) {
        foreach (var item in items) {
            item.UiShowDescription(item == itemSelect);
        }
    }

    public void DisableDotLine() {
        dotLine.gameObject.SetActive(false);
    }
}