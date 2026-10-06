"""Normaliza folhas 3×2 de inimigos em PNGs alinhados à base dos pés."""

from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
SOURCES = {
    name: ROOT / "assets/sprites/enemies/sources" / f"{name}-poses-v1.png"
    for name in ("voador", "rapido", "robusto")
}
ANIMATIONS = ("idle", "walk", "detect", "attack", "hurt", "dead")
CANVAS = (128, 96)


def visible_bounds(image: Image.Image) -> tuple[int, int, int, int]:
    alpha = image.getchannel("A")
    # Generated sheets use a very soft transparent fringe. Keep the sprite body,
    # but discard nearly invisible pixels that would enlarge its bounding box.
    mask = alpha.point(lambda value: 255 if value > 12 else 0)
    bounds = mask.getbbox()
    if bounds is None:
        raise ValueError("The source cell contains no visible pixels")
    return bounds


def prepare_sheet(enemy: str, source: Path) -> None:
    sheet = Image.open(source).convert("RGBA")
    if sheet.size[0] % 3 or sheet.size[1] % 2:
        raise ValueError(f"Unexpected sheet size for {source}: {sheet.size}")

    cell_width, cell_height = sheet.size[0] // 3, sheet.size[1] // 2
    target_dir = ROOT / "assets" / "sprites" / "enemies" / enemy
    target_dir.mkdir(parents=True, exist_ok=True)

    for index, animation in enumerate(ANIMATIONS):
        column, row = index % 3, index // 3
        cell = sheet.crop((column * cell_width, row * cell_height,
                           (column + 1) * cell_width, (row + 1) * cell_height))
        sprite = cell.crop(visible_bounds(cell))
        sprite.thumbnail((96, 80), Image.Resampling.LANCZOS)

        frame = Image.new("RGBA", CANVAS, (0, 0, 0, 0))
        x = (CANVAS[0] - sprite.width) // 2
        y = CANVAS[1] - sprite.height
        frame.alpha_composite(sprite, (x, y))
        frame.save(target_dir / f"{enemy}_{animation}_00.png")


for enemy_name, source_path in SOURCES.items():
    prepare_sheet(enemy_name, source_path)
