
Format: date — what I asked the agent — what it got wrong — what I changed and why.


2026-11-10  Asked Claude Code for 3 enemy types with a state machine each (Patroller, Shooter, Charger) on NavMesh.
            It made one shared EnemySight (distance + angle + Raycast) and a separate FSM script per enemy, so they are not 3 skins of one script.
2026-11-10  Agent placed my old tagged "Enemy" cubes in the new level — they kill the player instantly (Movement.cs GameOver).
            Removed them; new enemies are Untagged and hurt through Health.TakeDamage, so hearts and invincibility still work.
2026-11-10  Smoke test: the Patroller never chased. Not a bug — the test put the player behind it (outside the 110° view cone).
            Kept the cone; moved the test point in front of the patroller. Patrol -> Chase -> WindUp -> Cooldown -> Search all fire.
2026-11-10  Fairness: every attack waits until the enemy is on screen (EnemySight.IsOnScreen) and has a red wind-up:
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


2026-11-24  Asked Claude Code for the A12 sound pass: mixer (Master/Music/SFX), 8+ events, 2 music tracks, saved dB sliders, pause snapshot.
            Unity has no public API to make mixer groups/snapshots, so the agent made AudioMixerFactory (reflection, same calls as the
            Audio Mixer window) and tested it in an empty project first. The mixer file is in the repo, the factory only rebuilds it.
2026-11-24  First plan: pause snapshot lowers the Music group. Problem: an exposed parameter (my slider) ignores snapshots.
            Fix: slider volume on MusicBus, pause volume + lowpass on its child Music. Old code used AudioListener.pause = everything silent,
            not "softer" -> removed, now Paused snapshot. Mixer update mode = UnscaledTime, because pause sets timeScale 0.
2026-11-24  Sliders: dB = Log10(v) * 20 (VolumeMath), 0 -> -80 dB. sfxVol added to save.json next to musicVol; an old save without it
            gets the default 1.0 (test: OldSaveWithoutSfxVolume_GetsDefault). Every scene had its own music AudioSource that restarted
            on load -> removed, one AudioManager with DontDestroyOnLoad and a 1 s sin/cos crossfade, same track = no restart.
2026-11-24  Health.GameOver reloads the scene at once, so a death sound on the player object was cut off. Sounds now play from the
            AudioManager pool (survives the reload). Hit/jump/shoot/click: pitch 0.92-1.08 + 2-3 clips, never the same clip twice in a row.
2026-11-24  My playtest: a buzzing sound at the start of a level. Cause: Shooter/Rusher go Aim -> Guard -> Aim many times per second
            while the player is at the edge of their view, and every Aim played the saw-wave wind-up. Fix: Telegraph plays the sound
            once per attack (no replay while blinking, min 1.5 s per enemy), 0.3 s global cooldown, new softer "whoop" clip.
2026-11-24  Asked for 3 different maps. One arena scene stays; LevelTheme got floor/wall/prop colours and PropType got Tree and Lamp.
            Level 1 Desert Outpost, 2 Snow Base (white fog, pines), 3 Night Factory (dark, street lamps with point lights). Only data changed.
2026-11-24  Asked for Counter-Strike controls: first person, mouse/right-half-drag look, WASD strafe, hitscan from the crosshair,
            spread (move/jump/spray) + recoil, 30-round magazine and reload. Agent first let spray recover between shots, so it never grew;
            now it recovers only after you stop firing. Fire rate 0.3 s felt slow for a rifle -> 0.12 s.
2026-11-24  My playtest: you walk at the start, then the controls stop. Found 3 causes: the level has TWO MainCamera cameras
            (scene + old prefab), so the moved camera was not always the one you see -> FpsLook keeps one, turns the others off.
            Full-screen UI panels counted as "button", so after Esc the mouse never locked again and touch-look never started
            -> only Selectable / on-screen controls block now. Movement now reads keyboard + all gamepads (incl. the on-screen
            stick) directly instead of PlayerInput.OnMove. Smoke test: W+D, wait 5 s, S+A -> the player moves both times.
2026-11-24  Maps were too dark (night level sun 0.35) -> brighter light and ambient everywhere, level 3 is evening now.
            Asked for maze maps instead of one round arena: wrote make_mazes.py (DFS maze + flood-fill check of every point).
            First try: half the walls vanished because points sat on grid lines -> walls are split in halves; maze corners
            went through the round wall -> clipped; the exit was cut off -> doors in the middle of each side.
2026-11-24  Wanted the hero visible like Free Fire, not first person. FpsLook -> PlayerLook: camera over the right shoulder,
            SphereCast pulls it in front of maze walls. Shots: ray from the crosshair, counted only from the hero's depth on.
2026-11-24  Lag on the phone: 70+ maze walls each with its own MaterialPropertyBlock turned the URP SRP Batcher off, and the
            scene had two shadow-casting suns. Now one tinted material per colour (shared) and one sun; HUD text only rebuilds
            when the ammo changes. Button text vanished because the agent copied TMP font atlas files from its test copy
            over mine -> restore them from git (git checkout of the two TMP font assets).
2026-11-24  Asked for square maps with three different schemes (not three mazes). make_mazes.py -> make_maps.py with three
            layouts: town (buildings), maze, rooms. Square arena from data (arenaHalfSize); NavMesh baked at runtime after the
            map is built instead of carving obstacles. Smoke test found the new floor was on the wrong layer, so the player
            was never "grounded" (spread stuck at the jump value) -> the floor now copies the old floor's layer.
2026-11-25  Controls still froze in my test. Real cause: on PC/editor the cursor got locked on the first click, after that the
            on-screen joystick and buttons could not be clicked. Removed the cursor lock: look = drag on the right half
            (finger or left mouse), fire = FIRE / X / right mouse. Smoke test: a 200 px drag turns the view 40 degrees.
2026-11-25  Removed waves: each level now has 3 enemies (one Patroller, Shooter, Rusher) from the start, HUD shows
            "Enemies: N". Maps regenerated with make_maps.py for the new positions.

