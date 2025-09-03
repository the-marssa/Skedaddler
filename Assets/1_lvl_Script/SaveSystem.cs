using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string FileName = "uhylyant.save.json";
    private static string FullPath => Path.Combine(Application.persistentDataPath, FileName);

    public static GameSave LoadOrCreate()
    {
        try
        {
            if (File.Exists(FullPath))
            {
                var json = File.ReadAllText(FullPath);
                var data = JsonUtility.FromJson<GameSave>(json);
                if (data != null) return data;
            }
        }
        catch { /* ignore corrupt */ }
        return new GameSave();
    }

    public static void Save(GameSave data)
    {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FullPath, json);
    }
}
