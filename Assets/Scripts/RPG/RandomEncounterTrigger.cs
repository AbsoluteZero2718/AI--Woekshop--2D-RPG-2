using System;
using UnityEngine;

public class RandomEncounterTrigger : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] private float encounterChancePerUnit = 0.08f;
    [SerializeField] private float minDistanceBetweenEncounters = 3f;
    [SerializeField] private float encounterCooldown = 1.5f;

    private float distanceSinceLastEncounter;
    private float cooldownTimer;
    private bool encountersEnabled = true;

    public event Action OnEncounterTriggered;

    public bool EncountersEnabled
    {
        get => encountersEnabled;
        set => encountersEnabled = value;
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    public void RegisterMovement(float distanceMoved)
    {
        if (!encountersEnabled || cooldownTimer > 0f || distanceMoved <= 0f)
            return;

        distanceSinceLastEncounter += distanceMoved;

        if (distanceSinceLastEncounter < minDistanceBetweenEncounters)
            return;

        if (UnityEngine.Random.value <= encounterChancePerUnit * distanceMoved)
            TriggerEncounter();
    }

    public void ResetEncounterState()
    {
        distanceSinceLastEncounter = 0f;
        cooldownTimer = encounterCooldown;
    }

    private void TriggerEncounter()
    {
        distanceSinceLastEncounter = 0f;
        cooldownTimer = encounterCooldown;
        OnEncounterTriggered?.Invoke();
    }
}
