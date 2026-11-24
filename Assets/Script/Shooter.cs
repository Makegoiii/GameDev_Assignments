using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shooter : MonoBehaviour
{
    const float SCREEN_CENTER = 0.5f;
    const float TRACER_WIDTH = 0.03f;
    const float RECOVER_AFTER_SHOTS = 1.5f;
    static readonly Vector3 MUZZLE_OFFSET = new Vector3(0.3f, 1.2f, 0.6f);
    static readonly Color TRACER_COLOR = new Color(1f, 0.85f, 0.4f, 0.9f);

    public float enemyAimHeight = 1f;

    PlayerStats stats;
    PlayerLook look;
    Collider[] ownColliders;
    Material tracerMaterial;
    float nextShot;
    float lastShotTime = float.NegativeInfinity;
    float sprayHeat;
    bool moving;
    bool grounded = true;

    public int Ammo { get; private set; }
    public int MagazineSize => stats.magazineSize;
    public bool IsReloading { get; private set; }
    public float CurrentSpread =>
        stats.spreadBase + sprayHeat + (moving ? stats.spreadMove : 0f) + (grounded ? 0f : stats.spreadAir);

    void Start()
    {
        stats = GameConfig.Instance.player;
        look = GetComponent<PlayerLook>();
        ownColliders = GetComponentsInChildren<Collider>(true);
        Ammo = stats.magazineSize;
        Shader unlit = Shader.Find("Sprites/Default");
        if (unlit != null) tracerMaterial = new Material(unlit);
        CrosshairHud.Create(this);
    }

    public void SetMotion(bool isMoving, bool isGrounded)
    {
        moving = isMoving;
        grounded = isGrounded;
    }

    void Update()
    {
        bool stoppedFiring = Time.time - lastShotTime > stats.shootCooldown * RECOVER_AFTER_SHOTS;
        if (stoppedFiring) sprayHeat = Mathf.MoveTowards(sprayHeat, 0f, stats.spreadRecovery * Time.deltaTime);
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) Reload();
    }

    public void Reload()
    {
        if (IsReloading || Ammo == stats.magazineSize || PauseMenu.IsPaused) return;
        StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        IsReloading = true;
        Sfx.Play(SoundEvent.Reload);
        yield return new WaitForSeconds(stats.reloadTime);
        Ammo = stats.magazineSize;
        IsReloading = false;
    }

    public void TryShoot()
    {
        if (IsReloading || Time.time < nextShot || look == null || look.Cam == null) return;
        if (Ammo <= 0) { Reload(); return; }

        nextShot = Time.time + stats.shootCooldown;
        lastShotTime = Time.time;
        Ammo--;

        Transform cam = look.Cam.transform;
        float skip = Vector3.Dot(transform.position - cam.position, cam.forward);
        Vector3 origin = cam.position + cam.forward * Mathf.Max(0f, skip);
        Vector3 dir = AimAssist(origin, cam.forward);
        dir = ApplySpread(dir, CurrentSpread);
        Vector3 end = origin + dir * stats.range;

        if (RaycastIgnoringSelf(origin, dir, out RaycastHit hit))
        {
            end = hit.point;
            DamageTarget(hit.collider);
        }

        ShowTracer(transform.position + transform.rotation * MUZZLE_OFFSET, end);
        Sfx.Play(SoundEvent.Shoot);
        sprayHeat = Mathf.Min(sprayHeat + stats.spreadPerShot, stats.spreadMax);
        look.AddRecoil(stats.recoilKick, Random.Range(-stats.recoilSide, stats.recoilSide));
        if (Ammo == 0) Reload();
    }

    static Vector3 ApplySpread(Vector3 dir, float degrees)
    {
        Vector2 r = Random.insideUnitCircle * degrees;
        return Quaternion.AngleAxis(r.x, Vector3.up) * Quaternion.AngleAxis(r.y, Vector3.Cross(dir, Vector3.up)) * dir;
    }

    bool RaycastIgnoringSelf(Vector3 origin, Vector3 dir, out RaycastHit best)
    {
        best = default;
        float bestDist = float.MaxValue;
        foreach (RaycastHit h in Physics.RaycastAll(origin, dir, stats.range, ~0, QueryTriggerInteraction.Ignore))
        {
            if (System.Array.IndexOf(ownColliders, h.collider) >= 0 || h.distance >= bestDist) continue;
            best = h;
            bestDist = h.distance;
        }
        return bestDist < float.MaxValue;
    }

    void DamageTarget(Collider c)
    {
        EnemyHealth enemy = c.GetComponentInParent<EnemyHealth>();
        if (enemy != null) { enemy.TakeDamage(stats.bulletDamage); return; }
        Boss boss = c.GetComponentInParent<Boss>();
        if (boss != null) boss.TakeDamage(stats.bulletDamage);
    }

    Vector3 AimAssist(Vector3 origin, Vector3 forward)
    {
        if (!Application.isMobilePlatform) return forward;
        Vector3 best = forward;
        float bestAngle = stats.mobileAimAssistAngle;
        foreach (EnemyHealth enemy in FindObjectsByType<EnemyHealth>())
        {
            if (enemy.IsDead) continue;
            Vector3 to = enemy.transform.position + Vector3.up * enemyAimHeight - origin;
            if (to.magnitude > stats.aimAssistRange) continue;
            float angle = Vector3.Angle(forward, to);
            if (angle > bestAngle) continue;
            bestAngle = angle;
            best = to.normalized;
        }
        return best;
    }

    void ShowTracer(Vector3 from, Vector3 to)
    {
        if (tracerMaterial == null) return;
        LineRenderer line = new GameObject("Tracer").AddComponent<LineRenderer>();
        line.sharedMaterial = tracerMaterial;
        line.startColor = line.endColor = TRACER_COLOR;
        line.startWidth = line.endWidth = TRACER_WIDTH;
        line.positionCount = 2;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Destroy(line.gameObject, stats.tracerTime);
    }

    void OnDestroy()
    {
        if (tracerMaterial != null) Destroy(tracerMaterial);
    }
}
