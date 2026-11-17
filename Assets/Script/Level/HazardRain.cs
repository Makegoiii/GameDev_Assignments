using System.Collections;
using UnityEngine;

public class HazardRain : MonoBehaviour
{
    HazardRainSettings settings;
    GameConfig config;

    public void Begin(HazardRainSettings rain, GameConfig cfg)
    {
        settings = rain;
        config = cfg;
        if (settings != null && settings.enabled && TotalWeight() > 0)
            StartCoroutine(RainLoop());
    }

    public void Stop() => StopAllCoroutines();

    IEnumerator RainLoop()
    {
        yield return new WaitForSeconds(settings.startDelay);
        float interval = settings.interval;
        float nextRamp = Time.time + settings.rampEvery;

        while (true)
        {
            Hazard prefab = PickPrefab();
            if (prefab != null) Instantiate(prefab, RandomPoint(), Quaternion.identity);

            if (Time.time >= nextRamp)
            {
                interval = Mathf.Max(settings.minInterval, interval * settings.rampFactor);
                nextRamp = Time.time + settings.rampEvery;
            }
            yield return new WaitForSeconds(interval);
        }
    }

    Vector3 RandomPoint()
    {
        return new Vector3(
            settings.center.x + Random.Range(-settings.rangeX, settings.rangeX),
            settings.height,
            settings.center.z + Random.Range(-settings.rangeZ, settings.rangeZ));
    }

    int TotalWeight() => settings.rockWeight + settings.bombWeight + settings.heartWeight;

    Hazard PickPrefab()
    {
        int roll = Random.Range(0, TotalWeight());
        if (roll < settings.rockWeight) return config.rockPrefab;
        roll -= settings.rockWeight;
        if (roll < settings.bombWeight) return config.bombPrefab;
        return config.heartPrefab;
    }
}
