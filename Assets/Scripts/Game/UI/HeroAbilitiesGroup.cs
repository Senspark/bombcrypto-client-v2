using System;
using Engine.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public class HeroAbilitiesGroup : MonoBehaviour {
    private AbilityItem[] _abilitiesItems;
    private readonly int[] _abilitiesSizes = {40, 40, 40, 40, 40, 40, 40, 40, 35, 31, 28};

    private void Awake() {
        _abilitiesItems = GetComponentsInChildren<AbilityItem>();

        var pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerEnter;
        pointerDown.callback.AddListener(OnBtnControlEnter);

        var pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener(OnBtnControlExit);

        for (var i = 0; i < _abilitiesItems.Length; i++) {
            var item = _abilitiesItems[i];
            var ev = item.GetComponent<EventTrigger>();
            ev.triggers.Add(pointerDown);
            ev.triggers.Add(pointerExit);
        }
    }

    public void Hide() {
        _abilitiesItems ??= GetComponentsInChildren<AbilityItem>();
        Array.ForEach(_abilitiesItems, e => e.gameObject.SetActive(false));
    }

    public void Show(PlayerAbility[] abilities) {
        Hide();
        foreach (var ability in abilities) {
            if ((int)ability >= _abilitiesItems.Length) {
                continue;
            }
            _abilitiesItems[(int)ability].gameObject.SetActive(true);
            var itemSize = _abilitiesItems[(int)ability].GetComponent<RectTransform>().sizeDelta;
            var abilitiesCount = abilities.Length;
            _abilitiesItems[(int)ability].GetComponent<RectTransform>().sizeDelta = new Vector2(_abilitiesSizes[abilitiesCount], itemSize.y);
        }
    }

    public void OnBtnControlEnter(BaseEventData data) {
        var pointerEventData = (PointerEventData)data;
        if (pointerEventData == null) {
            return;
        }

        var ability = pointerEventData.pointerEnter.GetComponent<AbilityItem>();
        if (ability) {
            ability.ShowTip();
        }
    }

    public void OnBtnControlExit(BaseEventData data) {
        var pointerEventData = (PointerEventData)data;
        if (pointerEventData == null) {
            return;
        }

        var ability = pointerEventData.pointerEnter.GetComponent<AbilityItem>();
        if (ability) {
            ability.HideTip();
        }
    }
}