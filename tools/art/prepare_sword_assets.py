"""Normalize approved generated sprites; crop/resize only, preserving RGBA."""
from pathlib import Path
from PIL import Image
import shutil

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'assets/sprites/kairo_faisca'
SOURCE = OUT / 'sword_source_v1'
SOURCE.mkdir(exist_ok=True)
GENERATED = Path.home() / '.codex/generated_images/01a10e41-da9f-7523-aae8-dd69a73cf487'
for src, dst in [('exec-f6355bdf-49a3-427d-ad07-3a5102bcf24b.png', 'sword.png'),
                 ('exec-31cf619d-29f4-4277-b147-5260bb1cd878.png', 'jump.png')]:
    if not (SOURCE / dst).exists():
        shutil.copy2(GENERATED / src, SOURCE / dst)

def opaque_bounds(image):
    return image.getchannel('A').point(lambda a: 255 if a > 200 else 0).getbbox()

sword = Image.open(SOURCE / 'sword.png').convert('RGBA')
sword.crop(opaque_bounds(sword)).resize((8, 28), Image.Resampling.NEAREST).save(OUT / 'grass_sword_v1.png')
jump = Image.open(SOURCE / 'jump.png').convert('RGBA')
frame = Image.new('RGBA', (180, 170))
frame.paste(jump.crop(opaque_bounds(jump)).resize((98, 137), Image.Resampling.NEAREST), (43, 12))
frame.save(OUT / 'kairo_jump_unarmed_v1.png')
