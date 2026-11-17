using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    const float ARRIVE_DISTANCE = 0.1f;
    public bool faceDirection = false;

    Transform target;

    void Start()
    {
        target = pointB;
    }

    void Update()
    {
        if (pointA == null || pointB == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position, target.position, GameConfig.Instance.items.movingObstacleSpeed * Time.deltaTime);

        if (faceDirection)
        {
            Vector3 look = target.position;
            look.y = transform.position.y;
            if ((look - transform.position).sqrMagnitude > 0.001f)
                transform.LookAt(look);
        }

        if (Vector3.Distance(transform.position, target.position) < ARRIVE_DISTANCE)
            target = (target == pointA) ? pointB : pointA;
    }
}
