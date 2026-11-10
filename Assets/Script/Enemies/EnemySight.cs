using UnityEngine;

public class EnemySight : MonoBehaviour
{
    [Header("Vision")]
    [SerializeField] float viewDistance = 12f;
    [SerializeField, Range(10f, 360f)] float viewAngle = 110f;
    [SerializeField] float eyeHeight = 1.2f;
    [SerializeField] float playerAimHeight = 0.9f;
    [SerializeField] LayerMask obstacleMask = ~0;

    [Header("Debug ray (yellow = I see you)")]
    [SerializeField] LineRenderer visionRay;

    Transform player;
    Camera cam;

    public Transform Player => player;
    public float ViewDistance => viewDistance;
    public Vector3 Eye => transform.position + Vector3.up * eyeHeight;
    public Vector3 PlayerTarget => player.position + Vector3.up * playerAimHeight;

    void Start()
    {
        Movement m = FindAnyObjectByType<Movement>();
        if (m != null) player = m.transform;
        else Debug.LogWarning(name + ": no player (Movement) found in scene");
        cam = Camera.main;
        if (visionRay != null) visionRay.enabled = false;
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
                    && DistanceToPlayer() <= viewDistance
                    && InViewCone()
                    && HasLineOfSight();
        UpdateRay(seen);
        return seen;
    }

    public bool CanTrackPlayer()
    {
        bool seen = player != null && DistanceToPlayer() <= viewDistance && HasLineOfSight();
        UpdateRay(seen);
        return seen;
    }

    bool InViewCone()
    {
        Vector3 to = player.position - transform.position;
        to.y = 0f;
        return Vector3.Angle(transform.forward, to) <= viewAngle * 0.5f;
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

    void UpdateRay(bool seen)
    {
        if (visionRay == null) return;
        visionRay.enabled = seen;
        if (!seen) return;
        visionRay.SetPosition(0, Eye);
        visionRay.SetPosition(1, PlayerTarget);
    }

    void OnDisable()
    {
        if (visionRay != null) visionRay.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
        Quaternion half = Quaternion.Euler(0f, viewAngle * 0.5f, 0f);
        Gizmos.DrawRay(Eye, half * transform.forward * viewDistance);
        Gizmos.DrawRay(Eye, Quaternion.Inverse(half) * transform.forward * viewDistance);
    }
}
