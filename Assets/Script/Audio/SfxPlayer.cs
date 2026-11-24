using System.Collections.Generic;
using UnityEngine;

public class SfxPlayer
{
    readonly SoundLibrary lib;
    readonly AudioSource[] voices;
    readonly Dictionary<SoundEvent, float> lastPlayTime = new Dictionary<SoundEvent, float>();
    readonly Dictionary<SoundEvent, int> lastClipIndex = new Dictionary<SoundEvent, int>();
    readonly HashSet<SoundEvent> reportedMissing = new HashSet<SoundEvent>();
    int nextVoice;

    public SfxPlayer(Transform parent, SoundLibrary lib)
    {
        this.lib = lib;
        voices = new AudioSource[lib.sfxVoices];
        for (int i = 0; i < voices.Length; i++)
            voices[i] = CreateVoice(parent, i);
    }

    AudioSource CreateVoice(Transform parent, int index)
    {
        GameObject go = new GameObject("SfxVoice " + index);
        go.transform.SetParent(parent, false);
        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.outputAudioMixerGroup = lib.sfxGroup;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = lib.spatialMinDistance;
        src.maxDistance = lib.spatialMaxDistance;
        return src;
    }

    public void Play(SoundEvent id, Vector3? position)
    {
        SoundEntry entry = lib.Get(id);
        if (entry == null || entry.clips == null || entry.clips.Length == 0)
        {
            if (reportedMissing.Add(id)) Debug.LogWarning("[Audio] No clip for " + id);
            return;
        }
        if (IsCoolingDown(id, entry.cooldown)) return;

        AudioSource src = NextVoice();
        bool is3D = position.HasValue && entry.spatialBlend > 0f;
        src.transform.position = is3D ? position.Value : Vector3.zero;
        src.spatialBlend = is3D ? entry.spatialBlend : 0f;
        src.clip = PickClip(id, entry.clips);
        src.volume = entry.volume;
        src.pitch = entry.randomPitch ? Random.Range(lib.pitchRange.x, lib.pitchRange.y) : 1f;
        src.Play();
    }

    bool IsCoolingDown(SoundEvent id, float cooldown)
    {
        float now = Time.unscaledTime;
        if (lastPlayTime.TryGetValue(id, out float last) && now - last < cooldown) return true;
        lastPlayTime[id] = now;
        return false;
    }

    AudioClip PickClip(SoundEvent id, AudioClip[] clips)
    {
        if (clips.Length == 1) return clips[0];
        int last = lastClipIndex.TryGetValue(id, out int l) ? l : -1;
        int pick;
        if (last < 0)
        {
            pick = Random.Range(0, clips.Length);
        }
        else
        {
            pick = Random.Range(0, clips.Length - 1);
            if (pick >= last) pick++;
        }
        lastClipIndex[id] = pick;
        return clips[pick];
    }

    AudioSource NextVoice()
    {
        for (int i = 0; i < voices.Length; i++)
        {
            AudioSource v = voices[(nextVoice + i) % voices.Length];
            if (!v.isPlaying) { nextVoice = (nextVoice + i + 1) % voices.Length; return v; }
        }
        AudioSource oldest = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;
        return oldest;
    }
}
