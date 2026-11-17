using UnityEngine;

public static class EnemyMotion
{
    public static void FaceTowards(Transform self, Vector3 target, float turnSpeed)
    {
        Vector3 dir = target - self.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion look = Quaternion.LookRotation(dir);
        self.rotation = Quaternion.RotateTowards(self.rotation, look, turnSpeed * Time.deltaTime);
    }

    public static void HurtPlayer(Transform player, int damage)
    {
        if (player == null) return;
        Health h = player.GetComponentInParent<Health>();
        if (h != null) h.TakeDamage(damage);
    }

    public static void Fire(EnemyProjectile prefab, Vector3 from, Vector3 target, EnemyStats stats, string owner)
    {
        if (prefab == null) { Debug.LogWarning(owner + ": no projectile prefab"); return; }
        Vector3 dir = target - from;
        if (dir.sqrMagnitude < 0.0001f) return;
        EnemyProjectile p = Object.Instantiate(prefab, from, Quaternion.LookRotation(dir));
        p.Launch(stats.projectileSpeed, stats.damage, stats.projectileLifetime);
    }
}
