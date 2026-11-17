using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelFlow
{
    public static int CurrentLevel { get; private set; } = SaveData.FIRST_LEVEL;

    public static void StartLevel(int levelNumber)
    {
        GameConfig cfg = GameConfig.Instance;
        if (cfg.GetLevel(levelNumber) == null)
        {
            Debug.LogError("No LevelData for level " + levelNumber);
            return;
        }
        if (!SaveSystem.Data.IsUnlocked(levelNumber))
        {
            Debug.LogWarning("Level " + levelNumber + " is locked");
            return;
        }
        CurrentLevel = levelNumber;
        Time.timeScale = 1f;
        SceneManager.LoadScene(cfg.levelScene);
    }

    public static void ContinueGame() => StartLevel(SaveSystem.Data.HighestUnlocked());

    public static void GoToNextAfter(int finishedLevel)
    {
        GameConfig cfg = GameConfig.Instance;
        int next = finishedLevel + 1;
        if (cfg.GetLevel(next) != null) StartLevel(next);
        else LoadScene(cfg.winScene);
    }

    public static void ToMenu() => LoadScene(GameConfig.Instance.menuScene);

    static void LoadScene(string scene)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

#if UNITY_EDITOR
    public static void SetCurrentForEditor(int levelNumber) => CurrentLevel = levelNumber;
#endif
}
