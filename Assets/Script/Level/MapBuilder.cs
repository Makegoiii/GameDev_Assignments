using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public static class MapBuilder
{
    public static void BuildProps(LevelData level, GameConfig cfg)
    {
        Transform root = new GameObject("Map").transform;
        foreach (PropPlacement p in level.props)
            BuildProp(p, cfg, root);
    }

    static void BuildProp(PropPlacement p, GameConfig cfg, Transform parent)
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
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        go.isStatic = true;

        NavMeshObstacle obstacle = go.AddComponent<NavMeshObstacle>();
        obstacle.shape = isCylinder ? NavMeshObstacleShape.Capsule : NavMeshObstacleShape.Box;
        obstacle.carving = true;
        obstacle.carveOnlyStationary = true;
    }

    public static void ApplyTheme(LevelTheme theme)
    {
        if (theme == null) return;
        foreach (Light l in Object.FindObjectsByType<Light>())
        {
            if (l.type != LightType.Directional) continue;
            l.color = theme.sunColor;
            l.intensity = theme.sunIntensity;
        }
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = theme.ambientColor;
        RenderSettings.fog = theme.fog;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = theme.fogColor;
        RenderSettings.fogDensity = theme.fogDensity;

        Camera cam = Camera.main;
        if (cam == null || !theme.fog) return;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = theme.fogColor;
    }
}
