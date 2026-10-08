"""Normalize three generated weapon-free attack poses to Kairo's existing canvas."""
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / 'assets/sprites/kairo_faisca/sword_source_v2'
OUT = ROOT / 'assets/sprites/kairo_faisca'
PLACEMENTS = [(35, 15, 130, 153), (29, 26, 117, 140), (4, 28, 172, 137)]

for i, (x, y, width, height) in enumerate(PLACEMENTS):
    source = Image.open(SRC / f'attack_unarmed_{i:02}.png').convert('RGBA')
    box = source.getchannel('A').point(lambda alpha: 255 if alpha > 200 else 0).getbbox()
    if box is None:
        raise ValueError(f'Attack pose {i} has no opaque pixels')
    frame = Image.new('RGBA', (180, 170))
    frame.paste(source.crop(box).resize((width, height), Image.Resampling.NEAREST), (x, y))
    frame.save(OUT / f'kairo_attack_unarmed_v2_{i:02}.png')
