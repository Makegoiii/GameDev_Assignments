# Sound pass (Assignment 12): script map

## Who talks to whom

```
 Gameplay scripts                       Audio layer                                Unity
 ────────────────                       ───────────                                ─────
 Movement   (jump, coin, lose) ──┐
 Health     (hurt, heal, death) ─┤
 Shooter    (shoot)             ─┤
 Telegraph  (enemy WIND-UP)     ─┼──► Sfx.Play / Sfx.PlayAt ──► AudioManager ──► SfxPlayer (16 pooled AudioSources)
 EnemyHealth (hit, enemy death) ─┤        (static facade)          │                │  random clip + pitch 0.92–1.08
 EnemyMotion (enemy shot)       ─┤                                 │                └──► Mixer group SFX
 BombHazard (explosion)         ─┤                                 │
 LevelRunner / WinScreen (win)  ─┘                                 ├──► MusicPlayer (2 AudioSources, 1 s crossfade)
                                                                   │        └──► Mixer group Music (child of MusicBus)
 Every Button / Toggle ──(listener added on scene load)──► UI click │
                                                                   ├── sceneLoaded: menu/win scene → menu track,
 PauseMenu.SetPaused ─────────────────────────────────────────────►│               level scene → level track
                                                                   │   (same track → nothing happens, no restart)
                                                                   ├── SetPaused → snapshot Paused / Gameplay
 Settings sliders ─► ProfileSettings / MenuButtons                 │
                       └─► SaveSystem.SetMusicVolume/SetSfxVolume ─┴── SaveSystem.Changed → ApplyVolumes
                              (save.json)                                 → VolumeMath.ToDecibels → mixer.SetFloat
```

**Why this shape**
- Gameplay code only says *what happened* (`SoundEvent.Jump`). Which clip, how loud, 2D or 3D lives in one asset,
  `Assets/Resources/Audio/SoundLibrary.asset`, so I can re-mix without touching code.
- `AudioManager` is created before the first scene (`RuntimeInitializeOnLoadMethod`) and kept with `DontDestroyOnLoad`.
  That is why music does not restart on a level reload and why a death sound is not cut off when the scene reloads.
- Volumes are saved by the existing `SaveSystem` (week 11). `AudioManager` only listens to `SaveSystem.Changed`.

## Mixer `Assets/Audio/GameMixer.mixer`

| Group | Exposed parameter | Used by |
|---|---|---|
| Master | `MasterVolume` | (0 dB) |
| └ MusicBus | `MusicVolume` | Music slider |
|   └ Music | – | music sources; Paused snapshot: −10 dB, Lowpass 900 Hz |
| └ SFX | `SFXVolume` | SFX slider; every other AudioSource |

Snapshots: **Gameplay** (start) and **Paused**. Transition 0.3 s. Update mode = *Unscaled Time*,
because pause sets `Time.timeScale = 0` and a normal mixer would freeze the transition.

Why MusicBus: an exposed parameter ignores snapshots. So the player's volume is on MusicBus and
the pause effect is on Music.

## Sound events

| Event | Fired in | Clips | Notes |
|---|---|---|---|
| Jump | `Movement.OnJump` | sfx_jump_1/2 | pitch random |
| Shoot | `Shooter.Shoot` | sfx_shoot_1/2 | pitch random |
| Hit | `EnemyHealth.TakeDamage`, `Boss.TakeDamage` | sfx_hit_1/2/3 | 3D 40 % |
| PlayerHurt | `Health.TakeDamage` | sfx_hurt | |
| Pickup | `Movement` coin trigger | coin-pickup.mp3, sfx_pickup_2 | |
| Heal | `Health.Heal` (heart) | game-bonus-02.mp3 | |
| **EnemyWindUp** | `Telegraph.StartWarning` | sfx_enemy_windup (0.4 s rising, once per attack) | same frame as red blink, before the hit |
| EnemyShot | `EnemyMotion.Fire` | sfx_enemy_shot | 3D 50 % |
| Explosion | `BombHazard.PlayFx` | detonation.mp3 | |
| EnemyDeath | `EnemyHealth.Die` | sfx_enemy_death | |
| PlayerDeath | `Health.GameOver`, `Movement.GameOver` | sfx_player_death | |
| UiClick | every Button / Toggle | sfx_ui_click_1/2 | |
| Win | `LevelRunner.OnExitReached`, `WinScreen.Start` | sfx_win | |
| Lose | game over, time out, fall | sfx_lose | |
| Reload | `Shooter.Reload` | sfx_reload | |

Music: menu = `music_menu.wav` (own, CC0), level = `moodmode-that-game-arcade` (Pixabay).

## Tests (Window → General → Test Runner → EditMode → `SoundTests`)
dB conversion, Music/SFX volume survive a "restart" (`ForgetCachedData`), old save without `sfxVol`,
every SoundEvent has a clip, repeated sounds have variation, mixer has groups + both snapshots + exposed params,
two different music tracks.

## If the mixer must be made by hand
Window → Audio → Audio Mixer → `+` → `GameMixer` in `Assets/Audio`. Groups as in the table. Right-click the
Volume of MusicBus / SFX → *Expose*, rename to `MusicVolume` / `SFXVolume`. Add snapshot `Paused`, on Music:
volume −10 dB and an effect *Lowpass Simple* 900 Hz (Gameplay: 22000 Hz). Then run Build Sound Pass.

## Video plan (2–3 min, sound on, English)
1. Menu: menu music, UI clicks. Settings: move Music and SFX sliders (you hear the SFX preview).
2. Press Play → **crossfade** menu → level music (1 s). Restart the level from pause → music keeps going.
3. The 8+ events: jump, shoot, coin, hit an enemy, heart, explosion, enemy death, get hurt, win / lose.
4. **Ears test**: stand so an enemy is at the screen edge, turn away / close eyes: say "I hear it" on the
   rising wind-up, then the attack lands.
5. **Pause**: music gets quieter and muffled (Paused snapshot); resume → back.
6. **Settings test**: set Music low, close the app completely, open again → same value.
7. Script map: show this diagram and say what talks to what and why.
