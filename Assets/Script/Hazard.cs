using UnityEngine;

public class Hazard : MonoBehaviour
{
    protected static ItemValues Items => GameConfig.Instance.items;

    protected virtual void Start()
    {
        Destroy(gameObject, Items.hazardLifetime);
    }

    protected virtual void OnCollisionEnter(Collision c)
    {
        if (!c.gameObject.CompareTag("Player")) return;

        Health h = c.gameObject.GetComponent<Health>();
        if (h == null) h = c.gameObject.GetComponentInParent<Health>();
        if (h != null) OnHitPlayer(h);

        Destroy(gameObject);
    }

    protected virtual void OnHitPlayer(Health player)
    {
        player.TakeDamage(Items.rockDamage);
    }
}
