# Espada e tamanho dos mobs — 07/10/2026

Esta entrega v1 foi sucedida pela espada reta e pelo segundo aumento dos mobs. Estado atual e prompts v2: `straight-sword-v2-delivery.md`.

Pedido de Higor após aprovar o indicador: retirar a coloração verde de Kairo, animar espada aparecendo/recolhendo e ampliar um pouco os mobs.

- Removido ApplySwordTint; as cores originais são mantidas nos dois modos. Flashes de dano e ataque não foram alterados.
- GrassSwordVisual estende/recolhe a lâmina em 0,24 s a partir da mão, com tween interrompível que continua do ponto atual. O estado de combate e HUD mudam imediatamente; a animação não bloqueia input.
- Lâmina separada segue posições por pose/quadro e acompanha o espelhamento de Visual. Nos ataques, usa-se a lâmina já presente nos quadros originais, sem sobreposição. Um ataque iniciado continua até terminar, conforme regra existente.
- Pulo usa novo quadro sem lâmina; original preservado. Bandagem desenhada no corpo permanece. A01-T2 continua parcial por não haver bandagem como sprite independente.
- Formiga, tartaruga e vespa: escala de 0,4166667 para 0,5 (+20%), offset Y de -20 para -24 para conservar base dos pés. Sem mudança em colisões, dano, alcance, velocidade ou IA.

Validação: build sem erros/avisos; 19 verificações em SwordHudCheck e 160 em EnemyIntegrationCheck. Revisão visual das poses idle/run/jump/fall. SFX de espada continuam ausentes. Playtest completo e saves reais não foram executados.

`sword-toggle-mobs.gif` vem de 48 capturas reais do Godot, em câmera lenta para revisão (Kairo ampliado 4× e mobs 3×, não representa proporção entre eles no jogo). Reprodução em `scenes/art/SwordToggleReview.tscn`; opção `-- --poses` captura poses neutras. HUD permanece aprovado.

## Assets e geração

Ferramenta imagegen integrada, não CLI. Arquivos finais em `assets/sprites/kairo_faisca/grass_sword_v1.png` (8×28) e `kairo_jump_unarmed_v1.png` (180×170). Originais gerados preservados em `assets/sprites/kairo_faisca/sword_source_v1/`. Normalização somente por corte, escala e posicionamento, em `tools/art/prepare_sword_assets.py`.

Prompt da espada: “Use case: stylized-concept. Asset: one standalone game sprite, grass sword for the attached snow leopard character. Generate ONLY ONE isolated grass blade, no character, no hand, no bandage, no text. Match the green sword in the reference: compact tapered slightly curved leaf blade with a sharp tip, olive dark outline, chartreuse edge and pale vein, short wrapped vine grip. Pixel art crisp clusters, limited palette. Sword vertical tip pointing UP, handle pointing DOWN, perfectly aligned along center. Occupies most height with generous transparent margins. Truly transparent background. No glow cloud or particles. Reference is style only.” Referência: kairo_faisca_sheet.png.

Prompt do pulo: “Use case precise-object-edit. Edit the supplied transparent game sprite. Remove ONLY the green sword blade extending DOWN from the character's near hand on the left of the image. Retain the wrist green bandage and closed fist. Keep the entire character, face, outfit, spots, tail, jumping pose, proportions, pixel art style and placement UNCHANGED. Same 180x170 canvas composition with original transparent margins, no recentering or new elements. Restore transparent pixels where blade was, do not change hand or arm. Output transparent background.” Referência: kairo_faisca_jump_00.png. A geração foi normalizada ao canvas original e revisada, não é edição pixel a pixel idêntica.
