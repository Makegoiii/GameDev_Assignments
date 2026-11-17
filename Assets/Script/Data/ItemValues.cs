using UnityEngine;

[CreateAssetMenu(fileName = "ItemValues", menuName = "Coin Quest/Item Values")]
public class ItemValues : ScriptableObject
{
    [Header("Score")]
    [Min(0)] public int coinScore;
    [Min(0)] public int enemyScore;
    [Min(0)] public int timeBonusPerSecond;

    [Header("Pickups")]
    [Min(0)] public int heartHeal;

    [Header("Hazards")]
    [Min(0f)] public float hazardLifetime;
    [Min(0)] public int rockDamage;
    [Min(0)] public int bombHitDamage;
    [Min(0)] public int bombBlastDamage;
    [Min(0f)] public float bombFuse;
    [Min(0f)] public float bombBlastRadius;
    [Min(0f)] public float movingObstacleSpeed;
}
