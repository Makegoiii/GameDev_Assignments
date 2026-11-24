using System.Collections;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    SoundLibrary lib;
    AudioSource a;
    AudioSource b;
    AudioSource active;
    Coroutine fade;

    public AudioClip CurrentClip => active != null ? active.clip : null;

    public void Init(SoundLibrary library)
    {
        lib = library;
        a = CreateSource();
        b = CreateSource();
    }

    AudioSource CreateSource()
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f;
        src.volume = 0f;
        src.outputAudioMixerGroup = lib.musicGroup;
        return src;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (active != null && active.clip == clip && active.isPlaying) return;

        AudioSource from = active;
        AudioSource to = active == a ? b : a;
        bool alreadyPlaying = to.clip == clip && to.isPlaying;
        if (!alreadyPlaying)
        {
            to.clip = clip;
            to.volume = 0f;
            to.Play();
        }
        active = to;

        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Crossfade(from, to));
    }

    IEnumerator Crossfade(AudioSource from, AudioSource to)
    {
        float target = lib.musicSourceVolume;
        float fromStart = from != null ? from.volume : 0f;
        float toStart = to.volume;
        float t = 0f;
        while (t < lib.crossfadeTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / lib.crossfadeTime);
            to.volume = Mathf.Lerp(toStart, target, Mathf.Sin(k * Mathf.PI * 0.5f));
            if (from != null) from.volume = fromStart * Mathf.Cos(k * Mathf.PI * 0.5f);
            yield return null;
        }
        to.volume = target;
        if (from != null)
        {
            from.Stop();
            from.clip = null;
        }
        fade = null;
    }
}
