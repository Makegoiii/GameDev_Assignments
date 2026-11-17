using System.IO;
using UnityEditor;
using UnityEngine;

public static class MapArt
{
    const string FOLDER = EnemyLevelFactory.GEN_FOLDER + "/MapArt";
    const int SIZE = 256;
    const int SEED = 11;

    static readonly Color SAND = new Color(0.80f, 0.69f, 0.50f);
    static readonly Color STONE = new Color(0.74f, 0.63f, 0.47f);
    static readonly Color MORTAR = new Color(0.58f, 0.49f, 0.36f);
    static readonly Color WOOD = new Color(0.55f, 0.40f, 0.24f);
    static readonly Color WOOD_DARK = new Color(0.32f, 0.22f, 0.13f);
    static readonly Color BARREL = new Color(0.30f, 0.36f, 0.24f);
    static readonly Color BARREL_BAND = new Color(0.18f, 0.20f, 0.15f);
    static readonly Color SANDBAG = new Color(0.62f, 0.56f, 0.40f);

    public static Material Floor => Load("Sand");
    public static Material Wall => Load("Sandstone");

    public static void CreateAll(GameConfig config)
    {
        if (!AssetDatabase.IsValidFolder(FOLDER))
            AssetDatabase.CreateFolder(EnemyLevelFactory.GEN_FOLDER, "MapArt");
        Random.InitState(SEED);

        MakeMaterial("Sand", Noise(SAND, 0.08f), new Vector2(6f, 6f));
        Material wall = MakeMaterial("Sandstone", Bricks(), Vector2.one);
        Material crate = MakeMaterial("Crate", Crate(), Vector2.one);
        Material barrel = MakeMaterial("Barrel", Bands(BARREL, BARREL_BAND), Vector2.one);
        Material bags = MakeMaterial("Sandbags", Bags(), new Vector2(2f, 1f));

        SerializedObject so = new SerializedObject(config);
        so.FindProperty("crateMaterial").objectReferenceValue = crate;
        so.FindProperty("wallMaterial").objectReferenceValue = wall;
        so.FindProperty("barrelMaterial").objectReferenceValue = barrel;
        so.FindProperty("sandbagMaterial").objectReferenceValue = bags;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Material Load(string name) => AssetDatabase.LoadAssetAtPath<Material>(FOLDER + "/" + name + ".mat");

    static Material MakeMaterial(string name, Texture2D tex, Vector2 tiling)
    {
        string texPath = FOLDER + "/" + name + ".png";
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);
        Texture2D imported = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

        string matPath = FOLDER + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, matPath);
        }
        mat.SetTexture("_BaseMap", imported);
        mat.SetTextureScale("_BaseMap", tiling);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetFloat("_Smoothness", 0.1f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Texture2D NewTex() => new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, true);

    static Color Jitter(Color c, float amount)
    {
        float j = Random.Range(-amount, amount);
        return new Color(c.r + j, c.g + j, c.b + j, 1f);
    }

    static Texture2D Noise(Color baseColor, float amount)
    {
        Texture2D t = NewTex();
        for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) - 0.5f;
                Color c = Jitter(baseColor, amount * 0.5f);
                t.SetPixel(x, y, c + new Color(n, n, n) * amount);
            }
        t.Apply();
        return t;
    }

    static Texture2D Bricks()
    {
        const int rows = 8;
        const int cols = 4;
        const int mortar = 3;
        int h = SIZE / rows;
        int w = SIZE / cols;
        Texture2D t = NewTex();
        Color[] shades = new Color[rows * (cols + 1)];
        for (int i = 0; i < shades.Length; i++) shades[i] = Jitter(STONE, 0.05f);

        for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                int row = y / h;
                int shifted = x + (row % 2) * w / 2;
                int col = shifted / w;
                bool isMortar = y % h < mortar || shifted % w < mortar;
                Color c = isMortar ? MORTAR : shades[row * (cols + 1) + col % (cols + 1)];
                t.SetPixel(x, y, Jitter(c, 0.02f));
            }
        t.Apply();
        return t;
    }

    static Texture2D Crate()
    {
        const int border = 22;
        const int planks = 5;
        int plank = SIZE / planks;
        Texture2D t = NewTex();
        for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                bool edge = x < border || y < border || x >= SIZE - border || y >= SIZE - border;
                bool diagonal = Mathf.Abs(x - y) < border / 2;
                bool seam = y % plank < 2;
                Color c = edge || diagonal ? WOOD_DARK : WOOD;
                if (seam && !edge) c = WOOD_DARK;
                float grain = Mathf.PerlinNoise(x * 0.02f, y * 0.4f) * 0.08f;
                t.SetPixel(x, y, Jitter(c, 0.015f) + new Color(grain, grain, grain));
            }
        t.Apply();
        return t;
    }

    static Texture2D Bands(Color body, Color band)
    {
        const int bands = 3;
        const int bandHeight = 10;
        Texture2D t = NewTex();
        for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                bool isBand = (y + SIZE / (bands * 2)) % (SIZE / bands) < bandHeight;
                t.SetPixel(x, y, Jitter(isBand ? band : body, 0.02f));
            }
        t.Apply();
        return t;
    }

    static Texture2D Bags()
    {
        const int rows = 4;
        const int cols = 3;
        int h = SIZE / rows;
        int w = SIZE / cols;
        Texture2D t = NewTex();
        for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                int row = y / h;
                int shifted = x + (row % 2) * w / 2;
                float u = (shifted % w) / (float)w - 0.5f;
                float v = (y % h) / (float)h - 0.5f;
                float shade = 1f - (u * u + v * v) * 1.2f;
                Color c = SANDBAG * shade;
                c.a = 1f;
                t.SetPixel(x, y, Jitter(c, 0.02f));
            }
        t.Apply();
        return t;
    }
}
