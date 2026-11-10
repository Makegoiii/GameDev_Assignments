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
}
