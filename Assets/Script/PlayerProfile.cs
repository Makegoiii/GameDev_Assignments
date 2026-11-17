using TMPro;
using UnityEngine;

public class PlayerProfile : MonoBehaviour
{
    const string DEFAULT_NAME = "Player";

    public TextMeshProUGUI nameText;

    void Start()
    {
        if (nameText == null) return;
        string playerName = SaveSystem.Data.playerName;
        nameText.text = string.IsNullOrWhiteSpace(playerName) ? DEFAULT_NAME : playerName;
    }
}
