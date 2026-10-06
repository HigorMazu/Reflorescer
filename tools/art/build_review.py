"""Compose review GIFs from the production PNG frames (Pillow required)."""
from pathlib import Path
import math
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'docs/art-review'
OUT.mkdir(parents=True, exist_ok=True)
FONT = ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf', 18)
TITLE = ImageFont.truetype('C:/Windows/Fonts/segoeuib.ttf', 28)


def frame(path):
    return Image.open(ROOT / path).convert('RGBA')


def render_kairo():
    specs = [('idle', 2, 5), ('run', 2, 8), ('jump', 1, 5), ('fall', 1, 5),
             ('attack', 3, 10), ('hurt', 1, 5), ('dead', 1, 5)]
    images = []
    for step in range(60):
        image = Image.new('RGBA', (1260, 285), '#142622')
        draw = ImageDraw.Draw(image)
        draw.text((26, 14), 'KAIRO / sete animações núcleo · sem Faísca embutida', font=TITLE, fill='#e6e9d3')
        for index, (name, count, fps) in enumerate(specs):
            t = (step * .05) % 1.5
            idx = int(t * fps) % count if name in ('idle', 'run') else min(int(t * fps), count - 1)
            sprite = frame(f'assets/sprites/kairo_faisca/kairo_faisca_{name}_{idx:02}.png')
            image.alpha_composite(sprite, (index * 180, 70))
            draw.text((index * 180 + 58, 251), name, font=FONT, fill='#adc3ad')
        images.append(image.convert('RGB'))
    images[0].save(OUT / 'kairo-animations.gif', save_all=True, append_images=images[1:], duration=50, loop=0)
    images[0].save(ROOT / 'assets/sprites/sprite_preview.png')


def render_faisca():
    images = []
    for step in range(60):
        image = Image.new('RGBA', (960, 300), '#142622')
        draw = ImageDraw.Draw(image)
        draw.text((24, 14), 'FAÍSCA / curiosa, inquieta, independente', font=TITLE, fill='#e6e9d3')
        for i, (name, fps) in enumerate([('idle', 5), ('fly', 12), ('investigate', 6)]):
            idx = int(step * .05 * fps) % 4
            sprite = frame(f'assets/sprites/faisca/faisca_{name}_{idx:02}.png').resize((160, 160), Image.Resampling.NEAREST)
            image.alpha_composite(sprite, (55 + i * 310, 65))
            draw.text((85 + i * 310, 239), name, font=FONT, fill='#e4c876')
        draw.text((24, 274), 'Ampliação para revisão. No jogo: canvas de 24 px; corpo menor que Kairo.', font=FONT, fill='#adc3ad')
        images.append(image.convert('RGB'))
    images[0].save(OUT / 'faisca-animations.gif', save_all=True, append_images=images[1:], duration=50, loop=0)


if __name__ == '__main__':
    render_kairo()
    render_faisca()
