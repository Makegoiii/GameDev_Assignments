using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Bullet bulletPrefab;
    public float muzzleDistance = 1f;
    public float muzzleHeight = 0.5f;
    public float enemyAimHeight = 1f;
    public AudioClip shootClip;

    float nextShot;
    AudioSource src;

    void Start()
    {
        src = GetComponent<AudioSource>();
    }

    public void Shoot(Vector3 facing)
    {
        if (Time.time < nextShot) return;
        PlayerStats stats = GameConfig.Instance.player;
        nextShot = Time.time + stats.shootCooldown;

        facing.y = 0f;
        if (facing.sqrMagnitude < 0.0001f) facing = transform.forward;
        facing.Normalize();

        Vector3 muzzle = transform.position + facing * muzzleDistance + Vector3.up * muzzleHeight;
        Vector3 aimDir = AimAssist(muzzle, facing, stats);

        Bullet b = Instantiate(bulletPrefab, muzzle, Quaternion.LookRotation(aimDir));
        b.Launch(aimDir);

        if (src != null && shootClip != null) src.PlayOneShot(shootClip);
    }

    Vector3 AimAssist(Vector3 muzzle, Vector3 facing, PlayerStats stats)
    {
        Vector3 best = facing;
        float bestAngle = stats.aimAssistAngle;

        foreach (EnemyHealth enemy in FindObjectsByType<EnemyHealth>())
        {
            if (enemy.IsDead) continue;
            Vector3 to = enemy.transform.position + Vector3.up * enemyAimHeight - muzzle;
            Vector3 flat = new Vector3(to.x, 0f, to.z);
            if (flat.magnitude > stats.aimAssistRange) continue;

            float angle = Vector3.Angle(facing, flat);
            if (angle > bestAngle) continue;

            bestAngle = angle;
            best = to.normalized;
        }
        return best;
    }
}
