using UnityEngine;

public class EnemySight : MonoBehaviour
{
    [Header("Vision (range and angle come from EnemyStats)")]
    [SerializeField] EnemyStats stats;
    [SerializeField] float eyeHeight = 1.2f;
    [SerializeField] float playerAimHeight = 0.9f;
    [SerializeField] LayerMask obstacleMask = ~0;

    Transform player;
    Camera cam;

    public Transform Player => player;
    public float ViewDistance => stats != null ? stats.visionRange : 0f;
    float ViewAngle => stats != null ? stats.viewAngle : 0f;
    public Vector3 Eye => transform.position + Vector3.up * eyeHeight;
    public Vector3 PlayerTarget => player.position + Vector3.up * playerAimHeight;

    void Start()
    {
        Movement m = FindAnyObjectByType<Movement>();
        if (m != null) player = m.transform;
        else Debug.LogWarning(name + ": no player (Movement) found in scene");
        cam = Camera.main;
    }

    public float DistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        Vector3 d = player.position - transform.position;
        d.y = 0f;
        return d.magnitude;
    }

    public bool CanSeePlayer()
    {
        bool seen = player != null
                    && DistanceToPlayer() <= ViewDistance
                    && InViewCone()
                    && HasLineOfSight();
        return seen;
    }

    public bool CanTrackPlayer()
    {
        bool seen = player != null && DistanceToPlayer() <= ViewDistance && HasLineOfSight();
        return seen;
    }

    bool InViewCone()
    {
        Vector3 to = player.position - transform.position;
        to.y = 0f;
        return Vector3.Angle(transform.forward, to) <= ViewAngle * 0.5f;
    }

    public bool HasLineOfSight()
    {
        if (player == null) return false;
        Vector3 from = Eye;
        Vector3 dir = PlayerTarget - from;
        if (!Physics.Raycast(from, dir.normalized, out RaycastHit hit, dir.magnitude + 0.5f,
                             obstacleMask, QueryTriggerInteraction.Ignore))
            return true;
        if (hit.transform.IsChildOf(transform)) return true;
        return hit.transform == player || hit.transform.IsChildOf(player);
    }

    public bool IsOnScreen()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return true;
        Vector3 v = cam.WorldToViewportPoint(transform.position + Vector3.up);
        return v.z > 0f && v.x > 0.02f && v.x < 0.98f && v.y > 0.02f && v.y < 0.98f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ViewDistance);
        Quaternion half = Quaternion.Euler(0f, ViewAngle * 0.5f, 0f);
        Gizmos.DrawRay(Eye, half * transform.forward * ViewDistance);
        Gizmos.DrawRay(Eye, Quaternion.Inverse(half) * transform.forward * ViewDistance);
    }
}
