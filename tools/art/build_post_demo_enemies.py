"""Prepare six post-demo biome candidate sheets for Godot.

The input PNGs are generated source art. This script only slices, scales and
aligns their poses; it never paints new character details. Run from anywhere:
python tools/art/build_post_demo_enemies.py
"""

from pathlib import Path
from collections import deque

from PIL import Image, ImageDraw
import numpy as np


ROOT = Path(__file__).resolve().parents[2]
ASSETS = ROOT / "assets/sprites/enemies"
SOURCES = ASSETS / "sources/post_demo"
REVIEW = ROOT / "docs/art-review"
CANVAS = (128, 96)
SPECS = (
    ("deserto", "abelha_africanizada", "AbelhaAfricanizada", "Voador", 10, 14),
    ("deserto", "rato_preto", "RatoPreto", "Rapido", 5, 10),
    ("deserto", "burro_selvagem", "BurroSelvagem", "Robusto", 3, 5),
    ("tundra", "mosquito_artico", "MosquitoArtico", "Voador", 10, 14),
    ("tundra", "lebre_europeia", "LebreEuropeia", "Rapido", 5, 10),
    ("tundra", "ganso_das_neves", "GansoDasNeves", "Robusto", 3, 5),
)


def largest_figure(cell: Image.Image) -> Image.Image:
    """Discard detached pieces of an adjacent generated pose in a grid cell."""
    alpha = np.asarray(cell.getchannel("A"))
    solid = alpha > 24
    seen = np.zeros(solid.shape, dtype=bool)
    height, width = solid.shape
    best = []
    for y, x in zip(*np.nonzero(solid)):
        if seen[y, x]:
            continue
        group = []
        queue = deque([(int(y), int(x))])
        seen[y, x] = True
        while queue:
            cy, cx = queue.popleft()
            group.append((cy, cx))
            for ny, nx in ((cy - 1, cx), (cy + 1, cx), (cy, cx - 1), (cy, cx + 1),
                           (cy - 1, cx - 1), (cy - 1, cx + 1), (cy + 1, cx - 1), (cy + 1, cx + 1)):
                if 0 <= ny < height and 0 <= nx < width and solid[ny, nx] and not seen[ny, nx]:
                    seen[ny, nx] = True
                    queue.append((ny, nx))
        if len(group) > len(best):
            best = group
    assert best
    # Keep the original antialiased edge around the selected figure.
    chosen = Image.new("L", cell.size)
    pixels = chosen.load()
    for y, x in best:
        for ny in range(max(0, y - 2), min(height, y + 3)):
            for nx in range(max(0, x - 2), min(width, x + 3)):
                pixels[nx, ny] = 255
    result = cell.copy()
    result.putalpha(Image.composite(cell.getchannel("A"), Image.new("L", cell.size), chosen))
    return result


def sprite_frames_resource(title: str, paths: dict[str, list[Path]], idle_rate: int, walk_rate: int) -> None:
    rates = {"idle": idle_rate, "walk": walk_rate, "detect": 4, "attack": 4,
             "hurt": 2 / .3, "dead": 1 / .55}
    unique = list(dict.fromkeys(path for frames in paths.values() for path in frames))
    ids = {path: str(index + 1) for index, path in enumerate(unique)}
    lines = [f'[gd_resource type="SpriteFrames" load_steps={len(unique) + 1} format=3]', ""]
    for path in unique:
        lines.append(f'[ext_resource type="Texture2D" path="res://{path.relative_to(ROOT).as_posix()}" id="{ids[path]}"]')
    lines.extend(("", "[resource]", "animations = ["))
    blocks = []
    for state in ("idle", "walk", "detect", "attack", "hurt", "dead"):
        frames = ", ".join('{"duration": 1.0, "texture": ExtResource("' + ids[path] + '")}' for path in paths[state])
        blocks.append('{"frames": [' + frames + '], "loop": ' + str(state in ("idle", "walk")).lower()
                      + f', "name": &"{state}", "speed": {rates[state]:.6f}' + '}')
    lines.extend((",\n".join(blocks), "]"))
    (ASSETS / f"Enemy{title}SpriteFrames.tres").write_text("\n".join(lines) + "\n", encoding="utf-8")


def enemy_scene(biome: str, title: str, archetype: str) -> None:
    scene = f"""[gd_scene load_steps=3 format=3]

[ext_resource type="PackedScene" path="res://scenes/enemies/Enemy_{archetype}.tscn" id="1"]
[ext_resource type="SpriteFrames" path="res://assets/sprites/enemies/Enemy{title}SpriteFrames.tres" id="2"]

[node name="Enemy_{title}" instance=ExtResource("1")]
EnemyId = "enemy_{biome}_{title.lower()}"

[node name="AnimatedSprite2D" parent="." index="4"]
sprite_frames = ExtResource("2")
"""
    (ROOT / "scenes/enemies" / f"Enemy_{title}.tscn").write_text(scene, encoding="utf-8")


def build(spec: tuple) -> dict[str, list[Path]]:
    biome, key, title, archetype, idle_rate, walk_rate = spec
    source = Image.open(SOURCES / f"{key}-cycles-v1.png").convert("RGBA")
    assert source.getchannel("A").getextrema()[0] == 0, key
    width, height = source.size
    assert width % 4 == 0 and height % 3 == 0, (key, source.size)
    cell_w, cell_h = width // 4, height // 3
    cells = []
    for row in range(3):
        for col in range(4):
            cell = source.crop((col * cell_w, row * cell_h, (col + 1) * cell_w, (row + 1) * cell_h))
            cell = largest_figure(cell)
            bbox = cell.getchannel("A").point(lambda alpha: 255 if alpha > 16 else 0).getbbox()
            assert bbox is not None, (key, row, col)
            cells.append(cell.crop(bbox))

    # One scale for the entire character, including its extended attack pose.
    factor = min(96 / max(cell.width for cell in cells), 80 / max(cell.height for cell in cells))
    target = ASSETS / "post_demo" / key
    target.mkdir(parents=True, exist_ok=True)
    paths = {state: [] for state in ("idle", "walk", "detect", "attack", "hurt", "dead")}
    for row, state in enumerate(("idle", "walk", "attack")):
        for col in range(4):
            cell = cells[row * 4 + col]
            cell = cell.resize((round(cell.width * factor), round(cell.height * factor)), Image.Resampling.LANCZOS)
            frame = Image.new("RGBA", CANVAS)
            frame.alpha_composite(cell, ((CANVAS[0] - cell.width) // 2, CANVAS[1] - cell.height))
            path = target / f"{key}_{state}_{col:02}.png"
            frame.save(path)
            paths[state].append(path)

    # The generated sheet supplies four complete poses for each core motion.
    # Alert and recoil reuse character-specific source poses. The death pose
    # settles the last attack drawing onto its side so it reads during fade.
    paths["detect"] = [paths["idle"][0], paths["idle"][1]]
    paths["hurt"] = [paths["attack"][0], paths["idle"][0]]
    dead_source = Image.open(paths["attack"][3]).convert("RGBA")
    dead_source = dead_source.crop(dead_source.getchannel("A").getbbox())
    angle = -58 if archetype == "Voador" else -24
    collapsed = dead_source.rotate(angle, resample=Image.Resampling.BICUBIC, expand=True)
    collapsed = collapsed.resize((collapsed.width, max(1, round(collapsed.height * .75))), Image.Resampling.LANCZOS)
    collapsed.thumbnail((96, 76), Image.Resampling.LANCZOS)
    dead = Image.new("RGBA", CANVAS)
    dead.alpha_composite(collapsed, ((CANVAS[0] - collapsed.width) // 2, CANVAS[1] - collapsed.height))
    dead_path = target / f"{key}_dead_00.png"
    dead.save(dead_path)
    paths["dead"] = [dead_path]
    sprite_frames_resource(title, paths, idle_rate, walk_rate)
    enemy_scene(biome, title, archetype)
    return paths


def make_review(all_paths: dict[str, dict[str, list[Path]]]) -> None:
    REVIEW.mkdir(parents=True, exist_ok=True)
    names = (("abelha_africanizada", "rato_preto", "burro_selvagem"),
             ("mosquito_artico", "lebre_europeia", "ganso_das_neves"))
    for biome, keys in zip(("deserto", "tundra"), names):
        boards = []
        for tick in range(12):
            board = Image.new("RGB", (1000, 850), "#172821" if biome == "deserto" else "#1b2a38")
            draw = ImageDraw.Draw(board)
            draw.text((24, 16), f"REFLORESCER / POS-DEMO / {biome.upper()} / sprites ampliados 2x", fill="#f1e5c5")
            for col, key in enumerate(keys):
                draw.text((30 + col * 328, 54), key.replace("_", " ").upper(), fill="#d9bf8d")
                for row, state in enumerate(("idle", "walk", "detect", "attack", "hurt", "dead")):
                    sequence = all_paths[key][state]
                    frame = Image.open(sequence[tick % len(sequence)]).convert("RGBA")
                    x, y = 50 + col * 328, 76 + row * 125
                    # Preview at near-game size to assess legibility.
                    frame = frame.resize((160, 120), Image.Resampling.NEAREST)
                    board.paste(frame, (x, y), frame)
                    draw.text((30 + col * 328, y + 104), state, fill="#dae5d7")
            boards.append(board)
        boards[0].save(REVIEW / f"post-demo-{biome}-sprites.png")
        boards[0].save(REVIEW / f"post-demo-{biome}-sprites.gif", save_all=True,
                       append_images=boards[1:], duration=140, loop=0)


if __name__ == "__main__":
    result = {spec[1]: build(spec) for spec in SPECS}
    make_review(result)
    print("Six species, 72 core animation frames, six SpriteFrames, six inherited scenes, two previews.")
