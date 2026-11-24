using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileSettings : MonoBehaviour
{
    public Toggle fullscreenToggle;
    public Toggle musicToggle;
    public Slider volumeSlider;
    public Slider sfxSlider;
    public TMP_InputField nameField;
    public TMP_Dropdown colorDropdown;

    void Start()
    {
        SaveData d = SaveSystem.Data;
        if (nameField != null) nameField.SetTextWithoutNotify(d.playerName);
        if (colorDropdown != null) colorDropdown.SetValueWithoutNotify(d.colorIndex);
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(d.fullscreen);
        if (musicToggle != null) musicToggle.SetIsOnWithoutNotify(d.musicOn);
        if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(d.musicVol);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(d.sfxVol);
    }

    public void SetFullscreen(bool isOn)
    {
        Screen.fullScreen = isOn;
        SaveSystem.SetFullscreen(isOn);
    }

    public void SetMusic(bool isOn) => SaveSystem.SetMusicOn(isOn);

    public void SetMusicVolume(float value) => SaveSystem.SetMusicVolume(value);

    public void SetSfxVolume(float value)
    {
        SaveSystem.SetSfxVolume(value);
        Sfx.Play(SoundEvent.Pickup);
    }

    public void SaveName(string value) => SaveSystem.SetPlayerName(value);

    public void SaveColor(int index) => SaveSystem.SetColorIndex(index);
}
