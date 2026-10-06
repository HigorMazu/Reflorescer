"""Slice approved ImageGen cycles, keeping one scale per character, and build resources.

Run from any directory: python tools/art/build_enemy_cycles.py
Pillow only slices, resizes and arranges the generated art; it does not draw frames.
"""
from pathlib import Path
from PIL import Image, ImageDraw
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
ASSETS = ROOT / 'assets/sprites/enemies'
NAMES = {'voador': 'Voador', 'rapido': 'Rapido', 'robusto': 'Robusto'}
RATES = {'voador': (10, 14), 'rapido': (5, 10), 'robusto': (3, 5)}
MANIFEST = {}

def gutters(counts, pieces):
    """Locate empty gutters near expected grid lines (generation may offset rows)."""
    cuts = [0]
    for index in range(1, pieces):
        nominal = round(index * len(counts) / pieces)
        radius = round(len(counts) / pieces * .28)
        start, end = nominal - radius, nominal + radius
        empty = np.where(counts[start:end] <= 1)[0] + start
        if len(empty):
            runs = np.split(empty, np.where(np.diff(empty) > 1)[0] + 1)
            longest = max(runs, key=len)
            cuts.append(int((longest[0] + longest[-1]) // 2))
        else:
            cuts.append(nominal)
    return cuts + [len(counts)]

for enemy, title in NAMES.items():
    sheet = Image.open(ASSETS / 'sources' / f'{enemy}-cycles-v2.png').convert('RGBA')
    assert sheet.getchannel('A').getextrema()[0] == 0, 'Source must have real transparency'
    crops = []
    alpha = np.asarray(sheet)[:, :, 3] > 12
    rows = gutters(alpha.sum(axis=1), 3)
    for row in range(3):
        columns = gutters(alpha[rows[row]:rows[row + 1]].sum(axis=0), 4)
        for col in range(4):
            cell = sheet.crop((columns[col], rows[row], columns[col + 1], rows[row + 1]))
            bbox = cell.getchannel('A').point(lambda v: 255 if v > 12 else 0).getbbox()
            assert bbox, (enemy, row, col)
            crops.append(cell.crop(bbox))
    # A single factor across the whole sheet avoids resizing the body as wings/legs move.
    factor = min(96 / max(c.width for c in crops), 80 / max(c.height for c in crops))
    target = ASSETS / enemy / 'cycles'
    target.mkdir(exist_ok=True)
    animations = {}
    for row, state in enumerate(('idle', 'walk', 'attack')):
        animations[state] = []
        for frame in range(4):
            crop = crops[row * 4 + frame]
            crop = crop.resize((round(crop.width * factor), round(crop.height * factor)), Image.Resampling.NEAREST)
            canvas = Image.new('RGBA', (128, 96))
            canvas.alpha_composite(crop, ((128 - crop.width) // 2, 96 - crop.height))
            path = target / f'{enemy}_{state}_{frame:02}.png'
            canvas.save(path)
            animations[state].append(path)
    original = lambda state: ASSETS / enemy / f'{enemy}_{state}_00.png'
    animations['detect'] = [animations['idle'][0], original('detect')]
    animations['hurt'] = [animations['attack'][1] if enemy == 'robusto' else original('hurt'), animations['idle'][0]]
    animations['dead'] = [original('dead')]
    rates = dict(idle=RATES[enemy][0], walk=RATES[enemy][1], attack=4, detect=4, hurt=2 / .3, dead=1 / .55)
    paths = list(dict.fromkeys(path for frames in animations.values() for path in frames))
    ids = {path: str(i + 1) for i, path in enumerate(paths)}
    lines = [f'[gd_resource type="SpriteFrames" load_steps={len(paths) + 1} format=3]', '']
    for path in paths:
        lines.append(f'[ext_resource type="Texture2D" path="res://{path.relative_to(ROOT).as_posix()}" id="{ids[path]}"]')
    lines += ['', '[resource]', 'animations = [']
    blocks = []
    for state, paths_for_state in animations.items():
        frames = ', '.join('{"duration": 1.0, "texture": ExtResource("' + ids[p] + '")}' for p in paths_for_state)
        blocks.append('{"frames": [' + frames + '], "loop": ' + str(state in ('idle', 'walk')).lower() +
                      f', "name": &"{state}", "speed": {rates[state]:.6f}' + '}')
    lines += [',\n'.join(blocks), ']']
    (ASSETS / f'Enemy{title}SpriteFrames.tres').write_text('\n'.join(lines) + '\n', encoding='utf-8')
    MANIFEST[enemy] = animations

# Preview actual exported frames, with no added animation or interpolated poses.
preview_frames = []
states = ('idle', 'walk', 'detect', 'attack', 'hurt', 'dead')
for tick in range(24):
    board = Image.new('RGB', (900, 700), '#142720')
    draw = ImageDraw.Draw(board)
    draw.text((20, 12), 'REFLORESCER / ciclos dos inimigos / PNGs ampliados em relacao ao jogo', fill='#f0e7c3')
    for column, (enemy, title) in enumerate(NAMES.items()):
        draw.text((30 + column * 300, 40), title, fill='#d9be7c')
        for row, state in enumerate(states):
            frames = MANIFEST[enemy][state]
            frame = Image.open(frames[tick % len(frames)]).convert('RGBA')
            # PNGs at source size: already ~2.4x their gameplay size.
            board.paste(frame, (90 + column * 300, 62 + row * 103), frame)
            draw.text((20 + column * 300, 102 + row * 103), state, fill='#c7dbc5')
    preview_frames.append(board)
review = ROOT / 'docs/art-review'
preview_frames[0].save(review / 'enemy-cycles.png')
preview_frames[0].save(review / 'enemy-cycles.gif', save_all=True, append_images=preview_frames[1:], duration=140, loop=0)
print('36 generated animation frames; three resources; PNG/GIF previews exported.')
