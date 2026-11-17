using System.Collections;
using UnityEngine;

public class SpawnerRamp : MonoBehaviour
{
    public Hazard[] prefabs;

    IEnumerator Start()
    {
        HazardRainSettings rain = GameConfig.Instance.legacyRain;
        yield return new WaitForSeconds(rain.startDelay);
        float interval = rain.interval;
        float nextRamp = Time.time + rain.rampEvery;

        while (true)
        {
            Hazard pick = prefabs[Random.Range(0, prefabs.Length)];
            Vector3 pos = new Vector3(
                transform.position.x + Random.Range(-rain.rangeX, rain.rangeX),
                rain.height,
                transform.position.z + Random.Range(-rain.rangeZ, rain.rangeZ));
            Instantiate(pick, pos, Quaternion.identity);

            if (Time.time >= nextRamp)
            {
                interval = Mathf.Max(rain.minInterval, interval * rain.rampFactor);
                nextRamp = Time.time + rain.rampEvery;
            }
            yield return new WaitForSeconds(interval);
        }
    }
}
