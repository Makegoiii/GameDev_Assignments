using NUnit.Framework;

public class GameDataTests
{
    static GameConfig Config => GameConfig.Instance;

    [Test]
    public void Config_HasPlayerAndItemData()
    {
        Assert.IsNotNull(Config.player, "PlayerStats missing");
        Assert.IsNotNull(Config.items, "ItemValues missing");
        Assert.Greater(Config.player.moveSpeed, 0f);
        Assert.Greater(Config.player.maxHp, 0);
    }

    [Test]
    public void ThreeLevels_NumberedInOrder_WithEnemiesAndTime()
    {
        Assert.AreEqual(3, Config.LevelCount);
        for (int i = 0; i < Config.LevelCount; i++)
        {
            LevelData level = Config.levels[i];
            Assert.IsNotNull(level, "Level slot " + i + " empty");
            Assert.AreEqual(i + 1, level.levelNumber, level.name);
            Assert.Greater(level.timeLimit, 0f, level.name);
            Assert.Greater(level.EnemyCount, 0, level.name);
        }
    }

    [Test]
    public void EveryEnemyTypeUsed_HasAPrefabWithStats()
    {
        foreach (LevelData level in Config.levels)
            foreach (EnemyWave wave in level.waves)
                foreach (EnemySpawn spawn in wave.spawns)
                {
                    UnityEngine.GameObject prefab = Config.GetEnemyPrefab(spawn.type);
                    Assert.IsNotNull(prefab, spawn.type + " prefab missing: run Tools > Assignment 11 > Build Everything");
                    Assert.IsNotNull(prefab.GetComponent<EnemyHealth>(), spawn.type + " has no EnemyHealth");
                }
    }

    [Test]
    public void SceneNames_AreSet()
    {
        Assert.IsNotEmpty(Config.menuScene);
        Assert.IsNotEmpty(Config.levelScene);
        Assert.IsNotEmpty(Config.winScene);
    }
}
