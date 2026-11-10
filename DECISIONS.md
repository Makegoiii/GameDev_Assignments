# DECISIONS.md — AI decision log (Coin Quest 3D)

Format: date — what I asked the agent — what it got wrong — what I changed and why.

## Week 10 — Assignment 10: Three Enemy Types

2026-11-10  Asked Claude Code for 3 enemy types with a state machine each (Patroller, Shooter, Charger) on NavMesh.
            It made one shared EnemySight (distance + angle + Raycast) and a separate FSM script per enemy, so they are not 3 skins of one script.
2026-11-10  Agent placed my old tagged "Enemy" cubes in the new level — they kill the player instantly (Movement.cs GameOver).
            Removed them; new enemies are Untagged and hurt through Health.TakeDamage, so hearts and invincibility still work.
2026-11-10  Smoke test: the Patroller never chased. Not a bug — the test put the player behind it (outside the 110° view cone).
            Kept the cone; moved the test point in front of the patroller. Patrol -> Chase -> WindUp -> Cooldown -> Search all fire.
2026-11-10  Fairness: every attack waits until the enemy is on screen (EnemySight.IsOnScreen) and has a red wind-up:
            Patroller 0.5 s, Shooter 0.6 s + red laser, Charger 0.7 s. Charger is stunned 1.6 s after a dash and takes x2 damage.

<!-- TODO (me): fill after the phone playtest — real names, real tries, real numbers. -->
YYYY-MM-DD  Playtest: <tester 1> __ tries, <tester 2> __ tries.
YYYY-MM-DD  Changed <field> <old> -> <new> because ...
YYYY-MM-DD  Changed <field> <old> -> <new> because ...
