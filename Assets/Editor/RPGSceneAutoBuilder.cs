#if UNITY_EDITOR
using System.IO;
using UnityEditor;

[InitializeOnLoad]
internal static class RPGSceneAutoBuilder
{
    static RPGSceneAutoBuilder()
    {
        EditorApplication.delayCall += TryBuildMissingScene;
    }

    private static void TryBuildMissingScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (File.Exists(RPGSceneBuilder.ScenePath))
            return;

        RPGSceneBuilder.BuildScene();
    }
}
#endif
