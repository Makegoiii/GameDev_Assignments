using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DataLevelBuilder
{
    const string CONFIG_PATH = "Assets/Resources/GameConfig.asset";
    const string PREFAB_FOLDER = EnemyLevelFactory.GEN_FOLDER + "/Prefabs";
    const string SOURCE_SCENE = "Assets/Scenes/Level3_Enemies.unity";
    const string LEVEL_SCENE = "Assets/Scenes/DataLevel.unity";
    const string MENU_SCENE = "Assets/Main menu.unity";
    const string WIN_SCENE = "Assets/Scenes/Win.unity";
    const string COIN_PREFAB = "Assets/Prefabs/level/Coin.prefab";
    const string ROCK_PREFAB = "Assets/Prefabs/Rock.prefab";
    const string BOMB_PREFAB = "Assets/Prefabs/BombHazard.prefab";
    const string HEART_PREFAB = "Assets/Prefabs/Heart.prefab";

    static readonly string[] LEVEL_BUTTONS = { "Btn_Level1", "Btn_Level2", "Btn_Level3" };
    const string BACK_BUTTON = "Btn_LevelsBack";
    const float RESET_BUTTON_GAP = 40f;
    const float LABEL_MIN_FONT = 18f;
    const float ARENA_WALL_HEIGHT = 1.2f;
    const string NAVMESH_ASSET = EnemyLevelFactory.GEN_FOLDER + "/DataLevel_NavMesh.asset";

    [MenuItem("Tools/Assignment 11/Build Everything")]
    public static void BuildEverything()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Build();
    }

    public static void Build()
    {
        if (!BuildPrefabs()) return;
        BuildLevelScene();
        WireMainMenu();
        SetBuildScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("[A11] Done. Open 'Main menu' and press Play.");
    }

    static bool BuildPrefabs()
    {
        if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER))
            AssetDatabase.CreateFolder(EnemyLevelFactory.GEN_FOLDER, "Prefabs");

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(CONFIG_PATH);
        if (config == null) { Debug.LogError("Missing " + CONFIG_PATH); return false; }

        EnemyProjectile projectile = EnemyLevelFactory.MakeProjectilePrefab();

        SerializedObject so = new SerializedObject(config);
        so.FindProperty("patrollerPrefab").objectReferenceValue =
            SavePrefab(EnemyLevelFactory.MakePatroller(Vector3.zero, new Transform[0]));
        so.FindProperty("shooterPrefab").objectReferenceValue =
            SavePrefab(EnemyLevelFactory.MakeShooter(Vector3.zero, projectile));
        so.FindProperty("rusherPrefab").objectReferenceValue =
            SavePrefab(EnemyLevelFactory.MakeRusher(Vector3.zero, projectile));
        so.FindProperty("coinPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(COIN_PREFAB);
        so.FindProperty("rockPrefab").objectReferenceValue = LoadHazard(ROCK_PREFAB);
        so.FindProperty("bombPrefab").objectReferenceValue = LoadHazard(BOMB_PREFAB);
        so.FindProperty("heartPrefab").objectReferenceValue = LoadHazard(HEART_PREFAB);
        so.ApplyModifiedPropertiesWithoutUndo();
        MapArt.CreateAll(config);
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssetIfDirty(config);
        return true;
    }

    static GameObject SavePrefab(GameObject go)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PREFAB_FOLDER + "/" + go.name + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static Hazard LoadHazard(string path)
    {
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Hazard h = go != null ? go.GetComponent<Hazard>() : null;
        if (h == null) Debug.LogError("No Hazard component on " + path);
        return h;
    }

    static void BuildLevelScene()
    {
        AssetDatabase.DeleteAsset(LEVEL_SCENE);
        if (!AssetDatabase.CopyAsset(SOURCE_SCENE, LEVEL_SCENE))
        {
            Debug.LogError("Could not copy " + SOURCE_SCENE + " (build it with Tools > Assignment 10 first)");
            return;
        }
        Scene scene = EditorSceneManager.OpenScene(LEVEL_SCENE, OpenSceneMode.Single);

        RemoveHandPlacedContent(scene);
        DressArena(scene);
        RebakeNavMesh();
        TextMeshProUGUI counter = FindByName<TextMeshProUGUI>(scene, "EnemyCounter");
        Movement player = Object.FindAnyObjectByType<Movement>(FindObjectsInactive.Include);
        GameObject exit = FindByName<Transform>(scene, "Exit")?.gameObject;
        if (exit == null) Debug.LogError("No 'Exit' object in " + LEVEL_SCENE);

        GameObject runnerGo = new GameObject("LevelRunner");
        LevelRunner runner = runnerGo.AddComponent<LevelRunner>();
        SerializedObject so = new SerializedObject(runner);
        so.FindProperty("exitObject").objectReferenceValue = exit;
        so.FindProperty("counterText").objectReferenceValue = counter;
        so.FindProperty("messageText").objectReferenceValue = player != null ? player.winPanel : null;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void DressArena(Scene scene)
    {
        DestroyNamed(scene, "Cover");

        Renderer floor = FindByName<Renderer>(scene, "Arena");
        if (floor != null) floor.sharedMaterial = MapArt.Floor;

        Transform walls = FindByName<Transform>(scene, "ArenaWalls");
        if (walls == null) return;
        foreach (Renderer r in walls.GetComponentsInChildren<Renderer>(true))
        {
            Transform t = r.transform;
            t.localScale = new Vector3(t.localScale.x, ARENA_WALL_HEIGHT, t.localScale.z);
            t.position = new Vector3(t.position.x, ARENA_WALL_HEIGHT * 0.5f, t.position.z);
            r.sharedMaterial = MapArt.Wall;
        }
    }

    static void RebakeNavMesh()
    {
        NavMeshSurface surface = Object.FindAnyObjectByType<NavMeshSurface>(FindObjectsInactive.Include);
        if (surface == null) { Debug.LogError("No NavMeshSurface in " + LEVEL_SCENE); return; }

        Movement player = Object.FindAnyObjectByType<Movement>(FindObjectsInactive.Include);
        var hidden = Object.FindObjectsByType<Collider>()
            .Where(c => c.isTrigger || (player != null && c.transform.IsChildOf(player.transform)))
            .Select(c => c.gameObject).Distinct().Where(g => g.activeSelf).ToList();
        hidden.ForEach(g => g.SetActive(false));

        surface.BuildNavMesh();
        AssetDatabase.DeleteAsset(NAVMESH_ASSET);
        AssetDatabase.CreateAsset(surface.navMeshData, NAVMESH_ASSET);
        hidden.ForEach(g => g.SetActive(true));
    }

    static void RemoveHandPlacedContent(Scene scene)
    {
        var doomed = new List<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                bool isEnemy = t.GetComponent<EnemyHealth>() != null;
                bool isGoal = t.GetComponent<EnemyArenaGoal>() != null;
                bool isPickup = t.GetComponent<Hazard>() != null;
                bool isCoin = t.CompareTag("Coin");
                bool isOldSpawner = t.GetComponent<Spawner>() != null || t.GetComponent<SpawnerRamp>() != null;
                bool isRoute = t.name == "PatrolRoute";
                if (isEnemy || isGoal || isPickup || isCoin || isOldSpawner || isRoute) doomed.Add(t.gameObject);
            }
        foreach (GameObject go in doomed)
            if (go != null) Object.DestroyImmediate(go);
    }

    static void WireMainMenu()
    {
        Scene scene = EditorSceneManager.OpenScene(MENU_SCENE, OpenSceneMode.Single);

        Transform levelPanel = FindByName<Transform>(scene, "LevelSelectPanel");
        if (levelPanel == null) { Debug.LogError("No LevelSelectPanel in main menu"); return; }

        WireLevelSelect(scene, levelPanel);
        WireResetProgress(scene, levelPanel);
        WireSettings(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void WireLevelSelect(Scene scene, Transform panel)
    {
        LevelSelectMenu menu = panel.GetComponent<LevelSelectMenu>();
        if (menu == null) menu = panel.gameObject.AddComponent<LevelSelectMenu>();
        Button[] buttons = LEVEL_BUTTONS.Select(n => FindByName<Button>(scene, n)).ToArray();

        foreach (Button b in buttons.Where(b => b != null))
        {
            RemovePersistentListeners(b.onClick);
            TextMeshProUGUI label = b.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null) continue;
            label.enableAutoSizing = true;
            label.fontSizeMax = label.fontSize;
            label.fontSizeMin = LABEL_MIN_FONT;
            EditorUtility.SetDirty(label);
        }

        SerializedObject so = new SerializedObject(menu);
        SerializedProperty arr = so.FindProperty("levelButtons");
        arr.arraySize = buttons.Length;
        for (int i = 0; i < buttons.Length; i++)
            arr.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void WireResetProgress(Scene scene, Transform panel)
    {
        Button back = FindByName<Button>(scene, BACK_BUTTON);
        if (back == null) { Debug.LogError("No " + BACK_BUTTON + " to copy the style from"); return; }

        DestroyNamed(scene, "Btn_ResetProgress");
        DestroyNamed(scene, "ResetConfirmPanel");

        ResetProgressPanel reset = panel.GetComponent<ResetProgressPanel>();
        if (reset == null) reset = panel.gameObject.AddComponent<ResetProgressPanel>();

        Button resetButton = CopyButton(back, back.transform.parent, "Btn_ResetProgress", "Reset progress");
        RectTransform rt = (RectTransform)resetButton.transform;
        rt.anchoredPosition += new Vector2(rt.rect.width + RESET_BUTTON_GAP, 0f);
        UnityEventTools.AddVoidPersistentListener(resetButton.onClick, reset.AskReset);

        Canvas canvas = panel.GetComponentInParent<Canvas>();
        GameObject confirm = BuildConfirmPanel(canvas.transform, back, reset);

        SerializedObject so = new SerializedObject(reset);
        so.FindProperty("confirmPanel").objectReferenceValue = confirm;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static GameObject BuildConfirmPanel(Transform canvas, Button styleSource, ResetProgressPanel reset)
    {
        GameObject dim = NewUi("ResetConfirmPanel", canvas);
        Stretch((RectTransform)dim.transform);
        dim.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        GameObject box = NewUi("Box", dim.transform);
        RectTransform boxRt = (RectTransform)box.transform;
        boxRt.sizeDelta = new Vector2(900f, 520f);
        box.AddComponent<Image>().color = new Color(0.12f, 0.14f, 0.2f, 1f);

        GameObject textGo = NewUi("Question", box.transform);
        RectTransform textRt = (RectTransform)textGo.transform;
        textRt.anchorMin = new Vector2(0f, 0.45f);
        textRt.anchorMax = new Vector2(1f, 1f);
        textRt.offsetMin = new Vector2(40f, 0f);
        textRt.offsetMax = new Vector2(-40f, -30f);
        TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = "Reset all progress?\n<size=70%>Unlocked levels and best scores will be deleted. Settings stay.</size>";
        text.fontSize = 52f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        Button yes = CopyButton(styleSource, box.transform, "Btn_ResetYes", "Yes, reset");
        PlaceBottom((RectTransform)yes.transform, -1f);
        UnityEventTools.AddVoidPersistentListener(yes.onClick, reset.ConfirmReset);

        Button no = CopyButton(styleSource, box.transform, "Btn_ResetNo", "No");
        PlaceBottom((RectTransform)no.transform, 1f);
        UnityEventTools.AddVoidPersistentListener(no.onClick, reset.Cancel);

        dim.SetActive(false);
        return dim;
    }

    static void PlaceBottom(RectTransform rt, float side)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(360f, 130f);
        rt.anchoredPosition = new Vector2(side * 210f, 40f);
    }

    static void WireSettings(Scene scene)
    {
        ProfileSettings settings = Object.FindAnyObjectByType<ProfileSettings>(FindObjectsInactive.Include);
        if (settings == null) { Debug.LogWarning("No ProfileSettings in main menu"); return; }
        Transform panel = FindByName<Transform>(scene, "SettingsPanel");
        Slider slider = panel != null ? panel.GetComponentInChildren<Slider>(true) : null;
        SerializedObject so = new SerializedObject(settings);
        so.FindProperty("volumeSlider").objectReferenceValue = slider;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetBuildScenes()
    {
        string[] shipping = { MENU_SCENE, LEVEL_SCENE, WIN_SCENE };
        var list = shipping.Select(p => new EditorBuildSettingsScene(p, true)).ToList();
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
            if (!shipping.Contains(s.path)) list.Add(new EditorBuildSettingsScene(s.path, false));
        EditorBuildSettings.scenes = list.ToArray();
    }

    static Button CopyButton(Button source, Transform parent, string name, string label)
    {
        GameObject copy = Object.Instantiate(source.gameObject, parent);
        copy.name = name;
        Button b = copy.GetComponent<Button>();
        b.onClick = new Button.ButtonClickedEvent();
        TextMeshProUGUI t = copy.GetComponentInChildren<TextMeshProUGUI>(true);
        if (t != null) t.text = label;
        return b;
    }

    static void RemovePersistentListeners(UnityEventBase evt)
    {
        for (int i = evt.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(evt, i);
    }

    static GameObject NewUi(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");
        return go;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static T FindByName<T>(Scene scene, string name) where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (T c in root.GetComponentsInChildren<T>(true))
                if (c.name == name) return c;
        return null;
    }

    static void DestroyNamed(Scene scene, string name)
    {
        Transform t = FindByName<Transform>(scene, name);
        while (t != null)
        {
            Object.DestroyImmediate(t.gameObject);
            t = FindByName<Transform>(scene, name);
        }
    }
}
