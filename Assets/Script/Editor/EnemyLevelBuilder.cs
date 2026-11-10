using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class EnemyLevelBuilder
{
    const string SOURCE_SCENE = "Assets/Scenes/Level3.unity";
    const string TARGET_SCENE = "Assets/Scenes/Level3_Enemies.unity";
    const string NAVMESH_ASSET = EnemyLevelFactory.GEN_FOLDER + "/Level3_Enemies_NavMesh.asset";
    const float WALL_HEIGHT = 2.5f;
    const float LEVEL_TIME_LIMIT = 180f;
    static readonly Vector3 PLAYER_START = new Vector3(0f, 0.1f, -9f);

    [MenuItem("Tools/Assignment 10/Build Enemy Level")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!AssetDatabase.IsValidFolder(EnemyLevelFactory.GEN_FOLDER))
            AssetDatabase.CreateFolder("Assets", "EnemyGen");

        AssetDatabase.DeleteAsset(TARGET_SCENE);
        if (!AssetDatabase.CopyAsset(SOURCE_SCENE, TARGET_SCENE))
        {
            Debug.LogError("Could not copy " + SOURCE_SCENE);
            return;
        }
        Scene scene = EditorSceneManager.OpenScene(TARGET_SCENE, OpenSceneMode.Single);

        RemoveBossStuff(scene);
        Transform level = new GameObject("EnemyLevel").transform;
        BuildCover(level);
        Transform route = BuildRoute(level);
        Movement player = SetupPlayer();
        SetupFollowCamera(player);
        GameObject exit = BuildExit(level);

        BakeNavMesh(level, player);
        PlaceEnemies(level, route);
        BuildGoal(level, exit);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddToBuildSettings();
        Debug.Log("Enemy level built: " + TARGET_SCENE);
    }

    static void RemoveBossStuff(Scene scene)
    {
        var doomed = new List<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                string n = t.name;
                bool isBoss = t.GetComponent<Boss>() != null || n.StartsWith("Boss");
                bool isOldEnemy = t.GetComponent<PatrolEnemy>() != null || n.StartsWith("enemy");
                bool isSpawner = t.GetComponent<Spawner>() != null || t.GetComponent<SpawnerRamp>() != null;
                if (isBoss || isOldEnemy || isSpawner) doomed.Add(t.gameObject);
            }
        foreach (GameObject go in doomed)
            if (go != null) Object.DestroyImmediate(go);
    }

    static void BuildCover(Transform parent)
    {
        Material mat = EnemyLevelFactory.MakeMaterial("CoverMat", new Color(0.35f, 0.38f, 0.45f));
        Transform cover = new GameObject("Cover").transform;
        cover.SetParent(parent);
        MakeWall(cover, new Vector3(-4.5f, 0, -2f), new Vector3(4f, WALL_HEIGHT, 0.6f), mat);
        MakeWall(cover, new Vector3(4.5f, 0, -2f), new Vector3(4f, WALL_HEIGHT, 0.6f), mat);
        MakeWall(cover, new Vector3(0f, 0, 4.5f), new Vector3(5f, WALL_HEIGHT, 0.6f), mat);
        MakeWall(cover, new Vector3(-7.5f, 0, 6f), new Vector3(0.6f, WALL_HEIGHT, 4f), mat);
        MakeWall(cover, new Vector3(7.5f, 0, 6f), new Vector3(0.6f, WALL_HEIGHT, 4f), mat);
    }

    static void MakeWall(Transform parent, Vector3 groundPos, Vector3 size, Material mat)
    {
        GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w.name = "CoverWall";
        w.transform.SetParent(parent);
        w.transform.position = groundPos + Vector3.up * size.y * 0.5f;
        w.transform.localScale = size;
        w.GetComponent<Renderer>().sharedMaterial = mat;
        GameObjectUtility.SetStaticEditorFlags(w, StaticEditorFlags.BatchingStatic);
    }

    static Transform BuildRoute(Transform parent)
    {
        Transform route = new GameObject("PatrolRoute").transform;
        route.SetParent(parent);
        Vector3[] points = { new Vector3(-9f, 0, 1f), new Vector3(9f, 0, 1f), new Vector3(9f, 0, -6f), new Vector3(-9f, 0, -6f) };
        for (int i = 0; i < points.Length; i++)
        {
            Transform p = new GameObject("Waypoint " + i).transform;
            p.SetParent(route);
            p.position = points[i];
        }
        return route;
    }

    static Movement SetupPlayer()
    {
        Movement player = Object.FindAnyObjectByType<Movement>(FindObjectsInactive.Include);
        if (player == null) { Debug.LogError("No player (Movement) in Level3"); return null; }
        player.transform.position = PLAYER_START;
        player.transform.rotation = Quaternion.identity;
        SerializedObject so = new SerializedObject(player);
        so.FindProperty("timeLimit").floatValue = LEVEL_TIME_LIMIT;
        so.ApplyModifiedPropertiesWithoutUndo();
        return player;
    }

    static void SetupFollowCamera(Movement player)
    {
        CameraFollow follow = Object.FindAnyObjectByType<CameraFollow>(FindObjectsInactive.Include);
        if (follow == null) { Debug.LogError("No CameraFollow camera in the scene"); return; }

        foreach (Camera cam in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include))
            if (cam.GetComponent<CameraFollow>() == null) Object.DestroyImmediate(cam.gameObject);

        Camera followCam = follow.GetComponent<Camera>();
        follow.gameObject.SetActive(true);
        follow.gameObject.tag = "MainCamera";
        if (follow.GetComponent<AudioListener>() == null) follow.gameObject.AddComponent<AudioListener>();
        foreach (Canvas c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
            if (c.renderMode != RenderMode.ScreenSpaceOverlay) c.worldCamera = followCam;

        SerializedObject so = new SerializedObject(follow);
        so.FindProperty("target").objectReferenceValue = player != null ? player.transform : null;
        so.ApplyModifiedPropertiesWithoutUndo();

        if (player == null) return;
        Vector3 behind = player.transform.position - Vector3.forward * follow.distance + Vector3.up * follow.height;
        follow.transform.SetPositionAndRotation(behind, Quaternion.LookRotation(player.transform.position - behind));
    }

    static GameObject BuildExit(Transform parent)
    {
        GameObject exit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exit.name = "Exit";
        exit.tag = "Exit";
        exit.transform.SetParent(parent);
        exit.transform.position = new Vector3(0f, 1.25f, 10f);
        exit.transform.localScale = new Vector3(2.5f, 2.5f, 0.5f);
        exit.GetComponent<BoxCollider>().isTrigger = true;
        exit.GetComponent<Renderer>().sharedMaterial =
            EnemyLevelFactory.MakeMaterial("ExitMat", new Color(0.2f, 1f, 0.6f));
        return exit;
    }

    static void BakeNavMesh(Transform parent, Movement player)
    {
        var hidden = Object.FindObjectsByType<Collider>()
            .Where(c => c.isTrigger || (player != null && c.transform.IsChildOf(player.transform)))
            .Select(c => c.gameObject).Distinct().Where(g => g.activeSelf).ToList();
        hidden.ForEach(g => g.SetActive(false));

        NavMeshSurface surface = new GameObject("NavMesh").AddComponent<NavMeshSurface>();
        surface.transform.SetParent(parent);
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();

        AssetDatabase.DeleteAsset(NAVMESH_ASSET);
        AssetDatabase.CreateAsset(surface.navMeshData, NAVMESH_ASSET);
        hidden.ForEach(g => g.SetActive(true));
    }

    static void PlaceEnemies(Transform parent, Transform route)
    {
        EnemyProjectile projectile = EnemyLevelFactory.MakeProjectilePrefab();
        Transform[] points = route.Cast<Transform>().ToArray();

        GameObject patroller = EnemyLevelFactory.MakePatroller(new Vector3(-9f, 0, 1f), points);
        GameObject shooter = EnemyLevelFactory.MakeShooter(new Vector3(0f, 0, 8.5f), projectile);
        GameObject charger = EnemyLevelFactory.MakeCharger(new Vector3(8f, 0, 8f));
        shooter.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        charger.transform.rotation = Quaternion.Euler(0f, 225f, 0f);

        Transform enemies = new GameObject("Enemies").transform;
        enemies.SetParent(parent);
        foreach (GameObject e in new[] { patroller, shooter, charger })
        {
            e.transform.SetParent(enemies);
            if (NavMesh.SamplePosition(e.transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                e.transform.position = hit.position;
            else
                Debug.LogWarning(e.name + " is not on the NavMesh");
        }
    }

    static void BuildGoal(Transform parent, GameObject exit)
    {
        EnemyArenaGoal goal = new GameObject("ArenaGoal").AddComponent<EnemyArenaGoal>();
        goal.transform.SetParent(parent);

        SerializedObject so = new SerializedObject(goal);
        so.FindProperty("exitObject").objectReferenceValue = exit;
        so.FindProperty("counterText").objectReferenceValue = MakeCounterText();
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static TextMeshProUGUI MakeCounterText()
    {
        GameHUD hud = Object.FindAnyObjectByType<GameHUD>(FindObjectsInactive.Include);
        Canvas canvas = hud != null ? hud.GetComponentInParent<Canvas>() : Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogWarning("No HUD canvas, enemy counter skipped"); return null; }

        SafeArea safe = canvas.GetComponentInChildren<SafeArea>(true);
        Transform holder = safe != null ? safe.transform : canvas.transform;

        GameObject go = new GameObject("EnemyCounter", typeof(RectTransform));
        go.transform.SetParent(holder, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -30f);
        rt.sizeDelta = new Vector2(600f, 80f);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = 48f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.text = "Enemies: 3";
        return text;
    }

    static void AddToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
            .Where(s => s.path != TARGET_SCENE).ToList();
        int bossIndex = scenes.FindIndex(s => s.path == SOURCE_SCENE);
        var entry = new EditorBuildSettingsScene(TARGET_SCENE, true);
        if (bossIndex < 0) scenes.Add(entry);
        else scenes.Insert(bossIndex, entry);
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
