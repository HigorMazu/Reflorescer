# Reflorescer — revisão visual de 06/10/2026

Trabalho local na `master`, baseada em `8ccaf4f`. Designs dos três inimigos aprovados pelo Higor; ciclos e integração implementados em seguida. Commit, pull e push ficam por conta do usuário. Arte criada com ImageGen; recorte, escala e montagem com Pillow. Referências usadas para direção, sem copiar seus personagens ou cenários.

Entrega atual: revisão final das correções de Kairo e Korrag em `kairo-korrag-correction-v3.md`. Dash/apoio usam o desenho normal de Kairo com efeitos discretos ligados aos estados reais, a espada usa o punho original sem duplicação e o windup de Korrag não contém mais a perna separada. Aprovação visual e luta jogada permanecem pendentes.

## Direção e integração

“A vida nasce da terra”: musgo, húmus, raízes e pedra; floresta úmida em profundidade e luz âmbar da Faísca. O terreno é um protótipo de faixas, não um tileset modular completo. A repetição ainda aparece em trechos longos.

- Revisados configuração, quatro roadmaps, cenas, recursos e sistemas de personagem, companheiro, combate, mundo, progressão, UI e save. A demo atual tem quatro áreas tropicais; Novo Jogo entra pelo Sopé da Mata.
- Kairo: removida a Faísca embutida dos 11 quadros e corrigida a cabeça de `run_01`. Mantidos nomes legados `kairo_faisca_*`, canvas 180×170 e contrato das sete animações. `KairoFaiscaSpriteFrames.tres` permanece idêntico à base atual. A geração redesenhou detalhes: não preserva cada pixel original e precisa de aprovação artística.
- Sprite de Kairo deslocado uniformemente 20 pixels para baixo, de (-45,-85) para (-45,-65), alinhando os pés à colisão existente. Escala 0,5 e ancoragem consistente entre frames; física e hitboxes intactas.
- Faísca independente: 12 quadros 32×32, quatro por `idle` (5 fps), `fly` (12 fps), `investigate` (6 fps). Canvas 24×24 no jogo. Substituídos os quatro ColorRects provisórios; luz radial independente. Escala X negativa do sprite acompanha a convenção do controlador existente.
- `SopeDaMata.tscn`: fundo/parallax, partículas discretas, franja de grama animada e terreno grama → terra → raízes → pedra. O ponto x=1100 usa as listas existentes de visuais degradados/restaurados, fade e crescimento.
- Inimigos comuns: ciclos de quatro quadros para idle/walk/attack, dois para detect/hurt e pose de morte + fade. Canvas 128×96, até 40 px de largura no jogo. Espelhamento centralizado, ataque sincronizado ao cooldown por área e hitbox acompanhando a direção. O inimigo-base conserva o fallback visível no TestLevel.
- Stats, formas de colisão e plataformas preservados. Corrigidos encerramento da hitbox ao sair de ataque e desativação segura das colisões durante a morte. O PlayerController não foi alterado.

## Arquivos e prévias

- `assets/sprites/kairo_faisca/`: 11 frames e folha limpa; `assets/sprites/faisca/`: 12 frames e SpriteFrames próprios.
- `assets/environment/tropical/`: fundo, chão seco/verde, estudos de plataforma e props.
- `assets/shaders/foliage_sway.gdshader`, `scripts/art/ForestAmbience.gd`, `scenes/art/TropicalBackdrop.tscn`: movimento ambiental visual.
- `scenes/art/ArtReview.tscn`: painel isolado. `art-review.png` é captura desse painel, não gameplay.
- `sope-gameplay.png` e `sope-restored.png`: capturas reais da fase com C# ativo. Inimigos e HUD ainda provisórios.
- `kairo-animations.gif` e `faisca-animations.gif`: montagem dos PNGs do jogo; Faísca ampliada para inspeção.
- `enemy-archetypes.png`: estudo original de seis poses por inimigo aprovado pelo usuário. `enemy-cycles.gif` mostra o pacote animado posterior; é uma montagem de quadros, não uma captura de gameplay.
- `enemy-integration.png`: captura de `scenes/art/EnemyReview.tscn`, com instâncias das cenas reais, nos dois sentidos e ampliadas 2×.
- `tools/art/`: recorte, montagem, verificação estática e teste de integração. `generation-prompts.md` registra a geração.

## Validação

Compilação em cópia isolada com Godot .NET 4.7.2 e SDK .NET 10: **zero erros e zero avisos**. Importação e painel renderizados no Godot. Teste real no renderer Compatibility: **zero falhas**, incluindo movimento/run, flip, salto, ataque, pés alinhados, companheiro independente, avanço dos frames nos três estados da Faísca, interação E e troca seco→verde com crescimento. Registro em `validation.txt`.

Em 06/10, a `TestLevel` também foi executada por 180 quadros com os três novos inimigos carregados. Não houve erro de cenas, recursos ou C#. O console mantém avisos preexistentes de UIDs antigos nos PNGs do Kairo; o Godot aplica fallback pelo caminho correto e os avisos não foram introduzidos pelos inimigos.

Na primeira etapa, a verificação estática confirmou canvas/alfa e preservação de física do Kairo. Na etapa atual, 160 verificações dirigidas dos inimigos e seis da morte do Korrag passaram no Godot .NET. Logs: `enemy-validation.txt` e `boss-defeat-validation.txt`. Os testes de integração exercitam código C# real; não equivalem ao percurso completo da demo.

Limites: investigação automática da Faísca, C01-T2 completo, recarga do save, percurso das quatro áreas e benchmark de FPS continuam pendentes. Sons ausentes: `player_jump.wav`, `player_attack.wav`, `restoration.wav`, `enemy_hit.wav`, `enemy_death.wav` e `boss_defeat.wav`.

## Como conferir

1. No Godot .NET, compilar e abrir `scenes/areas/SopeDaMata.tscn` com F6. Novo Jogo também aponta para essa área.
2. A/D para andar/virar, Espaço para pular, K para atacar. Conferir uma única Faísca e pés no chão.
3. No ponto de restauração x=1100, pressionar E e observar terreno e broto. Usar save ainda não restaurado.
4. Abrir `scenes/art/ArtReview.tscn` com F6 para inspecionar animações; os GIFs dispensam Godot.
5. Abrir `scenes/art/EnemyReview.tscn` com F6 para inspecionar os ciclos novos nos dois sentidos.

Este checkout possui `Joguim.csproj` e configuração C# ativa. Use Godot .NET 4.7.2 com SDK .NET 10. Os testes atuais são cenas independentes; não iniciam Novo Jogo nem carregam slots. A fixture do boss não contém jogador, portanto eventuais pedidos de autosave não gravam arquivos.

## Próxima pequena etapa

A espada e a bandagem de A01-T2, bem como as bordas, a variação central e os props do Sopé, foram aprovados em 08/10. O QA dirigido de entrada/FPS do Sopé está em `sope-area-qa-2026-10-08.md`; a travessia jogada continua pendente. Galhos, troncos, fundo e restauração do Dossel também foram aprovados (`dossel-branches-v1.md`, `dossel-canopy-restoration-v1.md`). Wall slide/dash, áudio e playtest completo permanecem no roadmap.

O Igarapé tem fundo, margem alagada modular, troncos, três poças de lama e restauração aprovados por Higor (`igarape-visual-v1.md`, `modular-terrain-v1.md`). Teste jogado da lentidão e travessia completa seguem pendentes.

O Covil de Korrag recebeu uma arena fechada de raízes e terra revirada modular, sem elementos de exploração (`covil-visual-v1.md`, `modular-terrain-v1.md`). O visual foi aprovado por Higor; luta jogada e FPS seguem pendentes.
