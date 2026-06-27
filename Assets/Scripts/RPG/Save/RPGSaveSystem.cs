using System;
using System.IO;
using UnityEngine;

public static class RPGSaveSystem
{
    private const string SaveFileName = "rpg_save.json";

    public static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static bool HasSave()
    {
        return File.Exists(SaveFilePath);
    }

    public static bool TrySave(RPGSaveData data, out string error)
    {
        error = null;

        if (data == null)
        {
            error = "Save data was null.";
            return false;
        }

        try
        {
            data.saveVersion = RPGSaveData.CurrentVersion;
            data.savedAtUtc = DateTime.UtcNow.ToString("o");

            string directory = Path.GetDirectoryName(SaveFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"RPG save written to {SaveFilePath}");
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            Debug.LogWarning($"RPG save failed: {exception.Message}");
            return false;
        }
    }

    public static bool TryLoad(out RPGSaveData data, out string error)
    {
        data = null;
        error = null;

        if (!HasSave())
        {
            error = "No save file found.";
            return false;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            data = JsonUtility.FromJson<RPGSaveData>(json);

            if (data == null)
            {
                error = "Save file could not be parsed.";
                return false;
            }

            if (data.playerStats == null)
                data.playerStats = new CombatStats();

            if (data.saveVersion > RPGSaveData.CurrentVersion)
            {
                error = "Save file was created by a newer version of the game.";
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            Debug.LogWarning($"RPG load failed: {exception.Message}");
            return false;
        }
    }

    public static bool TryDeleteSave(out string error)
    {
        error = null;

        if (!HasSave())
            return true;

        try
        {
            File.Delete(SaveFilePath);
            Debug.Log("RPG save deleted.");
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            Debug.LogWarning($"RPG save delete failed: {exception.Message}");
            return false;
        }
    }
}
