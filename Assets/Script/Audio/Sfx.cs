using UnityEngine;

public static class Sfx
{
    public static void Play(SoundEvent id)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(id, null);
    }

    public static void PlayAt(SoundEvent id, Vector3 position)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(id, position);
    }
}
