using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SaveSystemTests
{
    string path;

    [SetUp]
    public void SetUp()
    {
        path = Path.Combine(Path.GetTempPath(), "coinquest_test_save.json");
        Cleanup();
        SaveSystem.PathOverrideForTests = path;
        SaveSystem.ForgetCachedData();
    }

    [TearDown]
    public void TearDown()
    {
        SaveSystem.PathOverrideForTests = null;
        SaveSystem.ForgetCachedData();
        Cleanup();
    }

    void Cleanup()
    {
        foreach (string p in new[] { path, path + ".broken", path + ".tmp" })
            if (File.Exists(p)) File.Delete(p);
    }

    static SettingsDefaults Defaults => GameConfig.Instance.defaults;

    [Test]
    public void MissingFile_GivesDefaults()
    {
        SaveData d = SaveSystem.Data;

        Assert.AreEqual(SaveData.CURRENT_VERSION, d.version);
        CollectionAssert.AreEqual(new[] { 1 }, d.unlocked);
        Assert.AreEqual(0, d.bestScore);
        Assert.AreEqual(Defaults.musicVolume, d.musicVol);
    }

    [Test]
    public void BrokenJson_GivesDefaults_KeepsBrokenCopy_AndRewritesFile()
    {
        File.WriteAllText(path, "{ \"version\": 1, \"bestScore\": 12 garbage ]]");
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Broken save"));

        SaveData d = SaveSystem.Data;

        Assert.AreEqual(0, d.bestScore);
        CollectionAssert.AreEqual(new[] { 1 }, d.unlocked);
        Assert.IsTrue(File.Exists(path + ".broken"));
        StringAssert.Contains("\"version\": 1", File.ReadAllText(path));
    }

    [Test]
    public void EmptyFile_GivesDefaults()
    {
        File.WriteAllText(path, "   ");
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Broken save"));

        Assert.AreEqual(0, SaveSystem.Data.bestScore);
    }

    [Test]
    public void HandEditedValues_AreRepaired()
    {
        File.WriteAllText(path,
            "{\"version\":1,\"bestScore\":-5,\"unlocked\":[0,2,2,99],\"musicVol\":7.5," +
            "\"levels\":[{\"level\":2,\"bestScore\":300},{\"level\":42,\"bestScore\":1}]}");

        SaveData d = SaveSystem.Data;

        Assert.AreEqual(0, d.bestScore);
        CollectionAssert.AreEqual(new[] { 1, 2 }, d.unlocked);
        Assert.AreEqual(1f, d.musicVol);
        Assert.AreEqual(1, d.levels.Count);
        Assert.AreEqual(300, d.BestFor(2));
    }

    [Test]
    public void LevelComplete_UnlocksNext_KeepsOnlyBestScore_AndSurvivesReload()
    {
        SaveSystem.RecordLevelComplete(1, 800, 2);
        bool secondIsBest = SaveSystem.RecordLevelComplete(1, 500, 2);

        SaveSystem.ForgetCachedData();
        SaveData reloaded = SaveSystem.Data;

        Assert.IsFalse(secondIsBest);
        Assert.AreEqual(800, reloaded.BestFor(1));
        Assert.AreEqual(800, reloaded.bestScore);
        CollectionAssert.AreEqual(new[] { 1, 2 }, reloaded.unlocked);
    }

    [Test]
    public void LastLevel_DoesNotUnlockMissingLevel()
    {
        int last = GameConfig.Instance.LevelCount;

        SaveSystem.RecordLevelComplete(last, 100, last + 1);

        Assert.IsFalse(SaveSystem.Data.IsUnlocked(last + 1));
    }

    [Test]
    public void ResetProgress_ClearsLevels_ButKeepsSettings()
    {
        SaveSystem.SetMusicVolume(0.3f);
        SaveSystem.SetPlayerName("Tester");
        SaveSystem.RecordLevelComplete(1, 900, 2);

        SaveSystem.ResetProgress();
        SaveSystem.ForgetCachedData();
        SaveData d = SaveSystem.Data;

        CollectionAssert.AreEqual(new[] { 1 }, d.unlocked);
        Assert.AreEqual(0, d.bestScore);
        Assert.AreEqual(0.3f, d.musicVol, 0.0001f);
        Assert.AreEqual("Tester", d.playerName);
    }

    [Test]
    public void SettingChange_IsWrittenToDiskImmediately()
    {
        SaveSystem.SetMusicOn(false);

        StringAssert.Contains("\"musicOn\": false", File.ReadAllText(path));
    }
}
