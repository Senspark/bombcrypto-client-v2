using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Share.Scripts.Dialog;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// "Farm Averages" side panel. Behaves like a drawer (mostly off-screen left, right edge
/// peeking, slides in on hover). Three buttons switch content: Tokens / Chests / Reset.
/// Reads live values from <see cref="SessionAveragesManager"/> and refreshes once a second;
/// Reset clears the running averages.
/// </summary>
public class MediasPanelController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private float shownX = 12f;
    [SerializeField] private float hiddenX = -115f;
    [SerializeField] private float duration = 0.3f;

    [SerializeField] private Button tokenTabButton;
    [SerializeField] private Button chestTabButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private GameObject tokensContent;
    [SerializeField] private GameObject chestsContent;
    [SerializeField] private Image tokenTabFrame;
    [SerializeField] private Image chestTabFrame;

    private static readonly Color SelectedTint = Color.white;
    private static readonly Color UnselectedTint = new Color(0.55f, 0.55f, 0.62f, 1f);

    private class RowBind
    {
        public TextMeshProUGUI Total;
        public TextMeshProUGUI Rate;
        public Func<SessionAveragesManager.Data, double> Get;
        public bool Token; // token => decimals, else integer count
    }

    private readonly List<RowBind> _rows = new List<RowBind>();
    private TextMeshProUGUI _timeValue;
    private float _refreshTimer;

    private void Awake()
    {
        if (tokenTabButton != null) tokenTabButton.onClick.AddListener(ShowTokens);
        if (chestTabButton != null) chestTabButton.onClick.AddListener(ShowChests);
        if (resetButton != null) resetButton.onClick.AddListener(OnReset);

        CacheBindings();
        SetX(hiddenX);
        ShowTokens();
        Refresh();
    }

    private void OnDestroy()
    {
        if (tokenTabButton != null) tokenTabButton.onClick.RemoveListener(ShowTokens);
        if (chestTabButton != null) chestTabButton.onClick.RemoveListener(ShowChests);
        if (resetButton != null) resetButton.onClick.RemoveListener(OnReset);
        if (panel != null) panel.DOKill();
    }

    private void Update()
    {
        _refreshTimer += Time.unscaledDeltaTime;
        if (_refreshTimer >= 1f) { _refreshTimer = 0f; Refresh(); }
    }

    // ------------------------------------------------------ drawer (hover)

    public void OnPointerEnter(PointerEventData eventData) => Slide(shownX);
    public void OnPointerExit(PointerEventData eventData) => Slide(hiddenX);

    private void Slide(float x)
    {
        if (panel == null) return;
        panel.DOKill();
        panel.DOAnchorPosX(x, duration).SetEase(Ease.OutQuad);
    }

    private void SetX(float x)
    {
        if (panel == null) return;
        var p = panel.anchoredPosition;
        p.x = x;
        panel.anchoredPosition = p;
    }

    // ------------------------------------------------------ tabs

    public void ShowTokens() { SelectTab(true); }
    public void ShowChests() { SelectTab(false); }

    private void SelectTab(bool tokens)
    {
        if (tokensContent != null) tokensContent.SetActive(tokens);
        if (chestsContent != null) chestsContent.SetActive(!tokens);
        if (tokenTabFrame != null) tokenTabFrame.color = tokens ? SelectedTint : UnselectedTint;
        if (chestTabFrame != null) chestTabFrame.color = tokens ? UnselectedTint : SelectedTint;
    }

    private void OnReset()
    {
        ConfirmReset().Forget();
    }

    private async UniTaskVoid ConfirmReset()
    {
        var canvas = FindDialogCanvas();
        if (canvas == null) { DoReset(); return; } // no canvas to host the dialog: reset directly
        var confirm = await DialogConfirm.Create();
        confirm.SetInfo("Reset the farm averages?\nAll counters and the timer restart from zero.",
            "Yes", "No", DoReset, null).Show(canvas);
    }

    private Canvas FindDialogCanvas()
    {
        var go = GameObject.Find("CanvasDialog");
        if (go != null && go.TryGetComponent<Canvas>(out var dialogCanvas)) return dialogCanvas;
        var own = GetComponentInParent<Canvas>();
        return own != null ? own.rootCanvas : null;
    }

    private void DoReset()
    {
        if (SessionAveragesManager.Instance != null) SessionAveragesManager.Instance.Reset();
        Refresh();
    }

    // ------------------------------------------------------ live values

    private void CacheBindings()
    {
        _timeValue = FindText("Body/TimeRow/TimePill/Value");
        AddRow("Body/TokensContent/Row_BCOIN", d => d.bcoin, true);
        AddRow("Body/TokensContent/Row_SEN", d => d.sen, true);
        AddRow("Body/ChestsContent/Row_TotalChests", d => d.chestTotal, false);
        AddRow("Body/ChestsContent/Row_Diamond", d => d.chestDiamond, false);
        AddRow("Body/ChestsContent/Row_Gold", d => d.chestGold, false);
        AddRow("Body/ChestsContent/Row_Silver", d => d.chestSilver, false);
        AddRow("Body/ChestsContent/Row_Wood", d => d.chestWood, false);
        AddRow("Body/ChestsContent/Row_Bombs", d => d.bombs, false);
    }

    private void AddRow(string rowPath, Func<SessionAveragesManager.Data, double> get, bool token)
    {
        _rows.Add(new RowBind
        {
            Total = FindText(rowPath + "/TotalValue"),
            Rate = FindText(rowPath + "/PerHourValue"),
            Get = get,
            Token = token,
        });
    }

    private TextMeshProUGUI FindText(string path)
    {
        var t = transform.Find(path);
        return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
    }

    private void Refresh()
    {
        var m = SessionAveragesManager.Instance;
        var data = m != null ? m.Snapshot() : null;
        double hours = m != null ? m.ElapsedHours() : 0.0;

        if (_timeValue != null)
            _timeValue.text = m != null ? FormatElapsed(m.Elapsed()) : "00:00:00";

        for (int i = 0; i < _rows.Count; i++)
        {
            var r = _rows[i];
            double v = data != null ? r.Get(data) : 0.0;
            if (r.Total != null)
                r.Total.text = r.Token ? v.ToString("#,##0.##") : v.ToString("#,##0");
            if (r.Rate != null)
            {
                double rate = hours > 0.0 ? v / hours : 0.0;
                r.Rate.text = (r.Token ? rate.ToString("#,##0.##") : rate.ToString("#,##0")) + "/h";
            }
        }
    }

    private static string FormatElapsed(TimeSpan t)
    {
        if (t.TotalDays >= 1.0)
            return string.Format("{0}d {1:00}:{2:00}:{3:00}", (int)t.TotalDays, t.Hours, t.Minutes, t.Seconds);
        return string.Format("{0:00}:{1:00}:{2:00}", (int)t.TotalHours, t.Minutes, t.Seconds);
    }
}
