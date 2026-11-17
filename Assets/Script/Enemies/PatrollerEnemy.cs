using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemySight))]
public class PatrollerEnemy : MonoBehaviour
{
    public enum State { Patrol, Chase, WindUp, Cooldown, Search }

    [SerializeField] EnemyStats stats;

    [Header("Route (scene waypoints, or set from LevelData)")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float waypointTolerance = 0.5f;

    Vector3[] route = new Vector3[0];

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
        if (stats == null) { Debug.LogError(name + ": no EnemyStats"); enabled = false; return; }
        if (waypoints != null && waypoints.Length > 0)
            route = System.Array.ConvertAll(waypoints, w => w != null ? w.position : transform.position);
    }

    void Start() { Enter(State.Patrol); }

    public void SetRoute(List<Vector3> points)
    {
        route = points != null ? points.ToArray() : new Vector3[0];
        waypointIndex = 0;
    }

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

        if (next == State.Patrol) { agent.speed = stats.moveSpeed; GoToWaypoint(); }
        if (next == State.Chase) agent.speed = stats.attackMoveSpeed;
        if (next == State.Search) { agent.speed = stats.moveSpeed; agent.SetDestination(lastSeenPos); }

        if (telegraph == null) return;
        if (next == State.WindUp) telegraph.StartWarning();
        else telegraph.Clear();
    }

    void UpdatePatrol()
    {
        if (sight.CanSeePlayer()) { Enter(State.Chase); return; }
        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            waypointIndex = route.Length == 0 ? 0 : (waypointIndex + 1) % route.Length;
            GoToWaypoint();
        }
    }

    void UpdateChase()
    {
        if (!sight.CanTrackPlayer()) { Enter(State.Search); return; }
        lastSeenPos = sight.Player.position;
        agent.SetDestination(lastSeenPos);

        if (sight.DistanceToPlayer() <= stats.attackRange && sight.IsOnScreen())
            Enter(State.WindUp);
    }

    void UpdateWindUp()
    {
        EnemyMotion.FaceTowards(transform, sight.Player.position, stats.turnSpeed);
        if (stateTimer < stats.windUpTime) return;

        if (sight.DistanceToPlayer() <= stats.hitRange)
            EnemyMotion.HurtPlayer(sight.Player, stats.damage);
        Enter(State.Cooldown);
    }

    void UpdateCooldown()
    {
        if (stateTimer >= stats.cooldownTime) Enter(State.Chase);
    }

    void UpdateSearch()
    {
        if (sight.CanSeePlayer()) { Enter(State.Chase); return; }
        if (stateTimer >= stats.searchTime) Enter(State.Patrol);
    }

    void GoToWaypoint()
    {
        if (route.Length == 0) return;
        agent.SetDestination(route[waypointIndex % route.Length]);
    }
}
