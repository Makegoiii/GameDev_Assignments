using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static class EnemyLevelFactory
{
    public const string GEN_FOLDER = "Assets/EnemyGen";

    static readonly Color PATROLLER_COLOR = new Color(1f, 0.55f, 0.1f);
    static readonly Color SHOOTER_COLOR = new Color(0.55f, 0.3f, 0.9f);
    static readonly Color CHARGER_COLOR = new Color(0.2f, 0.7f, 0.3f);

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
        GameObject root = MakeBody("Patroller", pos, PrimitiveType.Capsule, PATROLLER_COLOR, 3, 3.6f);
        PatrollerEnemy fsm = root.AddComponent<PatrollerEnemy>();
        Set(fsm, so => SetArray(so.FindProperty("waypoints"), route));
        return root;
    }

    public static GameObject MakeShooter(Vector3 pos, EnemyProjectile projectile)
    {
        GameObject root = MakeBody("Shooter", pos, PrimitiveType.Cylinder, SHOOTER_COLOR, 2, 2.5f);

        GameObject gun = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gun.name = "Gun";
        Object.DestroyImmediate(gun.GetComponent<Collider>());
        gun.transform.SetParent(root.transform, false);
        gun.transform.localPosition = new Vector3(0f, 1.3f, 0.5f);
        gun.transform.localScale = new Vector3(0.25f, 0.25f, 0.8f);
        gun.GetComponent<Renderer>().sharedMaterial = MakeMaterial("GunMat", Color.black);

        Transform muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(root.transform, false);
        muzzle.localPosition = new Vector3(0f, 1.3f, 1f);

        LineRenderer laser = MakeLine(root.transform, "AimLaser", MakeMaterial("LaserMat", Color.red, true), 0.06f);

        ShooterEnemy fsm = root.AddComponent<ShooterEnemy>();
        Set(fsm, so =>
        {
            so.FindProperty("projectilePrefab").objectReferenceValue = projectile;
            so.FindProperty("muzzle").objectReferenceValue = muzzle;
            so.FindProperty("aimLaser").objectReferenceValue = laser;
        });
        return root;
    }

    public static GameObject MakeCharger(Vector3 pos)
    {
        GameObject root = MakeBody("Charger", pos, PrimitiveType.Cube, CHARGER_COLOR, 4, 1.8f);
        Transform body = root.transform.Find("Body");
        body.localScale = new Vector3(1.4f, 1.2f, 1.6f);
        body.localPosition = new Vector3(0f, 0.6f, 0f);

        for (int side = -1; side <= 1; side += 2)
        {
            GameObject horn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            horn.name = "Horn";
            Object.DestroyImmediate(horn.GetComponent<Collider>());
            horn.transform.SetParent(root.transform, false);
            horn.transform.localPosition = new Vector3(0.45f * side, 1.1f, 0.9f);
            horn.transform.localScale = new Vector3(0.15f, 0.15f, 0.6f);
            horn.GetComponent<Renderer>().sharedMaterial = MakeMaterial("HornWhite", Color.white);
        }
        root.AddComponent<ChargerEnemy>();
        return root;
    }

    static GameObject MakeBody(string name, Vector3 pos, PrimitiveType shape, Color color, int hp, float speed)
    {
        GameObject root = new GameObject(name);
        root.transform.position = pos;

        GameObject body = GameObject.CreatePrimitive(shape);
        body.name = "Body";
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = Vector3.up;
        Renderer bodyRenderer = body.GetComponent<Renderer>();
        bodyRenderer.sharedMaterial = MakeMaterial(name + "Mat", color);

        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.center = Vector3.up;
        col.height = 2f;
        col.radius = 0.5f;

        NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.angularSpeed = 360f;
        agent.acceleration = 20f;
        agent.radius = 0.5f;
        agent.height = 2f;

        root.AddComponent<AudioSource>().playOnAwake = false;

        EnemySight sight = root.AddComponent<EnemySight>();
        LineRenderer ray = MakeLine(root.transform, "VisionRay", MakeMaterial("VisionRayMat", Color.yellow, true), 0.04f);
        Set(sight, so => so.FindProperty("visionRay").objectReferenceValue = ray);

        Telegraph tel = root.AddComponent<Telegraph>();
        Set(tel, so =>
        {
            so.FindProperty("body").objectReferenceValue = bodyRenderer;
            so.FindProperty("normalColor").colorValue = color;
        });

        EnemyHealth health = root.AddComponent<EnemyHealth>();
        Set(health, so => so.FindProperty("maxHp").intValue = hp);
        return root;
    }

    static LineRenderer MakeLine(Transform parent, string name, Material mat, float width)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.sharedMaterial = mat;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.enabled = false;
        return lr;
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
