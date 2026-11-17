using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileSettings : MonoBehaviour
{
    public Toggle fullscreenToggle;
    public Toggle musicToggle;
    public Slider volumeSlider;
    public AudioSource bgSound;
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
        SaveSystem.ApplyMusic(bgSound);
    }

    public void SetFullscreen(bool isOn)
    {
        Screen.fullScreen = isOn;
        SaveSystem.SetFullscreen(isOn);
    }

    public void SetMusic(bool isOn)
    {
        SaveSystem.SetMusicOn(isOn);
        SaveSystem.ApplyMusic(bgSound);
    }

    public void SaveName(string value) => SaveSystem.SetPlayerName(value);

    public void SaveColor(int index) => SaveSystem.SetColorIndex(index);
}
