using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemySight))]
public class ChargerEnemy : MonoBehaviour
{
    public enum State { Wander, WindUp, Charge, Stunned }

    [Header("Wander")]
    [SerializeField] float wanderSpeed = 1.8f;
    [SerializeField] float wanderRadius = 5f;
    [SerializeField] float turnSpeed = 240f;

    [Header("Charge")]
    [SerializeField] float windUpTime = 0.7f;
    [SerializeField] float chargeSpeed = 13f;
    [SerializeField] float chargeDistance = 11f;
    [SerializeField] float hitRadius = 1.2f;
    [SerializeField] int damage = 1;
    [SerializeField] float stunTime = 1.6f;
    [SerializeField] float stunDamageMultiplier = 2f;

    NavMeshAgent agent;
    EnemySight sight;
    Telegraph telegraph;
    EnemyHealth health;
    State state;
    float stateTimer;
    Vector3 home;
    Vector3 chargeDir;
    float travelled;
    bool hasHit;

    public State Current => state;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        sight = GetComponent<EnemySight>();
        telegraph = GetComponent<Telegraph>();
        health = GetComponent<EnemyHealth>();
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
            case State.Wander:  UpdateWander();  break;
            case State.WindUp:  UpdateWindUp();  break;
            case State.Charge:  UpdateCharge();  break;
            case State.Stunned: UpdateStunned(); break;
        }
    }

    void Enter(State next)
    {
        state = next;
        stateTimer = 0f;
        agent.isStopped = next != State.Wander;
        agent.updateRotation = next == State.Wander;
        agent.speed = wanderSpeed;
        if (health != null) health.DamageMultiplier = next == State.Stunned ? stunDamageMultiplier : 1f;

        if (next == State.Charge) { travelled = 0f; hasHit = false; }

        if (telegraph == null) return;
        if (next == State.WindUp) telegraph.StartWarning();
        else if (next == State.Stunned) telegraph.ShowStunned();
        else telegraph.Clear();
    }

    void UpdateWander()
    {
        if (sight.CanSeePlayer() && sight.IsOnScreen()) { Enter(State.WindUp); return; }
        if (!agent.pathPending && agent.remainingDistance <= 0.4f) PickWanderPoint();
    }

    void UpdateWindUp()
    {
        EnemyMotion.FaceTowards(transform, sight.Player.position, turnSpeed);
        if (stateTimer < windUpTime) return;

        chargeDir = transform.forward;
        Enter(State.Charge);
    }

    void UpdateCharge()
    {
        Vector3 before = transform.position;
        agent.Move(chargeDir * chargeSpeed * Time.deltaTime);
        float moved = (transform.position - before).magnitude;
        travelled += moved;

        if (!hasHit && sight.DistanceToPlayer() <= hitRadius)
        {
            hasHit = true;
            EnemyMotion.HurtPlayer(sight.Player, damage);
        }

        bool hitWall = moved < chargeSpeed * Time.deltaTime * 0.25f;
        if (hitWall || travelled >= chargeDistance) Enter(State.Stunned);
    }

    void UpdateStunned()
    {
        if (stateTimer >= stunTime) Enter(State.Wander);
    }

    void PickWanderPoint()
    {
        Vector2 r = Random.insideUnitCircle * wanderRadius;
        Vector3 p = home + new Vector3(r.x, 0f, r.y);
        if (NavMesh.SamplePosition(p, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }
}
