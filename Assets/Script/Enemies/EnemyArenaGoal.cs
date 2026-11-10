using TMPro;
using UnityEngine;

public class EnemyArenaGoal : MonoBehaviour
{
    [SerializeField] GameObject exitObject;
    [SerializeField] TextMeshProUGUI counterText;
    [SerializeField] string counterFormat = "Enemies: {0}";
    [SerializeField] string doneText = "Exit open!";

    int alive;

    void OnEnable()  { EnemyHealth.Died += OnEnemyDied; }
    void OnDisable() { EnemyHealth.Died -= OnEnemyDied; }

    void Start()
    {
        alive = FindObjectsByType<EnemyHealth>().Length;
        if (exitObject != null) exitObject.SetActive(alive == 0);
        ShowCounter();
    }

    void OnEnemyDied(EnemyHealth enemy)
    {
        alive = Mathf.Max(0, alive - 1);
        ShowCounter();
        if (alive == 0 && exitObject != null) exitObject.SetActive(true);
    }

    void ShowCounter()
    {
        if (counterText == null) return;
        counterText.text = alive > 0 ? string.Format(counterFormat, alive) : doneText;
    }
}
