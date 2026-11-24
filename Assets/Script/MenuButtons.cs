using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    public GameObject settingsPanel;

    public void OpenPanel(GameObject panel) => panel.SetActive(true);

    public void ClosePanel(GameObject panel) => panel.SetActive(false);

    public void Play() => LevelFlow.ContinueGame();

    public void LoadLevel(int levelNumber) => LevelFlow.StartLevel(levelNumber);

    public void Settings() => settingsPanel.SetActive(true);

    public void Back() => settingsPanel.SetActive(false);

    public void SetVolume(float value) => SaveSystem.SetMusicVolume(value);

    public void Quit()
    {
        SaveSystem.Save();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
