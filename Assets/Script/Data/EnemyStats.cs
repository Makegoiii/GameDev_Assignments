using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Coin Quest/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Body")]
    [Min(1)] public int maxHp;
    [Min(0f)] public float moveSpeed;
    [Min(0f)] public float attackMoveSpeed;
    [Min(0f)] public float turnSpeed;

    [Header("Vision")]
    [Min(0f)] public float visionRange;
    [Range(10f, 360f)] public float viewAngle;

    [Header("Attack")]
    [Min(1)] public int damage;
    [Min(0f)] public float windUpTime;
    [Min(0f)] public float cooldownTime;
    [Min(0f)] public float attackRange;
    [Min(0f)] public float hitRange;
    [Min(0f)] public float searchTime;

    [Header("Ranged (Shooter, Rusher)")]
    [Min(0f)] public float projectileSpeed;
    [Min(0f)] public float projectileLifetime;

    [Header("Shooter only")]
    [Min(0f)] public float scanSpeed;

    [Header("Rusher only")]
    [Min(0f)] public float wanderRadius;
}
