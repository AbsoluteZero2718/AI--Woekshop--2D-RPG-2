using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class RPGUIStyler
{
    public static void Apply(CombatUIController ui)
    {
        if (ui == null)
            return;

        StyleOverworldHud(ui);
        StyleCombatPanel(ui);
        StyleCharacterPanels(ui);
        StyleBars(ui);
        StyleLog(ui);
        StylePortrait(ui);
    }

    private static void StyleOverworldHud(CombatUIController ui)
    {
        if (ui.OverworldHud == null)
            return;

        EnsureOverworldChrome(ui);
    }

    private static void EnsureOverworldChrome(CombatUIController ui)
    {
        Transform hud = ui.OverworldHud.transform;

        Transform statsFrame = hud.Find("StatsFrame");
        if (statsFrame == null && ui.OverworldStatsText != null)
        {
            var frame = RPGUIBuilder.CreateBorderedPanel(hud, "StatsFrame",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-420f, -52f), new Vector2(420f, -8f),
                RPGUITheme.OverworldBarFill, RPGUITheme.OverworldBarBorder);
            ui.OverworldStatsText.transform.SetParent(frame.transform.Find("Inner"), false);
            var rt = ui.OverworldStatsText.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(12f, 4f);
            rt.offsetMax = new Vector2(-12f, -4f);
        }

        Transform hintFrame = hud.Find("HintFrame");
        if (hintFrame == null && ui.HintText != null)
        {
            var frame = RPGUIBuilder.CreateBorderedPanel(hud, "HintFrame",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-420f, 8f), new Vector2(420f, 52f),
                RPGUITheme.OverworldBarFill, RPGUITheme.OverworldBarBorder);
            ui.HintText.transform.SetParent(frame.transform.Find("Inner"), false);
            var rt = ui.HintText.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(12f, 4f);
            rt.offsetMax = new Vector2(-12f, -4f);
        }

        if (ui.OverworldStatsText != null)
        {
            ui.OverworldStatsText.fontSize = RPGUITheme.FontHud;
            ui.OverworldStatsText.alignment = TextAlignmentOptions.Center;
            ui.OverworldStatsText.color = RPGUITheme.TextPrimary;
            ui.OverworldStatsText.fontStyle = FontStyles.Bold;
        }

        if (ui.HintText != null)
        {
            ui.HintText.fontSize = RPGUITheme.FontHint;
            ui.HintText.alignment = TextAlignmentOptions.Center;
            ui.HintText.color = RPGUITheme.TextSecondary;
            ui.HintText.fontStyle = FontStyles.Italic;
        }
    }

    private static void StyleCombatPanel(CombatUIController ui)
    {
        if (ui.CombatPanel == null)
            return;

        StyleRootImage(ui.CombatPanel, RPGUITheme.Overlay);

        Transform title = ui.CombatPanel.transform.Find("BattleTitle");
        if (title == null)
        {
            var titleText = RPGUIBuilder.CreateText(ui.CombatPanel.transform, "BattleTitle",
                new Vector2(0.5f, 0.965f), new Vector2(0.5f, 0.995f),
                new Vector2(-120f, 0f), new Vector2(120f, 0f),
                RPGUITheme.FontLabel, TextAlignmentOptions.Center, RPGUITheme.TextGold, FontStyles.Bold);
            titleText.text = RPGFantasyAmbience.BattleTitle;
            titleText.characterSpacing = 4f;
        }

        StylePanelChild(ui.CombatPanel.transform, "LogPanel", RPGUITheme.PanelInset, RPGUITheme.PanelBorder);
        StylePanelChild(ui.CombatPanel.transform, "ActionPanel", RPGUITheme.PanelFill, RPGUITheme.PanelBorder);
    }

    private static void StyleCharacterPanels(CombatUIController ui)
    {
        if (ui.CombatPanel == null)
            return;

        StylePanelChild(ui.CombatPanel.transform, "PlayerPanel", RPGUITheme.PanelFill, RPGUITheme.PlayerHeader);
        StylePanelChild(ui.CombatPanel.transform, "EnemyPanel", RPGUITheme.PanelFill, RPGUITheme.EnemyHeader);

        RPGUIBuilder.StyleTitleText(ui.PlayerNameText, RPGUITheme.PlayerHeader);
        RPGUIBuilder.StyleTitleText(ui.EnemyNameText, RPGUITheme.EnemyHeader);
        RPGUIBuilder.StyleStatText(ui.PlayerHpText);
        RPGUIBuilder.StyleStatText(ui.PlayerMpText);
        RPGUIBuilder.StyleStatText(ui.EnemyHpText);

        if (ui.EnemyTypeText != null)
        {
            ui.EnemyTypeText.fontSize = RPGUITheme.FontLabel;
            ui.EnemyTypeText.fontStyle = FontStyles.Bold;
        }
    }

    private static void StyleBars(CombatUIController ui)
    {
        HealthBarUtility.StyleBar(ui.PlayerHpFill, RPGUITheme.HpFill, RPGUITheme.HpTrack);
        HealthBarUtility.StyleBar(ui.PlayerMpFill, RPGUITheme.MpFill, RPGUITheme.MpTrack);
        HealthBarUtility.StyleBar(ui.EnemyHpFill, RPGUITheme.EnemyHpFill, RPGUITheme.HpTrack);
    }

    private static void StyleLog(CombatUIController ui)
    {
        RPGUIBuilder.StyleLogText(ui.CombatLogText);
    }

    private static void StylePortrait(CombatUIController ui)
    {
        if (ui.EnemySprite == null)
            return;

        var parent = ui.EnemySprite.transform.parent;
        if (parent != null && parent.name == "Inner")
            return;

        // Portrait already in a styled frame from fresh builds.
    }

    private static void StylePanelChild(Transform root, string panelName, Color fill, Color border)
    {
        Transform panel = root.Find(panelName);
        if (panel == null)
            return;

        StyleRootImage(panel.gameObject, fill);

        Transform inner = panel.Find("Inner");
        if (inner == null)
        {
            WrapWithBorder(panel.gameObject, fill, border);
            return;
        }

        StyleRootImage(inner.gameObject, fill);
        StyleRootImage(panel.gameObject, border);
    }

    private static void WrapWithBorder(GameObject panel, Color fill, Color border)
    {
        var image = panel.GetComponent<Image>();
        if (image == null)
            return;

        image.sprite = HealthBarUtility.GetWhiteSprite();
        image.color = border;

        var inner = new GameObject("Inner", typeof(RectTransform));
        inner.transform.SetParent(panel.transform, false);
        var rt = inner.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(2f, 2f);
        rt.offsetMax = new Vector2(-2f, -2f);

        var innerImage = inner.AddComponent<Image>();
        innerImage.sprite = HealthBarUtility.GetWhiteSprite();
        innerImage.color = fill;

        for (int i = panel.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = panel.transform.GetChild(i);
            if (child == inner.transform)
                continue;

            child.SetParent(inner.transform, true);
        }
    }

    private static void StyleRootImage(GameObject go, Color color)
    {
        var image = go.GetComponent<Image>();
        if (image == null)
            return;

        image.sprite = HealthBarUtility.GetWhiteSprite();
        image.type = Image.Type.Simple;
        image.color = color;
    }
}
