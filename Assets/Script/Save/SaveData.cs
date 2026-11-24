using System;
using System.Collections.Generic;

[Serializable]
public class LevelRecord
{
    public int level;
    public int bestScore;
}

[Serializable]
public class SaveData
{
    public const int CURRENT_VERSION = 1;
    public const int FIRST_LEVEL = 1;

    public int version;
    public int bestScore;
    public List<int> unlocked = new List<int>();
    public List<LevelRecord> levels = new List<LevelRecord>();

    public float musicVol;
    public float sfxVol;
    public bool musicOn;
    public bool fullscreen;
    public string playerName;
    public int colorIndex;

    public static SaveData CreateDefault(SettingsDefaults d)
    {
        return new SaveData
        {
            version = CURRENT_VERSION,
            unlocked = new List<int> { FIRST_LEVEL },
            musicVol = d.musicVolume,
            sfxVol = d.sfxVolume,
            musicOn = d.musicOn,
            fullscreen = d.fullscreen,
            playerName = d.playerName ?? "",
        };
    }

    public bool IsUnlocked(int level) => unlocked.Contains(level);

    public int BestFor(int level)
    {
        LevelRecord r = levels.Find(x => x.level == level);
        return r != null ? r.bestScore : 0;
    }

    public int HighestUnlocked()
    {
        int max = FIRST_LEVEL;
        foreach (int l in unlocked) if (l > max) max = l;
        return max;
    }
}
