using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectMenu : MonoBehaviour
{
    [SerializeField] Button[] levelButtons;
    [SerializeField] TextMeshProUGUI totalBestText;

    [Header("Text")]
    [SerializeField] string unlockedFormat = "{0}. {1}\nBest: {2}";
    [SerializeField] string lockedFormat = "{0}. LOCKED";
    [SerializeField] string totalBestFormat = "Best score: {0}";
    [SerializeField] Color lockedTextColor = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] Color unlockedTextColor = Color.white;

    void Awake()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null) continue;
            int levelNumber = i + 1;
            levelButtons[i].onClick.AddListener(() => LevelFlow.StartLevel(levelNumber));
        }
    }

    void OnEnable()
    {
        SaveSystem.Changed += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        SaveSystem.Changed -= Refresh;
    }

    public void Refresh()
    {
        GameConfig cfg = GameConfig.Instance;
        SaveData save = SaveSystem.Data;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            Button button = levelButtons[i];
            if (button == null) continue;

            LevelData level = cfg.GetLevel(i + 1);
            button.gameObject.SetActive(level != null);
            if (level == null) continue;

            bool isUnlocked = save.IsUnlocked(level.levelNumber);
            button.interactable = isUnlocked;

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null) continue;
            label.text = isUnlocked
                ? string.Format(unlockedFormat, level.levelNumber, level.title, save.BestFor(level.levelNumber))
                : string.Format(lockedFormat, level.levelNumber);
            label.color = isUnlocked ? unlockedTextColor : lockedTextColor;
        }

        if (totalBestText != null)
            totalBestText.text = string.Format(totalBestFormat, save.bestScore);
    }
}
