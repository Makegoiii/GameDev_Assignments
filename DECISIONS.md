
Format: date — what I asked the agent — what it got wrong — what I changed and why.


2026-11-9  I used Claude to write the code for the enemies.I created all the models and settings of the characters and enemies inside the unit myself
2026-11-10  Agent placed my old tagged "Enemy" cubes in the new level — they kill the player instantly (Movement.cs GameOver).
            Removed them; new enemies are Untagged and hurt through Health.TakeDamage, so hearts and invincibility still work.
2026-11-10  Smoke test: the Patroller never chased. Not a bug — the test put the player behind it (outside the 110° view cone).
            Kept the cone; moved the test point in front of the patroller. Patrol -> Chase -> WindUp -> Cooldown -> Search all fire.
2026-11-10  Fairness every attack waits until the enemy is on screen (EnemySight.IsOnScreen) and has a red wind-up:
            Patroller 0.5 s, Shooter 0.6 s + red laser, Charger 0.7 s. Charger is stunned 1.6 s after a dash and takes x2 damage.

2026-11-17  Asked Claude Code to move every balance number into ScriptableObjects (PlayerStats, EnemyStats per type,
            ItemValues, LevelData, GameConfig in Resources). I checked: scripts read the asset when they need the value,
            so I can change a number in Play Mode and see it right away. This is my live-balance proof for the video.
2026-11-17  Asked for 3 levels from data. Result: one scene (DataLevel) + LevelRunner loads Level_01/02/03.asset
            (4 / 7 / 10 enemies, 90 / 120 / 150 s). Adding a level = adding an asset, no new code.
2026-11-17  Save: JsonUtility -> save.json in persistentDataPath. I replaced my old PlayerPrefs settings with it.
            Agent first wrote straight over save.json; I asked why a crash can't break the file -> now it writes .tmp first, then copies.
            JsonUtility can't save a Dictionary, so best scores per level are a List<LevelRecord>.
2026-11-17  Agent's builder crashed: "Object at index 0 is null". Cause: NewScene() unloaded GameConfig before it was used.
            Fix: load GameConfig after NewScene().
2026-11-17  My playtest: "forward" followed the camera, not the hero, and shots went the wrong way while turning.
            I asked for hero-relative controls (stick X turns, stick Y walks), camera behind the hero, and aim assist
            (aimAssistAngle / aimAssistRange are in PlayerStats.asset).
2026-11-17  Enemies were cubes and capsules. I wanted them to look like my hero: now they copy the Hero from the player prefab,
            only the colour is different (desert tan, olive, dark brown).
2026-11-17  Yellow vision ray and red laser lines across the screen looked bad -> removed. Red blink of the body is the warning now.
2026-11-17  Changed enemy roles: Patroller runs in and hits, Rusher (replaced Charger) runs at me and shoots, Shooter stands in
            the middle behind sandbags and shoots. Removed retreat and stun from code and data.
2026-11-17  Maps are data too: LevelData.props (crates, walls, barrels, sandbags) and LevelData.theme (sun, ambient, fog).
            Desert military style, textures generated in code (MapArt), not copied from any game.
2026-11-17  Outer arena walls 3 m felt like a box -> lowered to 1.2 m.
2026-11-17  Checked the "no magic numbers" rule with a search over all scripts. The old boss/hazard scripts still had numbers:
            moved them to BossStats.asset, ItemValues.movingObstacleSpeed and GameConfig.legacyRain / survivalGoal.
            What is left in code is camera/UI/visual tuning (camera distance, blink speed, text formats), not game balance.
