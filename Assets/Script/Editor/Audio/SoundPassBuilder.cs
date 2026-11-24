using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SoundPassBuilder
{
    const string AUDIO = "Assets/Audio";
    const string SFX = AUDIO + "/SFX/";
    const string MIXER_PATH = AUDIO + "/GameMixer.mixer";
    const string LIBRARY_FOLDER = "Assets/Resources/Audio";
    const string LIBRARY_PATH = LIBRARY_FOLDER + "/SoundLibrary.asset";
    const string MENU_MUSIC = AUDIO + "/Music/music_menu.wav";
    const string LEVEL_MUSIC = "Assets/Scenes/moodmode-that-game-arcade-236111.mp3";
    const string COIN_MP3 = "Assets/freesound_community-coin-pickup-98269.mp3";
    const string BONUS_MP3 = "Assets/universfield-game-bonus-02-294436.mp3";
    const string BLAST_MP3 = "Assets/joe_bou_khalil-procedural-sonic-detonation-570270.mp3";

    const string MENU_SCENE = "Assets/Main menu.unity";
    static readonly string[] BUILD_SCENES = { MENU_SCENE, "Assets/Scenes/DataLevel.unity", "Assets/Scenes/Win.unity" };
    static readonly string[] PREFAB_FOLDERS = { "Assets/Prefabs", "Assets/EnemyGen", "Assets/Level3Gen" };

    const float MUSIC_SOURCE_VOLUME = 0.55f;
    const float SLIDER_GAP = 45f;
    const float LABEL_WIDTH = 200f;
    const float LABEL_FONT = 24f;

    static readonly (SoundEvent id, string[] clips, float vol, bool pitch, float spatial, float cooldown)[] SOUNDS =
    {
        (SoundEvent.Jump,        new[] { SFX + "sfx_jump_1.wav", SFX + "sfx_jump_2.wav" },                      0.55f, true,  0f,   0.05f),
        (SoundEvent.Shoot,       new[] { SFX + "sfx_shoot_1.wav", SFX + "sfx_shoot_2.wav" },                    0.40f, true,  0f,   0.05f),
        (SoundEvent.Hit,         new[] { SFX + "sfx_hit_1.wav", SFX + "sfx_hit_2.wav", SFX + "sfx_hit_3.wav" }, 0.85f, true,  0.4f, 0.03f),
        (SoundEvent.PlayerHurt,  new[] { SFX + "sfx_hurt.wav" },                                                0.90f, true,  0f,   0.10f),
        (SoundEvent.Pickup,      new[] { COIN_MP3, SFX + "sfx_pickup_2.wav" },                                  0.70f, true,  0f,   0.06f),
        (SoundEvent.Heal,        new[] { BONUS_MP3 },                                                           0.80f, false, 0f,   0f),
        (SoundEvent.EnemyWindUp, new[] { SFX + "sfx_enemy_windup.wav" },                                        0.70f, true,  0.5f, 0.3f),
        (SoundEvent.EnemyShot,   new[] { SFX + "sfx_enemy_shot.wav" },                                          0.60f, true,  0.5f, 0.03f),
        (SoundEvent.Explosion,   new[] { BLAST_MP3 },                                                           0.90f, true,  0.6f, 0.05f),
        (SoundEvent.EnemyDeath,  new[] { SFX + "sfx_enemy_death.wav" },                                         0.90f, true,  0.4f, 0f),
        (SoundEvent.PlayerDeath, new[] { SFX + "sfx_player_death.wav" },                                        1.00f, false, 0f,   0.5f),
        (SoundEvent.UiClick,     new[] { SFX + "sfx_ui_click_1.wav", SFX + "sfx_ui_click_2.wav" },              0.60f, true,  0f,   0.03f),
        (SoundEvent.Win,         new[] { SFX + "sfx_win.wav" },                                                 0.90f, false, 0f,   0.5f),
        (SoundEvent.Lose,        new[] { SFX + "sfx_lose.wav" },                                                0.90f, false, 0f,   0.5f),
        (SoundEvent.Reload,      new[] { SFX + "sfx_reload.wav" },                                              0.80f, false, 0f,   0.5f),
    };

    [MenuItem("Tools/Assignment 12/Build Sound Pass")]
    public static void BuildFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Build();
    }

    public static void Build()
    {
        AssetDatabase.Refresh();
        AudioMixer mixer = LoadOrCreateMixer();
        if (mixer == null) return;
        ConfigureImports();
        SoundLibrary lib = BuildLibrary(mixer);
        if (lib == null) return;
        RoutePrefabs(lib.sfxGroup);
        foreach (string scene in BUILD_SCENES) ProcessScene(scene, lib);
        AssetDatabase.SaveAssets();
        Debug.Log("[A12] Sound pass built. Open 'Main menu' and press Play.");
    }

    static AudioMixer LoadOrCreateMixer()
    {
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MIXER_PATH);
        if (mixer != null) return mixer;
        try
        {
            return AudioMixerFactory.Create(MIXER_PATH);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[A12] Could not create the mixer from code (" + e.Message + "). " +
                           "Create it by hand: see Docs/SOUND.md");
            return null;
        }
    }

    static AudioMixerGroup Group(AudioMixer mixer, string name) =>
        mixer.FindMatchingGroups(string.Empty).FirstOrDefault(g => g.name == name);

    static void ConfigureImports()
    {
        foreach (string path in SOUNDS.SelectMany(s => s.clips).Distinct())
            SetImport(path, AudioClipLoadType.DecompressOnLoad, AudioCompressionFormat.ADPCM, false);
        SetImport(MENU_MUSIC, AudioClipLoadType.Streaming, AudioCompressionFormat.Vorbis, true);
        SetImport(LEVEL_MUSIC, AudioClipLoadType.Streaming, AudioCompressionFormat.Vorbis, true);
    }

    static void SetImport(string path, AudioClipLoadType load, AudioCompressionFormat format, bool background)
    {
        AudioImporter imp = AssetImporter.GetAtPath(path) as AudioImporter;
        if (imp == null) { Debug.LogError("[A12] Missing audio file " + path); return; }
        AudioImporterSampleSettings s = imp.defaultSampleSettings;
        if (s.loadType == load && s.compressionFormat == format && imp.loadInBackground == background) return;
        s.loadType = load;
        s.compressionFormat = format;
        imp.defaultSampleSettings = s;
        imp.loadInBackground = background;
        imp.SaveAndReimport();
    }

    static SoundLibrary BuildLibrary(AudioMixer mixer)
    {
        if (!AssetDatabase.IsValidFolder(LIBRARY_FOLDER)) AssetDatabase.CreateFolder("Assets/Resources", "Audio");
        SoundLibrary lib = AssetDatabase.LoadAssetAtPath<SoundLibrary>(LIBRARY_PATH);
        if (lib == null)
        {
            lib = ScriptableObject.CreateInstance<SoundLibrary>();
            AssetDatabase.CreateAsset(lib, LIBRARY_PATH);
        }

        lib.mixer = mixer;
        lib.musicGroup = Group(mixer, "Music");
        lib.sfxGroup = Group(mixer, "SFX");
        if (lib.musicGroup == null || lib.sfxGroup == null)
        {
            Debug.LogError("[A12] GameMixer needs groups named 'Music' and 'SFX'");
            return null;
        }
        lib.gameplaySnapshot = AudioMixerFactory.GAMEPLAY_SNAPSHOT;
        lib.pausedSnapshot = AudioMixerFactory.PAUSED_SNAPSHOT;
        lib.musicVolumeParam = AudioMixerFactory.MUSIC_PARAM;
        lib.sfxVolumeParam = AudioMixerFactory.SFX_PARAM;
        lib.menuMusic = Clip(MENU_MUSIC);
        lib.levelMusic = Clip(LEVEL_MUSIC);
        lib.musicSourceVolume = MUSIC_SOURCE_VOLUME;
        lib.sounds = SOUNDS.Select(s => new SoundEntry
        {
            id = s.id,
            clips = s.clips.Select(Clip).Where(c => c != null).ToArray(),
            volume = s.vol,
            randomPitch = s.pitch,
            spatialBlend = s.spatial,
            cooldown = s.cooldown,
        }).ToList();

        EditorUtility.SetDirty(lib);
        AssetDatabase.SaveAssetIfDirty(lib);
        return lib;
    }

    static AudioClip Clip(string path)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) Debug.LogError("[A12] Missing clip " + path);
        return clip;
    }

    static void RoutePrefabs(AudioMixerGroup sfx)
    {
        string[] folders = PREFAB_FOLDERS.Where(AssetDatabase.IsValidFolder).ToArray();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", folders))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!asset.GetComponentsInChildren<AudioSource>(true).Any(s => s.outputAudioMixerGroup != sfx)) continue;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            foreach (AudioSource src in root.GetComponentsInChildren<AudioSource>(true))
                src.outputAudioMixerGroup = sfx;
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log("[A12] Routed AudioSources to SFX in " + path);
        }
    }

    static void ProcessScene(string path, SoundLibrary lib)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        List<AudioSource> sources = scene.GetRootGameObjects()
            .SelectMany(g => g.GetComponentsInChildren<AudioSource>(true)).ToList();

        foreach (AudioSource src in sources)
        {
            bool isOldMusic = src.loop && !PrefabUtility.IsPartOfPrefabInstance(src);
            if (isOldMusic)
            {
                Debug.Log("[A12] " + path + ": removed old music source on '" + src.name + "' (AudioManager plays music now)");
                Object.DestroyImmediate(src);
                continue;
            }
            src.outputAudioMixerGroup = lib.sfxGroup;
            if (src.loop) src.enabled = false;
        }

        if (path == MENU_SCENE) AddVolumeSliders(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void AddVolumeSliders(Scene scene)
    {
        ProfileSettings profile = Find<ProfileSettings>(scene);
        if (profile == null || profile.volumeSlider == null)
        {
            Debug.LogError("[A12] Main menu: ProfileSettings or its music slider not found");
            return;
        }

        Slider music = profile.volumeSlider;
        music.name = "Slider_Music";
        Transform parent = music.transform.parent;
        Transform existing = parent.Find("Slider_SFX");
        Slider sfx = existing != null ? existing.GetComponent<Slider>() : null;
        if (sfx == null)
        {
            sfx = Object.Instantiate(music, parent);
            sfx.name = "Slider_SFX";
            ClearPersistent(sfx.onValueChanged);
            UnityEventTools.AddPersistentListener(sfx.onValueChanged, profile.SetSfxVolume);
            MoveBackButtonDown(parent);
        }
        RectTransform musicRect = (RectTransform)music.transform;
        RectTransform sfxRect = (RectTransform)sfx.transform;
        sfxRect.anchoredPosition = musicRect.anchoredPosition + Vector2.down * SLIDER_GAP;
        sfx.transform.SetSiblingIndex(music.transform.GetSiblingIndex() + 1);
        sfx.minValue = music.minValue;
        sfx.maxValue = music.maxValue;
        sfx.SetValueWithoutNotify(1f);

        AddSideLabel(music, "Label_Music", "Music");
        AddSideLabel(sfx, "Label_SFX", "SFX");

        SerializedObject so = new SerializedObject(profile);
        so.FindProperty("sfxSlider").objectReferenceValue = sfx;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void MoveBackButtonDown(Transform parent)
    {
        Transform back = parent.Find("Btn_Back");
        if (back is RectTransform rt) rt.anchoredPosition += Vector2.down * SLIDER_GAP;
    }

    static void AddSideLabel(Slider slider, string name, string text)
    {
        Transform parent = slider.transform.parent;
        Transform existing = parent.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
        if (label == null) label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = LABEL_FONT;
        label.alignment = TextAlignmentOptions.MidlineRight;
        label.raycastTarget = false;

        RectTransform s = (RectTransform)slider.transform;
        RectTransform r = (RectTransform)go.transform;
        r.anchorMin = s.anchorMin;
        r.anchorMax = s.anchorMax;
        r.pivot = new Vector2(1f, 0.5f);
        r.sizeDelta = new Vector2(LABEL_WIDTH, s.sizeDelta.y + LABEL_FONT);
        r.anchoredPosition = s.anchoredPosition + Vector2.left * (s.sizeDelta.x * 0.5f + LABEL_FONT * 0.5f);
    }

    static void ClearPersistent(UnityEventBase evt)
    {
        for (int i = evt.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(evt, i);
    }

    static T Find<T>(Scene scene) where T : Component =>
        scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).FirstOrDefault();
}
