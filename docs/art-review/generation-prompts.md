# Registro de geração

Ferramenta: ImageGen integrada. Os resultados selecionados foram copiados/recortados para o repositório. Pillow apenas exportou células, ajustou canvas e montou prévias; não desenhou substitutos para a arte.

## Kairo — edição da folha

Remover somente as Faíscas e suas asas/antenas/brilho da folha original, preservar identidade, poses, paleta, roupas, espada e layout 4×3 de células 180×170; fundo transparente. Resultado normalizado a 720×510 e recortado em 11 PNGs. A ferramenta redesenhou detalhes, portanto preservação pixel a pixel não é alegada.

## Faísca — prompt

Use case: stylized-concept. Production transparent pixel art animation spritesheet for Reflorescer, original tiny firefly companion Faísca. Exactly 4 columns and 3 rows of equal square cells, 12 frames total, canvas 1024x768. Each cell 256x256 with same creature centered at x=128 y=128, fully inside its cell, generous transparent gutters. Row 1 idle: breathing abdomen, wings flick, comical uneven antennae and one curious raised brow, blink on third frame. Row 2 fly: four genuinely distinct alternating wing beats, body leans slightly forward facing right, feet tucked. Row 3 investigate: leans and peers down, antennae curl, mischievous squint, head tilt returning. Same body size and registration across ALL frames. Tiny recognizable six-legged beetle firefly, dark olive thorax, warm amber bioluminescent segmented abdomen, two translucent mint wings, two slightly asymmetric antennae, expressive cream eyes but NOT human face, no clothes. Charming odd little natural creature, not a floating orb or fairy. Crisp hand-crafted pixel clusters, limited forest palette, enough simplicity to read at 18 game pixels. Each sprite occupies roughly 160x160 of its 256 cell. No floor, no props, no labels, no text, no borders, no baked checkerboard, no cast shadow, no halo beyond cell. Real alpha transparency.

## Terreno — prompt

Use case: stylized-concept. Production game terrain asset for an original nature restoration 2D metroidvania Reflorescer. Transparent sprite atlas, exactly two horizontal platforms one above the other, each fully separated by large transparent gutters. Canvas 1536x1024. Upper platform healthy tropical forest, lower platform degraded version of the SAME shape and footprint. Each platform spans x=64 through1472; top healthy walking surface at y=120, soil down to y=390; lower walking surface y=632, soil down to y=902. Strict orthographic SIDE VIEW no perspective top plane. Organic horizontal ledge with clear straight walkable top, lush finely clustered grass hanging slightly over dark fertile earth, winding intricate ochre roots entering moist angular slate rocks, moss and tiny ferns near corners, occasional small mushrooms. Readable layers grass -> earth -> roots -> stone. Irregular bottom silhouette but broad nearly rectangular soil mass suited to cropping and tiling as level ground. Degraded version pale sparse dried grass, cracked earthy umber clay, exposed dead roots and slate, same shape, no glow. Restrained handcrafted pixel art consistent with detailed 90-pixel-tall snow leopard character. Crisp square pixel clusters, no blurry painterly rendering. Rich natural olive/jade palette, warm brown roots, muted slate blue rocks, clear upper rim for gameplay. No characters, no background scenery, no text, no grids, no borders, no checkerboard. Actual alpha transparency.

O arquivo selecionado tem alfa transparente real, confirmado na leitura dos pixels e na renderização Godot. Uma tentativa adicional de extração de fundo foi descartada. Os módulos de chão são recortes internos da massa de terra; as plataformas inteiras são estudos para revisão.

## Fundo — prompt

Use case: stylized-concept. Game background panorama for Reflorescer, original 2D side scrolling tropical rainforest restoration game, 1536x864 landscape. Strict side-view background only, NO characters NO foreground platforms NO foreground ground cross section NO UI or lettering. Enormous old rainforest trees with buttress roots, layers of silhouettes receding into humid sage-green and muted teal mist, ferns and hanging lianas near left and right edges, canopy filtering sparse warm sunlight diagonally, small calm water glints far below. Preserve open negative space in middle and lower middle for foreground gameplay and a bright cream snow leopard silhouette. Handcrafted finely detailed pixel art with coherent square pixel clusters, dark forest edges, muted atmosphere, never photoreal or blurry. Background low contrast with no bright fireflies that could be confused with companion. Forest has its own natural identity, no temples no architecture. Frame left/right similar value for scrolling, strong depth, tranquil living ecosystem.

## Correção adicional de run_01

Edição do segundo quadro de corrida usando o primeiro como referência de identidade: completar cabeça/focinho/orelhas cortados, manter o passo oposto de corrida, não incluir Faísca, poeira ou cenário, deixar margem direita e canvas transparente 180:170. Exportação em 180×170 com base da silhueta em y=168.

