using System;
using UnityEngine;

[Serializable]
public class EnemyDefinition
{
    public string enemyName = "Slime";
    public int level = 1;
    public int maxHp = 12;
    public int attack = 5;
    public int defense = 2;
    public int speed = 4;
    public int experienceReward = 8;
    public int goldReward = 5;
    public ElementType elementType = ElementType.Water;
    public Color tint = new Color(0.4f, 0.85f, 0.45f);

    public CombatStats ToCombatStats()
    {
        return new CombatStats
        {
            displayName = enemyName,
            level = level,
            maxHp = maxHp,
            currentHp = maxHp,
            maxMp = 0,
            currentMp = 0,
            attack = attack,
            defense = defense,
            speed = speed
        };
    }
}
