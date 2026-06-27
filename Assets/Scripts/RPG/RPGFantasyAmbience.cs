using TMPro;
using UnityEngine;

public static class RPGFantasyAmbience
{
    public static readonly Color Sky = new Color(0.70f, 0.86f, 0.96f);
    public static readonly Color GrassLight = new Color(0.64f, 0.84f, 0.54f);
    public static readonly Color GrassDark = new Color(0.54f, 0.76f, 0.46f);
    public static readonly Color TreeA = new Color(0.42f, 0.68f, 0.48f);
    public static readonly Color TreeB = new Color(0.48f, 0.74f, 0.56f);
    public static readonly Color TreeC = new Color(0.38f, 0.62f, 0.44f);
    public static readonly Color HeroTint = new Color(0.48f, 0.76f, 0.98f);
    public static readonly Color MeadowSign = new Color(0.42f, 0.52f, 0.38f);

    public const float GlobalLightIntensity = 1.12f;
    public const string MeadowSignText = "Sunlit Meadows — gentle adventures await!";
    public const string BattleTitle = "✦  Meadow Duel  ✦";
    public const string ActionSection = "Adventurer";
    public const string MagicSection = "Spark Spells";

    public static void ApplyToOverworld()
    {
        var root = GameObject.Find("Overworld");
        if (root == null)
            return;

        foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer.sortingOrder == -10)
            {
                Vector3 pos = renderer.transform.position;
                bool lightTile = (Mathf.RoundToInt(pos.x) + Mathf.RoundToInt(pos.y)) % 2 == 0;
                renderer.color = lightTile ? GrassLight : GrassDark;
            }
            else if (renderer.sortingOrder == -5)
            {
                renderer.color = TreeA;
            }
        }

        foreach (TMP_Text label in root.GetComponentsInChildren<TMP_Text>(true))
        {
            label.text = MeadowSignText;
            label.color = MeadowSign;
            label.fontStyle = FontStyles.Italic;
        }

        ApplyCameraAndLight();
    }

    public static void ApplyCameraAndLight()
    {
        if (Camera.main != null)
            Camera.main.backgroundColor = Sky;

        var light = Object.FindFirstObjectByType<UnityEngine.Rendering.Universal.Light2D>();
        if (light != null && light.lightType == UnityEngine.Rendering.Universal.Light2D.LightType.Global)
            light.intensity = GlobalLightIntensity;
    }
}
