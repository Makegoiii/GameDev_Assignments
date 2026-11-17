using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Coin Quest/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Movement")]
    [Min(0f)] public float moveSpeed;
    [Min(0f)] public float turnSpeed;
    [Range(0f, 1f)] public float backwardSpeedFactor;
    [Min(0f)] public float jumpImpulse;
    [Range(0f, 1f)] public float airControl;
    public float fallKillY;

    [Header("Health")]
    [Min(1)] public int maxHp;
    [Min(0f)] public float invincibleTime;
    [Min(0.01f)] public float invincibleBlink;
    [Min(0f)] public float gameOverDelay;

    [Header("Shooting")]
    [Min(0f)] public float shootCooldown;
    [Min(0f)] public float bulletSpeed;
    [Min(0f)] public float bulletLifetime;
    [Min(1)] public int bulletDamage;
    [Range(0f, 90f)] public float aimAssistAngle;
    [Min(0f)] public float aimAssistRange;
}
