# Script map — Assignment 11 (Data and Saves)

Picture: [`ScriptMap_A11.pdf`](ScriptMap_A11.pdf) / [`ScriptMap_A11.png`](ScriptMap_A11.png)

```
                    Assets/Resources/GameConfig.asset   (GameConfig.Instance)
                     │ player ─► PlayerStats.asset ◄── Movement, Health, Shooter, Bullet
                     │ items  ─► ItemValues.asset  ◄── Hazard, Heart, BombHazard, LevelRunner (score)
                     │ boss   ─► BossStats.asset   ◄── Boss, Fireball (old boss scene)
                     │ levels ─► Level_01/02/03.asset (LevelData: props, theme, coins, waves, hazards)
                     │ enemy prefabs ─► EnemyStats_Patroller/Shooter/Rusher.asset
                     │                    ◄── PatrollerEnemy, ShooterEnemy, RusherEnemy, EnemySight, EnemyHealth
                     ▼
 Main menu ── MenuButtons.Play / LevelSelectMenu ──► LevelFlow.StartLevel(n) ──► DataLevel scene
     │            ▲ reads unlocked + best from                                   │
     │            │                                                              ▼
     │        SaveSystem (save.json) ◄─── RecordLevelComplete ◄────────── LevelRunner
     │            ▲   ▲                                                   (MapBuilder props + theme,
     │            │   └── SaveLifecycle: OnApplicationPause(true), quit     coins, waves, HazardRain,
     │            │                                                         exit, score)
     └── ProfileSettings / MenuButtons.SetVolume (save on every change)          │
     └── ResetProgressPanel (confirm Yes/No) ──► SaveSystem.ResetProgress        ▼
                                                                         LevelFlow.GoToNextAfter
                                                                         (next level or Win)
```

**Why this shape**
- Scripts never own balance numbers; they read the asset when they need it, so a value changed in the Inspector during Play Mode changes the game immediately (live balance).
- One `LevelRunner` + one scene: a new level is a new `LevelData` asset, no new code.
- `SaveSystem` is the only class that touches the file. Save rules (defaults, repair, temp-file write) are in one place.
- `SaveLifecycle` exists because a phone can kill a paused app without calling `OnApplicationQuit`.

**Score**
coins × `coinScore` + kills × `enemyScore` + seconds left × `timeBonusPerSecond` + `LevelData.clearBonus`.
