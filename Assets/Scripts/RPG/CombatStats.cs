using System;
using UnityEngine;

[Serializable]
public class CombatStats
{
    public string displayName = "Hero";
    public int level = 1;
    public int maxHp = 30;
    public int currentHp = 30;
    public int maxMp = 10;
    public int currentMp = 10;
    public int attack = 8;
    public int defense = 4;
    public int speed = 6;
    public int experience = 0;
    public int experienceToNext = 25;
    public int gold = 0;

    public bool IsAlive => currentHp > 0;
    public float HpPercent => maxHp > 0 ? (float)currentHp / maxHp : 0f;
    public float MpPercent => maxMp > 0 ? (float)currentMp / maxMp : 0f;

    public CombatStats Clone()
    {
        return (CombatStats)MemberwiseClone();
    }

    public void HealToFull()
    {
        currentHp = maxHp;
        currentMp = maxMp;
    }

    public void TakeDamage(int amount)
    {
        currentHp = Mathf.Max(0, currentHp - amount);
    }

    public void RestoreMp(int amount)
    {
        currentMp = Mathf.Min(maxMp, currentMp + amount);
    }

    public bool SpendMp(int amount)
    {
        if (currentMp < amount)
            return false;

        currentMp -= amount;
        return true;
    }

    public int RollAttackDamage(int bonus = 0, float variance = 0.2f)
    {
        int baseDamage = attack + bonus;
        float multiplier = 1f + UnityEngine.Random.Range(-variance, variance);
        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));
    }

    public int ApplyDefense(int incomingDamage, bool defending = false)
    {
        int mitigation = defending ? defense * 2 : defense;
        return Mathf.Max(1, incomingDamage - mitigation / 2);
    }

    public void GainExperience(int amount, Action<int> onLevelUp = null)
    {
        experience += amount;

        while (experience >= experienceToNext)
        {
            experience -= experienceToNext;
            LevelUp();
            onLevelUp?.Invoke(level);
        }
    }

    private void LevelUp()
    {
        level++;
        maxHp += 5;
        maxMp += 2;
        attack += 2;
        defense += 1;
        speed += 1;
        experienceToNext = Mathf.RoundToInt(experienceToNext * 1.35f);
        HealToFull();
    }
}
