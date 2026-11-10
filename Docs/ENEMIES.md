# Assignment 10 — Three Enemy Types

Level: `Assets/Scenes/Level3_Enemies.unity` (build index 3, between Level2 and the boss).
Goal: beat all 3 enemies -> the green Exit appears -> boss level.
Rebuild the level any time: **Tools > Assignment 10 > Build Enemy Level**.
Quick check: **Tools > Assignment 10 > Smoke Test** (plays the level, logs states, PASS/FAIL in Console).

## Script map

```
                    Movement (player)          Health (player HP)
                         ^  position                 ^ TakeDamage
                         |                           |
   EnemySight -----------+  distance, angle,     EnemyMotion.HurtPlayer
   (shared eyes)            Raycast (walls block)    ^          EnemyProjectile
        |                                            |                ^
        v                                            |                |
 +---------------+   +----------------+   +-----------------+         |
 | PatrollerEnemy|   |  ShooterEnemy  |---+-- fires --------+---------+
 | Patrol        |   |  Guard (scan)  |   |  ChargerEnemy   |
 | Chase         |   |  Aim 0.6s laser|   |  Wander         |
 | WindUp 0.5s   |   |  Cooldown 1.6s |   |  WindUp 0.7s    |
 | Cooldown 1.2s |   |  Retreat       |   |  Charge         |
 | Search        |   +----------------+   |  Stunned 1.6s x2|
 +---------------+                        +-----------------+
        |                  |                       |
        +------ Telegraph (red flash on wind-up, blue when stunned)
        +------ EnemyHealth (player Bullet -> damage) --Died event--> EnemyArenaGoal -> Exit + "Enemies: N"
        +------ NavMeshAgent on NavMeshSurface (baked)
```

## State rules

| Enemy | From -> To | Rule |
|---|---|---|
| Patroller | Patrol -> Chase | sees player (range 12, cone 110°, Raycast) |
| | Chase -> WindUp | distance <= 1.8 and on screen |
| | WindUp -> Cooldown | after 0.5 s; hits if player still <= 2.2 |
| | Chase -> Search -> Patrol | lost line of sight; searches 3 s at last seen spot |
| Shooter | Guard -> Aim | sees player and is on screen (red laser shows the aim) |
| | Aim -> Cooldown | after 0.6 s fires a red ball; Aim -> Guard if you hide |
| | Guard/Cooldown -> Retreat | player closer than 4 m, backs off 4 m |
| Charger | Wander -> WindUp | sees player and is on screen |
| | WindUp -> Charge | after 0.7 s, direction is locked (sidestep it!) |
| | Charge -> Stunned | hit a wall or 11 m travelled; 1.6 s, takes x2 damage |

## Tuning (all are [SerializeField], change them in the Inspector)

| Enemy | Field | Value |
|---|---|---|
| Patroller | patrolSpeed / chaseSpeed | 2 / 3.6 |
| Patroller | windUpTime / cooldownTime | 0.5 / 1.2 |
| Shooter | aimTime / cooldownTime | 0.6 / 1.6 |
| Shooter | projectile speed | 9 |
| Charger | windUpTime / chargeSpeed / stunTime | 0.7 / 13 / 1.6 |
| All | EnemySight viewDistance / viewAngle | 12 / 110° |
| All | EnemyHealth maxHp | Patroller 3, Shooter 2, Charger 4 |

## Playtest (fill in after testing on the phone)

| Tester | Tries | Notes |
|---|---|---|
| | | |
| | | |

| Changed | Old | New | Why |
|---|---|---|---|
| | | | |
| | | | |
