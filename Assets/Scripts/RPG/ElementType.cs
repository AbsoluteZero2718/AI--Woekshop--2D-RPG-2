using UnityEngine;

public enum ElementType
{
    None,
    Fire,
    Water,
    Earth,
    Wind,
    Lightning
}

public static class ElementalChart
{
    public static float GetEffectiveness(ElementType attack, ElementType defender)
    {
        if (attack == ElementType.None || defender == ElementType.None)
            return 1f;

        if (Beats(attack, defender))
            return 1.5f;

        if (Beats(defender, attack))
            return 0.5f;

        return 1f;
    }

    public static bool Beats(ElementType attacker, ElementType defender)
    {
        return GetCounterTarget(attacker) == defender;
    }

    public static ElementType GetCounterTarget(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return ElementType.Wind;
            case ElementType.Wind: return ElementType.Earth;
            case ElementType.Earth: return ElementType.Lightning;
            case ElementType.Lightning: return ElementType.Water;
            case ElementType.Water: return ElementType.Fire;
            default: return ElementType.None;
        }
    }

    public static string GetDisplayName(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return "Ember";
            case ElementType.Water: return "Tide";
            case ElementType.Earth: return "Stone";
            case ElementType.Wind: return "Breeze";
            case ElementType.Lightning: return "Spark";
            default: return "Plain";
        }
    }

    public static Color GetColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return new Color(1f, 0.62f, 0.38f);
            case ElementType.Water: return new Color(0.48f, 0.78f, 0.98f);
            case ElementType.Earth: return new Color(0.72f, 0.58f, 0.40f);
            case ElementType.Wind: return new Color(0.62f, 0.92f, 0.78f);
            case ElementType.Lightning: return new Color(0.98f, 0.88f, 0.46f);
            default: return new Color(0.82f, 0.78f, 0.72f);
        }
    }

    public static string DescribeEffectiveness(float multiplier)
    {
        if (multiplier >= 1.45f)
            return "A perfect match — extra sparkle!";

        if (multiplier <= 0.55f)
            return "The spell fizzles a little...";

        return null;
    }
}
