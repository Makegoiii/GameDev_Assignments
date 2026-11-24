import os
import random
import re
import subprocess
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))

ARENA_HALF = 12.5
INNER = 12.0
WALL_THICK = 0.4
WALL_HEIGHT = 2.6
PIECE = 1.0
KEEP_CLEAR = 1.1
DOOR = 2.2
AGENT_RADIUS = 0.55
GRID = 0.25
MAX_ATTEMPTS = 50
EXIT = (0.0, 10.0)
WALL, CRATE, BARREL, SANDBAGS, TREE, LAMP = 1, 0, 2, 3, 4, 5


def town(rng):
    segs = []
    for cx in (-7.0, 7.0):
        for cz in (-6.0, 6.0):
            x1, x2, z1, z2 = cx - 3.0, cx + 3.0, cz - 2.5, cz + 2.5
            walls = [(x1, z1, x2, z1), (x1, z2, x2, z2), (x1, z1, x1, z2), (x2, z1, x2, z2)]
            door = rng.randrange(4)
            for i, w in enumerate(walls):
                segs += door_wall(w) if i == door else [w]
    return segs


def maze(rng, n=6, braid=0.08):
    cell = 2 * INNER / n
    walls = {((x, z), d) for x in range(n) for z in range(n) for d in "EN"}
    seen, stack = {(0, 0)}, [(0, 0)]
    while stack:
        x, z = stack[-1]
        options = [(x + dx, z + dz) for dx, dz in ((1, 0), (-1, 0), (0, 1), (0, -1))
                   if 0 <= x + dx < n and 0 <= z + dz < n and (x + dx, z + dz) not in seen]
        if not options:
            stack.pop()
            continue
        nx, nz = rng.choice(options)
        walls.discard(((min(x, nx), z), "E") if nx != x else ((x, min(z, nz)), "N"))
        seen.add((nx, nz))
        stack.append((nx, nz))
    segs = []
    for (x, z), d in sorted(walls):
        if (d == "E" and x == n - 1) or (d == "N" and z == n - 1) or rng.random() < braid:
            continue
        x0, z0 = -INNER + x * cell, -INNER + z * cell
        segs.append((x0 + cell, z0, x0 + cell, z0 + cell) if d == "E" else (x0, z0 + cell, x0 + cell, z0 + cell))
    return segs


def rooms(rng):
    cuts = (-INNER, -4.0, 4.0, INNER)
    segs = []
    for a in (-4.0, 4.0):
        for b1, b2 in zip(cuts, cuts[1:]):
            segs += door_wall((a, b1, a, b2))
            segs += door_wall((b1, a, b2, a))
    return segs


SCHEMES = {1: (town, 101), 2: (maze, 202), 3: (rooms, 303)}


def door_wall(s):
    x1, z1, x2, z2 = s
    mx, mz = (x1 + x2) / 2, (z1 + z2) / 2
    k = (DOOR / 2) / (abs(x2 - x1) + abs(z2 - z1))
    return [(x1, z1, mx - (x2 - x1) * k, mz - (z2 - z1) * k),
            (mx + (x2 - x1) * k, mz + (z2 - z1) * k, x2, z2)]


def pieces(s):
    x1, z1, x2, z2 = s
    n = max(1, round((abs(x2 - x1) + abs(z2 - z1)) / PIECE))
    return [(x1 + (x2 - x1) * i / n, z1 + (z2 - z1) * i / n,
             x1 + (x2 - x1) * (i + 1) / n, z1 + (z2 - z1) * (i + 1) / n) for i in range(n)]


def normalized(s):
    return (min(s[0], s[2]), min(s[1], s[3]), max(s[0], s[2]), max(s[1], s[3]))


def merge(segs):
    vertical = sorted((s for s in map(normalized, segs) if s[0] == s[2]), key=lambda s: (s[0], s[1]))
    horizontal = sorted((s for s in map(normalized, segs) if s[0] != s[2]), key=lambda s: (s[1], s[0]))
    out = []
    for group, along in ((vertical, 1), (horizontal, 0)):
        for s in group:
            p = out[-1] if out else None
            same_line = p is not None and (p[0] == p[2]) == (s[0] == s[2]) and p[1 - along] == s[1 - along]
            if same_line and abs(p[along + 2] - s[along]) < 1e-6:
                out[-1] = (p[0], p[1], s[2], s[3])
            else:
                out.append(s)
    return out


def dist_point_seg(p, s):
    (px, pz), (x1, z1, x2, z2) = p, s
    dx, dz = x2 - x1, z2 - z1
    t = max(0.0, min(1.0, ((px - x1) * dx + (pz - z1) * dz) / (dx * dx + dz * dz)))
    return ((px - x1 - t * dx) ** 2 + (pz - z1 - t * dz) ** 2) ** 0.5


def cut_doors(segs, points):
    kept = [p for s in segs for p in pieces(s) if all(dist_point_seg(q, p) > KEEP_CLEAR for q in points)]
    return merge(kept)


def seg_to_prop(s):
    x1, z1, x2, z2 = s
    length = abs(x2 - x1) + abs(z2 - z1) + WALL_THICK
    size = (length, WALL_HEIGHT, WALL_THICK) if z1 == z2 else (WALL_THICK, WALL_HEIGHT, length)
    return (WALL, (x1 + x2) / 2, (z1 + z2) / 2, *size, 0.0)


POINT = r"\{x: ([-\d.]+), y: [-\d.]+, z: ([-\d.]+)\}"
PROP = (r"  - type: (\d)\n    position: \{x: ([-\d.]+), y: [-\d.]+, z: ([-\d.]+)\}\n"
        r"    size: \{x: ([-\d.]+), y: ([-\d.]+), z: ([-\d.]+)\}\n    yaw: ([-\d.]+)\n")


def parse_props(block):
    return [(int(t), float(x), float(z), float(sx), float(sy), float(sz), float(yaw))
            for t, x, z, sx, sy, sz, yaw in re.findall(PROP, block)]


def section(text, start, end):
    return text[text.index(start):text.index(end)]


def read_level(rel):
    text = open(os.path.join(ROOT, rel), encoding="utf-8").read()
    committed = subprocess.run(["git", "show", "HEAD:" + rel], capture_output=True, text=True, cwd=ROOT)
    base = committed.stdout if committed.returncode == 0 else text
    cover = [p for p in parse_props(section(base, "  props:", "  theme:")) if p[0] not in (WALL, TREE, LAMP)]
    decor = [p for p in parse_props(section(text, "  props:", "  theme:")) if p[0] in (TREE, LAMP)]
    start = re.search(r"playerStart: " + POINT, text)
    points = [(float(start.group(1)), float(start.group(2))), EXIT]
    for block in (section(text, "  coins:", "  props:"), section(text, "  waves:", "  hazards:")):
        points += [(float(x), float(z)) for x, z in re.findall(POINT, block)]
    return text, cover + decor, points


def prop_overlaps(prop, segs):
    _, x, z, sx, _, sz, _ = prop
    return any(dist_point_seg((x, z), s) < max(sx, sz) * 0.5 + WALL_THICK for s in segs)


def fmt(v):
    return f"{round(v, 3):g}"


def props_yaml(props):
    out = ["  props:\n"]
    for t, x, z, sx, sy, sz, yaw in props:
        out.append(f"  - type: {t}\n    position: {{x: {fmt(x)}, y: 0, z: {fmt(z)}}}\n"
                   f"    size: {{x: {fmt(sx)}, y: {fmt(sy)}, z: {fmt(sz)}}}\n    yaw: {fmt(yaw)}\n")
    return "".join(out)


def set_arena(text):
    text = re.sub(r"  arenaHalfSize: .*\n", "", text)
    return text.replace("  props:\n", f"  arenaHalfSize: {ARENA_HALF}\n  props:\n", 1)


def blocked(x, z, props):
    limit = ARENA_HALF - WALL_THICK / 2 - AGENT_RADIUS
    if abs(x) > limit or abs(z) > limit:
        return True
    for t, px, pz, sx, _, sz, yaw in props:
        if t in (TREE, LAMP):
            sx = sz = 0.3
        if yaw % 180 == 90:
            sx, sz = sz, sx
        if abs(x - px) < sx / 2 + AGENT_RADIUS and abs(z - pz) < sz / 2 + AGENT_RADIUS:
            return True
    return False


def unreachable(points, props):
    def key(p):
        return round(p[0] / GRID), round(p[1] / GRID)
    seen, queue = {key(points[0])}, [key(points[0])]
    while queue:
        cx, cz = queue.pop()
        for nb in ((cx + 1, cz), (cx - 1, cz), (cx, cz + 1), (cx, cz - 1)):
            if nb not in seen and not blocked(nb[0] * GRID, nb[1] * GRID, props):
                seen.add(nb)
                queue.append(nb)
    near = round(KEEP_CLEAR / GRID)
    return [p for p in points if not any((key(p)[0] + dx, key(p)[1] + dz) in seen
                                         for dx in range(-near, near + 1) for dz in range(-near, near + 1))]


def main():
    results = {}
    for level, (scheme, seed) in SCHEMES.items():
        rel = f"Assets/Data/Levels/Level_0{level}.asset"
        text, old_props, points = read_level(rel)
        for attempt in range(MAX_ATTEMPTS):
            segs = cut_doors(scheme(random.Random(seed + attempt)), points)
            props = [seg_to_prop(s) for s in segs] + [p for p in old_props if not prop_overlaps(p, segs)]
            bad = unreachable(points, props)
            if not bad:
                break
        if bad:
            sys.exit(f"Level {level}: cannot reach {bad} - nothing written")
        body = text[:text.index("  props:")] + props_yaml(props) + text[text.index("  theme:"):]
        results[os.path.join(ROOT, rel)] = set_arena(body)
        print(f"Level {level} ({scheme.__name__}): seed {seed + attempt}, {len(segs)} walls, "
              f"{len(props) - len(segs)} cover/decor props, {len(points)} points reachable")
    for path, text in results.items():
        open(path, "w", encoding="utf-8").write(text)


if __name__ == "__main__":
    main()
