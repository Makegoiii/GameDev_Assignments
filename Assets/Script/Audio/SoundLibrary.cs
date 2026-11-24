using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class SoundEntry
{
    public SoundEvent id;
    [Tooltip("2-3 clips = one is picked at random (never the same twice in a row)")]
    public AudioClip[] clips = new AudioClip[0];
    [Range(0f, 1f)] public float volume = 1f;
    public bool randomPitch = true;
    [Tooltip("0 = 2D (UI, player). >0 = heard from the enemy's position")]
    [Range(0f, 1f)] public float spatialBlend;
    [Tooltip("Minimum seconds between two plays, stops 10 hits in one frame from stacking")]
    [Min(0f)] public float cooldown;
}

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Coin Quest/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    const string RESOURCE_PATH = "Audio/SoundLibrary";

    [Header("Mixer")]
    public AudioMixer mixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    public string musicVolumeParam = "MusicVolume";
    public string sfxVolumeParam = "SFXVolume";
    public string gameplaySnapshot = "Gameplay";
    public string pausedSnapshot = "Paused";
    [Min(0f)] public float snapshotTransition = 0.3f;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip levelMusic;
    [Min(0.01f)] public float crossfadeTime = 1f;
    [Range(0f, 1f)] public float musicSourceVolume = 0.8f;

    [Header("Sound effects")]
    public Vector2 pitchRange = new Vector2(0.92f, 1.08f);
    [Min(1)] public int sfxVoices = 16;
    [Min(0.1f)] public float spatialMinDistance = 8f;
    [Min(1f)] public float spatialMaxDistance = 45f;
    public List<SoundEntry> sounds = new List<SoundEntry>();

    public SoundEntry Get(SoundEvent id) => sounds.Find(s => s.id == id);

    static SoundLibrary instance;
    static bool missingReported;

    public static SoundLibrary Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = Resources.Load<SoundLibrary>(RESOURCE_PATH);
            if (instance == null && !missingReported)
            {
                missingReported = true;
                Debug.LogWarning("SoundLibrary missing: run Tools > Assignment 12 > Build Sound Pass");
            }
            return instance;
        }
    }
}
