using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemySight))]
public class PatrollerEnemy : MonoBehaviour
{
    public enum State { Patrol, Chase, WindUp, Cooldown, Search }

    [Header("Route")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float waypointTolerance = 0.5f;

    [Header("Speed")]
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float chaseSpeed = 3.6f;
    [SerializeField] float turnSpeed = 360f;

    [Header("Attack")]
    [SerializeField] float attackRange = 1.8f;
    [SerializeField] float hitRange = 2.2f;
    [SerializeField] float windUpTime = 0.5f;
    [SerializeField] float cooldownTime = 1.2f;
    [SerializeField] int damage = 1;

    [Header("Search")]
    [SerializeField] float searchTime = 3f;

    NavMeshAgent agent;
    EnemySight sight;
    Telegraph telegraph;
    State state;
    float stateTimer;
    int waypointIndex;
    Vector3 lastSeenPos;

    public State Current => state;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        sight = GetComponent<EnemySight>();
        telegraph = GetComponent<Telegraph>();
    }

    void Start() { Enter(State.Patrol); }

    void Update()
    {
        if (PauseMenu.IsPaused || sight.Player == null) return;
        stateTimer += Time.deltaTime;

        switch (state)
        {
            case State.Patrol:   UpdatePatrol();   break;
            case State.Chase:    UpdateChase();    break;
            case State.WindUp:   UpdateWindUp();   break;
            case State.Cooldown: UpdateCooldown(); break;
            case State.Search:   UpdateSearch();   break;
        }
    }

    void Enter(State next)
    {
        state = next;
        stateTimer = 0f;
        agent.isStopped = next == State.WindUp || next == State.Cooldown;

        if (next == State.Patrol) { agent.speed = patrolSpeed; GoToWaypoint(); }
        if (next == State.Chase) agent.speed = chaseSpeed;
        if (next == State.Search) { agent.speed = patrolSpeed; agent.SetDestination(lastSeenPos); }

        if (telegraph == null) return;
        if (next == State.WindUp) telegraph.StartWarning();
        else telegraph.Clear();
    }

    void UpdatePatrol()
    {
        if (sight.CanSeePlayer()) { Enter(State.Chase); return; }
        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            waypointIndex = waypoints.Length == 0 ? 0 : (waypointIndex + 1) % waypoints.Length;
            GoToWaypoint();
        }
    }

    void UpdateChase()
    {
        if (!sight.CanTrackPlayer()) { Enter(State.Search); return; }
        lastSeenPos = sight.Player.position;
        agent.SetDestination(lastSeenPos);

        if (sight.DistanceToPlayer() <= attackRange && sight.IsOnScreen())
            Enter(State.WindUp);
    }

    void UpdateWindUp()
    {
        EnemyMotion.FaceTowards(transform, sight.Player.position, turnSpeed);
        if (stateTimer < windUpTime) return;

        if (sight.DistanceToPlayer() <= hitRange)
            EnemyMotion.HurtPlayer(sight.Player, damage);
        Enter(State.Cooldown);
    }

    void UpdateCooldown()
    {
        if (stateTimer >= cooldownTime) Enter(State.Chase);
    }

    void UpdateSearch()
    {
        if (sight.CanSeePlayer()) { Enter(State.Chase); return; }
        if (stateTimer >= searchTime) Enter(State.Patrol);
    }

    void GoToWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0 || waypoints[waypointIndex] == null) return;
        agent.SetDestination(waypoints[waypointIndex].position);
    }
}
