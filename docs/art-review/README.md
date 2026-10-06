# Reflorescer — revisão visual de 06/10/2026

Proposta local aguardando aprovação dos designs e autorização para commit/push. Retomada sobre `77ece44`, incluindo os assets incorporados pelo usuário em `f282c1b`. Arte criada com ImageGen; recorte, escala e montagem com Pillow. Referências usadas para direção, sem copiar seus personagens ou cenários.

## Direção e integração

“A vida nasce da terra”: musgo, húmus, raízes e pedra; floresta úmida em profundidade e luz âmbar da Faísca. O terreno é um protótipo de faixas, não um tileset modular completo. A repetição ainda aparece em trechos longos.

- Revisados configuração, quatro roadmaps, cenas, recursos e sistemas de personagem, companheiro, combate, mundo, progressão, UI e save. A demo atual tem quatro áreas tropicais; Novo Jogo entra pelo Sopé da Mata.
- Kairo: removida a Faísca embutida dos 11 quadros e corrigida a cabeça de `run_01`. Mantidos nomes legados `kairo_faisca_*`, canvas 180×170 e contrato das sete animações. `KairoFaiscaSpriteFrames.tres` permanece idêntico à base atual. A geração redesenhou detalhes: não preserva cada pixel original e precisa de aprovação artística.
- Sprite de Kairo deslocado uniformemente 20 pixels para baixo, de (-45,-85) para (-45,-65), alinhando os pés à colisão existente. Escala 0,5 e ancoragem consistente entre frames; física e hitboxes intactas.
- Faísca independente: 12 quadros 32×32, quatro por `idle` (5 fps), `fly` (12 fps), `investigate` (6 fps). Canvas 24×24 no jogo. Substituídos os quatro ColorRects provisórios; luz radial independente. Escala X negativa do sprite acompanha a convenção do controlador existente.
- `SopeDaMata.tscn`: fundo/parallax, partículas discretas, franja de grama animada e terreno grama → terra → raízes → pedra. O ponto x=1100 usa as listas existentes de visuais degradados/restaurados, fade e crescimento.
- Nenhum C#, stat, colisão, trigger ou posição de plataforma alterado. TestLevel, outras áreas, inimigos, boss e UI não receberam mudanças nesta retomada. A edição prévia do usuário em `project.godot` foi preservada.

## Arquivos e prévias

- `assets/sprites/kairo_faisca/`: 11 frames e folha limpa; `assets/sprites/faisca/`: 12 frames e SpriteFrames próprios.
- `assets/environment/tropical/`: fundo, chão seco/verde, estudos de plataforma e props.
- `assets/shaders/foliage_sway.gdshader`, `scripts/art/ForestAmbience.gd`, `scenes/art/TropicalBackdrop.tscn`: movimento ambiental visual.
- `scenes/art/ArtReview.tscn`: painel isolado. `art-review.png` é captura desse painel, não gameplay.
- `sope-gameplay.png` e `sope-restored.png`: capturas reais da fase com C# ativo. Inimigos e HUD ainda provisórios.
- `kairo-animations.gif` e `faisca-animations.gif`: montagem dos PNGs do jogo; Faísca ampliada para inspeção.
- `tools/art/`: recorte, montagem, verificação estática e teste de integração. `generation-prompts.md` registra a geração.

## Validação

Compilação em cópia isolada com Godot .NET 4.7.2 e SDK .NET 10: **zero erros e zero avisos**. Importação e painel renderizados no Godot. Teste real no renderer Compatibility: **zero falhas**, incluindo movimento/run, flip, salto, ataque, pés alinhados, companheiro independente, avanço dos frames nos três estados da Faísca, interação E e troca seco→verde com crescimento. Registro em `validation.txt`.

Verificação estática confirma canvas/alfa, nomes, FPS, contagens, loops, recursos e preservação de colisões/triggers/C#/stats. Inspeção das capturas confirmou o alinhamento corrigido.

Limites: estados da Faísca acionados diretamente, sem validar investigação automática; comportamento de seguir/altura não foi redesenhado. Não executados C01-T2 completo, dano/morte em gameplay, recarga do save, percurso das quatro áreas ou benchmark de FPS. Três sons já ausentes foram reportados: `player_jump.wav`, `player_attack.wav`, `restoration.wav`.

## Como conferir

1. No Godot .NET, compilar e abrir `scenes/areas/SopeDaMata.tscn` com F6. Novo Jogo também aponta para essa área.
2. A/D para andar/virar, Espaço para pular, K para atacar. Conferir uma única Faísca e pés no chão.
3. No ponto de restauração x=1100, pressionar E e observar terreno e broto. Usar save ainda não restaurado.
4. Abrir `scenes/art/ArtReview.tscn` com F6 para inspecionar animações; os GIFs dispensam Godot.

Este checkout não tem `.csproj` ativo e possui edição prévia em `project.godot`. Para validar sem mudar essa configuração, a cópia local `.godot/game_validation/project.godot` recebeu projeto C# temporário e diretório de save próprio. Ela é ignorada pelo Git e não usa saves reais.

## Próxima pequena etapa

Após aprovação: A05-T1, bordas e módulo central contínuo para o Sopé, reduzindo repetição sem alterar colisões. Dossel Vivo, Igarapé Sufocado e Covil de Korrag ficam para depois. O cofre Obsidian citado pelo projeto não foi localizado na pasta-pai; o registro disponível está nesta pasta e nos roadmaps.
