using System.Collections;
using UnityEngine;

public class RPGGameManager : MonoBehaviour
{
    [SerializeField] private CombatStats playerStats = new CombatStats
    {
        displayName = "Wanderer",
        level = 1,
        maxHp = 35,
        currentHp = 35,
        maxMp = 20,
        currentMp = 20,
        attack = 9,
        defense = 5,
        speed = 7,
        experienceToNext = 25
    };

    [SerializeField] private Vector3 respawnPosition = Vector3.zero;
    [SerializeField] private float combatTransitionDelay = 0.6f;

    private OverworldPlayerController playerController;
    private RandomEncounterTrigger encounterTrigger;
    private TurnBasedCombatManager combatManager;
    private CombatUIController combatUI;
    private Transform playerTransform;
    private bool inCombat;

    public bool IsInCombat => inCombat;
    public CombatStats PlayerStats => playerStats;

    private bool initialized;
    private RPGSaveManager saveManager;

    private void Awake()
    {
        CacheReferences();
        saveManager = FindFirstObjectByType<RPGSaveManager>();
        if (saveManager == null)
            saveManager = gameObject.AddComponent<RPGSaveManager>();

        if (FindFirstObjectByType<OverworldAmbience>() == null)
            gameObject.AddComponent<OverworldAmbience>();
    }

    private void Start()
    {
        if (initialized)
            return;

        initialized = true;
        BindSystems();
        respawnPosition = playerTransform != null ? playerTransform.position : respawnPosition;

        saveManager ??= FindFirstObjectByType<RPGSaveManager>();
        saveManager?.ApplyPendingLoad(this);

        EnterOverworld();
    }

    private void CacheReferences()
    {
        if (combatManager == null)
            combatManager = FindFirstObjectByType<TurnBasedCombatManager>();

        if (combatUI == null)
            combatUI = FindFirstObjectByType<CombatUIController>();

        if (playerController == null)
            playerController = FindFirstObjectByType<OverworldPlayerController>();

        if (playerController != null && playerTransform == null)
        {
            playerTransform = playerController.transform;
            encounterTrigger = playerController.GetComponent<RandomEncounterTrigger>();
        }
    }

    private void BindSystems()
    {
        CacheReferences();

        if (encounterTrigger != null)
            encounterTrigger.OnEncounterTriggered += HandleRandomEncounter;

        if (combatManager != null && combatUI != null)
        {
            combatUI.BindCombatEvents(combatManager);
            combatManager.OnCombatEnded += HandleCombatEnded;
        }
    }

    private void OnDestroy()
    {
        if (encounterTrigger != null)
            encounterTrigger.OnEncounterTriggered -= HandleRandomEncounter;

        if (combatManager != null)
            combatManager.OnCombatEnded -= HandleCombatEnded;
    }

    private void HandleRandomEncounter()
    {
        if (inCombat)
            return;

        StartCoroutine(TransitionToCombat());
    }

    private IEnumerator TransitionToCombat()
    {
        inCombat = true;
        playerController?.SetMovementEnabled(false);
        encounterTrigger.EncountersEnabled = false;

        combatUI.SetHint("Something rustles nearby...");
        yield return new WaitForSeconds(combatTransitionDelay);

        combatUI.ShowCombat();
        combatManager.BeginCombat(playerStats);
    }

    private void HandleCombatEnded(CombatStats updatedPlayer, int xpGained, int goldGained)
    {
        playerStats = updatedPlayer.Clone();
        StartCoroutine(FinishCombatRoutine(combatManager.Phase));
    }

    private IEnumerator FinishCombatRoutine(CombatPhase result)
    {
        yield return new WaitForSeconds(1.2f);

        if (result == CombatPhase.Defeat)
        {
            playerStats.currentHp = Mathf.Max(1, playerStats.maxHp / 2);
            playerStats.currentMp = playerStats.maxMp / 2;

            if (playerTransform != null)
                playerTransform.position = respawnPosition;

            combatUI.SetHint("You wake beneath a friendly tree, feeling a little wiser.");
        }
        else if (result == CombatPhase.Victory)
        {
            combatUI.SetHint("A lovely victory! The meadow smiles upon you.");
        }
        else if (result == CombatPhase.Fled)
        {
            combatUI.SetHint("You tiptoe away. The meadow will keep its secrets for now.");
        }

        EnterOverworld();
    }

    private void EnterOverworld()
    {
        inCombat = false;
        combatUI.ShowOverworld();
        combatUI.RefreshOverworldHud(playerStats);
        combatUI.SetHint("Wander the sunlit meadows.  F5 to tuck progress away  ·  F9 to remember your journey.");

        playerController?.SetMovementEnabled(true);

        if (encounterTrigger != null)
        {
            encounterTrigger.EncountersEnabled = true;
            encounterTrigger.ResetEncounterState();
        }
    }

    public RPGSaveData CreateSaveData()
    {
        CacheReferences();

        Vector3 playerPosition = playerTransform != null ? playerTransform.position : respawnPosition;

        return new RPGSaveData
        {
            playerStats = playerStats.Clone(),
            playerX = playerPosition.x,
            playerY = playerPosition.y,
            playerZ = playerPosition.z,
            respawnX = respawnPosition.x,
            respawnY = respawnPosition.y,
            respawnZ = respawnPosition.z
        };
    }

    public void ApplySaveData(RPGSaveData data, bool showFeedback = false)
    {
        if (data?.playerStats == null)
            return;

        CacheReferences();

        playerStats = data.playerStats.Clone();
        respawnPosition = new Vector3(data.respawnX, data.respawnY, data.respawnZ);

        if (playerTransform != null)
            playerTransform.position = new Vector3(data.playerX, data.playerY, data.playerZ);

        if (!inCombat)
        {
            combatUI?.RefreshOverworldHud(playerStats);

            if (showFeedback)
                ShowHint("Your journey continues from where you left off.");
        }
    }

    public void ShowHint(string message)
    {
        combatUI?.SetHint(message);
    }
}
