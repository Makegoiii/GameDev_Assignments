using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject pauseButton;
    public GameObject[] hideWhilePaused;

    public static bool IsPaused { get; private set; }

    void Start()
    {
        SetPaused(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SetPaused(!IsPaused);
    }

    public void Pause()   => SetPaused(true);
    public void Resume()  => SetPaused(false);

    public void Restart()
    {
        SetPaused(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToMenu()
    {
        SetPaused(false);
        LevelFlow.ToMenu();
    }

    void SetPaused(bool shouldPause)
    {
        if (shouldPause) SaveSystem.Save();
        IsPaused = shouldPause;
        Time.timeScale = shouldPause ? 0f : 1f;
        if (AudioManager.Instance != null) AudioManager.Instance.SetPaused(shouldPause);

        if (pausePanel != null) pausePanel.SetActive(shouldPause);
        if (pauseButton != null) pauseButton.SetActive(!shouldPause);
        if (hideWhilePaused == null) return;
        foreach (GameObject go in hideWhilePaused)
            if (go != null) go.SetActive(!shouldPause);
    }

    void OnDestroy()
    {
        IsPaused = false;
        Time.timeScale = 1f;
    }
}
