using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] float speed = 9f;
    [SerializeField] int damage = 1;
    [SerializeField] float lifeTime = 4f;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.isTrigger) return;
        if (c.GetComponentInParent<EnemyHealth>() != null) return;

        Health h = c.GetComponentInParent<Health>();
        if (h != null) h.TakeDamage(damage);
        Destroy(gameObject);
    }
}
