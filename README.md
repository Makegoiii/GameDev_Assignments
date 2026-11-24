# Coin Quest 3D

A 3D mobile platformer (Android / iOS, landscape) made in Unity for
Game Development Fundamentals, Narxoz University. Weeks 9–13 project (RK2). Assignment 12: full sound pass (see "Sound").

**Author:** Magzhan Kydyrbai (solo until week 14)

## About
The hero runs, jumps, collects coins and shoots. Collect every coin to open the gate,
reach the exit, survive the corridor of blades and rocks, and beat the boss.

- **Since Assignment 11 the build plays three data levels in one arena (see "Data and saves").** Older levels:
- Level 1 – learning level: coins, gate, exit.
- Level 2 – long corridor: holes, spinning blades, falling rocks.
- Level 3 – enemy arena (Assignment 10): Patroller, Shooter and Charger with their own state machines; beat all three to open the exit. See [`Docs/ENEMIES.md`](Docs/ENEMIES.md).
- Level 4 – boss fight: fireballs and bombs from the sky; hit the boss 15 times.
- Win screen.

Full design: [`Docs/GDD.md`](Docs/GDD.md) (PDF: `Docs/GDD.pdf`).

## Controls (Free Fire style third person)
| Phone | Editor / PC | Action |
|---|---|---|
| Joystick, bottom-left | WASD / arrows | Walk forward/back, strafe left/right |
| Drag on the right half of the screen | Drag with the left mouse button on the right half | Look / aim |
| FIRE, bottom-right (hold = spray) | FIRE button, X or right mouse button (hold) | Shoot (hitscan from the crosshair) |
| Tap the ammo counter | R | Reload (30 rounds, 2 s, auto when empty) |
| JUMP | Space | Jump |
| II, top-right | Esc | Pause |

The cursor is never locked, so the on-screen joystick and buttons always work with the mouse too.
Gamepad: left stick move, right stick look, West / right trigger fire.
Shooting feels like CS: first bullet is accurate when standing, spread grows when running / jumping /
spraying (crosshair opens), recoil lifts the view, spray recovers when you stop. Phone only: 4° aim assist.
Camera: over the right shoulder (3.2 m behind, 0.6 m right), pulled in when a maze wall is behind the hero.
All numbers are in `PlayerStats.asset` (look / third-person camera / weapon). Scripts: `PlayerLook`, `Shooter`, `CrosshairHud`.

## Three maps: square arena, three different schemes, 3 enemies each (data only)
| Level | Scheme | Style |
|---|---|---|
| 1 Desert Outpost | **Town**: 4 buildings with doors, a square in the middle, streets around | sand, bright sun |
| 2 Snow Base | **Maze**: 6×6 depth-first-search maze | snow, icy walls, white fog, pines |
| 3 Night Factory | **Rooms**: 3×3 warehouse rooms, a door in every wall | evening light, rusty walls, street lamps |

The arena is a 25 × 25 m square (`LevelData.arenaHalfSize`); `MapBuilder` hides the scene's round arena,
builds the floor + outer walls + props, then bakes the NavMesh at runtime (`NavMeshSurface.BuildNavMesh`),
so enemies walk around every wall.
No waves: every level has exactly one Patroller, one Shooter and one Rusher from the start; kill all 3 → exit opens. Layouts come from `Tools/level_gen/make_maps.py`: it never puts a wall
on a coin, enemy, patrol point, start or exit (that spot becomes a door) and checks with a flood fill that
everything can be reached; otherwise it writes nothing.

## UI screens (Assignment 8–9)
- **Main Menu** – title, Play, Levels, Settings, How to play, Quit.
- **HUD** – hearts, score (coins), timer, pause button, on-screen controls.
- **Pause** – Resume, Restart, Menu; `Time.timeScale = 0` while paused.

All canvases: *Scale With Screen Size*, 1920 × 1080, Match = 1 (landscape).
Buttons and text are inside a `SafeArea` panel (`Assets/Script/UI/SafeArea.cs`);
background art stays full screen. Buttons are at least 120 × 120 px, text at least 36 px.
Tested in Device Simulator: 16:9 phone, notch phone (19.5:9), 4:3 iPad.

Pause logic: `Assets/Script/UI/PauseMenu.cs`.

## How to open
1. Clone the repo and open the folder in Unity Hub, **Unity 6000.6.0f1** (URP).
2. Open `Assets/Main menu.unity` and press Play.

## Data and saves (Assignment 11)
All balance numbers live in data assets, not in scripts:

| Asset | What is inside |
|---|---|
| `Assets/Data/PlayerStats.asset` | speed, jump, HP, invincibility, shooting |
| `Assets/Data/Enemies/EnemyStats_*.asset` | HP, speed, damage, vision range, wind-up … per enemy type |
| `Assets/Data/BossStats.asset` | boss HP, attack rhythm, fireballs (old boss scene) |
| `Assets/Data/ItemValues.asset` | score per coin / enemy / second, heart heal, rock and bomb damage |
| `Assets/Data/Levels/Level_01..03.asset` | time limit, map props, light/fog theme, player start, coins, enemy waves, hazard rain |
| `Assets/Resources/GameConfig.asset` | links everything, scene names, default settings |

One scene (`Assets/Scenes/DataLevel.unity`) and one script (`LevelRunner`) load any level asset.
Progress and settings are saved as JSON in `Application.persistentDataPath/save.json`
(`SaveSystem`); a missing or broken file falls back to defaults. Script map: [`Docs/SCRIPT_MAP_A11.md`](Docs/SCRIPT_MAP_A11.md).

Setup after cloning: **Tools → Assignment 11 → Build Everything** (makes enemy prefabs, the DataLevel scene,
the level select / reset buttons and the build list). Tests: **Window → General → Test Runner → EditMode**.

## Scenes (Build Profiles order)
0 Main menu · 1 DataLevel (levels 1–3 from data) · 2 Win.
Older hand-made scenes (Level1, Level2, Level3_Enemies, boss Level3) stay in the project but are not in the build.

## Sound (Assignment 12)
- **AudioMixer** `Assets/Audio/GameMixer.mixer`: Master → MusicBus → **Music**, Master → **SFX**.
  Every AudioSource is routed: music → Music, everything else → SFX.
- **14 sound events** (`SoundEvent` enum): jump, shoot, hit, player hurt, pickup, heal, **enemy wind-up**,
  enemy shot, explosion, enemy death, player death, UI click, win, lose.
- **Enemy wind-up** sound starts in `Telegraph.StartWarning()`, the same frame the red blink starts
  (0.5–0.7 s before the attack lands).
- **Two music tracks** (menu + level), 1 s equal-power crossfade, one `AudioManager` with `DontDestroyOnLoad`:
  reloading a level does not restart the music.
- **Volume sliders** Music + SFX in Settings, converted to dB with `Log10(v) × 20`, saved in `save.json`.
- **Pause** → mixer snapshot *Paused*: music −10 dB + lowpass 900 Hz (mixer runs on unscaled time).
- **Variation**: random pitch 0.92–1.08 and 2–3 clips for jump, shoot, hit, pickup, UI click.

Script map and video plan: [`Docs/SOUND.md`](Docs/SOUND.md).
Setup after cloning: **Tools → Assignment 12 → Build Sound Pass** (mixer, SoundLibrary, routing, sliders).

## Sound sources and licenses
| File | Used for | Source | License |
|---|---|---|---|
| `Assets/Scenes/moodmode-that-game-arcade-236111.mp3` | level music | Pixabay, "That Game Arcade" by moodmode | Pixabay Content License |
| `Assets/Audio/Music/music_menu.wav` | menu music | made for this project with `Tools/sound_gen/make_sounds.py` | own work, CC0 |
| `Assets/freesound_community-coin-pickup-98269.mp3` | pickup (clip 1) | Pixabay, "coin-pickup" by freesound_community | Pixabay Content License |
| `Assets/universfield-game-bonus-02-294436.mp3` | heal (heart) | Pixabay, "game-bonus-02" by universfield | Pixabay Content License |
| `Assets/joe_bou_khalil-procedural-sonic-detonation-570270.mp3` | bomb explosion | Pixabay, "procedural-sonic-detonation" by joe_bou_khalil | Pixabay Content License |
| `Assets/freesound_community-game-start-6104.mp3` | not used any more | Pixabay, "game-start" by freesound_community | Pixabay Content License |
| `Assets/Audio/SFX/sfx_*.wav` (17 files: jump ×2, shoot ×2, hit ×3, hurt, pickup_2, enemy_windup, enemy_shot, enemy_death, player_death, ui_click ×2, win, lose) | sound effects | synthesised for this project with `Tools/sound_gen/make_sounds.py` (sine/square/saw/noise, no samples) | own work, CC0 |

## Other assets and licenses
- Character and animations: Mixamo (Adobe), free for games.
- Heart icon: made for this project. Level geometry: Unity primitives with own materials.
- TextMesh Pro: Unity.

## AI use
See [`AI_LOG.txt`](AI_LOG.txt) and [`DECISIONS.md`](DECISIONS.md).
