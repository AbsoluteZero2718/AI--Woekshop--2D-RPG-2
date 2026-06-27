using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class RPGUIBuilder
{
    public static GameObject CreateBorderedPanel(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color fill,
        Color border)
    {
        var shell = new GameObject(name, typeof(RectTransform));
        shell.transform.SetParent(parent, false);
        SetAnchors(shell.transform, anchorMin, anchorMax, offsetMin, offsetMax);

        var borderImage = shell.AddComponent<Image>();
        borderImage.sprite = HealthBarUtility.GetWhiteSprite();
        borderImage.type = Image.Type.Simple;
        borderImage.color = border;

        var inner = new GameObject("Inner", typeof(RectTransform));
        inner.transform.SetParent(shell.transform, false);
        var innerRt = inner.GetComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero;
        innerRt.anchorMax = Vector2.one;
        innerRt.offsetMin = new Vector2(2f, 2f);
        innerRt.offsetMax = new Vector2(-2f, -2f);

        var fillImage = inner.AddComponent<Image>();
        fillImage.sprite = HealthBarUtility.GetWhiteSprite();
        fillImage.type = Image.Type.Simple;
        fillImage.color = fill;

        return shell;
    }

    public static TMP_Text CreateText(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        float fontSize,
        TextAlignmentOptions alignment,
        Color color,
        FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        SetAnchors(go.transform, anchorMin, anchorMax, offsetMin, offsetMax);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.richText = true;
        return tmp;
    }

    public static TMP_Text CreateSectionLabel(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax)
    {
        var label = CreateText(parent, text + "Label", anchorMin, anchorMax,
            Vector2.zero, Vector2.zero, RPGUITheme.FontLabel, TextAlignmentOptions.MidlineLeft,
            RPGUITheme.TextMuted, FontStyles.Bold);
        label.text = text.ToUpperInvariant();
        label.characterSpacing = 2f;
        return label;
    }

    public static Image CreatePortraitFrame(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color tint)
    {
        var frame = CreateBorderedPanel(parent, name, anchorMin, anchorMax,
            Vector2.zero, Vector2.zero, new Color(0.12f, 0.14f, 0.20f, 1f), RPGUITheme.PanelBorder);

        var portrait = new GameObject("Portrait", typeof(RectTransform));
        portrait.transform.SetParent(frame.transform.Find("Inner"), false);
        var rt = portrait.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.08f, 0.08f);
        rt.anchorMax = new Vector2(0.92f, 0.92f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = portrait.AddComponent<Image>();
        img.sprite = HealthBarUtility.GetWhiteSprite();
        img.type = Image.Type.Simple;
        img.color = tint;
        return img;
    }

    public static void StyleButton(Button button, Color normal, Color highlight, Color pressed, Color border)
    {
        if (button == null)
            return;

        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f);
        colors.pressedColor = new Color(0.88f, 0.88f, 0.88f);
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.65f);
        button.colors = colors;

        var image = button.GetComponent<Image>();
        if (image != null)
            image.color = normal;

        EnsureButtonBorder(button.transform, border);
    }

    public static void StyleButtonLabel(TMP_Text label, float fontSize, Color color)
    {
        if (label == null)
            return;

        label.fontSize = fontSize;
        label.color = color;
        label.fontStyle = FontStyles.Bold;
        label.margin = new Vector4(4f, 2f, 4f, 2f);
    }

    public static void StyleLogText(TMP_Text log)
    {
        if (log == null)
            return;

        log.fontSize = RPGUITheme.FontLog;
        log.color = RPGUITheme.TextSecondary;
        log.lineSpacing = 2f;
        log.margin = new Vector4(14f, 10f, 14f, 10f);
        log.alignment = TextAlignmentOptions.TopLeft;
    }

    public static void StyleStatText(TMP_Text text, bool muted = false)
    {
        if (text == null)
            return;

        text.fontSize = RPGUITheme.FontStat;
        text.color = muted ? RPGUITheme.TextMuted : RPGUITheme.TextSecondary;
    }

    public static void StyleTitleText(TMP_Text text, Color accent)
    {
        if (text == null)
            return;

        text.fontSize = RPGUITheme.FontHero;
        text.color = RPGUITheme.TextPrimary;
        text.fontStyle = FontStyles.Bold;
    }

    private static void EnsureButtonBorder(Transform buttonTransform, Color borderColor)
    {
        Transform existing = buttonTransform.Find("Border");
        if (existing != null)
        {
            var img = existing.GetComponent<Image>();
            if (img != null)
                img.color = borderColor;
            return;
        }

        var borderGo = new GameObject("Border", typeof(RectTransform));
        borderGo.transform.SetParent(buttonTransform, false);
        borderGo.transform.SetAsFirstSibling();

        var rt = borderGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-1f, -1f);
        rt.offsetMax = new Vector2(1f, 1f);

        var borderImage = borderGo.AddComponent<Image>();
        borderImage.sprite = HealthBarUtility.GetWhiteSprite();
        borderImage.type = Image.Type.Simple;
        borderImage.color = borderColor;
        borderImage.raycastTarget = false;
    }

    public static void SetAnchors(
        Transform target,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        var rt = target as RectTransform ?? target.GetComponent<RectTransform>();
        if (rt == null)
            return;

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }
}
