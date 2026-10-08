# Correção visual de Kairo e Korrag — revisão v2, 08/10/2026

Esta revisão substitui a integração anterior de dash/apoio, empunhadura v6 e sprites recortados do Korrag.

## Kairo

- `dash` usa os mesmos quadros de corrida, canvas 180×170, escala, paleta e contorno das animações normais de Kairo. O squash horizontal já aplicado pela mecânica continua comunicando o impulso.
- `wall_slide` usa o quadro original de salto, espelhado pela regra normal do personagem quando a parede está à esquerda. O atlas gerado que mudava anatomia, escala e acabamento foi removido.
- A espada agora gira pelo centro do cabo. O punho duplicado (`SwordGripOverlay`) foi removido; a lâmina fica atrás da mão original desenhada no corpo.
- As âncoras foram medidas quadro a quadro em `idle`, `run`, `jump`, `fall`, `dash`, `wall_slide` e nos três quadros de `attack`.

Capturas: `kairo-movement-v2.png`, `sword-grip-v8-idle.png` e `sword-grip-v8-attack.png`.

## Korrag

O spritesheet anterior continha recortes contaminados, partes cortadas e várias animações de um único quadro. Ele foi substituído por `assets/sprites/korrag/korrag_animation_sheet_v2.png`, integrado como atlas regular no `KorragSpriteFrames.tres`.

O pacote de produção tem 13 estados: `idle`, `walk`, `intro`, `charge_windup`, `charge`, `stomp`, `attack`, `hurt`, `phase1_idle`, `phase2_transition`, `enraged_transition`, `dead` e `defeated`. Investida e pisada agora chamam animações próprias. O boss foi ampliado para escala 0,9 e alinhado ao chão da arena.

Capturas: `korrag-animation-v2.png` e `covil-arena-v2.png`.

O bitmap foi criado com a ferramenta integrada de geração de imagens, usando `korrag_sheet.png` como referência exata do personagem e `kairo_faisca_sheet.png` como referência de densidade, contorno e estilo de sprite. Prompt final:

> Use case: stylized-concept. Asset type: production sprite sheet for a 2D side-view Godot boss. Input images: Image 1 is the exact Korrag character design reference; Image 2 is the exact rendering scale, pixel density, outline treatment, and game sprite style reference. Create a CLEAN 4 columns by 4 rows sprite sheet of Korrag, a gigantic corrupted wild boar covered in dark bark armor, roots, moss, long ivory tusks and glowing red eyes. Korrag must keep exactly the same anatomy, armor, colors, silhouette and side-facing direction in every cell. Each cell must contain one complete full-body pose, centered on the same ground baseline, with generous transparent padding and absolutely no pixels crossing cell boundaries. Row 1: two calm idle breathing poses, two heavy walking poses. Row 2: intro roar pose, crouched charge windup, fast horizontal charge pose, ground stomp impact pose. Row 3: two melee tusk sweep poses, hurt recoil pose, phase-two transformation with stronger red corruption. Row 4: enraged idle with controlled red glow, death collapse start, death collapse end, defeated still pose. Style/medium: crisp detailed pixel art matching Image 2, consistent clustered pixels and limited palette; Korrag should read as roughly 2.5 times Kairo's height in game. Composition: exact regular 4x4 grid, equal cell dimensions, no grid lines, same ground baseline inside every cell. Constraints: genuinely transparent background, Korrag only, no Kairo, no Faísca, no other characters, no UI, no text, no labels, no borders, no scenery, no black background, no clipping, no duplicated heads or limbs, no frame overlap, no motion trails outside the character's own cell.

## Verificação

- Compilação .NET: 0 erros e 0 avisos.
- `KorragAnimationCheck.gd`: todos os 13 estados, contagens mínimas, escala, alinhamento e chamadas de investida/pisada aprovados.
- `BossDefeatCheck.tscn`: morte, `defeated`, fade e emissão única da vitória aprovados.
- `MovementPoseCheck.gd`: dash/apoio no `SpriteFrames` de produção e atlas incompatível removido.
- `SwordHudCheck.tscn`: cabo, camada da mão original e âncora de todos os quadros aprovados.
- `AttackThrustCheck.tscn`: preparação, impacto, hitbox e recuperação aprovados.

A aprovação visual de Higor permanece pendente; estes testes confirmam integração e consistência, mas não substituem o teste jogado.
