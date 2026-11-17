using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemySight))]
public class RusherEnemy : MonoBehaviour
{
    public enum State { Wander, Rush, Aim, Cooldown }

    const float ARRIVE_DISTANCE = 0.4f;

    [SerializeField] EnemyStats stats;
    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] Transform muzzle;

    NavMeshAgent agent;
    EnemySight sight;
    Telegraph telegraph;
    State state;
    float stateTimer;
    float lostTimer;
    Vector3 home;

    public State Current => state;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        sight = GetComponent<EnemySight>();
        telegraph = GetComponent<Telegraph>();
        if (stats == null) { Debug.LogError(name + ": no EnemyStats"); enabled = false; }
    }

    void Start()
    {
        home = transform.position;
        Enter(State.Wander);
    }

    void Update()
    {
        if (PauseMenu.IsPaused || sight.Player == null) return;
        stateTimer += Time.deltaTime;

        switch (state)
        {
            case State.Wander:   UpdateWander();   break;
            case State.Rush:     UpdateRush();     break;
            case State.Aim:      UpdateAim();      break;
            case State.Cooldown: UpdateCooldown(); break;
        }
    }

    void Enter(State next)
    {
        state = next;
        stateTimer = 0f;
        bool moving = next == State.Wander || next == State.Rush;
        agent.isStopped = !moving;
        agent.updateRotation = moving;
        agent.speed = next == State.Rush ? stats.attackMoveSpeed : stats.moveSpeed;
        if (next == State.Rush) lostTimer = 0f;

        if (telegraph == null) return;
        if (next == State.Aim) telegraph.StartWarning();
        else telegraph.Clear();
    }

    void UpdateWander()
    {
        if (sight.CanSeePlayer()) { Enter(State.Rush); return; }
        if (!agent.pathPending && agent.remainingDistance <= ARRIVE_DISTANCE) PickWanderPoint();
    }

    void UpdateRush()
    {
        bool tracking = sight.CanTrackPlayer();
        lostTimer = tracking ? 0f : lostTimer + Time.deltaTime;
        if (lostTimer >= stats.searchTime) { Enter(State.Wander); return; }

        agent.SetDestination(sight.Player.position);
        bool inRange = sight.DistanceToPlayer() <= stats.attackRange;
        if (tracking && inRange && sight.IsOnScreen()) Enter(State.Aim);
    }

    void UpdateAim()
    {
        EnemyMotion.FaceTowards(transform, sight.Player.position, stats.turnSpeed);
        if (!sight.CanTrackPlayer()) { Enter(State.Rush); return; }
        if (stateTimer < stats.windUpTime) return;

        Vector3 from = muzzle != null ? muzzle.position : sight.Eye;
        EnemyMotion.Fire(projectilePrefab, from, sight.PlayerTarget, stats, name);
        Enter(State.Cooldown);
    }

    void UpdateCooldown()
    {
        EnemyMotion.FaceTowards(transform, sight.Player.position, stats.turnSpeed);
        if (stateTimer >= stats.cooldownTime) Enter(State.Rush);
    }

    void PickWanderPoint()
    {
        Vector2 r = Random.insideUnitCircle * stats.wanderRadius;
        Vector3 p = home + new Vector3(r.x, 0f, r.y);
        if (NavMesh.SamplePosition(p, out NavMeshHit hit, stats.wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }
}
