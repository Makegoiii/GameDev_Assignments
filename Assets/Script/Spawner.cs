using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Hazard[] prefabs;

    HazardRainSettings Rain => GameConfig.Instance.legacyRain;

    void Start()
    {
        InvokeRepeating(nameof(Spawn), Rain.startDelay, Rain.interval);
    }

    void Spawn()
    {
        Hazard pick = prefabs[Random.Range(0, prefabs.Length)];

        Vector3 pos = new Vector3(
            transform.position.x + Random.Range(-Rain.rangeX, Rain.rangeX),
            Rain.height,
            transform.position.z + Random.Range(-Rain.rangeZ, Rain.rangeZ));

        Instantiate(pick, pos, Quaternion.identity);
    }
}
