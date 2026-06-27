#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class RPGSaveEditorUtility
{
    [MenuItem("Tools/RPG/Delete Save File")]
    public static void DeleteSaveFile()
    {
        if (RPGSaveSystem.TryDeleteSave(out string error))
            Debug.Log($"Deleted RPG save at {RPGSaveSystem.SaveFilePath}");
        else
            Debug.LogWarning($"Could not delete RPG save: {error}");
    }

    [MenuItem("Tools/RPG/Open Save Folder")]
    public static void OpenSaveFolder()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
#endif
