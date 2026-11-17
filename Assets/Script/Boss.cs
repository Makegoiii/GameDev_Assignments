using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Boss : MonoBehaviour
{
    const float FLASH_TIME = 0.06f;
    const int FLASH_COUNT = 4;

    [Header("Денсаулық")]
    public RectTransform hpFill;
    public Slider hpBar;

    [Header("Сілтемелер")]
    public Transform player;
    public Transform firePoint;
    public Transform arenaCenter;
    public GameObject fireballPrefab;

    [Header("Баптау")]
    public AudioClip hitClip;

    int hp;
    bool dead;
    Renderer[] rends;

    BossStats Stats => GameConfig.Instance.boss;

    void Start()
    {
        hp = Stats.maxHp;
        rends = GetComponentsInChildren<Renderer>();
        UpdateBar();
        StartCoroutine(AttackLoop());
    }

    void Update()
    {
        if (dead || player == null) return;

        Vector3 look = player.position;
        look.y = transform.position.y;
        transform.LookAt(look);
    }

    void OnTriggerEnter(Collider c)    { CheckBullet(c.gameObject); }
    void OnCollisionEnter(Collision c) { CheckBullet(c.gameObject); }

    void CheckBullet(GameObject obj)
    {
        if (obj.GetComponent<Bullet>() == null) return;
        Destroy(obj);
        TakeDamage(GameConfig.Instance.player.bulletDamage);
    }

    public void TakeDamage(int dmg)
    {
        if (dead) return;

        hp -= dmg;
        UpdateBar();
        if (hitClip) AudioSource.PlayClipAtPoint(hitClip, transform.position);

        if (hp <= 0)
        {
            dead = true;
            StopAllCoroutines();
            foreach (var r in rends) r.enabled = true;
            Invoke(nameof(Win), Stats.winDelay);
        }
        else
        {
            StartCoroutine(Flash());
        }
    }

    void UpdateBar()
    {
        float k = Mathf.Clamp01((float)hp / Stats.maxHp);
        if (hpFill) hpFill.localScale = new Vector3(k, 1f, 1f);
        if (hpBar) hpBar.value = k;
    }

    IEnumerator Flash()
    {
        for (int i = 0; i < FLASH_COUNT; i++)
        {
            foreach (var r in rends) r.enabled = !r.enabled;
            yield return new WaitForSeconds(FLASH_TIME);
        }
        foreach (var r in rends) r.enabled = true;
    }

    void Win()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(Stats.startDelay);

        while (!dead)
        {
            bool phase2 = hp <= Stats.maxHp / 2;
            int shots = phase2 ? Stats.phase2Shots : Stats.phase1Shots;

            if (Random.value < 0.5f)
                yield return StartCoroutine(Fan(shots));
            else
                yield return StartCoroutine(Rain(shots));

            yield return new WaitForSeconds(phase2 ? Stats.phase2Pause : Stats.phase1Pause);
        }
    }

    IEnumerator Fan(int n)
    {
        Vector3 to = player.position - firePoint.position;
        to.y = 0f;
        if (to.sqrMagnitude < 0.01f) to = transform.forward;
        Quaternion baseRot = Quaternion.LookRotation(to);

        for (int i = 0; i < n; i++)
        {
            float angle = (i - (n - 1) / 2f) * Stats.fanSpreadAngle;
            Instantiate(fireballPrefab, firePoint.position,
                        baseRot * Quaternion.Euler(0f, angle, 0f));
        }
        yield return null;
    }

    IEnumerator Rain(int n)
    {
        Vector3 c = arenaCenter ? arenaCenter.position : Vector3.zero;
        int count = n * Stats.rainDropsPerShot;

        for (int i = 0; i < count; i++)
        {
            Vector2 r = Random.insideUnitCircle * Stats.arenaRadius;
            Vector3 p = c + new Vector3(r.x, Random.Range(Stats.rainHeightMin, Stats.rainHeightMax), r.y);

            Instantiate(fireballPrefab, p, Quaternion.Euler(90f, 0f, 0f));
            yield return new WaitForSeconds(Random.Range(Stats.rainDelayMin, Stats.rainDelayMax));
        }
    }
}
