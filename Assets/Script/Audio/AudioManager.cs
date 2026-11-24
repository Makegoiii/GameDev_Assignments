using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    SoundLibrary lib;
    SfxPlayer sfx;
    MusicPlayer music;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        SoundLibrary library = SoundLibrary.Instance;
        if (library == null || library.mixer == null) return;

        GameObject go = new GameObject("AudioManager");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<AudioManager>();
        Instance.Init(library);
    }

    void Init(SoundLibrary library)
    {
        lib = library;
        sfx = new SfxPlayer(transform, lib);
        music = gameObject.AddComponent<MusicPlayer>();
        music.Init(lib);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SaveSystem.Changed += ApplyVolumes;
    }

    void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SaveSystem.Changed -= ApplyVolumes;
        Instance = null;
    }

    void Start() => ApplyVolumes();

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyVolumes();
        SetPaused(false);
        music.Play(IsMenuScene(scene.name) ? lib.menuMusic : lib.levelMusic);
        RouteUnassignedSources();
        AddClickSounds();
    }

    static bool IsMenuScene(string sceneName)
    {
        GameConfig cfg = GameConfig.Instance;
        return sceneName == cfg.menuScene || sceneName == cfg.winScene;
    }

    public void ApplyVolumes()
    {
        SaveData d = SaveSystem.Data;
        float musicLinear = d.musicOn ? d.musicVol : 0f;
        SetMixerDb(lib.musicVolumeParam, VolumeMath.ToDecibels(musicLinear));
        SetMixerDb(lib.sfxVolumeParam, VolumeMath.ToDecibels(d.sfxVol));
    }

    void SetMixerDb(string param, float db)
    {
        if (!lib.mixer.SetFloat(param, db))
            Debug.LogWarning("[Audio] Mixer has no exposed parameter '" + param + "'");
    }

    public void SetPaused(bool paused)
    {
        string name = paused ? lib.pausedSnapshot : lib.gameplaySnapshot;
        AudioMixerSnapshot snapshot = lib.mixer.FindSnapshot(name);
        if (snapshot == null) { Debug.LogWarning("[Audio] No snapshot '" + name + "'"); return; }
        snapshot.TransitionTo(lib.snapshotTransition);
    }

    public void PlaySfx(SoundEvent id, Vector3? position) => sfx.Play(id, position);

    void RouteUnassignedSources()
    {
        foreach (AudioSource src in FindObjectsByType<AudioSource>(FindObjectsInactive.Include))
            if (src.outputAudioMixerGroup == null) src.outputAudioMixerGroup = lib.sfxGroup;
    }

    void AddClickSounds()
    {
        foreach (Button b in FindObjectsByType<Button>(FindObjectsInactive.Include))
            b.onClick.AddListener(PlayClick);
        foreach (Toggle t in FindObjectsByType<Toggle>(FindObjectsInactive.Include))
            t.onValueChanged.AddListener(_ => PlayClick());
    }

    static void PlayClick() => Sfx.Play(SoundEvent.UiClick);
}
