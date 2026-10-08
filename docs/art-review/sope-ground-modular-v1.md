# Miolo modular do chão do Sopé da Mata — 08/10/2026

As quatro faixas de chão saudável e as três plataformas agora usam `scripts/art/TropicalGroundStrip.gd`. Cada faixa mantém pontas de 24 px da arte existente e alterna módulos centrais da textura original com uma nova variação de raízes, musgo e pedras. O desenho do solo muda ao longo das faixas sem alterar os sete `StaticBody2D` nem suas colisões. As duas texturas de restauração continuam usando `NinePatchRect` com pontas preservadas.

As capturas `sope-ground-modular-0.png` e `sope-ground-modular-900.png` vêm da cena de produção, via `scenes/art/SopeTerrainReview.tscn`. Higor aprovou a variação em 08/10. A05-T1 permanece parcial porque faltam os props definitivos, a revisão do percurso/FPS e o tileset de produção.

O novo bitmap está em `assets/environment/tropical/ground_modular_candidate_v1.png`. Foi criado com a ferramenta de geração de imagem integrada, usando `assets/environment/tropical/ground_restored.png` como referência de estilo. Prompt final:

> Use case: stylized-concept. Asset type: horizontally repeating 2D platformer ground strip for the Sopé da Mata level of Reflorescer. The supplied PNG is a style and palette reference, not an edit target. Generate one NEW terrain strip variant: crisp hand-crafted pixel art with a flat, readable grassy top edge, dark humid soil, intertwined fine roots, moss and a few gray stones hanging below. Keep the same earthy olive-green, dark umber, subdued gray palette and approximately the same grass-to-soil proportions as the reference. Fill a wide horizontal composition edge to edge, without a background scene. Make the left and right edges seamlessly tileable, with the grass line at exactly the same height on both edges. Transparent background around the hanging underside. No characters, UI, labels, border, shadows outside the terrain, or watermark.

Validação: a cena abriu e capturou os dois trechos no Godot 4.7.2; compilação .NET sem avisos ou erros; `SwordHudCheck` com 26 verificações e `AttackThrustCheck` com sete verificações aprovadas.
