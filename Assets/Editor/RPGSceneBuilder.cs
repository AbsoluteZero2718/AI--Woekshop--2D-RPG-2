#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public static class RPGSceneBuilder
{
    public const string ScenePath = "Assets/Scenes/RPGScene.unity";
    private const string DefaultSpritePath = "Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Square.png";

    [MenuItem("Tools/RPG/Build RPG Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateLighting();
        var camera = CreateCamera();
        var ground = CreateOverworldGround();
        var player = CreatePlayer();
        CreateUI();
        var managers = CreateManagers();

        camera.GetComponent<CameraFollow2D>().SetTarget(player.transform);

        var loader = new GameObject("RPG Bootstrap");
        loader.AddComponent<RPGRuntimeLoader>().enabled = false;

        EnsureDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);

        Selection.activeGameObject = player;
        Debug.Log($"RPG scene saved to {ScenePath}. Press Play to explore and fight random encounters!");
    }

    [MenuItem("Tools/RPG/Add Runtime Loader To Current Scene")]
    public static void AddLoaderToCurrentScene()
    {
        if (Object.FindFirstObjectByType<RPGRuntimeLoader>() != null)
        {
            Debug.LogWarning("RPGRuntimeLoader already exists in this scene.");
            return;
        }

        var loader = new GameObject("RPG Runtime Loader");
        loader.AddComponent<RPGRuntimeLoader>();
        Selection.activeGameObject = loader;
        Debug.Log("Added RPG Runtime Loader. Press Play to spawn the RPG overworld and combat systems.");
    }

    private static void CreateLighting()
    {
        var lightGo = new GameObject("Global Light 2D");
        var light = lightGo.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
        light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
        light.intensity = RPGFantasyAmbience.GlobalLightIntensity;
    }

    private static GameObject CreateCamera()
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
        camGo.AddComponent<CameraFollow2D>();

        return camGo;
    }

    private static GameObject CreateOverworldGround()
    {
        var root = new GameObject("Overworld");
        var sprite = LoadDefaultSprite();

        int width = 24;
        int height = 16;

        for (int x = -width / 2; x < width / 2; x++)
        {
            for (int y = -height / 2; y < height / 2; y++)
            {
                var tile = new GameObject($"Ground_{x}_{y}");
                tile.transform.SetParent(root.transform);
                tile.transform.position = new Vector3(x, y, 0f);

                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = -10;

                bool checker = (x + y) % 2 == 0;
                sr.color = checker
                    ? RPGFantasyAmbience.GrassLight
                    : RPGFantasyAmbience.GrassDark;
            }
        }

        CreateDecor(root.transform, sprite, new Vector3(-8f, 3f, 0f), RPGFantasyAmbience.TreeA, new Vector3(1.4f, 1.4f, 1f));
        CreateDecor(root.transform, sprite, new Vector3(7f, -2f, 0f), RPGFantasyAmbience.TreeB, new Vector3(2f, 1.2f, 1f));
        CreateDecor(root.transform, sprite, new Vector3(3f, 5f, 0f), RPGFantasyAmbience.TreeC, new Vector3(1.8f, 1.8f, 1f));

        var sign = CreateWorldLabel(root.transform, RPGFantasyAmbience.MeadowSignText, new Vector3(0f, -7f, 0f), 22f);
        sign.GetComponent<TextMeshPro>().color = RPGFantasyAmbience.MeadowSign;
        sign.GetComponent<TextMeshPro>().fontStyle = FontStyles.Italic;

        return root;
    }

    private static void CreateDecor(Transform parent, Sprite sprite, Vector3 pos, Color color, Vector3 scale)
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

    private static GameObject CreateWorldLabel(Transform parent, string text, Vector3 pos, float fontSize)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent);
        go.transform.position = pos;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return go;
    }

    private static GameObject CreatePlayer()
    {
        var player = new GameObject("Player");
        player.transform.position = Vector3.zero;

        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = LoadDefaultSprite();
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

    private static GameObject CreateManagers()
    {
        var root = new GameObject("GameManagers");
        root.AddComponent<OverworldAmbience>();
        root.AddComponent<RPGSaveManager>();
        root.AddComponent<RPGGameManager>();
        root.AddComponent<TurnBasedCombatManager>();
        return root;
    }

    private static CombatUIController CreateUI()
    {
        EnsureEventSystem();

        var canvasGo = new GameObject("RPG Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280f, 720f);
        canvasGo.AddComponent<GraphicRaycaster>();

        var ui = canvasGo.AddComponent<CombatUIController>();
        RPGCombatUILayout.Build(canvasGo.transform, ui);
        return ui;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
            return;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;

        var img = go.AddComponent<Image>();
        img.color = color;

        return go;
    }

    private static TMP_Text CreateTmpText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.text = name;

        return tmp;
    }

    private static Image CreateUIImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color = color;

        return img;
    }

    private static Sprite LoadDefaultSprite()
    {
        var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite != null)
            return sprite;

        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DefaultSpritePath);
        if (sprite != null)
            return sprite;

        var tex = new Texture2D(4, 4);
        for (int i = 0; i < 16; i++)
            tex.SetPixel(i % 4, i / 4, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
        {
            if (s.path == scenePath)
                return;
        }

        var list = new EditorBuildSettingsScene[scenes.Length + 1];
        for (int i = 0; i < scenes.Length; i++)
            list[i] = scenes[i];

        list[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = list;
    }
}
#endif
