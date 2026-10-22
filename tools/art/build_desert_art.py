"""Slice approved-candidate desert art into Godot textures and a boss SpriteFrames.

Source illustrations are generated images. This script only crops, aligns,
scales, and previews them; it does not invent poses or repaint the artwork.
"""

from pathlib import Path

from PIL import Image, ImageDraw, ImageOps

from build_post_demo_enemies import largest_figure


ROOT = Path(__file__).resolve().parents[2]
DESERT = ROOT / "assets/environment/desert"
SNAKE = ROOT / "assets/sprites/bosses/sand_viper"
REVIEW = ROOT / "docs/art-review"
CANVAS = (512, 320)


def grid_cells(
    source: Image.Image,
    columns: int,
    rows: int,
    *,
    keep_detached: bool = False,
) -> list[Image.Image]:
    cells = []
    for row in range(rows):
        upper = round(row * source.height / rows)
        lower = round((row + 1) * source.height / rows)
        for col in range(columns):
            left = round(col * source.width / columns)
            right = round((col + 1) * source.width / columns)
            cell = source.crop((left, upper, right, lower))
            if not keep_detached:
                cell = largest_figure(cell)
            bbox = cell.getchannel("A").point(lambda a: 255 if a > 16 else 0).getbbox()
            assert bbox is not None, (row, col)
            cells.append(cell.crop(bbox))
    return cells


def build_ground() -> None:
    source = Image.open(DESERT / "desert_ground_source_v1.png").convert("RGBA")
    # Remove the transparent margin above the sand; make one 1024-pixel strip.
    crop = source.crop((0, 130, source.width, 620))
    crop = crop.resize((1024, 232), Image.Resampling.LANCZOS)
    crop.save(DESERT / "desert_ground_tile_v1.png")


def build_interactables() -> None:
    source = Image.open(DESERT / "desert_interactables_source_v1.png").convert("RGBA")
    half_w, half_h = source.width // 2, source.height // 2
    specs = (
        (0, 0, "desert_checkpoint_off_v1.png", (192, 128), (172, 115)),
        (1, 0, "desert_checkpoint_on_v1.png", (192, 128), (172, 115)),
        (0, 1, "desert_portal_v1.png", (192, 176), (184, 170)),
        (1, 1, "desert_portal_forest_v1.png", (192, 176), (184, 170)),
    )
    for col, row, filename, canvas_size, fit in specs:
        left = 0 if col == 0 else half_w
        right = half_w if col == 0 else source.width
        upper = 0 if row == 0 else half_h
        lower = half_h if row == 0 else source.height
        cell = largest_figure(source.crop((left, upper, right, lower)))
        bbox = cell.getchannel("A").point(lambda a: 255 if a > 16 else 0).getbbox()
        assert bbox is not None, filename
        cell = cell.crop(bbox)
        factor = min(fit[0] / cell.width, fit[1] / cell.height)
        cell = cell.resize((round(cell.width * factor), round(cell.height * factor)), Image.Resampling.LANCZOS)
        canvas = Image.new("RGBA", canvas_size)
        canvas.alpha_composite(cell, ((canvas_size[0] - cell.width) // 2, canvas_size[1] - cell.height))
        canvas.save(DESERT / filename)


def build_area_modules() -> dict[str, Path]:
    source = Image.open(DESERT / "desert_modules_source_v1.png").convert("RGBA")
    assert source.getchannel("A").getextrema()[0] == 0
    cells = grid_cells(source, 4, 2)
    target = DESERT / "modules"
    target.mkdir(parents=True, exist_ok=True)
    specs = (
        ("platform_long", (384, 176), (368, 164)),
        ("ledge_short", (224, 176), (208, 164)),
        ("wall_cap", (208, 320), (192, 306)),
        ("rock_pillar", (176, 320), (160, 306)),
        ("ruin_arch", (272, 256), (256, 242)),
        ("buried_column", (224, 208), (208, 194)),
        ("rock_cluster", (256, 176), (240, 162)),
        ("dry_shrub", (224, 176), (208, 162)),
    )
    paths = {}
    for cell, (name, canvas_size, fit) in zip(cells, specs):
        factor = min(fit[0] / cell.width, fit[1] / cell.height)
        cell = cell.resize((round(cell.width * factor), round(cell.height * factor)), Image.Resampling.LANCZOS)
        canvas = Image.new("RGBA", canvas_size)
        canvas.alpha_composite(cell, ((canvas_size[0] - cell.width) // 2, canvas_size[1] - cell.height))
        path = target / f"desert_{name}_v1.png"
        canvas.save(path)
        paths[name] = path
    return paths


def build_hazard_and_restoration() -> tuple[list[Path], list[Path]]:
    source = Image.open(DESERT / "desert_quicksand_restoration_source_v1.png").convert("RGBA")
    assert source.getchannel("A").getextrema()[0] == 0
    # The bubbles and burst are detached from the sand surface by design.
    cells = grid_cells(source, 4, 2, keep_detached=True)
    target = DESERT / "hazards"
    target.mkdir(parents=True, exist_ok=True)

    quicksand = []
    restoration = []
    for index, cell in enumerate(cells[:4]):
        factor = min(304 / cell.width, 116 / cell.height)
        cell = cell.resize((round(cell.width * factor), round(cell.height * factor)), Image.Resampling.LANCZOS)
        canvas = Image.new("RGBA", (320, 128))
        canvas.alpha_composite(cell, ((320 - cell.width) // 2, 128 - cell.height))
        path = target / f"desert_quicksand_{index:02}.png"
        canvas.save(path)
        quicksand.append(path)
    for index, cell in enumerate(cells[4:]):
        factor = min(240 / cell.width, 145 / cell.height)
        cell = cell.resize((round(cell.width * factor), round(cell.height * factor)), Image.Resampling.LANCZOS)
        canvas = Image.new("RGBA", (256, 160))
        canvas.alpha_composite(cell, ((256 - cell.width) // 2, 160 - cell.height))
        path = target / f"desert_restoration_{index:02}.png"
        canvas.save(path)
        restoration.append(path)

    lines = ["[gd_resource type=\"SpriteFrames\" load_steps=5 format=3]", ""]
    for index, path in enumerate(quicksand, 1):
        lines.append(f'[ext_resource type="Texture2D" path="res://{path.relative_to(ROOT).as_posix()}" id="{index}"]')
    frames = ", ".join('{"duration": 1.0, "texture": ExtResource("' + str(i) + '")}' for i in range(1, 5))
    lines += ["", "[resource]", "animations = [",
              '{"frames": [' + frames + '], "loop": true, "name": &"bubble", "speed": 4.000000}', "]"]
    (target / "DesertQuicksandSpriteFrames.tres").write_text("\n".join(lines) + "\n", encoding="utf-8")
    return quicksand, restoration


def preview_area_kit(modules: dict[str, Path], quicksand: list[Path], restoration: list[Path]) -> None:
    REVIEW.mkdir(exist_ok=True)
    boards = []
    module_order = ("platform_long", "ledge_short", "wall_cap", "rock_pillar",
                    "ruin_arch", "buried_column", "rock_cluster", "dry_shrub")
    for tick in range(12):
        board = Image.new("RGB", (1500, 900), "#18243a")
        draw = ImageDraw.Draw(board)
        draw.text((28, 20), "REFLORESCER / DESERTO / KIT MODULAR + HAZARD + RESTAURACAO", fill="#edcf93")
        for index, name in enumerate(module_order):
            image = Image.open(modules[name]).convert("RGBA")
            col, row = index % 4, index // 4
            image.thumbnail((330, 270), Image.Resampling.LANCZOS)
            x, y = 30 + col * 365, 55 + row * 285
            board.paste(image, (x + (330 - image.width) // 2, y + 270 - image.height), image)
            draw.text((x, y + 274), name, fill="#efe2c6")
        hazard = Image.open(quicksand[tick % 4]).convert("RGBA")
        hazard = hazard.resize((480, 192), Image.Resampling.NEAREST)
        board.paste(hazard, (80, 665), hazard)
        draw.text((80, 845), "quicksand / loop: bolha sobe e estoura", fill="#efe2c6")
        stage = min(3, tick // 3)
        oasis = Image.open(restoration[stage]).convert("RGBA")
        oasis = oasis.resize((384, 240), Image.Resampling.NEAREST)
        board.paste(oasis, (885, 620), oasis)
        draw.text((885, 845), "restauracao / seco -> nascente viva", fill="#efe2c6")
        boards.append(board)
    boards[0].save(REVIEW / "desert-area-kit-v1.png")
    boards[0].save(REVIEW / "desert-area-kit-v1.gif", save_all=True,
                   append_images=boards[1:], duration=180, loop=0)


def build_area_backgrounds() -> dict[str, Path]:
    target = DESERT / "areas"
    names = ("portal_sol", "garganta_ruinas", "oasis_soterrado", "ninho_serpente")
    paths = {}
    for name in names:
        source = Image.open(target / f"desert_{name}_source_v1.png").convert("RGB")
        final = ImageOps.fit(source, (1672, 941), method=Image.Resampling.LANCZOS)
        path = target / f"desert_{name}_v1.png"
        final.save(path, optimize=True)
        paths[name] = path
    return paths


def preview_area_backgrounds(backgrounds: dict[str, Path], modules: dict[str, Path],
                             quicksand: list[Path], restoration: list[Path],
                             snake: dict[str, list[Path]]) -> None:
    board = Image.new("RGB", (1600, 1010), "#101827")
    draw = ImageDraw.Draw(board)
    draw.text((28, 18), "REFLORESCER / DESERTO / COMPOSICAO DAS AREAS", fill="#edcf93")
    labels = (
        ("portal_sol", "1  PORTAL DO SOL / entrada horizontal / dia"),
        ("garganta_ruinas", "2  GARGANTA DAS RUINAS / verticalidade / tarde"),
        ("oasis_soterrado", "3  OASIS SOTERRADO / ramificacao inferior / crepusculo"),
        ("ninho_serpente", "4  NINHO DA SERPENTE / arena horizontal / noite"),
    )
    for index, (name, label) in enumerate(labels):
        col, row = index % 2, index // 2
        x, y = 25 + col * 785, 50 + row * 475
        scene = ImageOps.fit(Image.open(backgrounds[name]).convert("RGB"), (760, 428),
                             method=Image.Resampling.LANCZOS).convert("RGBA")
        # A dark footer keeps the foreground kit readable over each distinct plate.
        shade = Image.new("RGBA", (760, 125), (10, 15, 24, 72))
        scene.alpha_composite(shade, (0, 303))
        if name == "portal_sol":
            props = [(modules["ruin_arch"], 18, 205, 150),
                     (modules["platform_long"], 170, 292, 420),
                     (modules["dry_shrub"], 600, 310, 120)]
        elif name == "garganta_ruinas":
            props = [(modules["wall_cap"], 8, 115, 170),
                     (modules["ledge_short"], 205, 230, 185),
                     (modules["ledge_short"], 410, 160, 185),
                     (modules["rock_pillar"], 625, 168, 120)]
        elif name == "oasis_soterrado":
            props = [(modules["rock_cluster"], 25, 290, 155),
                     (quicksand[2], 195, 315, 310),
                     (restoration[3], 535, 290, 180)]
        else:
            props = [(modules["platform_long"], 15, 300, 355),
                     (modules["platform_long"], 390, 300, 355),
                     (snake["idle"][1], 250, 172, 285)]
        for path, px, py, width in props:
            prop = Image.open(path).convert("RGBA")
            factor = width / prop.width
            prop = prop.resize((width, max(1, round(prop.height * factor))), Image.Resampling.LANCZOS)
            scene.alpha_composite(prop, (px, py))
        panel_draw = ImageDraw.Draw(scene)
        panel_draw.rectangle((0, 399, 760, 428), fill="#111827")
        panel_draw.text((10, 407), label, fill="#f4dfb0")
        board.paste(scene.convert("RGB"), (x, y))
    board.save(REVIEW / "desert-area-backgrounds-v1.png", optimize=True)


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

    # The approved follow-up sheet adds silhouettes that cannot be obtained by
    # reusing the frontal lunge: a broad tail sweep and two sand transitions.
    special = Image.open(SNAKE / "sand_viper_special_source_v1.png").convert("RGBA")
    assert special.size == (1536, 1024)
    assert special.getchannel("A").getextrema()[0] == 0
    special_cells: list[Image.Image] = []
    for row in range(3):
        upper = round(row * special.height / 3)
        lower = round((row + 1) * special.height / 3)
        for col in range(3):
            cell = special.crop((col * 512, upper, (col + 1) * 512, lower))
            # Generated poses can cross a cell edge. Keep the connected boss/effect
            # silhouette and discard fragments belonging to the neighbouring pose.
            cell = largest_figure(cell)
            bbox = cell.getchannel("A").point(lambda a: 255 if a > 16 else 0).getbbox()
            assert bbox is not None, ("special", row, col)
            special_cells.append(cell.crop(bbox))

    special_scale = min(456 / max(c.width for c in special_cells),
                        275 / max(c.height for c in special_cells))
    for row, state in enumerate(("tail_sweep", "burrow_enter", "burrow_exit")):
        paths[state] = []
        for col in range(3):
            cell = special_cells[row * 3 + col]
            cell = cell.resize((round(cell.width * special_scale), round(cell.height * special_scale)),
                               Image.Resampling.LANCZOS)
            frame = Image.new("RGBA", CANVAS)
            frame.alpha_composite(cell, ((CANVAS[0] - cell.width) // 2, CANVAS[1] - cell.height))
            path = SNAKE / f"sand_viper_{state}_{col:02}.png"
            frame.save(path)
            paths[state].append(path)

    unique = list(dict.fromkeys(p for frames in paths.values() for p in frames))
    ids = {p: str(index + 1) for index, p in enumerate(unique)}
    lines = [f'[gd_resource type="SpriteFrames" load_steps={len(unique) + 1} format=3]', ""]
    lines += [f'[ext_resource type="Texture2D" path="res://{p.relative_to(ROOT).as_posix()}" id="{ids[p]}"]' for p in unique]
    lines += ["", "[resource]", "animations = ["]
    speeds = {"idle": 5, "slither": 8, "attack": 5, "windup": 3, "hurt": 7, "dead": 1,
              "tail_sweep": 5, "burrow_enter": 4, "burrow_exit": 4}
    blocks = []
    for state in ("idle", "slither", "windup", "attack", "tail_sweep",
                  "burrow_enter", "burrow_exit", "hurt", "dead"):
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
        board = Image.new("RGB", (1500, 970), "#18243a")
        draw = ImageDraw.Draw(board)
        draw.text((28, 18), "REFLORESCER / DESERTO / COBRA CHEFE / sprites ampliados", fill="#edcf93")
        for index, state in enumerate(("idle", "slither", "windup", "attack", "tail_sweep",
                                       "burrow_enter", "burrow_exit", "hurt", "dead")):
            col, row = index % 3, index // 3
            path = paths[state][tick % len(paths[state])]
            sprite = Image.open(path).convert("RGBA").resize((512, 320), Image.Resampling.NEAREST)
            x, y = 30 + col * 490, 40 + row * 300
            sprite = sprite.resize((450, 281), Image.Resampling.NEAREST)
            board.paste(sprite, (x, y), sprite)
            draw.text((x, y + 252), state, fill="#efe2c6")
        frames.append(board)
    frames[0].save(REVIEW / "desert-snake-boss-v1.png")
    frames[0].save(REVIEW / "desert-snake-boss-v1.gif", save_all=True,
                   append_images=frames[1:], duration=150, loop=0)


if __name__ == "__main__":
    build_ground()
    build_interactables()
    modules = build_area_modules()
    quicksand, restoration = build_hazard_and_restoration()
    preview_area_kit(modules, quicksand, restoration)
    snake = build_snake()
    preview(snake)
    backgrounds = build_area_backgrounds()
    preview_area_backgrounds(backgrounds, modules, quicksand, restoration, snake)
    print("Desert terrain, area backgrounds, interactables and boss animation frames generated.")
