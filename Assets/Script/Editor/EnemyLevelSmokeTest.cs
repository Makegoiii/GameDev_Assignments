using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class EnemyLevelSmokeTest
{
    const string SCENE = "Assets/Scenes/Level3_Enemies.unity";
    static readonly (float time, Vector3 pos, string label)[] STEPS =
    {
        (1f,  new Vector3(0f, 0.1f, -9f),  "start (behind cover)"),
        (4f,  new Vector3(4f, 0.1f, 1.5f), "in front of patroller"),
        (9f,  new Vector3(-3f, 0.1f, 7.5f), "in shooter view"),
        (13f, new Vector3(5f, 0.1f, 4f),   "in charger view"),
    };
    const float KILL_TIME = 18f;
    const float END_TIME = 19f;

    static float startTime;
    static int step;
    static bool killed;
    static bool exitWhenDone;
    static float nextLog;
    const float LOG_EVERY = 0.5f;

    [MenuItem("Tools/Assignment 10/Smoke Test")]
    public static void Run() => Begin(false);

    public static void RunBatch() => Begin(true);

    static void Begin(bool exit)
    {
        exitWhenDone = exit;
        EditorSceneManager.OpenScene(SCENE);
        step = 0;
        killed = false;
        nextLog = 0f;
        EditorApplication.playModeStateChanged += OnPlayMode;
        EditorApplication.EnterPlaymode();
    }

    static void OnPlayMode(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredPlayMode) return;
        startTime = Time.realtimeSinceStartup;
        EditorApplication.update += Tick;
    }

    static void Tick()
    {
        float t = Time.realtimeSinceStartup - startTime;
        Movement player = Object.FindAnyObjectByType<Movement>();
        if (player == null) { Finish("FAIL: player missing (scene reloaded = player died?)"); return; }

        if (step < STEPS.Length && t >= STEPS[step].time)
        {
            player.GetComponent<Rigidbody>().position = STEPS[step].pos;
            player.transform.position = STEPS[step].pos;
            Debug.Log($"[Smoke] t={t:F1} teleport: {STEPS[step].label}");
            step++;
        }
        if (t >= nextLog) { nextLog = t + LOG_EVERY; LogStates(t, player); }

        if (!killed && t >= KILL_TIME)
        {
            killed = true;
            foreach (EnemyHealth e in Object.FindObjectsByType<EnemyHealth>())
                e.TakeDamage(100);
        }
        if (t >= END_TIME)
        {
            Transform exit = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)
                .FirstOrDefault(x => x.name == "Exit");
            bool open = exit != null && exit.gameObject.activeInHierarchy;
            int left = Object.FindObjectsByType<EnemyHealth>().Length;
            Finish(open && left == 0 ? "PASS: all enemies dead, exit open" : $"FAIL: exit open={open}, enemies left={left}");
        }
    }

    static void LogStates(float t, Movement player)
    {
        string s = $"[Smoke] t={t:F1} hp={player.GetComponent<Health>()?.hp}";
        PatrollerEnemy p = Object.FindAnyObjectByType<PatrollerEnemy>();
        ShooterEnemy sh = Object.FindAnyObjectByType<ShooterEnemy>();
        ChargerEnemy c = Object.FindAnyObjectByType<ChargerEnemy>();
        if (p) s += $" Patroller={p.Current}";
        if (sh) s += $" Shooter={sh.Current}";
        if (c) s += $" Charger={c.Current}";
        Debug.Log(s);
    }

    static void Finish(string result)
    {
        Debug.Log("[Smoke] " + result);
        EditorApplication.update -= Tick;
        EditorApplication.playModeStateChanged -= OnPlayMode;
        EditorApplication.ExitPlaymode();
        if (exitWhenDone) EditorApplication.Exit(result.StartsWith("PASS") ? 0 : 1);
    }
}
