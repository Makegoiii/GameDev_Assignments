using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Rendering;

public static class MapBuilder
{
    const string FLOOR_NAME = "Arena";
    const string WALLS_NAME = "ArenaWalls";
    const float FLOOR_THICKNESS = 0.5f;
    const float OUTER_WALL_HEIGHT = 2.6f;
    const float OUTER_WALL_THICKNESS = 0.6f;
    const float FLOOR_TILE_METERS = 4f;
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly Dictionary<(Material, Color), Material> tintedCache = new Dictionary<(Material, Color), Material>();

    const float TRUNK_WIDTH = 0.18f;
    const float TRUNK_HEIGHT = 0.35f;
    static readonly Color TRUNK_COLOR = new Color(0.35f, 0.24f, 0.15f);
    static readonly Color LEAVES_COLOR = new Color(0.18f, 0.38f, 0.22f);
    const float POLE_WIDTH = 0.12f;
    const float LAMP_HEAD = 0.45f;
    static readonly Color LAMP_POLE_COLOR = new Color(0.2f, 0.2f, 0.22f);
    static readonly Color LAMP_LIGHT_COLOR = new Color(1f, 0.75f, 0.4f);
    const float LAMP_RANGE = 9f;
    const float LAMP_INTENSITY = 3f;

    public static void BuildProps(LevelData level, GameConfig cfg)
    {
        Transform root = new GameObject("Map").transform;
        if (level.arenaHalfSize > 0f) BuildSquareArena(level, cfg, root);
        Color tint = level.theme != null ? level.theme.propColor : Color.white;
        foreach (PropPlacement p in level.props)
        {
            switch (p.type)
            {
                case PropType.Tree: BuildTree(p, root); break;
                case PropType.Lamp: BuildLamp(p, root); break;
                default: BuildBlock(p, cfg, root, tint); break;
            }
        }
    }

    static void BuildBlock(PropPlacement p, GameConfig cfg, Transform parent, Color tint)
    {
        PrimitiveType shape = p.type == PropType.Barrel ? PrimitiveType.Cylinder : PrimitiveType.Cube;
        GameObject go = GameObject.CreatePrimitive(shape);
        go.name = p.type.ToString();
        go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(
            p.position + Vector3.up * p.size.y * 0.5f,
            Quaternion.Euler(0f, p.yaw, 0f));

        bool isCylinder = shape == PrimitiveType.Cylinder;
        go.transform.localScale = isCylinder
            ? new Vector3(p.size.x, p.size.y * 0.5f, p.size.z)
            : p.size;

        Material mat = cfg.GetPropMaterial(p.type);
        Renderer r = go.GetComponent<Renderer>();
        if (mat != null) r.sharedMaterial = mat;
        Tint(r, tint);
        go.isStatic = true;
    }

    static void BuildSquareArena(LevelData level, GameConfig cfg, Transform parent)
    {
        LevelTheme theme = level.theme ?? new LevelTheme();
        float half = level.arenaHalfSize;
        GameObject oldFloor = GameObject.Find(FLOOR_NAME);
        Material floorMat = oldFloor != null ? oldFloor.GetComponent<Renderer>().sharedMaterial : null;
        if (oldFloor != null) oldFloor.SetActive(false);
        GameObject oldWalls = GameObject.Find(WALLS_NAME);
        if (oldWalls != null) oldWalls.SetActive(false);

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "SquareFloor";
        floor.transform.SetParent(parent, false);
        if (oldFloor != null) floor.layer = oldFloor.layer;
        floor.transform.position = Vector3.down * FLOOR_THICKNESS * 0.5f;
        floor.transform.localScale = new Vector3(half * 2f, FLOOR_THICKNESS, half * 2f);
        Renderer fr = floor.GetComponent<Renderer>();
        if (floorMat != null)
        {
            fr.sharedMaterial = new Material(floorMat) { name = "SquareFloor" };
            fr.sharedMaterial.mainTextureScale = Vector2.one * (half * 2f / FLOOR_TILE_METERS);
        }
        Tint(fr, theme.floorColor);
        floor.isStatic = true;

        float side = half * 2f + OUTER_WALL_THICKNESS;
        OuterWall(parent, cfg, theme, new Vector3(0f, 0f, half), new Vector3(side, OUTER_WALL_HEIGHT, OUTER_WALL_THICKNESS));
        OuterWall(parent, cfg, theme, new Vector3(0f, 0f, -half), new Vector3(side, OUTER_WALL_HEIGHT, OUTER_WALL_THICKNESS));
        OuterWall(parent, cfg, theme, new Vector3(half, 0f, 0f), new Vector3(OUTER_WALL_THICKNESS, OUTER_WALL_HEIGHT, side));
        OuterWall(parent, cfg, theme, new Vector3(-half, 0f, 0f), new Vector3(OUTER_WALL_THICKNESS, OUTER_WALL_HEIGHT, side));
    }

    static void OuterWall(Transform parent, GameConfig cfg, LevelTheme theme, Vector3 pos, Vector3 size)
    {
        PropPlacement p = new PropPlacement { type = PropType.Wall, position = pos, size = size };
        BuildBlock(p, cfg, parent, theme.wallColor);
    }

    public static void BakeNavMesh(GameObject player)
    {
        NavMeshSurface surface = Object.FindAnyObjectByType<NavMeshSurface>();
        if (surface == null) { Debug.LogError("MapBuilder: no NavMeshSurface in the scene"); return; }
        Collider[] playerColliders = player != null ? player.GetComponentsInChildren<Collider>() : new Collider[0];
        foreach (Collider c in playerColliders) c.enabled = false;
        surface.BuildNavMesh();
        foreach (Collider c in playerColliders) c.enabled = true;
    }

    static void BuildTree(PropPlacement p, Transform parent)
    {
        Transform root = new GameObject("Tree").transform;
        root.SetParent(parent, false);
        root.position = p.position;

        float trunkH = p.size.y * TRUNK_HEIGHT;
        Part(PrimitiveType.Cylinder, root, "Trunk", TRUNK_COLOR,
            new Vector3(0f, trunkH * 0.5f, 0f),
            new Vector3(p.size.x * TRUNK_WIDTH, trunkH * 0.5f, p.size.z * TRUNK_WIDTH));

        float crownH = p.size.y - trunkH;
        Part(PrimitiveType.Capsule, root, "Crown", LEAVES_COLOR,
            new Vector3(0f, trunkH + crownH * 0.5f, 0f),
            new Vector3(p.size.x, crownH * 0.5f, p.size.z));
    }

    static void BuildLamp(PropPlacement p, Transform parent)
    {
        Transform root = new GameObject("Lamp").transform;
        root.SetParent(parent, false);
        root.position = p.position;

        Part(PrimitiveType.Cylinder, root, "Pole", LAMP_POLE_COLOR,
            new Vector3(0f, p.size.y * 0.5f, 0f), new Vector3(POLE_WIDTH, p.size.y * 0.5f, POLE_WIDTH));

        GameObject head = Part(PrimitiveType.Sphere, root, "Bulb", LAMP_LIGHT_COLOR,
            new Vector3(0f, p.size.y, 0f), Vector3.one * LAMP_HEAD);
        Object.Destroy(head.GetComponent<Collider>());

        Light light = new GameObject("Light").AddComponent<Light>();
        light.transform.SetParent(root, false);
        light.transform.localPosition = new Vector3(0f, p.size.y - LAMP_HEAD, 0f);
        light.type = LightType.Point;
        light.color = LAMP_LIGHT_COLOR;
        light.range = LAMP_RANGE;
        light.intensity = LAMP_INTENSITY;
    }

    static GameObject Part(PrimitiveType shape, Transform parent, string name, Color color, Vector3 localPos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(shape);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        Tint(go.GetComponent<Renderer>(), color);
        go.isStatic = true;
        return go;
    }

    static void Tint(Renderer r, Color color)
    {
        if (r == null || r.sharedMaterial == null) return;
        Material src = r.sharedMaterial;
        if (!src.HasProperty(BaseColorId)) return;
        if (!tintedCache.TryGetValue((src, color), out Material tinted) || tinted == null)
        {
            tinted = new Material(src) { name = src.name + " (tinted)" };
            tinted.SetColor(BaseColorId, src.GetColor(BaseColorId) * color);
            tintedCache[(src, color)] = tinted;
        }
        r.sharedMaterial = tinted;
    }

    public static void ApplyTheme(LevelTheme theme)
    {
        if (theme == null) return;
        bool sunFound = false;
        foreach (Light l in Object.FindObjectsByType<Light>())
        {
            if (l.type != LightType.Directional) continue;
            if (sunFound) { l.enabled = false; continue; }
            sunFound = true;
            l.color = theme.sunColor;
            l.intensity = theme.sunIntensity;
        }
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = theme.ambientColor;
        RenderSettings.fog = theme.fog;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = theme.fogColor;
        RenderSettings.fogDensity = theme.fogDensity;
        TintArena(theme);

        Camera cam = Camera.main;
        if (cam == null || !theme.fog) return;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = theme.fogColor;
    }

    static void TintArena(LevelTheme theme)
    {
        GameObject floor = GameObject.Find(FLOOR_NAME);
        if (floor != null) Tint(floor.GetComponent<Renderer>(), theme.floorColor);

        GameObject walls = GameObject.Find(WALLS_NAME);
        if (walls == null) return;
        foreach (Renderer r in walls.GetComponentsInChildren<Renderer>(true))
            Tint(r, theme.wallColor);
    }
}
