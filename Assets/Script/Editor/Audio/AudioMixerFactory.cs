using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public static class AudioMixerFactory
{
    public const string MASTER_PARAM = "MasterVolume";
    public const string MUSIC_PARAM = "MusicVolume";
    public const string SFX_PARAM = "SFXVolume";
    public const string GAMEPLAY_SNAPSHOT = "Gameplay";
    public const string PAUSED_SNAPSHOT = "Paused";

    const string LOWPASS_EFFECT = "Lowpass Simple";
    const string LOWPASS_PARAM = "Cutoff freq";
    const float PAUSED_MUSIC_DB = -10f;
    const float PAUSED_CUTOFF_HZ = 900f;
    const float OPEN_CUTOFF_HZ = 22000f;

    const BindingFlags ANY = BindingFlags.Public | BindingFlags.NonPublic
                           | BindingFlags.Instance | BindingFlags.Static;

    public static AudioMixer Create(string path)
    {
        Type controllerType = FindType("UnityEditor.Audio.AudioMixerController");
        object mixer = Call(controllerType, null, "CreateMixerControllerAtPath", path);
        object master = Get(mixer, "masterGroup");

        object musicBus = CreateGroup(mixer, "MusicBus", master);
        object music = CreateGroup(mixer, "Music", musicBus);
        object sfx = CreateGroup(mixer, "SFX", master);

        Expose(mixer, (MASTER_PARAM, master), (MUSIC_PARAM, musicBus), (SFX_PARAM, sfx));

        object gameplay = ((Array)Get(mixer, "snapshots")).GetValue(0);
        ((UnityEngine.Object)gameplay).name = GAMEPLAY_SNAPSHOT;

        object lowpass = AddEffect(mixer, music, LOWPASS_EFFECT);
        GUID cutoff = (GUID)Call(lowpass.GetType(), lowpass, "GetGUIDForParameter", LOWPASS_PARAM);
        SetSnapshotValue(gameplay, cutoff, OPEN_CUTOFF_HZ);

        Call(mixer.GetType(), mixer, "CloneNewSnapshotFromTarget", false);
        object paused = ((Array)Get(mixer, "snapshots")).GetValue(1);
        ((UnityEngine.Object)paused).name = PAUSED_SNAPSHOT;
        SetSnapshotValue(paused, VolumeGuid(music), PAUSED_MUSIC_DB);
        SetSnapshotValue(paused, cutoff, PAUSED_CUTOFF_HZ);

        Set(mixer, "TargetSnapshot", gameplay);
        AudioMixer result = (AudioMixer)mixer;
        result.updateMode = AudioMixerUpdateMode.UnscaledTime;
        EditorUtility.SetDirty(result);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
    }

    static object CreateGroup(object mixer, string name, object parent)
    {
        object group = Call(mixer.GetType(), mixer, "CreateNewGroup", name, false);
        Call(mixer.GetType(), mixer, "AddChildToParent", group, parent);
        return group;
    }

    static void Expose(object mixer, params (string name, object group)[] items)
    {
        Type paramType = FindType("UnityEditor.Audio.ExposedAudioParameter");
        Array array = Array.CreateInstance(paramType, items.Length);
        for (int i = 0; i < items.Length; i++)
        {
            object p = Activator.CreateInstance(paramType);
            paramType.GetField("name", ANY).SetValue(p, items[i].name);
            paramType.GetField("guid", ANY).SetValue(p, VolumeGuid(items[i].group));
            array.SetValue(p, i);
        }
        Set(mixer, "exposedParameters", array);
        MethodInfo refresh = mixer.GetType().GetMethod("OnChangedExposedParameter", ANY);
        refresh?.Invoke(mixer, null);
    }

    static object AddEffect(object mixer, object group, string effectName)
    {
        Type defs = FindType("UnityEditor.Audio.MixerEffectDefinitions");
        Call(defs, null, "Refresh");
        Type effectType = FindType("UnityEditor.Audio.AudioMixerEffectController");
        object effect = Activator.CreateInstance(effectType, effectName);
        Call(mixer.GetType(), mixer, "AddNewSubAsset", effect, false);
        int count = ((Array)Get(group, "effects")).Length;
        Call(group.GetType(), group, "InsertEffect", effect, count);
        MethodInfo prealloc = effectType.GetMethod("PreallocateGUIDs", ANY);
        prealloc?.Invoke(effect, null);
        return effect;
    }

    static GUID VolumeGuid(object group) =>
        (GUID)Call(group.GetType(), group, "GetGUIDForVolume");

    static void SetSnapshotValue(object snapshot, GUID guid, float value) =>
        Call(snapshot.GetType(), snapshot, "SetValue", guid, value);

    static Type FindType(string fullName)
    {
        Type t = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(fullName))
            .FirstOrDefault(x => x != null);
        if (t == null) throw new MissingMemberException("Unity type not found: " + fullName);
        return t;
    }

    static object Call(Type type, object target, string method, params object[] args)
    {
        MethodInfo m = type.GetMethods(ANY).FirstOrDefault(x =>
            x.Name == method && x.GetParameters().Length == args.Length);
        if (m == null) throw new MissingMethodException(type.FullName, method);
        return m.Invoke(target, args);
    }

    static object Get(object target, string property)
    {
        PropertyInfo p = target.GetType().GetProperty(property, ANY);
        if (p == null) throw new MissingMemberException(target.GetType().FullName, property);
        return p.GetValue(target);
    }

    static void Set(object target, string property, object value)
    {
        PropertyInfo p = target.GetType().GetProperty(property, ANY);
        if (p == null) throw new MissingMemberException(target.GetType().FullName, property);
        p.SetValue(target, value);
    }
}
