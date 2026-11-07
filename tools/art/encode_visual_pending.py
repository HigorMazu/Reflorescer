"""Encode renderer captures from VisualPendingCapture.gd with a shared GIF palette."""
import os
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path(os.environ['APPDATA']) / 'Godot/app_userdata/Joguim - Metroidvania/visual-review-frames'
paths = [SOURCE / f'combat-{move}-{i:02d}.png'
         for move, count in [('StartStrike', 16), ('StartTail', 16), ('StartBurrow', 30)]
         for i in range(count)]
frames = [Image.open(path).convert('RGB').resize((960, 540), Image.Resampling.LANCZOS)
          for path in paths]
sample = Image.new('RGB', (960, 540 * 3))
for row, index in enumerate((4, 20, 45)):
    sample.paste(frames[index], (0, row * 540))
palette = sample.quantize(colors=192)
indexed = [frame.quantize(palette=palette, dither=Image.Dither.NONE) for frame in frames]
output = ROOT / 'docs/art-review/desert-combat-review-v3.gif'
indexed[0].save(output, save_all=True, append_images=indexed[1:], duration=100,
                loop=0, optimize=True)
print(f'{output.name}: {len(indexed)} frames, {output.stat().st_size} bytes')
