"""Bake a six-frame mud pool sheet from the existing Igarapé atlas.

The bank, plants and water stay pixel-identical across frames; only two small
patches of the mud surface rise, burst and settle. Run from the project root.
"""

from pathlib import Path
from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "assets/environment/tropical/igarape_water_log_atlas_candidate_v1.png"
OUTPUT = ROOT / "assets/environment/tropical/igarape_mud_bubble_sheet_v1.png"
BASE = Image.open(SOURCE).convert("RGBA").crop((450, 160, 1550, 300))
WIDTH, HEIGHT = BASE.size


def bubble(draw: ImageDraw.ImageDraw, x: int, y: int, phase: int) -> None:
    # Square-edged facets follow the source texture's pixel-art language.
    if phase in (1, 2):
        radius = 8 if phase == 1 else 17
        draw.ellipse((x - radius - 3, y - 4, x + radius + 3, y + 7), fill="#49390b")
        draw.ellipse((x - radius, y - radius // 2, x + radius, y + 5), fill="#80621a")
        draw.polygon([(x - radius + 3, y), (x - radius + 6, y - radius),
                      (x + radius - 6, y - radius - 3), (x + radius - 1, y)], fill="#9c771f")
        draw.polygon([(x - radius + 8, y - radius + 2),
                      (x + 1, y - radius - 1), (x + 7, y - radius + 2),
                      (x - 3, y - radius + 3)], fill="#d4a832")
        draw.rectangle((x - radius + 8, y - radius + 3, x - radius + 12, y - radius + 4), fill="#f1d264")
        draw.arc((x - radius - 6, y - 4, x + radius + 6, y + 11), 12, 166,
                 fill="#b5a25a", width=2)
    elif phase == 3:
        # At the burst frame the dome is gone: a dark opening, broken rim,
        # displaced mud and three flying droplets are visible instead.
        draw.polygon([(x - 19, y - 1), (x - 13, y - 7), (x - 5, y - 4),
                      (x + 4, y - 9), (x + 15, y - 3), (x + 20, y + 2),
                      (x + 12, y + 7), (x - 15, y + 7)], fill="#3d300b")
        draw.line([(x - 19, y), (x - 13, y - 7), (x - 5, y - 4)], fill="#e0b443", width=3)
        draw.line([(x + 4, y - 9), (x + 15, y - 3), (x + 20, y + 2)], fill="#e0b443", width=3)
        for dx, dy, size in [(-14, -17, 4), (-2, -21, 3), (13, -17, 4), (21, -10, 2)]:
            draw.rectangle((x + dx, y + dy, x + dx + size, y + dy + size), fill="#e4ba49")
    elif phase == 4:
        draw.arc((x - 25, y - 8, x + 25, y + 13), 5, 174, fill="#c6a044", width=3)
        draw.arc((x - 18, y - 3, x + 18, y + 10), 188, 350, fill="#7d6b32", width=2)
        for dx, dy in [(-19, -9), (18, -10), (3, -13)]:
            draw.rectangle((x + dx, y + dy, x + dx + 2, y + dy + 2), fill="#b89539")
    elif phase == 5:
        draw.arc((x - 29, y - 7, x + 29, y + 13), 7, 174, fill="#8a8047", width=2)
        draw.arc((x - 21, y - 3, x + 21, y + 8), 191, 345, fill="#74713f", width=1)


sheet = Image.new("RGBA", (WIDTH * 6, HEIGHT), (0, 0, 0, 0))
for frame in range(6):
    tile = BASE.copy()
    draw = ImageDraw.Draw(tile)
    bubble(draw, 250, 57, frame)
    bubble(draw, 780, 51, (frame + 3) % 6)
    sheet.paste(tile, (frame * WIDTH, 0))

sheet.save(OUTPUT, optimize=True)
print(f"Saved {OUTPUT} ({sheet.width}x{sheet.height})")
