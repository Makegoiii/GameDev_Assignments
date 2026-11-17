using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public int MaxHp => GameConfig.Instance.player.maxHp;
    public int Hp => hp;

    public AudioClip hurtClip;
    public AudioClip healClip;

    int hp;
    bool invincible;
    Renderer[] renderers;
    AudioSource src;

    void Start()
    {
        hp = MaxHp;
        renderers = GetComponentsInChildren<Renderer>();
        src = GetComponent<AudioSource>();
        Debug.Log("HP: " + hp);
    }

    public void TakeDamage(int dmg)
    {
        if (invincible) return;

        hp -= dmg;
        Debug.Log("OUCH! HP: " + hp);
        if (src != null && hurtClip != null) src.PlayOneShot(hurtClip);

        if (hp <= 0)
        {
            GameOver();
            return;
        }
        StartCoroutine(Invincibility());
    }

    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, MaxHp);
        Debug.Log("HEAL! HP: " + hp);
        if (src != null && healClip != null) src.PlayOneShot(healClip);
    }

    IEnumerator Invincibility()
    {
        invincible = true;
        PlayerStats stats = GameConfig.Instance.player;
        float end = Time.time + stats.invincibleTime;
        bool visible = true;
        while (Time.time < end)
        {
            visible = !visible;
            SetVisible(visible);
            yield return new WaitForSeconds(stats.invincibleBlink);
        }
        SetVisible(true);
        invincible = false;
    }

    void SetVisible(bool v)
    {
        foreach (Renderer r in renderers)
            if (r != null) r.enabled = v;
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
