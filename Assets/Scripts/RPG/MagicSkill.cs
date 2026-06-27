using System;
using UnityEngine;

[Serializable]
public struct MagicSkill
{
    public ElementType element;
    public string skillName;
    public int mpCost;
    public int basePower;

    public static MagicSkill[] DefaultPlayerSpells { get; } =
    {
        new MagicSkill { element = ElementType.Fire, skillName = "Ember Pop", mpCost = 5, basePower = 10 },
        new MagicSkill { element = ElementType.Water, skillName = "Gentle Splash", mpCost = 5, basePower = 10 },
        new MagicSkill { element = ElementType.Earth, skillName = "Pebble Toss", mpCost = 6, basePower = 12 },
        new MagicSkill { element = ElementType.Wind, skillName = "Breeze Waltz", mpCost = 5, basePower = 10 },
        new MagicSkill { element = ElementType.Lightning, skillName = "Sparkle Bolt", mpCost = 6, basePower = 12 }
    };

    public static MagicSkill GetByElement(ElementType element)
    {
        foreach (MagicSkill skill in DefaultPlayerSpells)
        {
            if (skill.element == element)
                return skill;
        }

        return default;
    }
}
