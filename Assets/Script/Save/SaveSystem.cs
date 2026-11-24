using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SaveSystem
{
    const string FILE_NAME = "save.json";
    const string BROKEN_SUFFIX = ".broken";
    const string TEMP_SUFFIX = ".tmp";

    static SaveData data;

    public static event Action Changed;

    public static string FilePath =>
#if UNITY_EDITOR
        PathOverrideForTests ??
#endif
        Path.Combine(Application.persistentDataPath, FILE_NAME);

    public static SaveData Data => data ??= Load();

    static SettingsDefaults Defaults => GameConfig.Instance.defaults;

    static SaveData Load()
    {
        string path = FilePath;
        if (!File.Exists(path))
        {
            Debug.Log("[Save] No save file, using defaults: " + path);
            return SaveData.CreateDefault(Defaults);
        }

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) throw new InvalidDataException("file is empty");

            SaveData loaded = SaveData.CreateDefault(Defaults);
            JsonUtility.FromJsonOverwrite(json, loaded);
            if (loaded.version <= 0) throw new InvalidDataException("no valid version field");

            Sanitize(loaded);
            Debug.Log("[Save] Loaded " + path);
            return loaded;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Save] Broken save, starting with defaults. Reason: " + e.Message);
            KeepBrokenCopy(path);
            SaveData fresh = SaveData.CreateDefault(Defaults);
            data = fresh;
            Save();
            return fresh;
        }
    }

    static void Sanitize(SaveData d)
    {
        int levelCount = Mathf.Max(SaveData.FIRST_LEVEL, GameConfig.Instance.LevelCount);
        d.version = SaveData.CURRENT_VERSION;
        d.bestScore = Mathf.Max(0, d.bestScore);
        d.musicVol = Mathf.Clamp01(d.musicVol);
        d.sfxVol = Mathf.Clamp01(d.sfxVol);
        d.colorIndex = Mathf.Max(0, d.colorIndex);
        d.playerName ??= "";

        d.unlocked = (d.unlocked ?? new List<int>())
            .Where(l => l >= SaveData.FIRST_LEVEL && l <= levelCount)
            .Append(SaveData.FIRST_LEVEL)
            .Distinct().OrderBy(l => l).ToList();

        d.levels = (d.levels ?? new List<LevelRecord>())
            .Where(r => r != null && r.level >= SaveData.FIRST_LEVEL && r.level <= levelCount)
            .GroupBy(r => r.level)
            .Select(g => new LevelRecord { level = g.Key, bestScore = Mathf.Max(0, g.Max(r => r.bestScore)) })
            .OrderBy(r => r.level).ToList();
    }

    static void KeepBrokenCopy(string path)
    {
        try
        {
            if (File.Exists(path)) File.Copy(path, path + BROKEN_SUFFIX, true);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Save] Could not back up the broken file: " + e.Message);
        }
    }

    public static bool Save()
    {
        if (data == null) return false;
        string path = FilePath;
        string temp = path + TEMP_SUFFIX;
        try
        {
            File.WriteAllText(temp, JsonUtility.ToJson(data, true));
            File.Copy(temp, path, true);
            File.Delete(temp);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[Save] Could not write " + path + ": " + e.Message);
            return false;
        }
    }

    public static bool RecordLevelComplete(int level, int score, int levelToUnlock)
    {
        SaveData d = Data;
        bool isNewBest = score > d.BestFor(level);

        if (isNewBest)
        {
            d.levels = d.levels.Where(r => r.level != level)
                .Append(new LevelRecord { level = level, bestScore = score })
                .OrderBy(r => r.level).ToList();
        }
        d.bestScore = Mathf.Max(d.bestScore, score);

        bool canUnlock = levelToUnlock <= GameConfig.Instance.LevelCount;
        if (canUnlock && !d.unlocked.Contains(levelToUnlock))
            d.unlocked = d.unlocked.Append(levelToUnlock).OrderBy(l => l).ToList();

        Save();
        Changed?.Invoke();
        return isNewBest;
    }

    public static void ResetProgress()
    {
        SaveData old = Data;
        SaveData fresh = SaveData.CreateDefault(Defaults);
        fresh.musicVol = old.musicVol;
        fresh.sfxVol = old.sfxVol;
        fresh.musicOn = old.musicOn;
        fresh.fullscreen = old.fullscreen;
        fresh.playerName = old.playerName;
        fresh.colorIndex = old.colorIndex;
        data = fresh;
        Save();
        Debug.Log("[Save] Progress reset");
        Changed?.Invoke();
    }

    public static void SetMusicVolume(float v) => ChangeSetting(d => d.musicVol = Mathf.Clamp01(v));
    public static void SetSfxVolume(float v) => ChangeSetting(d => d.sfxVol = Mathf.Clamp01(v));
    public static void SetMusicOn(bool on) => ChangeSetting(d => d.musicOn = on);
    public static void SetFullscreen(bool on) => ChangeSetting(d => d.fullscreen = on);
    public static void SetPlayerName(string n) => ChangeSetting(d => d.playerName = n ?? "");
    public static void SetColorIndex(int i) => ChangeSetting(d => d.colorIndex = Mathf.Max(0, i));

    static void ChangeSetting(Action<SaveData> change)
    {
        change(Data);
        Save();
        Changed?.Invoke();
    }

#if UNITY_EDITOR
    public static string PathOverrideForTests;

    public static void ForgetCachedData() => data = null;
#endif
}
