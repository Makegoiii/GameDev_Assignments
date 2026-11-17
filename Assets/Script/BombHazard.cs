using UnityEngine;

public class BombHazard : Hazard
{
    public GameObject explosionPrefab;
    public AudioClip explodeClip;
    public float explosionFxTime = 3f;

    bool armed;

    protected override void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Player"))
        {
            base.OnCollisionEnter(c);
            PlayFx();
            return;
        }

        if (!armed)
        {
            armed = true;
            Invoke(nameof(Explode), Items.bombFuse);
        }
    }

    protected override void OnHitPlayer(Health player)
    {
        player.TakeDamage(Items.bombHitDamage);
    }

    void Explode()
    {
        if (Items.bombBlastRadius > 0f)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, Items.bombBlastRadius);
            foreach (Collider col in hits)
            {
                if (!col.CompareTag("Player")) continue;
                Health h = col.GetComponentInParent<Health>();
                if (h != null) { h.TakeDamage(Items.bombBlastDamage); break; }
            }
        }
        PlayFx();
        Destroy(gameObject);
    }

    void PlayFx()
    {
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, explosionFxTime);
        }
        if (explodeClip != null)
            AudioSource.PlayClipAtPoint(explodeClip, transform.position);
    }
}
