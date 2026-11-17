# Coin Quest 3D

A 3D mobile platformer (Android / iOS, landscape) made in Unity for
Game Development Fundamentals, Narxoz University. Weeks 9–13 project (RK2).

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

## Controls
| On screen (phone) | PC | Action |
|---|---|---|
| Joystick, bottom-left | WASD / arrows | Move |
| JUMP, bottom-right | Space | Jump |
| FIRE, bottom-right | X / left mouse | Shoot (hold = auto fire) |
| II, top-right | Esc | Pause (Resume / Restart / Menu) |

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

## Assets and licenses
- Character and animations: Mixamo (Adobe), free for games.
- Sounds: Pixabay (Pixabay Content License) – coin-pickup, game-start (freesound_community),
  procedural-sonic-detonation (joe_bou_khalil), game-bonus-02 (universfield), that-game-arcade (moodmode).
- Heart icon: made for this project. Level geometry: Unity primitives with own materials.
- TextMesh Pro: Unity.

## AI use
See [`AI_LOG.txt`](AI_LOG.txt) and [`DECISIONS.md`](DECISIONS.md).
