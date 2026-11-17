using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SettingsDefaults
{
    [Range(0f, 1f)] public float musicVolume;
    public bool musicOn;
    public bool fullscreen;
    public string playerName;
}

[CreateAssetMenu(fileName = "GameConfig", menuName = "Coin Quest/Game Config")]
public class GameConfig : ScriptableObject
{
    const string RESOURCE_PATH = "GameConfig";

    [Header("Balance")]
    public PlayerStats player;
    public ItemValues items;
    public BossStats boss;

    [Header("Levels (order = level select order)")]
    public List<LevelData> levels = new List<LevelData>();

    [Header("Prefabs")]
    public GameObject patrollerPrefab;
    public GameObject shooterPrefab;
    public GameObject rusherPrefab;
    public GameObject coinPrefab;
    public Hazard rockPrefab;
    public Hazard bombPrefab;
    public Hazard heartPrefab;

    [Header("Map materials")]
    public Material crateMaterial;
    public Material wallMaterial;
    public Material barrelMaterial;
    public Material sandbagMaterial;

    public Material GetPropMaterial(PropType type)
    {
        switch (type)
        {
            case PropType.Crate: return crateMaterial;
            case PropType.Wall: return wallMaterial;
            case PropType.Barrel: return barrelMaterial;
            case PropType.Sandbags: return sandbagMaterial;
            default: return null;
        }
    }

    [Header("Scenes")]
    public string menuScene;
    public string levelScene;
    public string winScene;

    [Header("Flow")]
    [Min(0f)] public float resultScreenTime;
    [Min(1f)] public float fallbackTimeLimit;
    [Min(1f)] public float survivalGoal;
    public HazardRainSettings legacyRain = new HazardRainSettings();

    [Header("Default settings (used for a new or broken save)")]
    public SettingsDefaults defaults = new SettingsDefaults();

    static GameConfig instance;

    public static GameConfig Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = Resources.Load<GameConfig>(RESOURCE_PATH);
            if (instance == null)
            {
                Debug.LogError("GameConfig missing: run Tools > Assignment 11 > Build Everything");
                instance = CreateInstance<GameConfig>();
            }
            return instance;
        }
    }

    public int LevelCount => levels.Count;

    public LevelData GetLevel(int levelNumber)
    {
        int index = levelNumber - 1;
        return index >= 0 && index < levels.Count ? levels[index] : null;
    }

    public GameObject GetEnemyPrefab(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Patroller: return patrollerPrefab;
            case EnemyType.Shooter: return shooterPrefab;
            case EnemyType.Rusher: return rusherPrefab;
            default: return null;
        }
    }
}
