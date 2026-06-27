using UnityEngine;
using UnityEngine.UI;

public static class HealthBarUtility
{
    private static Sprite whiteSprite;

    public static Sprite GetWhiteSprite()
    {
        if (whiteSprite != null)
            return whiteSprite;

        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();

        whiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0f, 0.5f), 4f);
        return whiteSprite;
    }

    public static Image CreateBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color fillColor, Color trackColor)
    {
        var bgGo = new GameObject(name + "_BG", typeof(RectTransform));
        bgGo.transform.SetParent(parent, false);

        var bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = anchorMin;
        bgRt.anchorMax = anchorMax;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        var bgImage = bgGo.AddComponent<Image>();
        bgImage.sprite = GetWhiteSprite();
        bgImage.type = Image.Type.Simple;
        bgImage.color = trackColor;

        var fillGo = new GameObject("Fill", typeof(RectTransform));
        fillGo.transform.SetParent(bgGo.transform, false);

        var fill = fillGo.AddComponent<Image>();
        fill.sprite = GetWhiteSprite();
        fill.type = Image.Type.Simple;
        fill.color = fillColor;

        SetFill(fill, 1f);
        ApplyFillPadding(fill);
        return fill;
    }

    public static Image CreateBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color fillColor)
    {
        return CreateBar(parent, name, anchorMin, anchorMax, fillColor, RPGUITheme.HpTrack);
    }

    public static void StyleBar(Image fill, Color fillColor, Color trackColor)
    {
        if (fill == null)
            return;

        fill.color = fillColor;
        fill.sprite = GetWhiteSprite();
        ApplyFillPadding(fill);

        var track = fill.transform.parent != null ? fill.transform.parent.GetComponent<Image>() : null;
        if (track != null)
        {
            track.sprite = GetWhiteSprite();
            track.color = trackColor;
        }
    }

    public static void SetFill(Image fillImage, float percent)
    {
        if (fillImage == null)
            return;

        if (fillImage.sprite == null)
            fillImage.sprite = GetWhiteSprite();

        percent = Mathf.Clamp01(percent);
        var rt = fillImage.rectTransform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(percent, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        ApplyFillPadding(fillImage);
    }

    private static void ApplyFillPadding(Image fill)
    {
        var rt = fill.rectTransform;
        rt.offsetMin = new Vector2(3f, 3f);
        rt.offsetMax = new Vector2(-2f, -3f);
    }
}
