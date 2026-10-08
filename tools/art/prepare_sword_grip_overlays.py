"""Extract Kairo's existing closed fists so the sword grip can render behind them."""
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
SPRITES = ROOT / "assets/sprites/kairo_faisca"
FRAMES = [
    ("kairo_faisca_idle_00.png", "kairo_grip_idle_00.png", 39, 130),
    ("kairo_faisca_idle_01.png", "kairo_grip_idle_01.png", 42, 127),
    ("kairo_faisca_run_00.png", "kairo_grip_run_00.png", 81, 119),
    ("kairo_faisca_run_01.png", "kairo_grip_run_01.png", 77, 113),
    ("kairo_jump_unarmed_v1.png", "kairo_grip_jump_00.png", 55, 98),
    ("kairo_faisca_fall_00.png", "kairo_grip_fall_00.png", 100, 120),
    ("kairo_attack_unarmed_v2_00.png", "kairo_grip_attack_00.png", 78, 120),
    ("kairo_attack_unarmed_v2_01.png", "kairo_grip_attack_01.png", 122, 114),
    ("kairo_attack_unarmed_v2_02.png", "kairo_grip_attack_02.png", 170, 100),
]
SIZE = 30
HALF = SIZE // 2

for source_name, output_name, x, y in FRAMES:
    source = Image.open(SPRITES / source_name).convert("RGBA")
    overlay = Image.new("RGBA", (SIZE, SIZE))
    crop = source.crop((x - HALF, y - HALF, x + HALF, y + HALF))
    overlay.paste(crop, (0, 0), crop)
    overlay.save(SPRITES / output_name)
