using System.Collections;
using System.Collections.Generic;

using App;

using Senspark;

using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData) {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
    }
}
