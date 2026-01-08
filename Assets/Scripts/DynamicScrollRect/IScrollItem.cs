using UnityEngine;

namespace Game.Dialog
{
    public interface IScrollItem
    {
        void Reset();
        int CurrentIndex { get; set; }
        RectTransform RectTransform { get; }
    }
}