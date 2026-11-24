using UnityEngine;

public static class VolumeMath
{
    public const float MIN_DB = -80f;
    public const float MIN_LINEAR = 0.0001f;

    public static float ToDecibels(float linear)
    {
        float v = Mathf.Clamp01(linear);
        return v <= MIN_LINEAR ? MIN_DB : Mathf.Log10(v) * 20f;
    }
}
