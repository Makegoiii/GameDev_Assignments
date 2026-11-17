using UnityEngine;

[CreateAssetMenu(fileName = "BossStats", menuName = "Coin Quest/Boss Stats")]
public class BossStats : ScriptableObject
{
    [Min(1)] public int maxHp;
    [Min(0f)] public float arenaRadius;
    [Min(0f)] public float startDelay;
    [Min(0f)] public float winDelay;

    [Header("Fan attack")]
    [Min(1)] public int phase1Shots;
    [Min(1)] public int phase2Shots;
    [Min(0f)] public float fanSpreadAngle;

    [Header("Rain attack")]
    [Min(0f)] public float rainHeightMin;
    [Min(0f)] public float rainHeightMax;
    [Min(0f)] public float rainDelayMin;
    [Min(0f)] public float rainDelayMax;
    [Min(1)] public int rainDropsPerShot;

    [Header("Rhythm")]
    [Min(0f)] public float phase1Pause;
    [Min(0f)] public float phase2Pause;

    [Header("Fireball")]
    [Min(0f)] public float fireballSpeed;
    [Min(0)] public int fireballDamage;
}
