# Correção de piso e empunhadura — 09/10/2026

## Lama do Igarapé

As três poças continuam sendo áreas de lentidão de 55%. O desenho modular do chão agora deixa três aberturas reais para o leito de lama. A água, as pedras e a vegetação do atlas ocupam essas aberturas na mesma linha física do terreno; as extremidades recebem uma transição suavizada.

As poças agora usam um sheet de seis quadros recortados da própria arte de lama. Duas bolhas crescem, rompem a superfície, soltam pequenas gotas e se desfazem em ondulações. As margens, pedras e plantas permanecem nos mesmos pixels em todos os quadros; as três poças começam em fases diferentes. A lama fica abaixo de Kairo, Faísca e dos inimigos.

Capturas atuais: `igarape-integrated-v3.png`, `igarape-bubbles-v3-0.png` e `igarape-bubbles-v3-2.png`.

Prévia atualizada: `igarape-mud-sheet-v1.gif` e `igarape-mud-sheet-in-game-v4.png`. O sheet usado pelo jogo é `assets/environment/tropical/igarape_mud_bubble_sheet_v1.png` e pode ser regenerado com `python tools/art/build_mud_sheet.py`.

O ImageGen foi usado para estudar a sequência visual com este pedido: "Seis quadros iguais da poça pixel art original, com bolha pequena, crescimento, rompimento com borda irregular e gotas, anéis de ondulação e retorno à superfície plana; margens, pedras e plantas fixas; fundo transparente." O estudo está em `mud-bubble-sheet-concept-v1.png`. O sheet final foi montado sobre o atlas existente para conservar os mesmos pixels do terreno entre os quadros.

## Linha do chão do Covil

O desenho modular do chão estava 40 pixels abaixo da colisão. Ele foi elevado até a mesma linha usada por Kairo e Korrag, eliminando a aparência de flutuação.

Captura: `covil-ground-alignment-v3.png`.

## Espada de Grama

A variação de cabo marrom foi retirada. Kairo voltou à Espada de Grama verde. A arte renderizada é recortada antes da guarda: somente a lâmina deixa o punho direito fechado. A mão e o antebraço cobrem a empunhadura. A lâmina ativa foi alongada em 40% sem aumentar sua largura e mantém ângulos próprios em corrida, queda e dash.

Nos dois quadros de corrida, a âncora foi medida novamente no punho. Apenas nessa pose, a lâmina passa à frente da cauda e uma pequena região da mão original é desenhada por cima, cobrindo o cabo sem criar uma segunda mão. As outras poses da espada não foram alteradas nesta revisão.

Capturas atuais: `sword-long-v11-idle.png` e `sword-long-v11-attack.png`.

Corrida revisada: `sword-run-grip-v12-0.png` e `sword-run-grip-v12-1.png`.

Prévia dos dois quadros: `sword-run-grip-v12.gif`.


## Verificação

- Compilação .NET: 0 erros e 0 avisos.
- `IgarapeAreaCheck.gd`: 0 falhas, incluindo as três poças no nível exato do chão.
- `CovilAreaCheck.gd`: 0 falhas, incluindo a linha comum entre arte e colisão.
- `SwordHudCheck.tscn`: 0 falhas, incluindo a lâmina visível, o punho, o toggle e os três quadros de ataque.
- O sheet tem seis quadros de 1100×140; a prévia GIF conserva os seis. A renderização do Godot foi inspecionada nos dois quadros de corrida e no terceiro trecho da fase.
