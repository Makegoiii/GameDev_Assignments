using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameHUD : MonoBehaviour
{
    public Image[] hearts;
    public Color fullColor = Color.white;
    public Color emptyColor = new Color(1f, 1f, 1f, 0.25f);

    Health health;

    void Start()
    {
        health = FindAnyObjectByType<Health>();
    }

    void Update()
    {
        if (health == null || hearts == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            bool exists = i < health.MaxHp;
            hearts[i].gameObject.SetActive(exists);
            if (exists)
                hearts[i].color = i < health.Hp ? fullColor : emptyColor;
        }
    }

    public void ToMenu()
    {
        LevelFlow.ToMenu();
    }
}
