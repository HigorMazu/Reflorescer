# Reflorescer — entrega visual de 05/10/2026

Proposta local para aprovação. Sem commit ou push. Arte criada com ImageGen integrado; recorte, escala e montagem de prévias com Pillow. As referências do usuário orientam atmosfera, camadas naturais e leitura; não foram copiadas para o jogo.

## Direção

“A vida nasce da terra”: musgo verde-oliva, folhagem jade, húmus escuro, raízes ocre e pedra fria. Fundo com contraste reduzido, névoa e luz atravessando a copa. O âmbar da Faísca é um ponto de atenção pequeno. A primeira entrega de terreno é um conjunto de faixas/protótipos, **não um tileset modular completo**.

## Auditoria e alterações

- Revisados os quatro roadmaps, configuração, cenas, recursos, sistemas de personagem/companheiro, combate, mundo/restauração, progressão, UI e save; addon MCP identificado como infraestrutura existente.
- Kairo tinha Faísca embutida nos 11 quadros, duplicada pelo companheiro separado. A folha de referência também apresentava cortes e resíduos. Os quadros foram editados mantendo identidade, poses correspondentes e contrato das sete animações. A geração não preserva todos os pixels originais: essa revisão artística precisa de aprovação.
- Corrigida também a cabeça cortada em `run_01`. Não foram adicionadas animações opcionais.
- Mantidos nomes de arquivos antigos `kairo_faisca_*` por compatibilidade. **Esses PNGs agora contêm somente Kairo.** Os nomes não significam que a Faísca continua embutida.
- `KairoFaiscaSpriteFrames.tres` e `Player.tscn` permanecem idênticos à base: 180×170, posição (-45,-85), escala 0,5, centered=false. Mesmo flip por `Visual.Scale.X`, FPS, loops, duração e contagem de quadros.
- Faísca ganhou 12 PNGs 32×32: quatro quadros por `idle` (5 fps), `fly` (12 fps), `investigate` (6 fps). Canvas no jogo: 24×24; silhueta menor que esse canvas. Foram removidos os quatro ColorRects provisórios do companheiro.
- Escala X negativa só no sprite da Faísca compensa a convenção atual de `UpdateFacing`; luz é um nó irmão e não é espelhada. Seguir, distância, altura e máquina de estados permanecem intactos.
- Luz independente recebeu GradientTexture2D radial com energia moderada. Não havia textura atribuída ao PointLight2D.
- `TestLevel` recebeu fundo/parallax, faixas de grama/terra/raízes/pedra, movimento discreto da franja, partículas ambientais e pequenos elementos seco/verde no ponto de restauração. Reaproveita `DegradedVisuals`/`RestoredVisuals`, crescimento, fade e persistência existentes.
- Nenhum C#, stat, colisão, posição de plataforma, inimigo, boss, UI, Deserto ou Tundra foi alterado.

## Arquivos

- `assets/sprites/kairo_faisca/`: 11 PNGs atualizados e folha de referência limpa.
- `assets/sprites/faisca/`: 12 PNGs, folha e `FaiscaSpriteFrames.tres`.
- `assets/environment/tropical/`: fundo, duas faixas de chão, dois estudos de plataforma e dois props de restauração.
- `assets/shaders/foliage_sway.gdshader`, `scripts/art/ForestAmbience.gd`: movimento exclusivamente visual.
- `scenes/art/TropicalBackdrop.tscn`: fundo usado na fase.
- `scenes/art/ArtReview.tscn`, `scripts/art/ArtReview.gd`: painel de revisão isolado, sem save nem física.
- Nesta pasta: captura do Godot, GIFs e registro de geração. A captura é do painel de arte, não de um playtest.
- `tools/art/`: recorte técnico, montagem de GIFs e verificação do contrato.

## Verificação realizada

- `validate_delivery.py`: nomes exatos, 11/12 frames, canvas, alfa, caminhos de recursos, contrato de animação de Kairo, ausência de placeholders da Faísca, igualdade de colisões/triggers e ausência de alterações C#/stats.
- Importação e renderização reais no Godot 4.7.2, OpenGL Compatibility, em cópia isolada sem autoloads C#. `ArtReview` renderizou sem erros; `TropicalBackdrop` carregou e executou sem erros.
- Inspeção visual dos frames, transparência e tamanhos; correção adicional de `run_01` após a primeira renderização.

**Limite:** este checkout tem apenas `Joguim.csproj.old`, e o Godot disponível é o executável sem .NET. Há SDK .NET instalado fora do PATH, mas isso não adiciona suporte C# ao Godot padrão. Portanto não declaro aprovado C01-T2, transições reais da Faísca, save/restauração em gameplay ou FPS da demo. Não alterei a configuração do projeto para contornar isso.

## Como testar

1. No ambiente Godot **.NET** já usado pelo projeto, compilar o C# e abrir `scenes/levels/TestLevel.tscn` com F6. A entrada normal por `Main.tscn` usa a mesma fase.
2. A/D: verificar corrida/flip e **uma única Faísca**. Espaço: jump/fall. K: attack. Tomar dano e morrer: hurt/dead. Confirmar pivô visual e ausência de saltos na troca de frames, inclusive o novo `run_01`.
3. Ir ao ponto de restauração perto do início (x=120) e pressionar E. Conferir seco→verde, crescimento, fade e persistência ao retornar. Não usar um save já restaurado para avaliar o antes/depois inicial.
4. Testar `idle`, `fly` e `investigate` diretamente no AnimatedSprite2D da Faísca ou no painel `ArtReview`. O fluxo atual entra em Follow/fly e não possui gatilho automático de investigação: a arte está pronta, o comportamento não foi ampliado nesta etapa.
5. Repetir C01-T2 e A03-T1 no ambiente .NET, e medir a fase completa no renderer Compatibility antes de marcar QA aprovado.

Prévia local sem C#: `.godot/art_validation/project.godot` (cópia de validação ignorada pelo Git). Os GIFs funcionam sem Godot. Não confundir a escala de apresentação das plataformas do painel com a geometria rasa existente no TestLevel.

## Próxima pequena etapa

Depois da aprovação e do playtest .NET: A05-T1, preparar bordas esquerda/direita e um módulo central realmente contínuo para o terreno, substituindo as faixas provisórias sem mudar colisões. Não expandir para novos biomas antes disso. As notas de design estão aqui porque o cofre Obsidian citado pelo roadmap não está presente na pasta-pai deste checkout; nenhuma nota externa foi inventada ou alterada.
