using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject combatPanel;
    [SerializeField] private GameObject overworldHud;

    [Header("Player")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerHpText;
    [SerializeField] private TMP_Text playerMpText;
    [SerializeField] private Image playerHpFill;
    [SerializeField] private Image playerMpFill;

    [Header("Enemy")]
    [SerializeField] private TMP_Text enemyNameText;
    [SerializeField] private TMP_Text enemyTypeText;
    [SerializeField] private TMP_Text enemyHpText;
    [SerializeField] private Image enemyHpFill;
    [SerializeField] private Image enemySprite;

    [Header("Combat Log")]
    [SerializeField] private TMP_Text combatLogText;

    [Header("Actions")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button defendButton;
    [SerializeField] private Button fleeButton;
    [SerializeField] private Button fireMagicButton;
    [SerializeField] private Button waterMagicButton;
    [SerializeField] private Button earthMagicButton;
    [SerializeField] private Button windMagicButton;
    [SerializeField] private Button lightningMagicButton;

    [Header("Overworld HUD")]
    [SerializeField] private TMP_Text overworldStatsText;
    [SerializeField] private TMP_Text hintText;

    private readonly StringBuilder logBuilder = new StringBuilder();
    private readonly Dictionary<ElementType, Button> magicButtons = new Dictionary<ElementType, Button>();
    private TurnBasedCombatManager combatManager;

    public void Configure(
        GameObject combat,
        GameObject overworld,
        TMP_Text playerName,
        TMP_Text playerHp,
        TMP_Text playerMp,
        Image playerHpBar,
        Image playerMpBar,
        TMP_Text enemyName,
        TMP_Text enemyType,
        TMP_Text enemyHp,
        Image enemyHpBar,
        Image enemyImage,
        TMP_Text log,
        TMP_Text overworldStats,
        TMP_Text hint,
        Button attack,
        Button defend,
        Button flee,
        Button fireMagic,
        Button waterMagic,
        Button earthMagic,
        Button windMagic,
        Button lightningMagic)
    {
        combatPanel = combat;
        overworldHud = overworld;
        playerNameText = playerName;
        playerHpText = playerHp;
        playerMpText = playerMp;
        playerHpFill = playerHpBar;
        playerMpFill = playerMpBar;
        enemyNameText = enemyName;
        enemyTypeText = enemyType;
        enemyHpText = enemyHp;
        enemyHpFill = enemyHpBar;
        enemySprite = enemyImage;
        combatLogText = log;
        overworldStatsText = overworldStats;
        hintText = hint;
        attackButton = attack;
        defendButton = defend;
        fleeButton = flee;
        fireMagicButton = fireMagic;
        waterMagicButton = waterMagic;
        earthMagicButton = earthMagic;
        windMagicButton = windMagic;
        lightningMagicButton = lightningMagic;

        CacheMagicButtons();
        WireButtons();
    }

    public void UpgradeCombatUI()
    {
        EnsureModernCombatUI();
        CacheMagicButtons();
        WireButtons();
        RPGUIStyler.Apply(this);
    }

    private void Awake()
    {
        combatManager = FindFirstObjectByType<TurnBasedCombatManager>();
        EnsureModernCombatUI();
        CacheMagicButtons();
        WireButtons();
    }

    private void EnsureModernCombatUI()
    {
        if (fireMagicButton != null && waterMagicButton != null)
            return;

        Transform actionPanel = GetActionPanelTransform();
        if (actionPanel == null)
        {
            Debug.LogWarning("CombatUIController: Could not find ActionPanel to upgrade combat UI.");
            return;
        }

        CombatActionPanelBuilder.ActionPanelRefs refs = CombatActionPanelBuilder.UpgradePanel(
            actionPanel,
            attackButton,
            defendButton,
            fleeButton);

        attackButton = refs.attack;
        defendButton = refs.defend;
        fleeButton = refs.flee;
        fireMagicButton = refs.fire;
        waterMagicButton = refs.water;
        earthMagicButton = refs.earth;
        windMagicButton = refs.wind;
        lightningMagicButton = refs.lightning;

        if (combatPanel != null)
            CombatActionPanelBuilder.ApplyCombatPanelLayout(combatPanel.transform);

        Transform enemyPanel = enemyNameText != null ? enemyNameText.transform.parent : null;
        enemyTypeText = CombatActionPanelBuilder.EnsureEnemyTypeLabel(enemyPanel, enemyNameText, enemyTypeText);
    }

    public GameObject CombatPanel => combatPanel;
    public GameObject OverworldHud => overworldHud;
    public TMP_Text OverworldStatsText => overworldStatsText;
    public TMP_Text HintText => hintText;
    public TMP_Text CombatLogText => combatLogText;
    public TMP_Text PlayerNameText => playerNameText;
    public TMP_Text EnemyNameText => enemyNameText;
    public TMP_Text EnemyTypeText => enemyTypeText;
    public TMP_Text PlayerHpText => playerHpText;
    public TMP_Text PlayerMpText => playerMpText;
    public TMP_Text EnemyHpText => enemyHpText;
    public Image PlayerHpFill => playerHpFill;
    public Image PlayerMpFill => playerMpFill;
    public Image EnemyHpFill => enemyHpFill;
    public Image EnemySprite => enemySprite;

    private Transform GetActionPanelTransform()
    {
        if (attackButton != null)
            return attackButton.transform.parent;

        if (defendButton != null)
            return defendButton.transform.parent;

        if (fleeButton != null)
            return fleeButton.transform.parent;

        if (combatPanel != null)
        {
        Transform panel = combatPanel.transform.Find("ActionPanel");
        if (panel != null)
        {
            Transform inner = panel.Find("Inner");
            return inner != null ? inner : panel;
        }
        }

        Transform nested = transform.Find("CombatPanel/ActionPanel");
        return nested;
    }

    private void CacheMagicButtons()
    {
        magicButtons.Clear();
        RegisterMagicButton(ElementType.Fire, fireMagicButton);
        RegisterMagicButton(ElementType.Water, waterMagicButton);
        RegisterMagicButton(ElementType.Earth, earthMagicButton);
        RegisterMagicButton(ElementType.Wind, windMagicButton);
        RegisterMagicButton(ElementType.Lightning, lightningMagicButton);
    }

    private void RegisterMagicButton(ElementType element, Button button)
    {
        if (button != null)
            magicButtons[element] = button;
    }

    private void WireButtons()
    {
        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(() => combatManager?.SubmitPlayerAction(PlayerCombatAction.Attack));
        }

        if (defendButton != null)
        {
            defendButton.onClick.RemoveAllListeners();
            defendButton.onClick.AddListener(() => combatManager?.SubmitPlayerAction(PlayerCombatAction.Defend));
        }

        if (fleeButton != null)
        {
            fleeButton.onClick.RemoveAllListeners();
            fleeButton.onClick.AddListener(() => combatManager?.SubmitPlayerAction(PlayerCombatAction.Flee));
        }

        foreach (KeyValuePair<ElementType, Button> entry in magicButtons)
        {
            ElementType element = entry.Key;
            Button button = entry.Value;
            MagicSkill skill = MagicSkill.GetByElement(element);

            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => combatManager?.SubmitMagic(element));

            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = $"{skill.skillName}\n({skill.mpCost} MP)";
        }
    }

    public void ShowOverworld()
    {
        if (combatPanel != null)
            combatPanel.SetActive(false);

        if (overworldHud != null)
            overworldHud.SetActive(true);
    }

    public void ShowCombat()
    {
        if (combatPanel != null)
            combatPanel.SetActive(true);

        if (overworldHud != null)
            overworldHud.SetActive(false);

        logBuilder.Clear();
        RefreshCombatLog();
    }

    public void BindCombatEvents(TurnBasedCombatManager manager)
    {
        combatManager = manager;
        manager.OnCombatLog += AppendLog;
        manager.OnPhaseChanged += HandlePhaseChanged;
        manager.OnCombatStarted += RefreshAll;
        WireButtons();
    }

    public void RefreshAll(CombatStats player, CombatStats enemy, EnemyDefinition enemyDef)
    {
        RefreshPlayer(player);
        RefreshEnemy(enemy, enemyDef);
        SetActionButtonsEnabled(false);
    }

    public void RefreshPlayer(CombatStats stats)
    {
        if (playerNameText != null)
        {
            playerNameText.richText = true;
            string muted = ColorUtility.ToHtmlStringRGB(RPGUITheme.TextMuted);
            playerNameText.text = $"<b>{stats.displayName}</b>  <size=16><color=#{muted}>Lv.{stats.level}</color></size>";
        }

        if (playerHpText != null)
        {
            playerHpText.richText = true;
            string label = ColorUtility.ToHtmlStringRGB(RPGUITheme.TextMuted);
            playerHpText.text = $"<color=#{label}>HP</color>  {stats.currentHp} / {stats.maxHp}";
        }

        if (playerMpText != null)
        {
            playerMpText.richText = true;
            string label = ColorUtility.ToHtmlStringRGB(RPGUITheme.TextMuted);
            playerMpText.text = $"<color=#{label}>MP</color>  {stats.currentMp} / {stats.maxMp}";
        }

        HealthBarUtility.SetFill(playerHpFill, stats.HpPercent);
        HealthBarUtility.SetFill(playerMpFill, stats.MpPercent);
    }

    public void RefreshEnemy(CombatStats stats, EnemyDefinition def)
    {
        if (enemyNameText != null)
            enemyNameText.text = stats.displayName;

        if (enemyTypeText != null && def != null)
        {
            Color typeColor = ElementalChart.GetColor(def.elementType);
            string typeName = ElementalChart.GetDisplayName(def.elementType);
            enemyTypeText.richText = true;
            enemyTypeText.text =
                $"<color=#{ColorUtility.ToHtmlStringRGB(typeColor)}>✿</color> {typeName} critter";
            enemyTypeText.color = RPGUITheme.TextGold;
        }

        if (enemyHpText != null)
        {
            enemyHpText.richText = true;
            string label = ColorUtility.ToHtmlStringRGB(RPGUITheme.TextMuted);
            enemyHpText.text = $"<color=#{label}>HP</color>  {stats.currentHp} / {stats.maxHp}";
        }

        HealthBarUtility.SetFill(enemyHpFill, stats.HpPercent);

        if (enemySprite != null && def != null)
            enemySprite.color = def.tint;
    }

    public void RefreshOverworldHud(CombatStats stats)
    {
        if (overworldStatsText != null)
        {
            overworldStatsText.text =
                $"Lv.{stats.level}  HP {stats.currentHp}/{stats.maxHp}  MP {stats.currentMp}/{stats.maxMp}  Coins {stats.gold}  XP {stats.experience}/{stats.experienceToNext}";
        }
    }

    public void SetHint(string message)
    {
        if (hintText != null)
            hintText.text = message;
    }

    private void AppendLog(string message)
    {
        if (logBuilder.Length > 0)
            logBuilder.AppendLine();

        logBuilder.Append(message);
        RefreshCombatLog();

        if (combatManager != null)
        {
            RefreshPlayer(combatManager.PlayerStats);
            RefreshEnemy(combatManager.EnemyStats, combatManager.CurrentEnemyDef);
        }
    }

    private void RefreshCombatLog()
    {
        if (combatLogText != null)
            combatLogText.text = logBuilder.ToString();
    }

    private void HandlePhaseChanged(CombatPhase phase)
    {
        bool playerTurn = phase == CombatPhase.PlayerTurn;
        SetActionButtonsEnabled(playerTurn);

        if (combatManager != null)
        {
            RefreshPlayer(combatManager.PlayerStats);
            RefreshEnemy(combatManager.EnemyStats, combatManager.CurrentEnemyDef);
            RefreshMagicButtonStates(playerTurn);
        }
    }

    private void RefreshMagicButtonStates(bool playerTurn)
    {
        foreach (KeyValuePair<ElementType, Button> entry in magicButtons)
        {
            if (entry.Value == null || combatManager == null)
                continue;

            bool canCast = playerTurn && combatManager.CanAffordMagic(entry.Key);
            entry.Value.interactable = canCast;
        }
    }

    private void SetActionButtonsEnabled(bool enabled)
    {
        if (attackButton != null)
            attackButton.interactable = enabled;

        if (defendButton != null)
            defendButton.interactable = enabled;

        if (fleeButton != null)
            fleeButton.interactable = enabled;

        RefreshMagicButtonStates(enabled);
    }
}
