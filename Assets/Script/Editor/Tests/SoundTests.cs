using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;

public class SoundTests
{
    const int MIN_SOUND_EVENTS = 8;
    static readonly SoundEvent[] REPEATED = { SoundEvent.Jump, SoundEvent.Shoot, SoundEvent.Hit, SoundEvent.Pickup, SoundEvent.UiClick };

    string path;

    [SetUp]
    public void SetUp()
    {
        path = Path.Combine(Path.GetTempPath(), "coinquest_sound_test_save.json");
        Cleanup();
        SaveSystem.PathOverrideForTests = path;
        SaveSystem.ForgetCachedData();
    }

    [TearDown]
    public void TearDown()
    {
        SaveSystem.PathOverrideForTests = null;
        SaveSystem.ForgetCachedData();
        Cleanup();
    }

    void Cleanup()
    {
        foreach (string p in new[] { path, path + ".broken", path + ".tmp" })
            if (File.Exists(p)) File.Delete(p);
    }

    [Test]
    public void FullVolume_IsZeroDecibels()
    {
        Assert.AreEqual(0f, VolumeMath.ToDecibels(1f), 0.001f);
    }

    [Test]
    public void HalfVolume_IsAboutMinusSixDecibels_NotHalfOfTheRange()
    {
        Assert.AreEqual(-6.02f, VolumeMath.ToDecibels(0.5f), 0.01f);
    }

    [Test]
    public void ZeroOrNegative_IsMixerOff()
    {
        Assert.AreEqual(VolumeMath.MIN_DB, VolumeMath.ToDecibels(0f));
        Assert.AreEqual(VolumeMath.MIN_DB, VolumeMath.ToDecibels(-1f));
    }

    [Test]
    public void AboveOne_IsClampedToZeroDecibels()
    {
        Assert.AreEqual(0f, VolumeMath.ToDecibels(3f), 0.001f);
    }

    [Test]
    public void SfxAndMusicVolume_SurviveRestart()
    {
        SaveSystem.SetMusicVolume(0.3f);
        SaveSystem.SetSfxVolume(0.25f);

        SaveSystem.ForgetCachedData();
        SaveData reloaded = SaveSystem.Data;

        Assert.AreEqual(0.3f, reloaded.musicVol, 0.0001f);
        Assert.AreEqual(0.25f, reloaded.sfxVol, 0.0001f);
    }

    [Test]
    public void OldSaveWithoutSfxVolume_GetsDefault()
    {
        File.WriteAllText(path, "{ \"version\": 1, \"musicVol\": 0.4, \"musicOn\": true }");

        SaveData d = SaveSystem.Data;

        Assert.AreEqual(GameConfig.Instance.defaults.sfxVolume, d.sfxVol, 0.0001f);
        Assert.AreEqual(0.4f, d.musicVol, 0.0001f);
    }

    [Test]
    public void ResetProgress_KeepsVolumeSettings()
    {
        SaveSystem.SetSfxVolume(0.2f);

        SaveSystem.ResetProgress();

        Assert.AreEqual(0.2f, SaveSystem.Data.sfxVol, 0.0001f);
    }

    static SoundLibrary Library()
    {
        SoundLibrary lib = Resources.Load<SoundLibrary>("Audio/SoundLibrary");
        Assert.IsNotNull(lib, "Run Tools > Assignment 12 > Build Sound Pass");
        return lib;
    }

    [Test]
    public void EverySoundEvent_HasAClip()
    {
        SoundLibrary lib = Library();
        foreach (SoundEvent id in Enum.GetValues(typeof(SoundEvent)))
        {
            SoundEntry e = lib.Get(id);
            Assert.IsNotNull(e, "No entry for " + id);
            Assert.IsTrue(e.clips.Length > 0 && e.clips.All(c => c != null), "No clip for " + id);
        }
        Assert.GreaterOrEqual(Enum.GetValues(typeof(SoundEvent)).Length, MIN_SOUND_EVENTS);
    }

    [Test]
    public void RepeatedSounds_HaveVariation()
    {
        SoundLibrary lib = Library();
        foreach (SoundEvent id in REPEATED)
        {
            SoundEntry e = lib.Get(id);
            Assert.IsTrue(e.randomPitch || e.clips.Length >= 2, id + " would sound robotic");
        }
        Assert.That(lib.pitchRange.x, Is.InRange(0.9f, 1f));
        Assert.That(lib.pitchRange.y, Is.InRange(1f, 1.1f));
    }

    [Test]
    public void Mixer_HasGroupsSnapshotsAndExposedVolumes()
    {
        SoundLibrary lib = Library();
        AudioMixer mixer = lib.mixer;
        Assert.IsNotNull(mixer);
        Assert.AreEqual("Music", lib.musicGroup.name);
        Assert.AreEqual("SFX", lib.sfxGroup.name);
        Assert.IsNotNull(mixer.FindSnapshot(lib.gameplaySnapshot));
        Assert.IsNotNull(mixer.FindSnapshot(lib.pausedSnapshot));
        Assert.IsTrue(mixer.GetFloat(lib.musicVolumeParam, out _), "MusicVolume not exposed");
        Assert.IsTrue(mixer.GetFloat(lib.sfxVolumeParam, out _), "SFXVolume not exposed");
        Assert.AreEqual(AudioMixerUpdateMode.UnscaledTime, mixer.updateMode, "Pause uses timeScale 0");
    }

    [Test]
    public void TwoDifferentMusicTracks()
    {
        SoundLibrary lib = Library();
        Assert.IsNotNull(lib.menuMusic);
        Assert.IsNotNull(lib.levelMusic);
        Assert.AreNotSame(lib.menuMusic, lib.levelMusic);
        Assert.AreEqual(1f, lib.crossfadeTime, 0.5f);
    }
}
