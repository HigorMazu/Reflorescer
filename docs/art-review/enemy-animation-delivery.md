# Integração de inimigos — 06/10/2026

Branch: `master`. Alterações locais, sem commit/push. O usuário aprovou os designs; este pacote acrescenta movimento e validação à primeira passagem de poses.

## Entrega

- 36 quadros novos (12 por arquétipo), preservados em `assets/sprites/enemies/<arquétipo>/cycles/`.
- Cada SpriteFrames: idle 4, walk 4, attack 4, detect 2, hurt 2, dead 1 + fade. Detect/hurt reutilizam poses, sem prometer quadros exclusivos para cada estado.
- Canvas 128×96; escala 5/12 e centro em (0,-20). Extensão máxima desenhada: 40 px de largura, sem modificar o corpo de colisão 28×32.
- Idle: vespa 10 fps, formiga 5, tartaruga 3. Walk: 14/10/5 fps. Attack: quatro quadros ajustados por instância ao cooldown do recurso de área, com impacto no terceiro quadro (metade do intervalo).
- Robusto mantém `InterruptOnHit=false`; seu hurt é disponível para revisão, mas não interrompe ataques durante gameplay.
- Ataque reinicia mesmo se o estado anterior também era attack. Hitbox espelha com a direção e fecha ao sair de ataque. Na morte, colisões são desativadas de forma diferida, dano cessa imediatamente e a barra de vida some.
- O inimigo-base permanece com seu placeholder visível; apenas as três cenas com sprites ocultam corpo/olhos provisórios.
- Korrag: dead → defeated no início do fade; mesma duração total e um único evento de vitória.

## Verificação

`dotnet build Joguim.csproj --no-restore`: zero erros e avisos.

`tools/art/EnemyIntegrationCheck.tscn`: 160 verificações dirigidas, zero falhas. Testa cenas reais com C#, chão, alvo de dano e uma hitbox que mata durante a física. Inclui patrulha/detecção naturais, voo, estados/quadros, pivô/escala/flip, ataques à esquerda/direita, fechamento da janela, nove combinações arquétipo/área e morte. As checagens de ataque também controlam estados e relógio para isolar o momento de impacto; não substituem uma partida completa.

`tools/art/BossDefeatCheck.tscn`: seis verificações, zero falhas. Confirma dead/defeated/fade e vitória única. Rodar como cena inicial em processo novo, sem abrir um slot. O alvo não contém jogador, logo solicitações de autosave não gravam arquivos. Áudio ainda pendente (`enemy_hit.wav`, `enemy_death.wav`, `boss_defeat.wav`).

`scenes/art/EnemyReview.tscn`: painel com cenas de produção, captura no renderer Compatibility. `enemy-integration.png` mostra direita/esquerda em 2×. `enemy-cycles.gif` é montagem de PNGs com cadência uniforme para inspeção; a velocidade real está nos recursos/Godot.

Não cobertos: percurso completo, equilíbrio de dificuldade, FPS nas quatro áreas, sons e save/respawn. Esses itens continuam abertos em A07.

## Reprodução dos assets

Ferramenta de criação: ImageGen integrada (sem CLI/API). Fontes v1 e v2 estão em `assets/sprites/enemies/sources/`; `.gdignore` evita importar folhas de produção como assets do jogo. Quadros v1 aprovados não foram sobrescritos pelos ciclos v2.

Com Python, Pillow e NumPy, executar `python tools/art/build_enemy_cycles.py`. O script somente recorta os sprites gerados, redimensiona em nearest-neighbor com um fator comum por personagem, alinha os pés e monta recursos/prévias. Detecta os corredores transparentes para não recortar pés/antenas nas divisões aproximadas da geração. O helper `prepare_enemy_sprites.py` reproduz a passagem v1 a partir das folhas locais, sem depender de caminhos pessoais.

## Prompts utilizados

Referência nas três chamadas: `docs/art-review/enemy-archetypes.png`, usada somente para identidade e estilo; fundo transparente solicitado em todas. Taxonomia `stylized-concept`.

**Vespa:** Reflorescer production animation. Use only the hornet from the left reference column. New transparent sheet precisely four columns and three rows, twelve equal cells. Same dark brown and amber giant hornet, right-facing side view, smoky translucent teal wings. Row 1 four hovering wing beats: up/middle/down/middle. Row 2 four flight frames, forward lean and tucked legs. Row 3 anticipation, curled-abdomen windup, forward attack impact, recovery. Crisp pixel clusters, same body size/center in every cell, generous margins, readable at 40 px. True alpha; no text, grid, floor, shadows, aura, fragments or other animals.

**Formiga:** Reflorescer production animation. Use only the red fire ant from the center reference column. New transparent four-column, three-row sheet. Same red chitin, dark abdomen, ochre legs, antennae and mandibles, facing right. Row 1 idle breathing/antenna movement. Row 2 four sequential quick walk frames with alternating tripod gait. Row 3 anticipation, raised-mandible windup, snapping thrust impact, recovery. Constant scale and baseline, crisp pixel art, generous margins. True alpha; no labels, grid, scenery, shadows, glow or fragments.

**Tartaruga:** Reflorescer production animation. Use only turtle in right reference column. New transparent four-column, three-row sheet. Same red-eared slider, olive skin, wet umber domed shell/golden scutes, red patch behind eye, facing right. Row 1 breathing/head motion; row 2 four slow walk frames alternating near/far feet; row 3 anticipation, head pulled into shell, extended snapping impact, recovery. Constant shell scale, foot baseline, crisp pixel clusters and margins. True alpha; no labels, grid, scenery, floor, shadows, glow or other species.

## Próximas entregas

1. A01-T2: espada e bandagem com troca visual; verificar a espada ainda desenhada nos quadros atuais antes de adicionar um sprite separado.
2. A05-T1: módulos e bordas contínuas do Sopé; depois Dossel, Igarapé e Covil.
3. A01-T3/T4 e A06: wall slide/dash e UI; mecânicas existentes não devem ser reimplementadas.
4. C11/A07: áudio, percurso completo com morte/respawn e playtest externo.
