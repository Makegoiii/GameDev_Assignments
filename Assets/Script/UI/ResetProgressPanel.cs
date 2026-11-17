using UnityEngine;

public class ResetProgressPanel : MonoBehaviour
{
    [SerializeField] GameObject confirmPanel;

    void Start()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    public void AskReset()
    {
        if (confirmPanel != null) confirmPanel.SetActive(true);
    }

    public void ConfirmReset()
    {
        SaveSystem.ResetProgress();
        Cancel();
    }

    public void Cancel()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }
}
