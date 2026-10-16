"""Slice approved-candidate desert art into Godot textures and a boss SpriteFrames.

Source illustrations are generated images. This script only crops, aligns,
scales, and previews them; it does not invent poses or repaint the artwork.
"""

from pathlib import Path

from PIL import Image, ImageDraw

from build_post_demo_enemies import largest_figure


ROOT = Path(__file__).resolve().parents[2]
DESERT = ROOT / "assets/environment/desert"
SNAKE = ROOT / "assets/sprites/bosses/sand_viper"
REVIEW = ROOT / "docs/art-review"
CANVAS = (512, 320)


def build_ground() -> None:
    source = Image.open(DESERT / "desert_ground_source_v1.png").convert("RGBA")
    # Remove the transparent margin above the sand; make one 1024-pixel strip.
    crop = source.crop((0, 130, source.width, 620))
    crop = crop.resize((1024, 232), Image.Resampling.LANCZOS)
    crop.save(DESERT / "desert_ground_tile_v1.png")


def build_snake() -> dict[str, list[Path]]:
    source = Image.open(SNAKE / "sand_viper_source_v1.png").convert("RGBA")
    assert source.size == (1536, 1024)
    assert source.getchannel("A").getextrema()[0] == 0
    cells: list[Image.Image] = []
    for row in range(3):
        for col in range(4):
            left, right = col * 384, (col + 1) * 384
            if row == 2 and col == 2:
                left, right = 670, 1180  # Long strike extends beyond its nominal cell.
            upper, lower = row * 341, min((row + 1) * 341, 1024)
            if row == 2:
                upper = 620  # The striking head starts above the nominal third-row cut.
                lower = 1024
            cell = source.crop((left, upper, right, lower))
            if row == 2 and col == 1:
                # The long lunge in the next cell overlaps this pose's right edge.
                ImageDraw.Draw(cell).rectangle((685 - left, 0, right - left, 855 - upper), fill=(0, 0, 0, 0))
            cell = largest_figure(cell)
            bbox = cell.getchannel("A").point(lambda a: 255 if a > 16 else 0).getbbox()
            assert bbox is not None, (row, col)
            cells.append(cell.crop(bbox))

    scale = min(456 / max(c.width for c in cells), 275 / max(c.height for c in cells))
    paths = {key: [] for key in ("idle", "slither", "attack", "windup", "hurt", "dead")}
    for row, state in enumerate(("idle", "slither", "attack")):
        for col in range(4):
            cell = cells[row * 4 + col]
            cell = cell.resize((round(cell.width * scale), round(cell.height * scale)), Image.Resampling.LANCZOS)
            frame = Image.new("RGBA", CANVAS)
            frame.alpha_composite(cell, ((CANVAS[0] - cell.width) // 2, CANVAS[1] - cell.height))
            path = SNAKE / f"sand_viper_{state}_{col:02}.png"
            frame.save(path)
            paths[state].append(path)
    paths["windup"] = [paths["idle"][3], paths["attack"][0]]
    paths["hurt"] = [paths["attack"][1], paths["idle"][0]]
    fallen = Image.open(paths["slither"][2]).convert("RGBA")
    fallen = fallen.crop(fallen.getchannel("A").getbbox())
    fallen = fallen.resize((fallen.width, round(fallen.height * .62)), Image.Resampling.LANCZOS)
    dead = Image.new("RGBA", CANVAS)
    dead.alpha_composite(fallen, ((CANVAS[0] - fallen.width) // 2, CANVAS[1] - fallen.height))
    dead_path = SNAKE / "sand_viper_dead_00.png"
    dead.save(dead_path)
    paths["dead"] = [dead_path]

    unique = list(dict.fromkeys(p for frames in paths.values() for p in frames))
    ids = {p: str(index + 1) for index, p in enumerate(unique)}
    lines = [f'[gd_resource type="SpriteFrames" load_steps={len(unique) + 1} format=3]', ""]
    lines += [f'[ext_resource type="Texture2D" path="res://{p.relative_to(ROOT).as_posix()}" id="{ids[p]}"]' for p in unique]
    lines += ["", "[resource]", "animations = ["]
    speeds = {"idle": 5, "slither": 8, "attack": 5, "windup": 3, "hurt": 7, "dead": 1}
    blocks = []
    for state in ("idle", "slither", "windup", "attack", "hurt", "dead"):
        frames = ", ".join('{"duration": 1.0, "texture": ExtResource("' + ids[p] + '")}' for p in paths[state])
        blocks.append('{"frames": [' + frames + '], "loop": ' + str(state in ("idle", "slither")).lower()
                      + f', "name": &"{state}", "speed": {speeds[state]:.6f}' + '}')
    lines += [",\n".join(blocks), "]"]
    (SNAKE / "SandViperSpriteFrames.tres").write_text("\n".join(lines) + "\n", encoding="utf-8")
    return paths


def preview(paths: dict[str, list[Path]]) -> None:
    REVIEW.mkdir(exist_ok=True)
    frames = []
    for tick in range(12):
        board = Image.new("RGB", (1200, 740), "#18243a")
        draw = ImageDraw.Draw(board)
        draw.text((28, 18), "REFLORESCER / DESERTO / COBRA CHEFE / sprites ampliados", fill="#edcf93")
        for index, state in enumerate(("idle", "slither", "windup", "attack", "hurt", "dead")):
            col, row = index % 2, index // 2
            path = paths[state][tick % len(paths[state])]
            sprite = Image.open(path).convert("RGBA").resize((512, 320), Image.Resampling.NEAREST)
            x, y = 40 + col * 590, 40 + row * 228
            sprite = sprite.resize((360, 225), Image.Resampling.NEAREST)
            board.paste(sprite, (x, y), sprite)
            draw.text((x, y + 190), state, fill="#efe2c6")
        frames.append(board)
    frames[0].save(REVIEW / "desert-snake-boss-v1.png")
    frames[0].save(REVIEW / "desert-snake-boss-v1.gif", save_all=True,
                   append_images=frames[1:], duration=150, loop=0)


if __name__ == "__main__":
    build_ground()
    preview(build_snake())
    print("Desert ground tile and 12 boss cycle frames generated.")
