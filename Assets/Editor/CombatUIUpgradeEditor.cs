#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CombatUIUpgradeEditor
{
    [MenuItem("Tools/RPG/Upgrade Combat UI In Open Scene")]
    public static void UpgradeCombatUIInOpenScene()
    {
        var controllers = Object.FindObjectsByType<CombatUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (controllers.Length == 0)
        {
            Debug.LogWarning("No CombatUIController found in the open scene.");
            return;
        }

        foreach (CombatUIController controller in controllers)
        {
            controller.UpgradeCombatUI();
            EditorUtility.SetDirty(controller);
        }

        RPGFantasyAmbience.ApplyToOverworld();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"Upgraded and restyled combat UI on {controllers.Length} CombatUIController(s). Save the scene to keep changes.");
    }
}
#endif
