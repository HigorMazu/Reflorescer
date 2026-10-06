"""One-time integration for the 77ece44 scene layout, preserving gameplay nodes."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]


def main():
    p = ROOT / 'scenes/companion/Faisca.tscn'
    s = p.read_text(encoding='utf-8-sig')
    assert 'FaiscaSpriteFrames.tres' not in s, 'Already integrated'
    i = s.index('[sub_resource')
    s = s[:i] + '[ext_resource type="SpriteFrames" path="res://assets/sprites/faisca/FaiscaSpriteFrames.tres" id="art_frames"]\n\n' + s[i:]
    s = re.sub(r'\[sub_resource type="SpriteFrames"[^\n]*\n[\s\S]*?(?=\[node)', '''[sub_resource type="Gradient" id="Gradient_glow"]
offsets = PackedFloat32Array(0, 0.3, 1)
colors = PackedColorArray(1, 1, 1, 0.55, 1, 1, 1, 0.18, 1, 1, 1, 0)

[sub_resource type="GradientTexture2D" id="GradientTexture2D_glow"]
gradient = SubResource("Gradient_glow")
width = 64
height = 64
fill = 1
fill_from = Vector2(0.5, 0.5)
fill_to = Vector2(0.5, 0)

''', s)
    s = re.sub(r'\[node name="(?:Body|WingLeft|WingRight|Eye)"[^\n]*\n[\s\S]*?(?=\[node|\Z)', '', s)
    s = s.replace('sprite_frames = SubResource("SpriteFrames_faisca")', 'scale = Vector2(-0.75, 0.75)\ntexture_filter = 1\nsprite_frames = ExtResource("art_frames")')
    s = s.replace('energy = 1.5', 'energy = 0.65\ntexture = SubResource("GradientTexture2D_glow")')
    p.write_text(s, encoding='utf-8')

    p = ROOT / 'scenes/areas/SopeDaMata.tscn'
    s = p.read_text(encoding='utf-8-sig')
    assert 'art_ground' not in s, 'Already integrated'
    i = s.index('[sub_resource')
    refs = '''[ext_resource type="Texture2D" path="res://assets/environment/tropical/ground_restored.png" id="art_ground"]
[ext_resource type="Texture2D" path="res://assets/environment/tropical/ground_degraded.png" id="art_dry"]
[ext_resource type="Texture2D" path="res://assets/environment/tropical/sprout_restored.png" id="art_sprout"]
[ext_resource type="Texture2D" path="res://assets/environment/tropical/sprout_degraded.png" id="art_stump"]
[ext_resource type="PackedScene" path="res://scenes/art/TropicalBackdrop.tscn" id="art_forest"]
[ext_resource type="Shader" path="res://assets/shaders/foliage_sway.gdshader" id="art_sway"]

[sub_resource type="ShaderMaterial" id="ShaderMaterial_grass"]
shader = ExtResource("art_sway")

'''
    s = s[:i] + refs + s[i:]
    s = re.sub(r'\[node name="Background"[^\n]*\n[\s\S]*?(?=\[node)', '[node name="Background" parent="." instance=ExtResource("art_forest")]\n\n', s)

    def terrain(m):
        b = m[0].replace('type="ColorRect"', 'type="NinePatchRect"')
        b = re.sub(r'^color = .*\n', '', b, flags=re.M)
        b = re.sub(r'offset_top = (-?[\d.]+)', lambda t: f'offset_top = {float(t[1])-4}', b)
        return b.rstrip() + '\ntexture_filter = 1\nmaterial = SubResource("ShaderMaterial_grass")\ntexture = ExtResource("art_ground")\naxis_stretch_horizontal = 2\n\n'

    s = re.sub(r'\[node name="Visual" type="ColorRect" parent="Geometry/(?:Ground[^\"]*|Platform\d+)"[^\n]*\n[\s\S]*?(?=\[node|\Z)', terrain, s)
    s = re.sub(r'\[node name="Top" type="ColorRect" parent="Geometry/(?:Ground[^\"]*|Platform\d+)"[^\n]*\n[\s\S]*?(?=\[node|\Z)', '', s)
    for name, texture in [('DegradedGround', 'art_dry'), ('RestoredGround', 'art_ground'), ('Stump', 'art_stump'), ('Sprout', 'art_sprout')]:
        is_ground = name.endswith('Ground')
        kind = 'NinePatchRect' if is_ground else 'TextureRect'
        hidden = 'visible = false\n' if name in ('RestoredGround', 'Sprout') else ''
        props = 'offset_left = -56.0\noffset_top = -4.0\noffset_right = 56.0\noffset_bottom = 80.0\naxis_stretch_horizontal = 2' if is_ground else 'offset_left = -20.0\noffset_top = -32.0\noffset_right = 20.0\noffset_bottom = 0.0'
        def replace(m):
            header = m[0].split('\n')[0].replace('type="ColorRect"', f'type="{kind}"')
            return header + '\n' + hidden + props + f'\ntexture_filter = 1\nmouse_filter = 2\ntexture = ExtResource("{texture}")\n\n'
        s = re.sub(r'\[node name="' + name + r'" type="ColorRect" parent="Interactables/RestorationPoint1"[^\n]*\n[\s\S]*?(?=\[node|\Z)', replace, s)
    p.write_text(s, encoding='utf-8')


if __name__ == '__main__':
    main()
