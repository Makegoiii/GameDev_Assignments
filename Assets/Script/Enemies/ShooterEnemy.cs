using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemySight))]
public class ShooterEnemy : MonoBehaviour
{
    public enum State { Guard, Aim, Cooldown }

    [SerializeField] EnemyStats stats;
    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] Transform muzzle;

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
        agent.updateRotation = false;
        if (stats == null) { Debug.LogError(name + ": no EnemyStats"); enabled = false; }
    }

    void Start()
    {
        agent.isStopped = true;
        Enter(State.Guard);
    }

    void Update()
    {
        if (PauseMenu.IsPaused || sight.Player == null) return;
        stateTimer += Time.deltaTime;

        switch (state)
        {
            case State.Guard:    UpdateGuard();    break;
            case State.Aim:      UpdateAim();      break;
            case State.Cooldown: UpdateCooldown(); break;
        }
    }

    void Enter(State next)
    {
        state = next;
        stateTimer = 0f;
        if (telegraph == null) return;
        if (next == State.Aim) telegraph.StartWarning();
        else telegraph.Clear();
    }

    void UpdateGuard()
    {
        if (!sight.CanSeePlayer())
        {
            transform.Rotate(0f, stats.scanSpeed * Time.deltaTime, 0f);
            return;
        }
        EnemyMotion.FaceTowards(transform, sight.Player.position, stats.turnSpeed);
        if (sight.IsOnScreen()) Enter(State.Aim);
    }

    void UpdateAim()
    {
        EnemyMotion.FaceTowards(transform, sight.Player.position, stats.turnSpeed);
        if (!sight.CanTrackPlayer()) { Enter(State.Guard); return; }
        if (stateTimer < stats.windUpTime) return;

        EnemyMotion.Fire(projectilePrefab, MuzzlePos, sight.PlayerTarget, stats, name);
        Enter(State.Cooldown);
    }

    void UpdateCooldown()
    {
        if (sight.CanTrackPlayer())
            EnemyMotion.FaceTowards(transform, sight.Player.position, stats.turnSpeed);
        if (stateTimer >= stats.cooldownTime) Enter(State.Guard);
    }

    Vector3 MuzzlePos => muzzle != null ? muzzle.position : sight.Eye;
}
