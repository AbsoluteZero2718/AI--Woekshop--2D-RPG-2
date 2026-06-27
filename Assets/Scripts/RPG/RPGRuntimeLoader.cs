using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(-500)]
public class RPGRuntimeLoader : MonoBehaviour
{
    [SerializeField] private bool loadOnAwake = true;

    private static bool loaded;

    private void Awake()
    {
        if (!loadOnAwake || loaded || FindFirstObjectByType<RPGGameManager>() != null)
            return;

        loaded = true;
        BuildGame();
        Destroy(gameObject);
    }

    public static void BuildGame()
    {
        CreateLighting();
        var player = CreatePlayer();
        CreateCamera(player.transform);
        CreateOverworld();
        CreateUI();
        CreateManagers();
        EnsureEventSystem();
    }

    private static void CreateLighting()
    {
        if (FindFirstObjectByType<UnityEngine.Rendering.Universal.Light2D>() != null)
            return;

        var lightGo = new GameObject("Global Light 2D");
        var light = lightGo.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
        light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
        light.intensity = RPGFantasyAmbience.GlobalLightIntensity;
    }

    private static void CreateCamera(Transform target)
    {
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camGo.AddComponent<AudioListener>();

        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = RPGFantasyAmbience.Sky;
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGo.transform.position = new Vector3(0f, 0f, -10f);

        camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        var follow = camGo.AddComponent<CameraFollow2D>();
        follow.SetTarget(target);
    }

    private static void CreateOverworld()
    {
        var root = new GameObject("Overworld");
        var sprite = HealthBarUtility.GetWhiteSprite();

        for (int x = -12; x < 12; x++)
        {
            for (int y = -8; y < 8; y++)
            {
                var tile = new GameObject($"Ground_{x}_{y}");
                tile.transform.SetParent(root.transform);
                tile.transform.position = new Vector3(x, y, 0f);

                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = -10;
                sr.color = (x + y) % 2 == 0
                    ? RPGFantasyAmbience.GrassLight
                    : RPGFantasyAmbience.GrassDark;
            }
        }

        CreateTree(root.transform, sprite, new Vector3(-8f, 3f, 0f), new Vector3(1.4f, 1.4f, 1f), RPGFantasyAmbience.TreeA);
        CreateTree(root.transform, sprite, new Vector3(7f, -2f, 0f), new Vector3(2f, 1.2f, 1f), RPGFantasyAmbience.TreeB);
        CreateTree(root.transform, sprite, new Vector3(3f, 5f, 0f), new Vector3(1.8f, 1.8f, 1f), RPGFantasyAmbience.TreeC);

        CreateMeadowSign(root.transform);
    }

    private static void CreateMeadowSign(Transform parent)
    {
        var go = new GameObject("MeadowSign");
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(0f, -7f, 0f);

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = RPGFantasyAmbience.MeadowSignText;
        tmp.fontSize = 22f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = RPGFantasyAmbience.MeadowSign;
        tmp.fontStyle = FontStyles.Italic;
    }

    private static void CreateTree(Transform parent, Sprite sprite, Vector3 pos, Vector3 scale, Color color)
    {
        var go = new GameObject("Tree");
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = -5;
    }

    private static GameObject CreatePlayer()
    {
        var player = new GameObject("Player");
        player.transform.position = Vector3.zero;

        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = HealthBarUtility.GetWhiteSprite();
        sr.color = RPGFantasyAmbience.HeroTint;
        sr.sortingOrder = 5;

        var body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;

        var collider = player.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.8f, 0.8f);

        player.AddComponent<OverworldPlayerController>();
        player.AddComponent<RandomEncounterTrigger>();

        return player;
    }

    private static void CreateManagers()
    {
        var root = new GameObject("GameManagers");
        root.AddComponent<OverworldAmbience>();
        root.AddComponent<RPGSaveManager>();
        root.AddComponent<RPGGameManager>();
        root.AddComponent<TurnBasedCombatManager>();
    }

    private static void CreateUI()
    {
        var canvasGo = new GameObject("RPG Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);

        canvasGo.AddComponent<GraphicRaycaster>();
        var ui = canvasGo.AddComponent<CombatUIController>();

        RPGCombatUILayout.Build(canvasGo.transform, ui);
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }
}
