using System;
using System.Collections;
using UnityEngine;

public enum CombatPhase
{
    Intro,
    PlayerTurn,
    EnemyTurn,
    Resolving,
    Victory,
    Defeat,
    Fled
}

public enum PlayerCombatAction
{
    Attack,
    Defend,
    Flee
}

public class TurnBasedCombatManager : MonoBehaviour
{
    [SerializeField] private EnemyDefinition[] enemyPool =
    {
        new EnemyDefinition { enemyName = "Bubble Slime", maxHp = 12, attack = 5, defense = 2, speed = 4, experienceReward = 8, goldReward = 4, elementType = ElementType.Water, tint = new Color(0.55f, 0.92f, 0.72f) },
        new EnemyDefinition { enemyName = "Flutterbat", maxHp = 10, attack = 7, defense = 1, speed = 9, experienceReward = 10, goldReward = 6, elementType = ElementType.Wind, tint = new Color(0.72f, 0.68f, 0.88f) },
        new EnemyDefinition { enemyName = "Moss Goblin", maxHp = 18, attack = 8, defense = 3, speed = 6, experienceReward = 15, goldReward = 10, elementType = ElementType.Earth, tint = new Color(0.52f, 0.78f, 0.52f) },
        new EnemyDefinition { enemyName = "Meadow Wolf", maxHp = 22, attack = 10, defense = 4, speed = 8, experienceReward = 20, goldReward = 12, elementType = ElementType.Fire, tint = new Color(0.78f, 0.76f, 0.82f) },
        new EnemyDefinition { enemyName = "Rattlebones", maxHp = 16, attack = 9, defense = 5, speed = 5, experienceReward = 18, goldReward = 14, elementType = ElementType.Lightning, tint = new Color(0.92f, 0.90f, 0.78f) }
    };

    [SerializeField] private float actionDelay = 0.85f;
    [SerializeField] [Range(0f, 1f)] private float fleeBaseChance = 0.55f;

    private CombatStats playerStats;
    private CombatStats enemyStats;
    private EnemyDefinition currentEnemyDef;
    private CombatPhase phase = CombatPhase.Intro;
    private bool playerDefending;
    private bool enemyDefending;
    private Coroutine combatRoutine;

    public CombatPhase Phase => phase;
    public CombatStats PlayerStats => playerStats;
    public CombatStats EnemyStats => enemyStats;
    public EnemyDefinition CurrentEnemyDef => currentEnemyDef;
    public MagicSkill[] PlayerSpells => MagicSkill.DefaultPlayerSpells;

    public event Action<string> OnCombatLog;
    public event Action<CombatPhase> OnPhaseChanged;
    public event Action<CombatStats, CombatStats, EnemyDefinition> OnCombatStarted;
    public event Action<CombatStats, int, int> OnCombatEnded;

    public void BeginCombat(CombatStats player)
    {
        if (combatRoutine != null)
            StopCoroutine(combatRoutine);

        playerStats = player.Clone();
        currentEnemyDef = enemyPool[UnityEngine.Random.Range(0, enemyPool.Length)];
        enemyStats = currentEnemyDef.ToCombatStats();
        playerDefending = false;
        enemyDefending = false;
        phase = CombatPhase.Intro;

        OnCombatStarted?.Invoke(playerStats, enemyStats, currentEnemyDef);

        string typeLabel = ElementalChart.GetDisplayName(currentEnemyDef.elementType);
        Log($"A cheerful {typeLabel} {enemyStats.displayName} bounces into the meadow!");

        combatRoutine = StartCoroutine(StartCombatRoutine());
    }

    public void SubmitPlayerAction(PlayerCombatAction action)
    {
        if (phase != CombatPhase.PlayerTurn)
            return;

        if (combatRoutine != null)
            StopCoroutine(combatRoutine);

        combatRoutine = StartCoroutine(ResolvePlayerAction(action));
    }

    public void SubmitMagic(ElementType element)
    {
        if (phase != CombatPhase.PlayerTurn)
            return;

        MagicSkill skill = MagicSkill.GetByElement(element);
        if (skill.element == ElementType.None)
            return;

        if (combatRoutine != null)
            StopCoroutine(combatRoutine);

        combatRoutine = StartCoroutine(ResolvePlayerMagic(skill));
    }

    public bool CanAffordMagic(ElementType element)
    {
        MagicSkill skill = MagicSkill.GetByElement(element);
        return skill.element != ElementType.None && playerStats.currentMp >= skill.mpCost;
    }

    public int GetMagicMpCost(ElementType element)
    {
        return MagicSkill.GetByElement(element).mpCost;
    }

    private IEnumerator StartCombatRoutine()
    {
        SetPhase(CombatPhase.Intro);
        yield return new WaitForSeconds(actionDelay);

        if (playerStats.speed >= enemyStats.speed)
        {
            Log("You got the first turn!");
            SetPhase(CombatPhase.PlayerTurn);
        }
        else
        {
            Log($"{enemyStats.displayName} is quicker on their feet!");
            SetPhase(CombatPhase.EnemyTurn);
            combatRoutine = StartCoroutine(ResolveEnemyTurn());
        }
    }

    private IEnumerator ResolvePlayerAction(PlayerCombatAction action)
    {
        SetPhase(CombatPhase.Resolving);
        playerDefending = false;

        switch (action)
        {
            case PlayerCombatAction.Attack:
                yield return ResolvePlayerAttack(bonus: 0, label: "You land a tidy hit", element: ElementType.None);
                break;

            case PlayerCombatAction.Defend:
                playerDefending = true;
                Log("You raise your guard, ready for anything.");
                yield return new WaitForSeconds(actionDelay);
                break;

            case PlayerCombatAction.Flee:
                float fleeRoll = fleeBaseChance + (playerStats.speed - enemyStats.speed) * 0.03f;
                if (UnityEngine.Random.value <= fleeRoll)
                {
                    Log("You slip away through the tall grass!");
                    EndCombat(fled: true);
                    yield break;
                }

                Log("The path is blocked — no escape this turn.");
                yield return new WaitForSeconds(actionDelay);
                break;
        }

        if (ShouldEndPlayerActionEarly())
            yield break;

        SetPhase(CombatPhase.EnemyTurn);
        combatRoutine = StartCoroutine(ResolveEnemyTurn());
    }

    private IEnumerator ResolvePlayerMagic(MagicSkill skill)
    {
        SetPhase(CombatPhase.Resolving);
        playerDefending = false;

        if (!playerStats.SpendMp(skill.mpCost))
        {
            Log("Your mana feels a little low...");
            SetPhase(CombatPhase.PlayerTurn);
            yield break;
        }

        yield return ResolvePlayerAttack(
            bonus: skill.basePower + playerStats.level / 2,
            label: skill.skillName,
            element: skill.element,
            isMagic: true);

        if (ShouldEndPlayerActionEarly())
            yield break;

        SetPhase(CombatPhase.EnemyTurn);
        combatRoutine = StartCoroutine(ResolveEnemyTurn());
    }

    private bool ShouldEndPlayerActionEarly()
    {
        if (phase == CombatPhase.Victory || phase == CombatPhase.Defeat || phase == CombatPhase.Fled)
            return true;

        if (!enemyStats.IsAlive)
        {
            HandleVictory();
            return true;
        }

        return false;
    }

    private IEnumerator ResolvePlayerAttack(int bonus, string label, ElementType element, bool isMagic = false)
    {
        int raw = isMagic
            ? RollMagicDamage(bonus)
            : playerStats.RollAttackDamage(bonus);

        float effectiveness = ElementalChart.GetEffectiveness(element, currentEnemyDef.elementType);
        int adjusted = Mathf.Max(1, Mathf.RoundToInt(raw * effectiveness));
        int damage = isMagic
            ? ApplyMagicDefense(adjusted, enemyDefending, enemyStats)
            : enemyStats.ApplyDefense(adjusted, enemyDefending);

        enemyStats.TakeDamage(damage);
        enemyDefending = false;

        string typeNote = element != ElementType.None
            ? $" ({ElementalChart.GetDisplayName(element)})"
            : string.Empty;

        Log($"{label}{typeNote} for {damage} harm!");

        string effectivenessNote = ElementalChart.DescribeEffectiveness(effectiveness);
        if (!string.IsNullOrEmpty(effectivenessNote))
            Log(effectivenessNote);

        yield return new WaitForSeconds(actionDelay);
    }

    private IEnumerator ResolveEnemyTurn()
    {
        SetPhase(CombatPhase.EnemyTurn);
        enemyDefending = false;

        if (!enemyStats.IsAlive)
        {
            HandleVictory();
            yield break;
        }

        yield return new WaitForSeconds(actionDelay * 0.5f);

        int roll = UnityEngine.Random.Range(0, 100);
        if (roll < 15)
        {
            enemyDefending = true;
            Log($"{enemyStats.displayName} hops back cautiously.");
        }
        else if (roll < 35 && currentEnemyDef.elementType != ElementType.None)
        {
            yield return ResolveEnemyMagic();
        }
        else if (roll < 85)
        {
            int raw = enemyStats.RollAttackDamage();
            int damage = playerStats.ApplyDefense(raw, playerDefending);
            playerStats.TakeDamage(damage);
            Log($"{enemyStats.displayName} tags you for {damage} harm!");
        }
        else
        {
            int raw = enemyStats.RollAttackDamage(bonus: 3);
            int damage = playerStats.ApplyDefense(raw, playerDefending);
            playerStats.TakeDamage(damage);
            Log($"{enemyStats.displayName} tries a bold little rush for {damage} harm!");
        }

        playerDefending = false;
        yield return new WaitForSeconds(actionDelay);

        if (!playerStats.IsAlive)
        {
            Log("You tumble into the clover... don't give up!");
            SetPhase(CombatPhase.Defeat);
            OnCombatEnded?.Invoke(playerStats, 0, 0);
            yield break;
        }

        SetPhase(CombatPhase.PlayerTurn);
    }

    private IEnumerator ResolveEnemyMagic()
    {
        ElementType element = currentEnemyDef.elementType;
        string skillName = $"{ElementalChart.GetDisplayName(element)} Whirl";
        int raw = RollMagicDamage(currentEnemyDef.attack + 2);
        int damage = ApplyMagicDefense(raw, playerDefending, playerStats);
        playerStats.TakeDamage(damage);

        Log($"{enemyStats.displayName} casts {skillName} for {damage} harm!");
        yield return new WaitForSeconds(actionDelay);
    }

    private static int RollMagicDamage(int basePower, float variance = 0.15f)
    {
        float multiplier = 1f + UnityEngine.Random.Range(-variance, variance);
        return Mathf.Max(1, Mathf.RoundToInt(basePower * multiplier));
    }

    private static int ApplyMagicDefense(int incomingDamage, bool defending, CombatStats target)
    {
        int mitigation = defending ? target.defense : target.defense / 3;
        return Mathf.Max(1, incomingDamage - mitigation / 2);
    }

    private void HandleVictory()
    {
        int xp = currentEnemyDef.experienceReward + enemyStats.level * 2;
        int gold = currentEnemyDef.goldReward + UnityEngine.Random.Range(0, 4);
        playerStats.GainExperience(xp);
        playerStats.gold += gold;

        Log($"All done! +{xp} XP and +{gold} shiny coins.");
        SetPhase(CombatPhase.Victory);
        OnCombatEnded?.Invoke(playerStats, xp, gold);
    }

    private void EndCombat(bool fled)
    {
        SetPhase(fled ? CombatPhase.Fled : CombatPhase.Victory);
        OnCombatEnded?.Invoke(playerStats, 0, 0);
    }

    private void SetPhase(CombatPhase newPhase)
    {
        phase = newPhase;
        OnPhaseChanged?.Invoke(phase);
    }

    private void Log(string message)
    {
        OnCombatLog?.Invoke(message);
    }
}
