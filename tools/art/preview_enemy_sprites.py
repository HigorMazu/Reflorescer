"""Monta uma prévia legível dos PNGs finais dos inimigos."""

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
ENEMIES = (
    ("Vespa-asiática gigante", "voador"),
    ("Formiga-lava-pé", "rapido"),
    ("Tartaruga-de-orelha-vermelha", "robusto"),
)
STATES = ("idle", "walk", "detect", "attack", "hurt", "dead")
SCALE = 2
CELL = (256, 224)
TITLE_HEIGHT = 64


def checkerboard(size: tuple[int, int]) -> Image.Image:
    image = Image.new("RGB", size, "#13231b")
    draw = ImageDraw.Draw(image)
    block = 16
    for y in range(0, size[1], block):
        for x in range(0, size[0], block):
            if (x // block + y // block) % 2:
                draw.rectangle((x, y, x + block - 1, y + block - 1), fill="#183026")
    return image


canvas = checkerboard((CELL[0] * 3, TITLE_HEIGHT + CELL[1] * 6))
draw = ImageDraw.Draw(canvas)
font = ImageFont.load_default()

for column, (title, enemy) in enumerate(ENEMIES):
    x0 = column * CELL[0]
    draw.text((x0 + 12, 18), title, fill="#e7d7a2", font=font)
    for row, state in enumerate(STATES):
        frame = Image.open(ROOT / "assets" / "sprites" / "enemies" / enemy / f"{enemy}_{state}_00.png").convert("RGBA")
        frame = frame.resize((frame.width * SCALE, frame.height * SCALE), Image.Resampling.NEAREST)
        x = x0 + (CELL[0] - frame.width) // 2
        y = TITLE_HEIGHT + row * CELL[1] + 12
        canvas.paste(frame, (x, y), frame)
        draw.text((x0 + 10, TITLE_HEIGHT + row * CELL[1] + CELL[1] - 22), state, fill="#c4d6bd", font=font)

target = ROOT / "docs" / "art-review" / "enemy-archetypes.png"
target.parent.mkdir(parents=True, exist_ok=True)
canvas.save(target)
