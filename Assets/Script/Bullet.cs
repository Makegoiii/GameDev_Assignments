using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int Damage => GameConfig.Instance.player.bulletDamage;

    void Start()
    {
        Destroy(gameObject, GameConfig.Instance.player.bulletLifetime);
    }

    public void Launch(Vector3 direction)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = direction.normalized * GameConfig.Instance.player.bulletSpeed;
    }
}
