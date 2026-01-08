using UnityEngine;

public class SafeArea : MonoBehaviour
{
    RectTransform Panel;
    Rect LastSafeArea = new Rect(0, 0, 0, 0);

    public RectTransform leftMenuTop;
    public RectTransform leftMenuBottom;
    public RectTransform rightMenuTop;
    public RectTransform rightMenuBottom;

    void Awake()
    {
        Panel = GetComponent<RectTransform>();

        if (Panel == null)
        {
            Debug.LogError("Cannot apply safe area - no RectTransform found on " + name);
            Destroy(gameObject);
        }

        Refresh();
    }

    void Update()
    {
        Refresh();
    }

    void Refresh()
    {
        var safeArea = GetSafeArea();

        if (safeArea != LastSafeArea) {
            ApplySafeArea(safeArea);
        }
    }

    Rect GetSafeArea()
    {
        var safeArea = Screen.safeArea;
        return safeArea;
    }

    void ApplySafeArea(Rect r)
    {
        LastSafeArea = r;

        var ratio = 800.0f / Screen.width;

        var dleft = r.position.x * ratio;
        var dright = (Screen.width - (r.position.x + r.size.x)) * ratio;

        updateLeft(leftMenuTop, dleft);
        updateLeft(leftMenuBottom, dleft);
        updateRight(rightMenuTop, dright);
        updateRight(rightMenuBottom, dright);

    }

    private void updateLeft(RectTransform trans, float dleft)
    {
        if (trans != null)
        {
            var leftPos = trans.anchoredPosition;
            leftPos.x = dleft;
            trans.anchoredPosition = leftPos;
        }
    }

    private void updateRight(RectTransform trans, float dright)
    {
        if (trans != null)
        {
            var rightPos = trans.anchoredPosition;
            rightPos.x = -dright;
            trans.anchoredPosition = rightPos;
        }
    }
}
