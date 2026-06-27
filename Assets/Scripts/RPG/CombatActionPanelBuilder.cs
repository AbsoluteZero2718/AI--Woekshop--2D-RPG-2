using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class CombatActionPanelBuilder
{
    public struct ActionPanelRefs
    {
        public Button attack;
        public Button defend;
        public Button flee;
        public Button fire;
        public Button water;
        public Button earth;
        public Button wind;
        public Button lightning;
    }

    public static ActionPanelRefs UpgradePanel(
        Transform actionPanel,
        Button existingAttack,
        Button existingDefend,
        Button existingFlee)
    {
        Transform content = actionPanel.Find("Inner") ?? actionPanel;

        ApplyActionPanelLayout(actionPanel);
        StyleActionPanelShell(actionPanel);
        RemoveLegacyButton(content, "PowerBtn");
        EnsureSectionLabels(content, RPGFantasyAmbience.ActionSection, RPGFantasyAmbience.MagicSection);

        Vector2[] basics = BuildRowAnchors(3, 0.58f, 0.94f, 0.02f, 0.50f);
        Vector2[] magic = BuildRowAnchors(5, 0.08f, 0.50f, 0.02f, 0.98f);

        ActionPanelRefs refs = default;
        refs.attack = LayoutExistingOrFind(content, existingAttack, "AttackBtn", basics[0], basics[1], "Attack");
        refs.defend = LayoutExistingOrFind(content, existingDefend, "DefendBtn", basics[2], basics[3], "Defend");
        refs.flee = LayoutExistingOrFind(content, existingFlee, "FleeBtn", basics[4], basics[5], "Flee");

        refs.fire = LayoutMagicButton(content, "FireBtn", ElementType.Fire, magic[0], magic[1]);
        refs.water = LayoutMagicButton(content, "WaterBtn", ElementType.Water, magic[2], magic[3]);
        refs.earth = LayoutMagicButton(content, "EarthBtn", ElementType.Earth, magic[4], magic[5]);
        refs.wind = LayoutMagicButton(content, "WindBtn", ElementType.Wind, magic[6], magic[7]);
        refs.lightning = LayoutMagicButton(content, "LightningBtn", ElementType.Lightning, magic[8], magic[9]);

        return refs;
    }

    public static void ApplyCombatPanelLayout(Transform combatPanel)
    {
        if (combatPanel == null)
            return;

        Transform logPanel = combatPanel.Find("LogPanel");
        if (logPanel != null)
            SetRectAnchors(logPanel, new Vector2(0.02f, 0.34f), new Vector2(0.98f, 0.54f));
    }

    public static TMP_Text EnsureEnemyTypeLabel(Transform enemyPanel, TMP_Text enemyNameText, TMP_Text existingTypeLabel)
    {
        if (existingTypeLabel != null)
        {
            StyleTypeBadge(existingTypeLabel);
            return existingTypeLabel;
        }

        if (enemyPanel == null)
            return null;

        var label = RPGUIBuilder.CreateText(enemyPanel, "EnemyType",
            new Vector2(0.05f, 0.70f), new Vector2(0.60f, 0.80f),
            Vector2.zero, Vector2.zero, RPGUITheme.FontLabel,
            TextAlignmentOptions.MidlineLeft, RPGUITheme.TextGold, FontStyles.Bold);
        StyleTypeBadge(label);
        return label;
    }

    public static void StyleTypeBadge(TMP_Text label)
    {
        if (label == null)
            return;

        label.fontSize = RPGUITheme.FontLabel;
        label.fontStyle = FontStyles.Bold;
        label.characterSpacing = 1f;
    }

    private static void ApplyActionPanelLayout(Transform actionPanel)
    {
        SetRectAnchors(actionPanel, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.32f));
    }

    private static void StyleActionPanelShell(Transform actionPanel)
    {
        var image = actionPanel.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = HealthBarUtility.GetWhiteSprite();
            image.color = RPGUITheme.PanelFill;
        }
    }

    private static void EnsureSectionLabels(Transform actionPanel, string basicLabel, string magicLabel)
    {
        EnsureLabel(actionPanel, "BasicActionsLabel", basicLabel,
            new Vector2(0.02f, 0.945f), new Vector2(0.50f, 1f));
        EnsureLabel(actionPanel, "MagicActionsLabel", magicLabel,
            new Vector2(0.02f, 0.515f), new Vector2(0.98f, 0.575f));
    }

    private static void EnsureLabel(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax)
    {
        Transform existing = parent.Find(name);
        TMP_Text label;
        if (existing != null)
            label = existing.GetComponent<TMP_Text>();
        else
            label = RPGUIBuilder.CreateSectionLabel(parent, text, anchorMin, anchorMax);

        if (label != null)
            label.text = text.ToUpperInvariant();
    }

    private static Vector2[] BuildRowAnchors(int count, float rowMinY, float rowMaxY, float left, float right)
    {
        float totalWidth = right - left;
        float gap = RPGUITheme.ButtonGap;
        float buttonWidth = (totalWidth - gap * (count - 1)) / count;

        var anchors = new Vector2[count * 2];
        float x = left;
        for (int i = 0; i < count; i++)
        {
            anchors[i * 2] = new Vector2(x, rowMinY);
            anchors[i * 2 + 1] = new Vector2(x + buttonWidth, rowMaxY);
            x += buttonWidth + gap;
        }

        return anchors;
    }

    private static void RemoveLegacyButton(Transform parent, string name)
    {
        Transform legacy = parent.Find(name);
        if (legacy == null)
            return;

        if (Application.isPlaying)
            Object.Destroy(legacy.gameObject);
        else
            Object.DestroyImmediate(legacy.gameObject);
    }

    private static Button LayoutExistingOrFind(
        Transform parent,
        Button existing,
        string fallbackName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        string label)
    {
        Button button = existing != null ? existing : FindButton(parent, fallbackName);
        if (button == null)
            button = CreateBasicButton(parent, fallbackName, anchorMin, anchorMax, label);
        else
        {
            SetRectAnchors(button.transform, anchorMin, anchorMax);
            StyleBasicButton(button, label);
        }

        return button;
    }

    private static Button LayoutMagicButton(
        Transform parent,
        string name,
        ElementType element,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        Button button = FindButton(parent, name);
        if (button == null)
            button = CreateMagicButton(parent, name, anchorMin, anchorMax, element);
        else
            SetRectAnchors(button.transform, anchorMin, anchorMax);

        StyleMagicButton(button, element);
        return button;
    }

    private static Button FindButton(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        return child != null ? child.GetComponent<Button>() : null;
    }

    private static void SetRectAnchors(Transform target, Vector2 anchorMin, Vector2 anchorMax)
    {
        RPGUIBuilder.SetAnchors(target, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
    }

    private static Button CreateBasicButton(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        string label)
    {
        var go = CreateButtonShell(parent, name, anchorMin, anchorMax, RPGUITheme.ButtonNormal);
        var btn = go.AddComponent<Button>();
        RPGUIBuilder.StyleButton(btn, RPGUITheme.ButtonNormal, RPGUITheme.ButtonHighlight, RPGUITheme.ButtonPressed, RPGUITheme.ButtonBorder);

        var text = RPGUIBuilder.CreateText(go.transform, "Label", Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, RPGUITheme.FontButton, TextAlignmentOptions.Center, RPGUITheme.TextPrimary);
        text.text = label;
        RPGUIBuilder.StyleButtonLabel(text, RPGUITheme.FontButton, RPGUITheme.TextPrimary);
        return btn;
    }

    private static void StyleBasicButton(Button button, string label)
    {
        RPGUIBuilder.StyleButton(button, RPGUITheme.ButtonNormal, RPGUITheme.ButtonHighlight, RPGUITheme.ButtonPressed, RPGUITheme.ButtonBorder);
        var text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = label;
            RPGUIBuilder.StyleButtonLabel(text, RPGUITheme.FontButton, RPGUITheme.TextPrimary);
        }
    }

    private static Button CreateMagicButton(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        ElementType element)
    {
        MagicSkill skill = MagicSkill.GetByElement(element);
        Color accent = ElementalChart.GetColor(element);
        Color buttonFill = Color.Lerp(RPGUITheme.ButtonNormal, accent, 0.35f);

        var go = CreateButtonShell(parent, name, anchorMin, anchorMax, buttonFill);
        var btn = go.AddComponent<Button>();
        RPGUIBuilder.StyleButton(btn, buttonFill, Color.Lerp(buttonFill, Color.white, 0.15f), Color.Lerp(buttonFill, Color.black, 0.12f), accent);

        var text = RPGUIBuilder.CreateText(go.transform, "Label", Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, RPGUITheme.FontMagic, TextAlignmentOptions.Center, RPGUITheme.TextPrimary);
        text.text = $"{skill.skillName}\n<size=10><color=#{ColorUtility.ToHtmlStringRGB(RPGUITheme.TextGold)}>{skill.mpCost} MP</color></size>";
        RPGUIBuilder.StyleButtonLabel(text, RPGUITheme.FontMagic, RPGUITheme.TextPrimary);
        return btn;
    }

    private static void StyleMagicButton(Button button, ElementType element)
    {
        if (button == null)
            return;

        MagicSkill skill = MagicSkill.GetByElement(element);
        Color accent = ElementalChart.GetColor(element);
        Color buttonFill = Color.Lerp(RPGUITheme.ButtonNormal, accent, 0.35f);

        var image = button.GetComponent<Image>();
        if (image != null)
            image.color = buttonFill;

        RPGUIBuilder.StyleButton(button, buttonFill, Color.Lerp(buttonFill, Color.white, 0.15f), Color.Lerp(buttonFill, Color.black, 0.12f), accent);

        var label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.richText = true;
            label.text = $"{skill.skillName}\n<size=10><color=#{ColorUtility.ToHtmlStringRGB(RPGUITheme.TextGold)}>{skill.mpCost} MP</color></size>";
            RPGUIBuilder.StyleButtonLabel(label, RPGUITheme.FontMagic, RPGUITheme.TextPrimary);
        }
    }

    private static GameObject CreateButtonShell(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        SetRectAnchors(go.transform, anchorMin, anchorMax);

        var img = go.AddComponent<Image>();
        img.sprite = HealthBarUtility.GetWhiteSprite();
        img.type = Image.Type.Simple;
        img.color = color;
        return go;
    }
}
