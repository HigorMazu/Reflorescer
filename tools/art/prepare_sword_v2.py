"""Prepare the second grass-sword design without changing the generated original."""
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'assets/sprites/kairo_faisca/sword_source_v2/straight_sword.png'
TARGET = ROOT / 'assets/sprites/kairo_faisca/grass_sword_v2.png'
image = Image.open(SOURCE).convert('RGBA')
opaque = image.getchannel('A').point(lambda alpha: 255 if alpha > 200 else 0)
box = opaque.getbbox()
if box is None:
    raise ValueError('Generated sword has no opaque pixels')
image.crop(box).resize((11, 40), Image.Resampling.NEAREST).save(TARGET)
