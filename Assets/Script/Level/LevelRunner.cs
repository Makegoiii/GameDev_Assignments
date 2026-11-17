using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HazardRain))]
public class LevelRunner : MonoBehaviour
{
    const float NAVMESH_SNAP_RADIUS = 3f;

    [Header("Scene links (set by the builder)")]
    [SerializeField] GameObject exitObject;
    [SerializeField] TextMeshProUGUI counterText;
    [SerializeField] TextMeshProUGUI messageText;

    [Header("Text")]
    [SerializeField] string titleFormat = "Level {0}: {1}";
    [SerializeField] string counterFormat = "Wave {0}/{1}   Enemies: {2}";
    [SerializeField] string exitOpenText = "Exit open!";
    [SerializeField] string waveText = "Wave {0}!";
    [SerializeField] string resultFormat = "Level complete!\nScore {0}{1}";
    [SerializeField] string newBestText = "   NEW BEST!";

    public static LevelRunner Active { get; private set; }
    public LevelData Level { get; private set; }

    GameConfig config;
    Movement player;
    Transform enemyRoot;
    int waveIndex;
    int alive;
    int kills;
    bool finished;

    void Awake()
    {
        Active = this;
        config = GameConfig.Instance;
        Level = config.GetLevel(LevelFlow.CurrentLevel);
        if (Level == null)
        {
            Debug.LogError("LevelRunner: no LevelData for level " + LevelFlow.CurrentLevel);
            enabled = false;
            return;
        }

        player = FindAnyObjectByType<Movement>();
        enemyRoot = new GameObject("Enemies").transform;
        MapBuilder.ApplyTheme(Level.theme);
        MapBuilder.BuildProps(Level, config);
        PlacePlayer();
        SpawnCoins();
        if (exitObject != null) exitObject.SetActive(false);
        SpawnWave(0);
    }

    void Start()
    {
        if (Level == null) return;
        GetComponent<HazardRain>().Begin(Level.hazards, config);
        ShowMessage(string.Format(titleFormat, Level.levelNumber, Level.title));
    }

    void OnEnable() { EnemyHealth.Died += OnEnemyDied; }

    void OnDisable() { EnemyHealth.Died -= OnEnemyDied; }

    void OnDestroy()
    {
        if (Active == this) Active = null;
    }

    void PlacePlayer()
    {
        if (player == null) { Debug.LogError("LevelRunner: no player in scene"); return; }
        player.transform.position = Level.playerStart;
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.position = Level.playerStart;
    }

    void SpawnCoins()
    {
        if (config.coinPrefab == null) { Debug.LogWarning("LevelRunner: no coin prefab"); return; }
        Transform root = new GameObject("Coins").transform;
        foreach (Vector3 p in Level.coins)
            Instantiate(config.coinPrefab, p, Quaternion.identity, root);
    }

    void SpawnWave(int index)
    {
        waveIndex = index;
        alive = 0;
        foreach (EnemySpawn s in Level.waves[index].spawns)
            if (SpawnEnemy(s)) alive++;
        UpdateCounter();
        if (alive == 0) OnWaveCleared();
    }

    bool SpawnEnemy(EnemySpawn s)
    {
        GameObject prefab = config.GetEnemyPrefab(s.type);
        if (prefab == null) { Debug.LogError("LevelRunner: no prefab for " + s.type); return false; }

        Vector3 pos = s.position;
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, NAVMESH_SNAP_RADIUS, NavMesh.AllAreas))
            pos = hit.position;
        else
            Debug.LogWarning("LevelRunner: " + s.type + " at " + s.position + " is off the NavMesh");

        GameObject enemy = Instantiate(prefab, pos, Quaternion.Euler(0f, s.yaw, 0f), enemyRoot);
        PatrollerEnemy patroller = enemy.GetComponent<PatrollerEnemy>();
        if (patroller != null) patroller.SetRoute(s.route);
        return true;
    }

    void OnEnemyDied(EnemyHealth enemy)
    {
        if (finished) return;
        kills++;
        alive = Mathf.Max(0, alive - 1);
        UpdateCounter();
        if (alive == 0) OnWaveCleared();
    }

    void OnWaveCleared()
    {
        int next = waveIndex + 1;
        if (next < Level.waves.Count)
        {
            StartCoroutine(NextWaveAfterDelay(next));
            return;
        }
        if (exitObject != null) exitObject.SetActive(true);
        UpdateCounter();
    }

    IEnumerator NextWaveAfterDelay(int next)
    {
        ShowMessage(string.Format(waveText, next + 1));
        yield return new WaitForSeconds(Level.waveDelay);
        SpawnWave(next);
    }

    void UpdateCounter()
    {
        if (counterText == null) return;
        bool allCleared = alive == 0 && waveIndex >= Level.waves.Count - 1;
        counterText.text = allCleared
            ? exitOpenText
            : string.Format(counterFormat, waveIndex + 1, Level.waves.Count, alive);
    }

    public void OnExitReached()
    {
        if (finished) return;
        finished = true;
        GetComponent<HazardRain>().Stop();

        int score = CalculateScore();
        bool isNewBest = SaveSystem.RecordLevelComplete(Level.levelNumber, score, Level.levelNumber + 1);
        string result = string.Format(resultFormat, score, isNewBest ? newBestText : "");
        if (player != null) player.ShowResult(result);
        else ShowMessage(result);
        StartCoroutine(GoNextAfterDelay());
    }

    int CalculateScore()
    {
        ItemValues items = config.items;
        int coins = player != null ? player.CoinsCollected : 0;
        int secondsLeft = player != null ? Mathf.FloorToInt(player.TimeLeft) : 0;
        return coins * items.coinScore
             + kills * items.enemyScore
             + secondsLeft * items.timeBonusPerSecond
             + Level.clearBonus;
    }

    IEnumerator GoNextAfterDelay()
    {
        yield return new WaitForSecondsRealtime(config.resultScreenTime);
        LevelFlow.GoToNextAfter(Level.levelNumber);
    }

    void ShowMessage(string text)
    {
        if (player != null) player.ShowMessage(text);
        else if (messageText != null) messageText.text = text;
    }
}
