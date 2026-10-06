"""Check the art contract and ensure the art pass did not change gameplay."""
from pathlib import Path
import re
import subprocess
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]


def baseline(path):
    return subprocess.check_output(['git', 'show', f'HEAD:{path}'], cwd=ROOT).decode('utf-8-sig')


def blocks(text):
    return re.findall(r'\[(?:node|sub_resource) [\s\S]*?(?=\n\[|\Z)', text)


def main():
    resources = ['assets/sprites/kairo_faisca/KairoFaiscaSpriteFrames.tres',
                 'assets/sprites/faisca/FaiscaSpriteFrames.tres']
    for path, names, size, count in [
        (resources[0], {'idle', 'run', 'jump', 'fall', 'attack', 'hurt', 'dead'}, (180, 170), 11),
        (resources[1], {'idle', 'fly', 'investigate'}, (32, 32), 12),
    ]:
        text = (ROOT / path).read_text(encoding='utf-8-sig')
        assert set(re.findall(r'"name": &"([^"]+)"', text)) == names
        textures = re.findall(r'path="res://([^\"]+\.png)"', text)
        assert len(textures) == count
        for texture in textures:
            image = Image.open(ROOT / texture)
            assert image.size == size, (texture, image.size)
            assert image.mode == 'RGBA'
            assert image.getchannel('A').getextrema()[0] == 0
        print(f'PASS {path}: {count} frames, {size}, names and alpha')
    assert (ROOT / resources[0]).read_text(encoding='utf-8-sig') == baseline(resources[0]).replace('\r\n', '\n')
    print('PASS Kairo frame counts, FPS, duration and loop flags unchanged')
    for path in ['scenes/player/Player.tscn', 'scenes/levels/TestLevel.tscn', 'scenes/areas/SopeDaMata.tscn']:
        old = baseline(path).replace('\r\n', '\n')
        new = (ROOT / path).read_text(encoding='utf-8-sig')
        if '/player/' in path:
            expected = old.replace('position = Vector2(-45, -85)', 'position = Vector2(-45, -65)')
            assert expected == new, 'Unexpected change outside the uniform 20px visual alignment'
        else:
            def collision(text):
                return [b.strip() for b in blocks(text) if any(t in b.split('\n')[0] for t in
                        ['Shape2D', 'StaticBody2D', 'Area2D'])]
            assert collision(old) == collision(new), 'Gameplay collision or trigger changed'
    changed = subprocess.check_output(['git', 'diff', '--name-only'], cwd=ROOT, text=True).splitlines()
    assert not any(p.endswith('.cs') or p.startswith('resources/') for p in changed)
    print('PASS uniform visual alignment; collision shapes, triggers, C# and gameplay stats unchanged')
    for directory in ['scenes/art', 'assets/sprites/faisca']:
        for path in (ROOT / directory).glob('*'):
            if path.suffix not in ('.tscn', '.tres'): continue
            for ref in re.findall(r'path="res://([^\"]+)"', path.read_text(encoding='utf-8-sig')):
                assert (ROOT / ref).is_file(), ref
    companion = (ROOT / 'scenes/companion/Faisca.tscn').read_text(encoding='utf-8-sig')
    assert '"texture": null' not in companion
    assert 'type="ColorRect"' not in companion
    assert 'FaiscaSpriteFrames.tres' in companion
    print('PASS separate Faísca resource; no null frames or duplicated placeholder shapes')


if __name__ == '__main__':
    main()
