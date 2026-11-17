using UnityEngine;

public class SaveLifecycle : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        GameObject go = new GameObject(nameof(SaveLifecycle));
        DontDestroyOnLoad(go);
        go.AddComponent<SaveLifecycle>();
        Screen.fullScreen = SaveSystem.Data.fullscreen;
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SaveSystem.Save();
    }

    void OnApplicationQuit()
    {
        SaveSystem.Save();
    }
}
