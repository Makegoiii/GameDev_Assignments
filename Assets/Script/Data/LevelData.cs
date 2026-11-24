using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { Patroller, Shooter, Rusher }

public enum PropType { Crate, Wall, Barrel, Sandbags, Tree, Lamp }

[Serializable]
public class PropPlacement
{
    public PropType type;
    public Vector3 position;
    public Vector3 size;
    public float yaw;
}

[Serializable]
public class LevelTheme
{
    public Color sunColor;
    [Min(0f)] public float sunIntensity;
    public Color ambientColor;
    public bool fog;
    public Color fogColor;
    [Min(0f)] public float fogDensity;

    [Header("Map style: colour of floor, outer walls and props")]
    public Color floorColor = Color.white;
    public Color wallColor = Color.white;
    public Color propColor = Color.white;
}

[Serializable]
public class EnemySpawn
{
    public EnemyType type;
    public Vector3 position;
    public float yaw;
    public List<Vector3> route = new List<Vector3>();
}

[Serializable]
public class EnemyWave
{
    public List<EnemySpawn> spawns = new List<EnemySpawn>();
}

[Serializable]
public class HazardRainSettings
{
    public bool enabled;
    [Min(0f)] public float startDelay;
    [Min(0.05f)] public float interval;
    [Min(0.05f)] public float minInterval;
    [Min(0.1f)] public float rampEvery;
    [Range(0.1f, 1f)] public float rampFactor;
    public Vector3 center;
    [Min(0f)] public float rangeX;
    [Min(0f)] public float rangeZ;
    public float height;
    [Min(0)] public int rockWeight;
    [Min(0)] public int bombWeight;
    [Min(0)] public int heartWeight;
}

[CreateAssetMenu(fileName = "Level_00", menuName = "Coin Quest/Level Data")]
public class LevelData : ScriptableObject
{
    [Min(1)] public int levelNumber;
    public string title;

    [Header("Goal")]
    [Min(1f)] public float timeLimit;
    [Min(0)] public int clearBonus;
    [Min(0f)] public float waveDelay;

    [Header("Layout")]
    public Vector3 playerStart;
    [Tooltip("Square arena: half of its side. 0 = the old round arena of the scene")]
    [Min(0f)] public float arenaHalfSize;
    public List<Vector3> coins = new List<Vector3>();
    public List<PropPlacement> props = new List<PropPlacement>();
    public LevelTheme theme = new LevelTheme();
    public List<EnemyWave> waves = new List<EnemyWave>();
    public HazardRainSettings hazards = new HazardRainSettings();

    public int EnemyCount
    {
        get
        {
            int n = 0;
            foreach (EnemyWave w in waves) n += w.spawns.Count;
            return n;
        }
    }
}
