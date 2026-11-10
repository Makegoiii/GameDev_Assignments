# DECISIONS.md — AI decision log (7 Qaraqshy)

Format: date — what I asked the agent — what it got wrong — what I changed and why.

## Week 10 — Assignment 10: Three Enemy Types

2026-11-9  I used Claude to write the code for the enemies.I created all the models and settings of the characters and enemies inside the unit myself
2026-11-10  Agent placed my old tagged "Enemy" cubes in the new level — they kill the player instantly (Movement.cs GameOver).
            Removed them; new enemies are Untagged and hurt through Health.TakeDamage, so hearts and invincibility still work.
2026-11-10  Smoke test: the Patroller never chased. Not a bug — the test put the player behind it (outside the 110° view cone).
            Kept the cone; moved the test point in front of the patroller. Patrol -> Chase -> WindUp -> Cooldown -> Search all fire.
2026-11-10  Fairness every attack waits until the enemy is on screen (EnemySight.IsOnScreen) and has a red wind-up:
            Patroller 0.5 s, Shooter 0.6 s + red laser, Charger 0.7 s. Charger is stunned 1.6 s after a dash and takes x2 damage.
.
