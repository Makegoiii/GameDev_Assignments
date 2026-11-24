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

    [Header("FPS look (Counter-Strike style)")]
    [Min(0f)] public float mouseSensitivity = 0.12f;
    [Min(0f)] public float touchSensitivity = 0.2f;
    [Min(0f)] public float gamepadLookSpeed = 160f;
    [Range(10f, 89f)] public float maxPitch = 80f;
    [Min(0f)] public float eyeBelowTop = 0.15f;
    [Range(0f, 1f)] public float strafeSpeedFactor = 0.9f;

    [Header("Third-person camera (Free Fire style)")]
    [Min(0.5f)] public float cameraDistance = 3.2f;
    public float shoulderOffset = 0.6f;
    public float shoulderHeight = 0.35f;
    [Min(0.05f)] public float cameraCollisionRadius = 0.25f;
    [Min(0f)] public float cameraMinDistance = 0.4f;

    [Header("FPS weapon")]
    [Min(1)] public int magazineSize = 30;
    [Min(0f)] public float reloadTime = 2f;
    [Min(1f)] public float range = 60f;
    [Min(0f)] public float spreadBase = 0.4f;
    [Min(0f)] public float spreadMove = 2f;
    [Min(0f)] public float spreadAir = 4f;
    [Min(0f)] public float spreadPerShot = 0.5f;
    [Min(0f)] public float spreadMax = 4f;
    [Min(0f)] public float spreadRecovery = 8f;
    [Min(0f)] public float recoilKick = 0.7f;
    [Min(0f)] public float recoilSide = 0.25f;
    [Range(0f, 30f)] public float mobileAimAssistAngle = 4f;
    [Min(0f)] public float tracerTime = 0.05f;
}
