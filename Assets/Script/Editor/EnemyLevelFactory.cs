using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static class EnemyLevelFactory
{
    public const string GEN_FOLDER = "Assets/EnemyGen";
    public const string STATS_FOLDER = "Assets/Data/Enemies";
    const string PLAYER_PREFAB = "Assets/Prefabs/level/Sphere.prefab";
    const string HERO_CHILD = "Hero";

    static readonly Color PATROLLER_COLOR = new Color(0.78f, 0.66f, 0.46f);
    static readonly Color SHOOTER_COLOR = new Color(0.43f, 0.48f, 0.29f);
    static readonly Color RUSHER_COLOR = new Color(0.36f, 0.28f, 0.21f);

    const float MUZZLE_HEIGHT = 1f;
    const float MUZZLE_FORWARD = 0.5f;

    public static Material MakeMaterial(string name, Color color, bool unlit = false)
    {
        string path = GEN_FOLDER + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            string shader = unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit";
            mat = new Material(Shader.Find(shader));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    public static EnemyProjectile MakeProjectilePrefab()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "EnemyProjectile";
        go.transform.localScale = Vector3.one * 0.35f;
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial("ProjectileMat", Color.red);
        go.GetComponent<SphereCollider>().isTrigger = true;
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        go.AddComponent<EnemyProjectile>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, GEN_FOLDER + "/EnemyProjectile.prefab");
        Object.DestroyImmediate(go);
        return prefab.GetComponent<EnemyProjectile>();
    }

    public static GameObject MakePatroller(Vector3 pos, Transform[] route)
    {
        GameObject root = MakeBody("Patroller", pos, PATROLLER_COLOR);
        PatrollerEnemy fsm = root.AddComponent<PatrollerEnemy>();
        Set(fsm, so =>
        {
            SetArray(so.FindProperty("waypoints"), route);
            so.FindProperty("stats").objectReferenceValue = LoadStats("Patroller");
        });
        return root;
    }

    public static GameObject MakeShooter(Vector3 pos, EnemyProjectile projectile)
    {
        GameObject root = MakeBody("Shooter", pos, SHOOTER_COLOR);
        Transform muzzle = MakeMuzzle(root.transform);

        ShooterEnemy fsm = root.AddComponent<ShooterEnemy>();
        Set(fsm, so =>
        {
            so.FindProperty("stats").objectReferenceValue = LoadStats("Shooter");
            so.FindProperty("projectilePrefab").objectReferenceValue = projectile;
            so.FindProperty("muzzle").objectReferenceValue = muzzle;
        });
        return root;
    }

    public static GameObject MakeRusher(Vector3 pos, EnemyProjectile projectile)
    {
        GameObject root = MakeBody("Rusher", pos, RUSHER_COLOR);
        Transform muzzle = MakeMuzzle(root.transform);
        RusherEnemy fsm = root.AddComponent<RusherEnemy>();
        Set(fsm, so =>
        {
            so.FindProperty("stats").objectReferenceValue = LoadStats("Rusher");
            so.FindProperty("projectilePrefab").objectReferenceValue = projectile;
            so.FindProperty("muzzle").objectReferenceValue = muzzle;
        });
        return root;
    }

    static Transform MakeMuzzle(Transform root)
    {
        Transform muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(root, false);
        muzzle.localPosition = new Vector3(0f, MUZZLE_HEIGHT, MUZZLE_FORWARD);
        return muzzle;
    }

    public static EnemyStats LoadStats(string enemyName)
    {
        string path = STATS_FOLDER + "/EnemyStats_" + enemyName + ".asset";
        EnemyStats stats = AssetDatabase.LoadAssetAtPath<EnemyStats>(path);
        if (stats == null) Debug.LogError("Missing enemy data asset: " + path);
        return stats;
    }

    static GameObject MakeBody(string name, Vector3 pos, Color color)
    {
        EnemyStats stats = LoadStats(name);
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB);
        Transform hero = playerPrefab.transform.Find(HERO_CHILD);
        float scale = playerPrefab.transform.localScale.x;
        CapsuleCollider playerCol = playerPrefab.GetComponent<CapsuleCollider>();

        GameObject root = new GameObject(name);
        root.transform.position = pos;

        GameObject body = Object.Instantiate(hero.gameObject, root.transform, false);
        body.name = "Body";
        body.transform.localPosition = hero.localPosition * scale;
        body.transform.localRotation = hero.localRotation;
        body.transform.localScale = hero.localScale * scale;
        foreach (Collider c in body.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);

        Animator anim = body.GetComponentInChildren<Animator>();
        if (anim != null) anim.applyRootMotion = false;
        root.AddComponent<EnemyAnimator>();

        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.center = playerCol.center * scale;
        col.height = playerCol.height * scale;
        col.radius = playerCol.radius * scale * 1.5f;

        NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
        if (stats != null) agent.speed = stats.moveSpeed;
        agent.angularSpeed = 360f;
        agent.acceleration = 20f;
        agent.radius = col.radius;
        agent.height = col.height;

        EnemySight sight = root.AddComponent<EnemySight>();
        Set(sight, so => so.FindProperty("stats").objectReferenceValue = stats);

        Renderer[] renderers = body.GetComponentsInChildren<Renderer>(true);
        Telegraph tel = root.AddComponent<Telegraph>();
        Set(tel, so =>
        {
            SetArray(so.FindProperty("bodies"), renderers);
            so.FindProperty("normalColor").colorValue = color;
        });

        EnemyHealth health = root.AddComponent<EnemyHealth>();
        Set(health, so => so.FindProperty("stats").objectReferenceValue = stats);
        return root;
    }

    static void Set(Object target, System.Action<SerializedObject> edit)
    {
        SerializedObject so = new SerializedObject(target);
        edit(so);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetArray(SerializedProperty prop, Object[] values)
    {
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }
}
