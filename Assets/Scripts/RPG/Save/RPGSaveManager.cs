using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DefaultExecutionOrder(-200)]
public class RPGSaveManager : MonoBehaviour
{
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool autoSaveAfterCombat = true;
    [SerializeField] private bool autoSaveOnQuit = true;

    private RPGGameManager gameManager;
    private TurnBasedCombatManager combatManager;
    private RPGSaveData pendingLoad;
    private bool subscribed;

    public static RPGSaveManager Instance { get; private set; }

    public bool HasSaveFile => RPGSaveSystem.HasSave();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (loadOnStart)
            TryQueueLoad();
    }

    private void Start()
    {
        CacheReferences();
        SubscribeCombatEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeCombatEvents();

        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (gameManager == null || gameManager.IsInCombat)
            return;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.f5Key.wasPressedThisFrame)
            SaveGame(showFeedback: true);

        if (keyboard.f9Key.wasPressedThisFrame)
            LoadGame(showFeedback: true);
#else
        if (Input.GetKeyDown(KeyCode.F5))
            SaveGame(showFeedback: true);

        if (Input.GetKeyDown(KeyCode.F9))
            LoadGame(showFeedback: true);
#endif
    }

    private void OnApplicationQuit()
    {
        if (autoSaveOnQuit)
            SaveGame(showFeedback: false);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && autoSaveOnQuit)
            SaveGame(showFeedback: false);
    }

    public void TryQueueLoad()
    {
        if (RPGSaveSystem.TryLoad(out RPGSaveData data, out _))
            pendingLoad = data;
    }

    public bool ApplyPendingLoad(RPGGameManager manager)
    {
        if (manager == null || pendingLoad == null)
            return false;

        manager.ApplySaveData(pendingLoad, showFeedback: false);
        pendingLoad = null;
        return true;
    }

    public bool SaveGame(bool showFeedback)
    {
        CacheReferences();

        if (gameManager == null)
        {
            if (showFeedback)
                ShowFeedback("Save failed — game manager not found.");

            return false;
        }

        RPGSaveData data = gameManager.CreateSaveData();
        bool success = RPGSaveSystem.TrySave(data, out string error);

        if (showFeedback)
        {
            ShowFeedback(success
                ? "Your adventure has been tucked away safely."
                : $"Could not save: {error}");
        }

        return success;
    }

    public bool LoadGame(bool showFeedback)
    {
        CacheReferences();

        if (!RPGSaveSystem.TryLoad(out RPGSaveData data, out string error))
        {
            if (showFeedback)
                ShowFeedback(string.IsNullOrEmpty(error) ? "No saved journey found yet." : $"Could not load: {error}");

            return false;
        }

        if (gameManager == null)
        {
            if (showFeedback)
                ShowFeedback("Load failed — game manager not found.");

            return false;
        }

        if (gameManager.IsInCombat)
        {
            if (showFeedback)
                ShowFeedback("Can't load a save in the middle of a duel.");

            return false;
        }

        gameManager.ApplySaveData(data, showFeedback);

        if (showFeedback)
            ShowFeedback("Your journey continues from the saved meadow.");

        return true;
    }

    public bool DeleteSave(bool showFeedback)
    {
        bool success = RPGSaveSystem.TryDeleteSave(out string error);

        if (showFeedback)
        {
            ShowFeedback(success
                ? "Save cleared. A fresh adventure awaits."
                : $"Could not delete save: {error}");
        }

        return success;
    }

    private void CacheReferences()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<RPGGameManager>();

        if (combatManager == null)
            combatManager = FindFirstObjectByType<TurnBasedCombatManager>();
    }

    private void SubscribeCombatEvents()
    {
        if (subscribed || combatManager == null || !autoSaveAfterCombat)
            return;

        combatManager.OnCombatEnded += HandleCombatEnded;
        subscribed = true;
    }

    private void UnsubscribeCombatEvents()
    {
        if (!subscribed || combatManager == null)
            return;

        combatManager.OnCombatEnded -= HandleCombatEnded;
        subscribed = false;
    }

    private void HandleCombatEnded(CombatStats _, int __, int ___)
    {
        if (!autoSaveAfterCombat)
            return;

        SaveGame(showFeedback: false);
    }

    private void ShowFeedback(string message)
    {
        CacheReferences();
        gameManager?.ShowHint(message);
    }
}
