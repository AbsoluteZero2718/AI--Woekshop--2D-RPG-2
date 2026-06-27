using System;
using UnityEngine;

[Serializable]
public class RPGSaveData
{
    public const int CurrentVersion = 1;

    public int saveVersion = CurrentVersion;
    public string savedAtUtc;
    public CombatStats playerStats = new CombatStats();
    public float playerX;
    public float playerY;
    public float playerZ;
    public float respawnX;
    public float respawnY;
    public float respawnZ;
}
