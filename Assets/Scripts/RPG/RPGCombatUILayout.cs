using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class RPGCombatUILayout
{
    public static void Build(Transform canvasRoot, CombatUIController ui)
    {
        var overworldHud = new GameObject("OverworldHUD", typeof(RectTransform));
        overworldHud.transform.SetParent(canvasRoot, false);
        RPGUIBuilder.SetAnchors(overworldHud.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var statsText = RPGUIBuilder.CreateText(overworldHud.transform, "StatsText",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(-420f, -52f), new Vector2(420f, -8f),
            RPGUITheme.FontHud, TextAlignmentOptions.Center, RPGUITheme.TextPrimary, FontStyles.Bold);

        var hintText = RPGUIBuilder.CreateText(overworldHud.transform, "HintText",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(-420f, 52f), new Vector2(420f, 8f),
            RPGUITheme.FontHint, TextAlignmentOptions.Center, RPGUITheme.TextSecondary, FontStyles.Italic);

        var combatPanel = RPGUIBuilder.CreateBorderedPanel(canvasRoot, "CombatPanel",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            RPGUITheme.Overlay, RPGUITheme.PanelBorder);
        combatPanel.SetActive(false);

        var playerPanel = RPGUIBuilder.CreateBorderedPanel(combatPanel.transform, "PlayerPanel",
            new Vector2(0.02f, 0.56f), new Vector2(0.48f, 0.98f),
            Vector2.zero, Vector2.zero, RPGUITheme.PanelFill, RPGUITheme.PlayerHeader);
        var enemyPanel = RPGUIBuilder.CreateBorderedPanel(combatPanel.transform, "EnemyPanel",
            new Vector2(0.52f, 0.56f), new Vector2(0.98f, 0.98f),
            Vector2.zero, Vector2.zero, RPGUITheme.PanelFill, RPGUITheme.EnemyHeader);

        Transform playerInner = playerPanel.transform.Find("Inner");
        Transform enemyInner = enemyPanel.transform.Find("Inner");

        var playerName = RPGUIBuilder.CreateText(playerInner, "PlayerName",
            new Vector2(0.06f, 0.82f), new Vector2(0.94f, 0.98f),
            Vector2.zero, Vector2.zero, RPGUITheme.FontHero, TextAlignmentOptions.MidlineLeft, RPGUITheme.TextPrimary, FontStyles.Bold);
        var playerHpText = RPGUIBuilder.CreateText(playerInner, "PlayerHP",
            new Vector2(0.06f, 0.64f), new Vector2(0.94f, 0.78f),
            Vector2.zero, Vector2.zero, RPGUITheme.FontStat, TextAlignmentOptions.MidlineLeft, RPGUITheme.TextSecondary);
        var playerMpText = RPGUIBuilder.CreateText(playerInner, "PlayerMP",
            new Vector2(0.06f, 0.50f), new Vector2(0.94f, 0.62f),
            Vector2.zero, Vector2.zero, RPGUITheme.FontStat, TextAlignmentOptions.MidlineLeft, RPGUITheme.TextSecondary);
        var playerHpFill = HealthBarUtility.CreateBar(playerInner, "PlayerHPFill",
            new Vector2(0.06f, 0.36f), new Vector2(0.94f, 0.46f), RPGUITheme.HpFill, RPGUITheme.HpTrack);
        var playerMpFill = HealthBarUtility.CreateBar(playerInner, "PlayerMPFill",
            new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.32f), RPGUITheme.MpFill, RPGUITheme.MpTrack);

        var enemyName = RPGUIBuilder.CreateText(enemyInner, "EnemyName",
            new Vector2(0.06f, 0.82f), new Vector2(0.94f, 0.98f),
            Vector2.zero, Vector2.zero, RPGUITheme.FontHero, TextAlignmentOptions.MidlineLeft, RPGUITheme.TextPrimary, FontStyles.Bold);
        var enemyTypeText = RPGUIBuilder.CreateText(enemyInner, "EnemyType",
            new Vector2(0.06f, 0.70f), new Vector2(0.58f, 0.80f),
            Vector2.zero, Vector2.zero, RPGUITheme.FontLabel, TextAlignmentOptions.MidlineLeft, RPGUITheme.TextGold, FontStyles.Bold);
        var enemyHpText = RPGUIBuilder.CreateText(enemyInner, "EnemyHP",
            new Vector2(0.06f, 0.56f), new Vector2(0.58f, 0.68f),
            Vector2.zero, Vector2.zero, RPGUITheme.FontStat, TextAlignmentOptions.MidlineLeft, RPGUITheme.TextSecondary);
        var enemySprite = RPGUIBuilder.CreatePortraitFrame(enemyInner, "EnemyPortrait",
            new Vector2(0.62f, 0.12f), new Vector2(0.94f, 0.72f), new Color(0.5f, 0.8f, 0.5f));
        var enemyHpFill = HealthBarUtility.CreateBar(enemyInner, "EnemyHPFill",
            new Vector2(0.06f, 0.36f), new Vector2(0.58f, 0.46f), RPGUITheme.EnemyHpFill, RPGUITheme.HpTrack);

        var logPanel = RPGUIBuilder.CreateBorderedPanel(combatPanel.transform, "LogPanel",
            new Vector2(0.02f, 0.34f), new Vector2(0.98f, 0.54f),
            Vector2.zero, Vector2.zero, RPGUITheme.PanelInset, RPGUITheme.PanelBorder);
        var combatLog = RPGUIBuilder.CreateText(logPanel.transform.Find("Inner"), "CombatLog",
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            Vector2.zero, Vector2.zero, RPGUITheme.FontLog, TextAlignmentOptions.TopLeft, RPGUITheme.TextSecondary);
        RPGUIBuilder.StyleLogText(combatLog);

        var actionPanel = RPGUIBuilder.CreateBorderedPanel(combatPanel.transform, "ActionPanel",
            new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.32f),
            Vector2.zero, Vector2.zero, RPGUITheme.PanelFill, RPGUITheme.PanelBorder);
        CombatActionPanelBuilder.ActionPanelRefs actionRefs =
            CombatActionPanelBuilder.UpgradePanel(actionPanel.transform.Find("Inner"), null, null, null);

        ui.Configure(combatPanel, overworldHud,
            playerName, playerHpText, playerMpText, playerHpFill, playerMpFill,
            enemyName, enemyTypeText, enemyHpText, enemyHpFill, enemySprite,
            combatLog, statsText, hintText,
            actionRefs.attack, actionRefs.defend, actionRefs.flee,
            actionRefs.fire, actionRefs.water, actionRefs.earth, actionRefs.wind, actionRefs.lightning);

        RPGUIStyler.Apply(ui);
    }
}
