#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor tool for the in-game "Farm Averages" panel. The panel MIRRORS the Ranking
/// Pool construction exactly: same background sprite split into a TopBG title strip
/// (35px) + BottomBG body, same title style (12pt gold + info icon), same 35x35
/// buttons with 22px icons, same Frame_Rarity_Pool rows (~130 wide) and font sizes
/// (names 10 / values 9). Content follows the approved mockup: Tokens / Chests /
/// Reset tabs, Time pill, TOTAL & PER HOUR columns, footer note.
/// Menus:
///  - Build Preview Panel: transient preview in the open scene (NEVER save the scene!).
///  - Inject Into FarmingScene Prefab / Remove From FarmingScene Prefab.
/// </summary>
public static class MediasPanelBuilder
{
    const string CanvasName = "MediasPreviewCanvas";
    const string InjectedRootName = "MediasUI_Root";
    const string RankingPrefabPath = "Assets/Scenes/FarmingScene/Prefabs/FarmingScene.prefab";

    const string FontPath = "Assets/Resources/Fonts/tahomabd SDF - Black Outline.asset";
    const string FramePanelPath = "Assets/Scenes/HoangAssets/Dialogs/Frame_Ranking_Pool.png";
    const string FrameRowPath = "Assets/Scenes/HoangAssets/Icons/Frame_Rarity_Pool.png";
    const string BtnGreenPath = "Assets/Scenes/HoangAssets/Icons/btn_green.png";
    const string BtnCyanPath = "Assets/Scenes/HoangAssets/Icons/btn_cyan.png";
    const string BtnPurplePath = "Assets/Scenes/HoangAssets/Icons/btn_purple.png";
    const string BtnBlackPath = "Assets/Scenes/HoangAssets/Icons/btn_black.png";
    const string InfoIconPath = "Assets/Scenes/TreasureModeScene/Textures/Pack/TonUsing/icon_info.png";
    const string ClockIconPath = "Assets/Textures/Misc_UI/PvpMode/clock.png";
    const string ResetIconPath = "Assets/Textures/Polygon UI/Item/reset.png";

    const string BcoinIcon = "Assets/Textures/NewAssets/Ton/TokenIcon/bcoin_bnb.png";
    const string SenIcon = "Assets/Textures/NewAssets/Ton/Tokens/SEN_BNB.png";
    const string ChestStatsIcon = "Assets/Textures/ItemIcon/icon_chest_stats.png";
    const string DiamondIcon = "Assets/Textures/Others/Bricks/diamondChest/brick_00.png";
    const string GoldIcon = "Assets/Textures/Others/Shop/GoldChest/Icon_GoldChest.png";
    const string SilverIcon = "Assets/Textures/Others/Shop/SilverChest/Icon_SilverChest.png";
    const string WoodIcon = "Assets/Textures/Others/Shop/BronzeChest/Icon_BronzeChest.png";
    const string BombIcon = "Assets/Scenes/HoangAssets/Icons/icon_bomb.png";

    // Mirrors the Ranking Pool: panel 145 wide, TopBG strip 35 + BottomBG body,
    // title 12pt gold, buttons 35x35 w/ 22px icons, rows ~130 wide, names 10 / values 9.
    const float PanelWidth = 145f;
    const float PanelHeight = 410f;
    const float PanelYOffset = -20f;

    static readonly Color C_Title = new Color(0.9764706f, 0.8039216f, 0f, 1f); // exact RP title gold
    static readonly Color C_Name = Color.white;
    static readonly Color C_Value = new Color(0.15f, 0.48f, 0.10f, 1f);
    static readonly Color C_MiniLabel = new Color(0.42f, 0.36f, 0.38f, 1f);
    static readonly Color C_Footer = new Color(0.62f, 0.68f, 0.95f, 1f);

    // ---------------------------------------------------------------- Preview

    [MenuItem("Tools/Medias/Build Preview Panel")]
    public static void BuildPreview()
    {
        DestroyByName(CanvasName);
        DestroyByName("MediasMockCanvas");

        var cam = Camera.main;
        if (cam == null)
        {
            var cams = Object.FindObjectsOfType<Camera>();
            if (cams.Length > 0) cam = cams[0];
        }

        var canvasGo = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 1f;
        canvas.sortingOrder = 32000;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        var panel = BuildPanel(canvasGo.transform);
        var pr = (RectTransform)panel.transform;
        pr.anchorMin = new Vector2(0f, 0.5f);
        pr.anchorMax = new Vector2(0f, 0.5f);
        pr.pivot = new Vector2(0f, 0.5f);
        pr.anchoredPosition = new Vector2(12f, 0f); // preview shown fully in

        // Preview shows the CHESTS tab (fuller view). Do NOT save the scene with this preview!
        SetTabStatic(panel.transform, tokens: false);

        Canvas.ForceUpdateCanvases();
        Selection.activeGameObject = canvasGo;
        Debug.Log("[Medias] Preview built (chests tab shown). NEVER save the scene with the preview in it.");
    }

    [MenuItem("Tools/Medias/Remove Preview Panel")]
    public static void RemovePreview()
    {
        DestroyByName(CanvasName);
        DestroyByName("MediasMockCanvas");
        Debug.Log("[Medias] Preview panel(s) removed.");
    }

    // ---------------------------------------------------------- Prefab inject

    [MenuItem("Tools/Medias/Inject Into FarmingScene Prefab")]
    public static void InjectIntoPrefab()
    {
        var root = PrefabUtility.LoadPrefabContents(RankingPrefabPath);
        try
        {
            Component mpc = null;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                var c = t.GetComponent("MinePoolController");
                if (c != null) { mpc = c; break; }
            }
            if (mpc == null) { Debug.LogError("[Medias] MinePoolController not found in prefab. Nothing injected."); return; }

            var uiParent = mpc.transform.parent; // same Canvas as the Ranking Pool

            var existing = uiParent.Find(InjectedRootName);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // Full-screen root (single object -> easy to remove), same z-level as RP.
            var uiRoot = MakeUI(InjectedRootName, uiParent);
            var rr = (RectTransform)uiRoot.transform;
            rr.anchorMin = Vector2.zero; rr.anchorMax = Vector2.one; rr.offsetMin = Vector2.zero; rr.offsetMax = Vector2.zero;
            uiRoot.transform.SetSiblingIndex(mpc.transform.GetSiblingIndex() + 1);

            // Drawer: the panel sits mostly off-screen LEFT with only its RIGHT edge peeking,
            // and slides fully in (to the RIGHT) on mouse hover. Anchored to the screen's left edge.
            var panel = BuildPanel(uiRoot.transform);
            var pr = (RectTransform)panel.transform;
            pr.anchorMin = new Vector2(0f, 0.5f);
            pr.anchorMax = new Vector2(0f, 0.5f);
            pr.pivot = new Vector2(0f, 0.5f);
            float peek = 34f;                       // right edge visible when collapsed
            float shownX = 12f;                     // fully in
            float hiddenX = -(PanelWidth - peek);   // mostly off-screen; only the right edge peeks
            pr.anchoredPosition = new Vector2(hiddenX, PanelYOffset);

            // Controller lives ON the panel so hover is detected over the panel/peek only.
            var ctrlType = mpc.GetType().Assembly.GetType("MediasPanelController");
            if (ctrlType == null) { Debug.LogError("[Medias] MediasPanelController type not found (compile it first)."); return; }
            var ctrl = panel.AddComponent(ctrlType);
            var so = new SerializedObject(ctrl);
            SetObj(so, "panel", pr);
            SetFloat(so, "shownX", shownX);
            SetFloat(so, "hiddenX", hiddenX);
            SetFloat(so, "duration", 0.3f);
            SetObj(so, "tokenTabButton", panel.transform.Find("Tabs/TokenTab").GetComponent<Button>());
            SetObj(so, "chestTabButton", panel.transform.Find("Tabs/ChestTab").GetComponent<Button>());
            SetObj(so, "resetButton", panel.transform.Find("Tabs/ResetTab").GetComponent<Button>());
            SetObj(so, "tokensContent", panel.transform.Find("Body/TokensContent").gameObject);
            SetObj(so, "chestsContent", panel.transform.Find("Body/ChestsContent").gameObject);
            SetObj(so, "tokenTabFrame", panel.transform.Find("Tabs/TokenTab").GetComponent<Image>());
            SetObj(so, "chestTabFrame", panel.transform.Find("Tabs/ChestTab").GetComponent<Image>());
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, RankingPrefabPath);
            Debug.Log("[Medias] Injected '" + InjectedRootName + "' (RP-style Farm Averages) into FarmingScene.prefab.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [MenuItem("Tools/Medias/Remove From FarmingScene Prefab")]
    public static void RemoveFromPrefab()
    {
        var root = PrefabUtility.LoadPrefabContents(RankingPrefabPath);
        try
        {
            var found = FindDeep(root.transform, InjectedRootName);
            if (found != null)
            {
                Object.DestroyImmediate(found.gameObject);
                PrefabUtility.SaveAsPrefabAsset(root, RankingPrefabPath);
                Debug.Log("[Medias] Removed '" + InjectedRootName + "' from FarmingScene.prefab.");
            }
            else Debug.Log("[Medias] Nothing to remove.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
    // ---------------------------------------------------------------- Panel

    static GameObject BuildPanel(Transform parent)
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        var framePanel = LoadSprite(FramePanelPath);
        var frameRow = LoadSprite(FrameRowPath);

        var panel = MakeUI("MediasPanel", parent);
        var pr = (RectTransform)panel.transform;
        pr.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        var rootImg = panel.AddComponent<Image>();
        rootImg.color = new Color(0f, 0f, 0f, 0f); // transparent raycast target so the whole panel is hoverable

        // ---- Background exactly like the RP: a TopBG title strip + a BottomBG body,
        // both Frame_Ranking_Pool sliced, 2px wider than the panel on each side.
        var topBg = MakeUI("TopBG", panel.transform);
        var topRt = (RectTransform)topBg.transform;
        topRt.anchorMin = new Vector2(0f, 1f);
        topRt.anchorMax = new Vector2(1f, 1f);
        topRt.pivot = new Vector2(0.5f, 1f);
        topRt.offsetMin = new Vector2(-2f, -35f);
        topRt.offsetMax = new Vector2(2f, 0f);
        var topImg = topBg.AddComponent<Image>();
        topImg.sprite = framePanel;
        topImg.type = Image.Type.Sliced;

        var botBg = MakeUI("BottomBG", panel.transform);
        var botRt = (RectTransform)botBg.transform;
        botRt.anchorMin = new Vector2(0f, 0f);
        botRt.anchorMax = new Vector2(1f, 1f);
        botRt.offsetMin = new Vector2(-2f, 0f);
        botRt.offsetMax = new Vector2(2f, -37f);
        var botImg = botBg.AddComponent<Image>();
        botImg.sprite = framePanel;
        botImg.type = Image.Type.Sliced;

        // ---- Title strip content: info icon + gold 12pt title (RP style)
        var header = MakeUI("Header", panel.transform);
        var hRt = (RectTransform)header.transform;
        hRt.anchorMin = new Vector2(0f, 1f);
        hRt.anchorMax = new Vector2(1f, 1f);
        hRt.pivot = new Vector2(0.5f, 1f);
        hRt.offsetMin = new Vector2(13f, -35f);
        hRt.offsetMax = new Vector2(-13f, 0f);
        var titleGo = MakeUI("Title", header.transform);
        Stretch(titleGo);
        AddText(titleGo, font, "Farm Averages", 12f, C_Title, TextAlignmentOptions.Center, true);

        // ---- Tabs: Tokens / Chests / Reset — 35x35 buttons w/ 22px icons like the RP token buttons.
        var tabs = MakeUI("Tabs", panel.transform);
        var tabsRt = (RectTransform)tabs.transform;
        tabsRt.anchorMin = new Vector2(0f, 1f);
        tabsRt.anchorMax = new Vector2(1f, 1f);
        tabsRt.pivot = new Vector2(0.5f, 1f);
        tabsRt.offsetMin = new Vector2(6f, -82f);
        tabsRt.offsetMax = new Vector2(-6f, -40f);
        var tabsH = tabs.AddComponent<HorizontalLayoutGroup>();
        tabsH.childControlWidth = false;
        tabsH.childControlHeight = false;
        tabsH.childForceExpandWidth = false;
        tabsH.childForceExpandHeight = false;
        tabsH.childAlignment = TextAnchor.MiddleCenter;
        tabsH.spacing = 8f;
        MakeTabButton(tabs.transform, "TokenTab", BtnGreenPath, BcoinIcon, 22f);
        MakeTabButton(tabs.transform, "ChestTab", BtnCyanPath, WoodIcon, 22f);
        MakeTabButton(tabs.transform, "ResetTab", BtnPurplePath, ResetIconPath, 18f);

        // ---- Body: time pill + tab contents + footer
        var body = MakeUI("Body", panel.transform);
        var bRt = (RectTransform)body.transform;
        bRt.anchorMin = new Vector2(0f, 0f);
        bRt.anchorMax = new Vector2(1f, 1f);
        bRt.offsetMin = new Vector2(7f, 12f);
        bRt.offsetMax = new Vector2(-7f, -84f);

        // Time row
        var timeRow = MakeUI("TimeRow", body.transform);
        var trRt = (RectTransform)timeRow.transform;
        trRt.anchorMin = new Vector2(0f, 1f);
        trRt.anchorMax = new Vector2(1f, 1f);
        trRt.pivot = new Vector2(0.5f, 1f);
        trRt.offsetMin = new Vector2(0f, -20f);
        trRt.offsetMax = new Vector2(0f, 0f);
        var timeLabel = MakeUI("Label", timeRow.transform);
        var tlRt = (RectTransform)timeLabel.transform;
        tlRt.anchorMin = new Vector2(0f, 0.5f);
        tlRt.anchorMax = new Vector2(0f, 0.5f);
        tlRt.pivot = new Vector2(0f, 0.5f);
        tlRt.sizeDelta = new Vector2(40f, 18f);
        tlRt.anchoredPosition = new Vector2(2f, 0f);
        AddText(timeLabel, font, "Time:", 10f, C_Title, TextAlignmentOptions.Left, true);
        var pill = MakeUI("TimePill", timeRow.transform);
        var pillImg = pill.AddComponent<Image>();
        pillImg.sprite = LoadSprite(BtnBlackPath);
        pillImg.type = Image.Type.Sliced;
        var pillRt = (RectTransform)pill.transform;
        pillRt.anchorMin = new Vector2(1f, 0.5f);
        pillRt.anchorMax = new Vector2(1f, 0.5f);
        pillRt.pivot = new Vector2(1f, 0.5f);
        pillRt.sizeDelta = new Vector2(80f, 18f);
        pillRt.anchoredPosition = new Vector2(-2f, 0f);
        var clock = MakeUI("Clock", pill.transform);
        var clockImg = clock.AddComponent<Image>();
        clockImg.sprite = LoadIcon(ClockIconPath);
        clockImg.preserveAspect = true;
        var clockRt = (RectTransform)clock.transform;
        clockRt.anchorMin = clockRt.anchorMax = new Vector2(0f, 0.5f);
        clockRt.pivot = new Vector2(0f, 0.5f);
        clockRt.sizeDelta = new Vector2(12f, 12f);
        clockRt.anchoredPosition = new Vector2(6f, 0f);
        var timeTxt = MakeUI("Value", pill.transform);
        Stretch(timeTxt);
        var timeTmp = AddText(timeTxt, font, "00:30:00", 9f, Color.white, TextAlignmentOptions.Center, true);
        timeTmp.margin = new Vector4(16f, 0f, 2f, 0f);

        // Tokens content (default tab)
        var tokens = MakeContent(body.transform, "TokensContent");
        AddBigRow(tokens.transform, font, frameRow, LoadIcon(BcoinIcon), "BCOIN", "15.51", "8.35/h", false);
        AddBigRow(tokens.transform, font, frameRow, LoadIcon(SenIcon), "SEN", "21.67", "11.6/h", false);

        // Chests content
        var chests = MakeContent(body.transform, "ChestsContent");
        AddBigRow(chests.transform, font, frameRow, LoadIcon(ChestStatsIcon), "Total Chests", "7,030", "3783/h", false);
        AddBigRow(chests.transform, font, frameRow, LoadIcon(DiamondIcon), "Diamond", "33", "18/h", false);
        AddBigRow(chests.transform, font, frameRow, LoadIcon(GoldIcon), "Gold", "59", "32/h", false);
        AddBigRow(chests.transform, font, frameRow, LoadIcon(SilverIcon), "Silver", "369", "199/h", false);
        AddBigRow(chests.transform, font, frameRow, LoadIcon(WoodIcon), "Wood", "1,001", "539/h", false);
        AddBigRow(chests.transform, font, frameRow, LoadIcon(BombIcon), "Bombs", "17,379", "9352/h", false);
        chests.SetActive(false);

        return panel;
    }

    static void MakeTabButton(Transform parent, string name, string framePath, string iconPath, float iconSize)
    {
        var go = MakeUI(name, parent);
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(35f, 35f); // exact RP token-button size
        var img = go.AddComponent<Image>();
        img.sprite = LoadSprite(framePath);
        img.type = Image.Type.Sliced;
        go.AddComponent<Button>();
        var le = go.AddComponent<LayoutElement>();
        le.minWidth = 35f; le.preferredWidth = 35f;
        le.minHeight = 35f; le.preferredHeight = 35f;
        var icon = MakeUI("Icon", go.transform);
        var iconImg = icon.AddComponent<Image>();
        iconImg.sprite = LoadIcon(iconPath);
        iconImg.preserveAspect = true;
        var iRt = (RectTransform)icon.transform;
        iRt.anchorMin = iRt.anchorMax = new Vector2(0.5f, 0.5f);
        iRt.pivot = new Vector2(0.5f, 0.5f);
        iRt.sizeDelta = new Vector2(iconSize, iconSize);
        iRt.anchoredPosition = Vector2.zero;
    }

    static GameObject MakeContent(Transform body, string name)
    {
        var go = MakeUI(name, body);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(0f, 4f);
        rt.offsetMax = new Vector2(0f, -24f);  // leave room for the time row
        var v = go.AddComponent<VerticalLayoutGroup>();
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        v.childAlignment = TextAnchor.UpperCenter;
        v.spacing = 4f;
        v.padding = new RectOffset(0, 0, 0, 0);
        return go;
    }

    // Row in RP proportions (Frame_Rarity_Pool, ~131 wide): icon 20 left, name 10pt top,
    // TOTAL / PER HOUR columns below (mini labels 6.5pt, values 9pt like RP amounts).
    static void AddBigRow(Transform parent, TMP_FontAsset font, Sprite frameRow, Sprite icon, string name, string total, string perHour, bool showMiniLabels)
    {
        var row = MakeUI("Row_" + name.Replace(" ", ""), parent);
        var le = row.AddComponent<LayoutElement>();
        le.minHeight = 41f;
        le.preferredHeight = 41f;
        var bg = row.AddComponent<Image>();
        bg.sprite = frameRow;
        bg.type = Image.Type.Sliced;

        var icoGo = MakeUI("Icon", row.transform);
        var ico = icoGo.AddComponent<Image>();
        ico.sprite = icon;
        ico.preserveAspect = true;
        ico.enabled = icon != null;
        var icoRt = (RectTransform)icoGo.transform;
        icoRt.anchorMin = icoRt.anchorMax = new Vector2(0f, 0.5f);
        icoRt.pivot = new Vector2(0f, 0.5f);
        icoRt.sizeDelta = new Vector2(20f, 20f);
        icoRt.anchoredPosition = new Vector2(11f, 0f); // small gap from the left border

        // Name on top, values below; text clears the icon on the left.
        float textX = 38f;

        var nameGo = MakeUI("Name", row.transform);
        var nRt = (RectTransform)nameGo.transform;
        nRt.anchorMin = new Vector2(0f, 1f);
        nRt.anchorMax = new Vector2(1f, 1f);
        nRt.pivot = new Vector2(0.5f, 1f);
        nRt.offsetMin = new Vector2(textX, showMiniLabels ? -16f : -20f);
        nRt.offsetMax = new Vector2(-4f, showMiniLabels ? -3f : -6f);
        AddText(nameGo, font, name, 10f, C_Name, TextAlignmentOptions.Left, true);

        if (showMiniLabels)
        {
            var lt = MakeUI("LabelTotal", row.transform);
            var ltRt = (RectTransform)lt.transform;
            ltRt.anchorMin = new Vector2(0f, 1f); ltRt.anchorMax = new Vector2(0.50f, 1f);
            ltRt.pivot = new Vector2(0.5f, 1f);
            ltRt.offsetMin = new Vector2(30f, -25f); ltRt.offsetMax = new Vector2(0f, -16f);
            AddText(lt, font, "TOTAL", 6.5f, C_MiniLabel, TextAlignmentOptions.Left, false).outlineWidth = 0f;
            var lh = MakeUI("LabelPerHour", row.transform);
            var lhRt = (RectTransform)lh.transform;
            lhRt.anchorMin = new Vector2(0.56f, 1f); lhRt.anchorMax = new Vector2(1f, 1f);
            lhRt.pivot = new Vector2(0.5f, 1f);
            lhRt.offsetMin = new Vector2(0f, -25f); lhRt.offsetMax = new Vector2(-4f, -16f);
            AddText(lh, font, "PER HOUR", 6.5f, C_MiniLabel, TextAlignmentOptions.Left, false).outlineWidth = 0f;
        }

        float valTop = showMiniLabels ? 15f : 19f;
        float valBot = showMiniLabels ? 3f : 7f;

        var tv = MakeUI("TotalValue", row.transform);
        var tvRt = (RectTransform)tv.transform;
        tvRt.anchorMin = new Vector2(0f, 0f); tvRt.anchorMax = new Vector2(0.50f, 0f);
        tvRt.pivot = new Vector2(0.5f, 0f);
        tvRt.offsetMin = new Vector2(textX, valBot); tvRt.offsetMax = new Vector2(0f, valTop);
        AddText(tv, font, total, 8.5f, C_Value, TextAlignmentOptions.Left, true).outlineWidth = 0f;

        var hv = MakeUI("PerHourValue", row.transform);
        var hvRt = (RectTransform)hv.transform;
        hvRt.anchorMin = new Vector2(0.48f, 0f); hvRt.anchorMax = new Vector2(1f, 0f);
        hvRt.pivot = new Vector2(0.5f, 0f);
        hvRt.offsetMin = new Vector2(0f, valBot); hvRt.offsetMax = new Vector2(-13f, valTop);
        AddText(hv, font, perHour, 8.5f, C_Value, TextAlignmentOptions.Right, true).outlineWidth = 0f;
    }

    // Static tab state for the preview (no runtime controller in edit mode).
    static void SetTabStatic(Transform panel, bool tokens)
    {
        var tok = panel.Find("Body/TokensContent");
        var che = panel.Find("Body/ChestsContent");
        if (tok != null) tok.gameObject.SetActive(tokens);
        if (che != null) che.gameObject.SetActive(!tokens);
        var tokenTab = panel.Find("Tabs/TokenTab");
        var chestTab = panel.Find("Tabs/ChestTab");
        var dim = new Color(0.55f, 0.55f, 0.62f, 1f);
        if (tokenTab != null) tokenTab.GetComponent<Image>().color = tokens ? Color.white : dim;
        if (chestTab != null) chestTab.GetComponent<Image>().color = tokens ? dim : Color.white;
    }

    // ---------------------------------------------------------------- helpers

    static Sprite LoadSprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

    // Non-destructive icon load: imported Sprite if present, else wrap the Texture2D.
    static Sprite LoadIcon(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null) return s;
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex != null)
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        return null;
    }

    static void DestroyByName(string name)
    {
        GameObject go;
        while ((go = GameObject.Find(name)) != null)
            Object.DestroyImmediate(go);
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform c in root)
        {
            var r = FindDeep(c, name);
            if (r != null) return r;
        }
        return null;
    }

    static void SetObj(SerializedObject so, string prop, Object value)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.objectReferenceValue = value;
    }

    static void SetFloat(SerializedObject so, string prop, float value)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.floatValue = value;
    }

    static GameObject MakeUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(GameObject go)
    {
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static TextMeshProUGUI AddText(GameObject go, TMP_FontAsset font, string text, float size, Color color, TextAlignmentOptions align, bool bold)
    {
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.enableWordWrapping = false;
        t.overflowMode = TextOverflowModes.Overflow;
        if (bold) t.fontStyle = FontStyles.Bold;
        t.outlineColor = Color.black;
        t.outlineWidth = 0.25f;
        return t;
    }
}
#endif
