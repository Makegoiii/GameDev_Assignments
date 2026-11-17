using System;
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] EnemyStats stats;
    [SerializeField] float hitFlashTime = 0.08f;
    [SerializeField] AudioClip hitClip;

    public static event Action<EnemyHealth> Died;

    int hp;
    bool dead;
    Renderer[] rends;

    public bool IsDead => dead;

    void Awake()
    {
        if (stats == null) Debug.LogError(name + ": no EnemyStats");
        hp = stats != null ? stats.maxHp : 1;
        rends = GetComponentsInChildren<Renderer>();
    }

    void OnTriggerEnter(Collider c) { CheckBullet(c.gameObject); }
    void OnCollisionEnter(Collision c) { CheckBullet(c.gameObject); }

    void CheckBullet(GameObject obj)
    {
        Bullet bullet = obj.GetComponent<Bullet>();
        if (dead || bullet == null) return;
        Destroy(obj);
        TakeDamage(bullet.Damage);
    }

    public void TakeDamage(int dmg)
    {
        if (dead) return;
        hp -= dmg;
        if (hitClip != null) AudioSource.PlayClipAtPoint(hitClip, transform.position);
        if (hp <= 0) { Die(); return; }
        StartCoroutine(HitFlash());
    }

    IEnumerator HitFlash()
    {
        SetVisible(false);
        yield return new WaitForSeconds(hitFlashTime);
        SetVisible(true);
    }

    void SetVisible(bool v)
    {
        foreach (Renderer r in rends)
            if (r != null && !(r is LineRenderer)) r.enabled = v;
    }

    void Die()
    {
        dead = true;
        Died?.Invoke(this);
        Destroy(gameObject);
    }
}
