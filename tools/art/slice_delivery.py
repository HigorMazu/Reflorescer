"""Technical slicing of ImageGen sheets; no procedural replacement artwork.

Usage: python tools/art/slice_delivery.py <kairo-sheet> <faisca-sheet> [run-fix]
Requires Pillow. Run from the repository root.
"""
from pathlib import Path
import sys
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
NEAREST = Image.Resampling.NEAREST


def save(image, relative):
    path = ROOT / relative
    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path)


def main():
    kairo = Image.open(sys.argv[1]).convert('RGBA').resize((720, 510), NEAREST)
    names = ['idle_00', 'idle_01', 'run_00', 'run_01', 'jump_00',
             'fall_00', 'attack_00', 'attack_01', 'attack_02', 'hurt_00', 'dead_00']
    for index, name in enumerate(names):
        x, y = index % 4 * 180, index // 4 * 170
        save(kairo.crop((x, y, x + 180, y + 170)),
             f'assets/sprites/kairo_faisca/kairo_faisca_{name}.png')
    save(kairo, 'assets/sprites/kairo_faisca/kairo_faisca_sheet.png')

    if len(sys.argv) > 3:
        corrected = Image.open(sys.argv[3]).convert('RGBA').resize((180, 170), NEAREST)
        canvas = Image.new('RGBA', (180, 170))
        canvas.paste(corrected, (0, 168 - corrected.getbbox()[3]))
        save(canvas, 'assets/sprites/kairo_faisca/kairo_faisca_run_01.png')
        kairo.paste(canvas, (540, 0))
        save(kairo, 'assets/sprites/kairo_faisca/kairo_faisca_sheet.png')

    source = Image.open(sys.argv[2]).convert('RGBA')
    sheet = Image.new('RGBA', (128, 96))
    resources, animations = [], []
    for row, (name, fps) in enumerate([('idle', 5), ('fly', 12), ('investigate', 6)]):
        frames = []
        for column in range(4):
            # Fixed cells keep relative head/abdomen motion; never trim each frame.
            box = (round(column * source.width / 4), round(row * source.height / 3),
                   round((column + 1) * source.width / 4), round((row + 1) * source.height / 3))
            frame = source.crop(box).resize((32, 32), NEAREST)
            filename = f'assets/sprites/faisca/faisca_{name}_{column:02}.png'
            save(frame, filename)
            sheet.paste(frame, (column * 32, row * 32))
            rid = row * 4 + column + 1
            resources.append(f'[ext_resource type="Texture2D" path="res://{filename}" id="{rid}"]')
            frames.append('{"duration": 1.0, "texture": ExtResource("%s")}' % rid)
        animations.append('{"frames": [%s], "loop": true, "name": &"%s", "speed": %s.0}' %
                          (', '.join(frames), name, fps))
    save(sheet, 'assets/sprites/faisca/faisca_sheet.png')
    (ROOT / 'assets/sprites/faisca/FaiscaSpriteFrames.tres').write_text(
        '[gd_resource type="SpriteFrames" load_steps=13 format=3]\n\n' + '\n'.join(resources) +
        '\n\n[resource]\nanimations = [' + ',\n'.join(animations) + ']\n', encoding='utf-8')


if __name__ == '__main__':
    main()
