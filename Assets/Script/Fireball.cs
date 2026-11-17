using UnityEngine;

public class Fireball : Hazard
{
    protected override void Start()
    {
        base.Start();
        GetComponent<Rigidbody>().linearVelocity = transform.forward * GameConfig.Instance.boss.fireballSpeed;
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.GetComponentInParent<Boss>()) return;
        if (c.isTrigger) return;

        Health h = c.GetComponentInParent<Health>();
        if (h != null) OnHitPlayer(h);

        Destroy(gameObject);
    }

    protected override void OnHitPlayer(Health player)
    {
        player.TakeDamage(GameConfig.Instance.boss.fireballDamage);
    }
}
