import math
import os
import random
import struct
import wave

SFX_RATE = 44100
MUSIC_RATE = 22050
FADE_SECONDS = 0.004
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SFX_DIR = os.path.join(ROOT, "Assets", "Audio", "SFX")
MUSIC_DIR = os.path.join(ROOT, "Assets", "Audio", "Music")

rng = random.Random(12)


def sine(phase):
    return math.sin(2 * math.pi * phase)


def square(phase):
    return 1.0 if (phase % 1.0) < 0.5 else -1.0


def saw(phase):
    return 2.0 * (phase % 1.0) - 1.0


def triangle(phase):
    return 4.0 * abs((phase % 1.0) - 0.5) - 1.0


def noise(_phase):
    return rng.uniform(-1.0, 1.0)


def sweep(wave_fn, f0, f1, seconds, rate=SFX_RATE, curve=1.0, decay=3.0, vibrato=0.0):
    n = int(seconds * rate)
    out, phase = [], 0.0
    for i in range(n):
        t = i / n
        f = f0 + (f1 - f0) * (t ** curve)
        if vibrato:
            f *= 1.0 + vibrato * math.sin(2 * math.pi * 9 * i / rate)
        phase += f / rate
        out.append(wave_fn(phase) * math.exp(-decay * t))
    return out


def tone(wave_fn, freq, seconds, rate=SFX_RATE, decay=4.0):
    return sweep(wave_fn, freq, freq, seconds, rate, decay=decay)


def mix(*layers):
    n = max(len(l) for l, _ in layers)
    out = [0.0] * n
    for samples, gain in layers:
        for i, s in enumerate(samples):
            out[i] += s * gain
    return out


def concat(*parts):
    out = []
    for p in parts:
        out.extend(p)
    return out


def lowpass(samples, amount):
    out, y = [], 0.0
    for s in samples:
        y += amount * (s - y)
        out.append(y)
    return out


def fade_edges(samples, rate):
    n = max(1, int(FADE_SECONDS * rate))
    out = list(samples)
    for i in range(min(n, len(out))):
        g = i / n
        out[i] *= g
        out[-1 - i] *= g
    return out


def normalize(samples, peak):
    top = max(abs(s) for s in samples) or 1.0
    return [s / top * peak for s in samples]


def write_wav(path, samples, rate, peak=0.85):
    samples = normalize(fade_edges(samples, rate), peak)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(rate)
        w.writeframes(b"".join(struct.pack("<h", int(s * 32767)) for s in samples))
    print("wrote", os.path.relpath(path, ROOT), f"{len(samples) / rate:.2f}s")


def note(midi):
    return 440.0 * 2 ** ((midi - 69) / 12)


def make_sfx():
    s = {}
    s["sfx_jump_1"] = mix((sweep(square, 260, 620, 0.16, curve=0.6, decay=2.5), 0.35),
                          (sweep(sine, 260, 620, 0.16, curve=0.6, decay=2.5), 0.65))
    s["sfx_jump_2"] = mix((sweep(square, 300, 720, 0.15, curve=0.5, decay=2.5), 0.3),
                          (sweep(triangle, 300, 720, 0.15, curve=0.5, decay=2.5), 0.7))

    s["sfx_shoot_1"] = mix((sweep(square, 1100, 220, 0.12, curve=0.4, decay=4), 0.5),
                           (lowpass(sweep(noise, 1, 1, 0.08, decay=8), 0.5), 0.5))
    s["sfx_shoot_2"] = mix((sweep(square, 950, 180, 0.13, curve=0.45, decay=4), 0.5),
                           (lowpass(sweep(noise, 1, 1, 0.08, decay=8), 0.4), 0.5))

    for i, (f0, nz) in enumerate([(170, 0.5), (150, 0.6), (190, 0.45)], start=1):
        body = sweep(sine, f0, 55, 0.16, curve=0.5, decay=5)
        crack = lowpass(sweep(noise, 1, 1, 0.06, decay=10), 0.35)
        s[f"sfx_hit_{i}"] = mix((body, 1.0), (crack, nz))

    s["sfx_hurt"] = mix((sweep(saw, 520, 160, 0.3, curve=0.7, decay=3), 0.6),
                        (sweep(square, 260, 80, 0.3, curve=0.7, decay=3), 0.3))

    s["sfx_pickup_2"] = concat(tone(square, note(84), 0.06, decay=1.5),
                               tone(square, note(91), 0.16, decay=4))

    windup = mix((sweep(triangle, 300, 900, 0.4, curve=1.2, decay=0.5), 0.8),
                 (sweep(sine, 600, 1800, 0.4, curve=1.2, decay=0.5), 0.2))
    s["sfx_enemy_windup"] = lowpass(windup, 0.5)

    s["sfx_enemy_shot"] = mix((sweep(sine, 1500, 260, 0.2, curve=0.35, decay=3), 0.7),
                              (sweep(square, 1500, 260, 0.2, curve=0.35, decay=3), 0.2))

    s["sfx_enemy_death"] = mix((sweep(square, 420, 60, 0.45, curve=0.6, decay=3), 0.45),
                               (lowpass(sweep(noise, 1, 1, 0.45, decay=4), 0.25), 0.55))

    s["sfx_player_death"] = mix((sweep(saw, 620, 70, 0.95, curve=0.8, decay=2.2, vibrato=0.04), 0.5),
                                (sweep(sine, 310, 35, 0.95, curve=0.8, decay=2.2), 0.5))

    s["sfx_ui_click_1"] = mix((tone(sine, 1400, 0.045, decay=9), 0.8),
                              (tone(square, 2800, 0.02, decay=12), 0.15))
    s["sfx_ui_click_2"] = mix((tone(sine, 1250, 0.045, decay=9), 0.8),
                              (tone(square, 2500, 0.02, decay=12), 0.15))

    win_notes = [72, 76, 79, 84]
    win = concat(*[mix((tone(square, note(n), 0.13, decay=1.8), 0.35),
                       (tone(sine, note(n), 0.13, decay=1.8), 0.65)) for n in win_notes[:-1]])
    win += mix((tone(square, note(84), 0.7, decay=2.5), 0.3), (tone(sine, note(84), 0.7, decay=2.5), 0.4),
               (tone(sine, note(76), 0.7, decay=2.5), 0.3))
    s["sfx_win"] = win

    lose = concat(*[sweep(triangle, note(n), note(n) * 0.985, 0.28, decay=1.2) for n in (67, 63, 60)])
    lose += sweep(triangle, note(56), note(56) * 0.94, 0.8, decay=2.2, vibrato=0.012)
    s["sfx_lose"] = lose

    click = lambda f: mix((lowpass(sweep(noise, 1, 1, 0.05, decay=14), 0.6), 0.7), (tone(square, f, 0.04, decay=10), 0.3))
    silence = lambda sec: [0.0] * int(sec * SFX_RATE)
    s["sfx_reload"] = concat(click(1800), silence(0.45), click(900), silence(0.12), click(1400), silence(0.5), click(1100))

    for name, samples in s.items():
        write_wav(os.path.join(SFX_DIR, name + ".wav"), samples, SFX_RATE)


def make_menu_music():
    bpm = 84
    beat = 60.0 / bpm
    bars_per_chord = 2
    chords = [(57, [57, 60, 64]), (53, [53, 57, 60]), (48, [52, 55, 60]), (55, [55, 59, 62])]
    loops = 2
    beats_total = len(chords) * bars_per_chord * 4 * loops
    n = int(beats_total * beat * MUSIC_RATE)
    out = [0.0] * n

    def add(start_sec, samples, gain):
        start = int(start_sec * MUSIC_RATE)
        for i, v in enumerate(samples):
            j = (start + i) % n
            out[j] += v * gain

    chord_len = bars_per_chord * 4 * beat
    t = 0.0
    for _ in range(loops):
        for bass, triad in chords:
            pad_len = chord_len + 0.6
            for m in triad:
                add(t, pad_tone(note(m), pad_len), 0.18)
            add(t, pluck(note(bass - 12), chord_len), 0.35)
            arp = triad + [triad[0] + 12, triad[1] + 12, triad[0] + 12]
            steps = int(chord_len / (beat / 2))
            for k in range(steps):
                m = arp[k % len(arp)] + 12
                add(t + k * beat / 2, pluck(note(m), beat * 0.9), 0.10)
            t += chord_len

    out = lowpass(out + out, 0.45)[n:]
    out = normalize(out, 0.7)
    os.makedirs(MUSIC_DIR, exist_ok=True)
    path = os.path.join(MUSIC_DIR, "music_menu.wav")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(MUSIC_RATE)
        w.writeframes(b"".join(struct.pack("<h", int(s * 32767)) for s in out))
    print("wrote", os.path.relpath(path, ROOT), f"{len(out) / MUSIC_RATE:.2f}s")


def pad_tone(freq, seconds):
    count = int(seconds * MUSIC_RATE)
    attack, release = 0.5, 0.6
    res = []
    for i in range(count):
        tt = i / MUSIC_RATE
        env = min(1.0, tt / attack) * min(1.0, (seconds - tt) / release)
        v = sine(freq * tt) + 0.3 * sine(freq * 2.003 * tt) + 0.15 * triangle(freq * 0.998 * tt)
        res.append(v * max(0.0, env))
    return res


def pluck(freq, seconds):
    count = int(seconds * MUSIC_RATE)
    res = []
    for i in range(count):
        tt = i / MUSIC_RATE
        env = min(1.0, tt / 0.005) * math.exp(-4.0 * tt / seconds)
        res.append((triangle(freq * tt) * 0.7 + sine(freq * tt) * 0.3) * env)
    return fade_edges(res, MUSIC_RATE)


if __name__ == "__main__":
    make_sfx()
    make_menu_music()
