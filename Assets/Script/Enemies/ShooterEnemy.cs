using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemySight))]
public class ShooterEnemy : MonoBehaviour
{
    public enum State { Guard, Aim, Cooldown, Retreat }

    [Header("Movement")]
    [SerializeField] float moveSpeed = 2.5f;
    [SerializeField] float turnSpeed = 180f;
    [SerializeField] float scanSpeed = 40f;
    [SerializeField] float tooCloseDistance = 4f;
    [SerializeField] float retreatStep = 4f;

    [Header("Attack")]
    [SerializeField] float aimTime = 0.6f;
    [SerializeField] float cooldownTime = 1.6f;
    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] Transform muzzle;
    [SerializeField] LineRenderer aimLaser;

    NavMeshAgent agent;
    EnemySight sight;
    Telegraph telegraph;
    State state;
    float stateTimer;

    public State Current => state;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        sight = GetComponent<EnemySight>();
        telegraph = GetComponent<Telegraph>();
        agent.speed = moveSpeed;
        agent.updateRotation = false;
    }

    void Start() { Enter(State.Guard); }

    void Update()
    {
        if (PauseMenu.IsPaused || sight.Player == null) return;
        stateTimer += Time.deltaTime;

        switch (state)
        {
            case State.Guard:    UpdateGuard();    break;
            case State.Aim:      UpdateAim();      break;
            case State.Cooldown: UpdateCooldown(); break;
            case State.Retreat:  UpdateRetreat();  break;
        }
    }

    void Enter(State next)
    {
        state = next;
        stateTimer = 0f;
        if (aimLaser != null) aimLaser.enabled = next == State.Aim;

        if (next == State.Retreat) StartRetreat();
        else agent.ResetPath();

        if (telegraph == null) return;
        if (next == State.Aim) telegraph.StartWarning();
        else telegraph.Clear();
    }

    void UpdateGuard()
    {
        if (!sight.CanSeePlayer())
        {
            transform.Rotate(0f, scanSpeed * Time.deltaTime, 0f);
            return;
        }
        EnemyMotion.FaceTowards(transform, sight.Player.position, turnSpeed);
        if (sight.DistanceToPlayer() < tooCloseDistance) { Enter(State.Retreat); return; }
        if (sight.IsOnScreen()) Enter(State.Aim);
    }

    void UpdateAim()
    {
        EnemyMotion.FaceTowards(transform, sight.Player.position, turnSpeed);
        UpdateLaser();

        if (!sight.CanTrackPlayer()) { Enter(State.Guard); return; }
        if (stateTimer < aimTime) return;

        Fire();
        Enter(State.Cooldown);
    }

    void UpdateCooldown()
    {
        if (sight.CanTrackPlayer())
            EnemyMotion.FaceTowards(transform, sight.Player.position, turnSpeed);
        if (stateTimer < cooldownTime) return;

        bool tooClose = sight.DistanceToPlayer() < tooCloseDistance;
        Enter(tooClose ? State.Retreat : State.Guard);
    }

    void UpdateRetreat()
    {
        EnemyMotion.FaceTowards(transform, sight.Player.position, turnSpeed);
        if (!agent.pathPending && agent.remainingDistance <= 0.3f) Enter(State.Guard);
    }

    void StartRetreat()
    {
        Vector3 away = transform.position - sight.Player.position;
        away.y = 0f;
        Vector3 wanted = transform.position + away.normalized * retreatStep;
        if (NavMesh.SamplePosition(wanted, out NavMeshHit hit, retreatStep, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void UpdateLaser()
    {
        if (aimLaser == null) return;
        Vector3 from = muzzle != null ? muzzle.position : sight.Eye;
        aimLaser.SetPosition(0, from);
        aimLaser.SetPosition(1, sight.PlayerTarget);
    }

    void Fire()
    {
        if (projectilePrefab == null) { Debug.LogWarning(name + ": no projectile prefab"); return; }
        Vector3 from = muzzle != null ? muzzle.position : sight.Eye;
        Vector3 dir = sight.PlayerTarget - from;
        Instantiate(projectilePrefab, from, Quaternion.LookRotation(dir));
    }
}
