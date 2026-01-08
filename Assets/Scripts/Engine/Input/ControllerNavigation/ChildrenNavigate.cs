using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.Events;

public interface IChildrenNavigate {
    void Navigate(Vector2Int moveOut);
    void Out();
    void Choose(UnityEvent onChoose);
    Vector2Int Position { get; }
    void Init(Action<Vector2Int> forceChoose);
    void SetActiveHighLight(bool value);
    List<Vector2Int> IndexPosition { get; }

}
public class ChildrenNavigate : MonoBehaviour, IChildrenNavigate
{
    [SerializeField] [CanBeNull] private GameObject highlight;
    [SerializeField] private UnityEvent<bool> onNavigate;
    [SerializeField] private UnityEvent choose;

    [SerializeField]
    private List<Vector2Int> indexPosition;
    private RectTransform _rectTransform;
    private Action<Vector2Int> _forceChoose;
    public Vector2Int Position { get; private set; }

    public void SetActiveHighLight(bool value) {
        GetHighLightObject();
        SetActiveHighlight(value);
    }

    public List<Vector2Int> IndexPosition => indexPosition;

    public void Init(Action<Vector2Int> forceChoose) {
        if(_rectTransform != null)
            return;
        
        _rectTransform = GetComponent<RectTransform>();
        _forceChoose = forceChoose;
        Position = new Vector2Int((int)_rectTransform.localPosition.x, (int)_rectTransform.localPosition.y);
        GetHighLightObject();
        SetActiveHighlight(false);
    }

    private void GetHighLightObject() {
        if(highlight == null) {
            var hl = GetComponent<HighLightNavigate>();
            if(hl != null) {
                highlight = hl.gameObject;
            } else {
                var hls = GetComponentsInChildren<HighLightNavigate>(true);
                if(hls.Length > 0) {
                    highlight = hls[0].gameObject;
                }
            }
        }
    }

    public void Navigate(Vector2Int moveOut) {
        onNavigate.Invoke(true);
        SetActiveHighlight(true);
    }

    public void Out() {
        onNavigate.Invoke(false);
        SetActiveHighlight(false);
    }

    public void Choose(UnityEvent onChoose) {
        onChoose.Invoke();
        choose.Invoke();
    }
    
    public void ForceChoose() {
        _forceChoose?.Invoke(Position);
    }

    private void SetActiveHighlight(bool active) {
        if(highlight == null) {
            return;
        }
        highlight.SetActive(active);
    }
    
}
