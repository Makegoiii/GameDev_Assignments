using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public int MaxHp => GameConfig.Instance.player.maxHp;
    public int Hp => hp;

    public static event Action Damaged;

    int hp;
    bool invincible;
    Renderer[] renderers;

    void Start()
    {
        hp = MaxHp;
        renderers = GetComponentsInChildren<Renderer>();
        Debug.Log("HP: " + hp);
    }

    public void TakeDamage(int dmg)
    {
        if (invincible) return;

        hp -= dmg;
        Debug.Log("OUCH! HP: " + hp);

        if (hp <= 0)
        {
            GameOver();
            return;
        }
        Sfx.Play(SoundEvent.PlayerHurt);
        Damaged?.Invoke();
        StartCoroutine(Invincibility());
    }

    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, MaxHp);
        Debug.Log("HEAL! HP: " + hp);
        Sfx.Play(SoundEvent.Heal);
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
        Sfx.Play(SoundEvent.PlayerDeath);
        Sfx.Play(SoundEvent.Lose);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
