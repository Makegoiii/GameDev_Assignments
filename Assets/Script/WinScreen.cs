using TMPro;
using UnityEngine;

public class WinScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI bestScoreText;
    [SerializeField] string bestScoreFormat = "Best score: {0}";

    void Start()
    {
        Time.timeScale = 1f;
        SaveSystem.Save();
        if (bestScoreText != null)
            bestScoreText.text = string.Format(bestScoreFormat, SaveSystem.Data.bestScore);
    }

    public void ToMenu() => LevelFlow.ToMenu();

    public void PlayAgain() => LevelFlow.StartLevel(SaveData.FIRST_LEVEL);
}
