---
tipo: projeto
categoria: roadmap
status: ativo
criado: 2026-09-15
atualizado: 2026-10-26
tags: [projeto, roadmap]
projeto: "[[🗂️ Reflorescer]]"
area: arte
papel: execucao
responsavel: Higor
par: "[[ROADMAP_QA_INTEGRACAO]]"
descricao: Backlog de sprites, animação e estilização da demo e dos biomas pós-demo
epicos_total: 9
epicos_concluidos: 8
tasks_total: 33
tasks_prontas: 30
tasks_bloqueadas: 0
tasks_liberadas: 0
---
# Roadmap de Arte — Sprites, Animação e Estilização (Higor)

Backlog de entrega de arte: Épicos → Tasks → Sub-tasks, na ordem pensada pra desbloquear o Gustavo o quanto antes (Kairo + inimigos comuns primeiro, porque bloqueiam mais coisas; boss por último, porque é o pacote mais isolado e já veio com o escopo simplificado — ver EPIC-A04). Cada sub-task é um checkbox — marcar conforme for entregue e integrado.

Entrega incremental: o código já tem fallback seguro — uma animação que ainda não existe simplesmente não troca de sprite, sem quebrar nada (`PlayAnimation()` cai pro placeholder se `HasAnimation()` for falso). Dá pra entregar **uma animação de cada vez** e testar contra o [[ROADMAP_QA_INTEGRACAO]] sem esperar o pacote inteiro de um personagem.

Cada Épico aqui tem um Épico **gêmeo de mesmo ID** no [[ROADMAP_QA_INTEGRACAO]] (ex: `EPIC-A02`), com os casos de teste que confirmam a integração. Fluxo por Task: entregar as sub-tasks (frames/animações) → rodar as sub-tasks de verificação da Task equivalente no QA de integração → escrever a linha `Status:` → seguir pra próxima. Toda entrega parcial ou decisão nova (nome de animação, formato de arquivo, ajuste de timing) vira uma nota registrada ali mesmo, no formato **Tipo (DD/MM) — resumo**, logo abaixo do checklist da Task — e, se for decisão de design (não só detalhe técnico), também registrada em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (com uma linha no [[Projetos/Reflorescer/🗓️ Log de Atualizações|🗓️ Log de Atualizações]]).

Ordem recomendada: `EPIC-A00` (padrões técnicos, ler primeiro) → `EPIC-A01-T1/T2` (Kairo núcleo + espada) → `EPIC-A02-T1/T2/T3` (inimigos comuns, os 3 arquétipos, reaproveitados sem reskin nas 4 áreas) → `EPIC-A03` (Faísca) → `EPIC-A05` (cenário das 4 áreas, na ordem Sopé da Mata → Dossel Vivo → Igarapé Sufocado → Covil de Korrag) → `EPIC-A04` (boss Korrag) → `EPIC-A01-T3/T4` (wall jump/dash, conforme o Gustavo for implementando) → `EPIC-A06` (UI) → `EPIC-A04-T2` (opcional, se sobrar tempo) → *pós-demo, fora desta entrega:* `EPIC-A02-T4/T5` (reskins Deserto/Tundra).

> [!info] Decidido em 29/09/2026 — a demo virou 4 áreas dentro da Floresta Tropical
> A demo deixou de ser Floresta Tropical → Deserto → Tundra e passou a ser 4 áreas dentro da própria Floresta Tropical — Sopé da Mata → Dossel Vivo → Igarapé Sufocado → Covil de Korrag —, estilo Hollow Knight. Deserto e Tundra saem do escopo da demo (ficam pro jogo completo). Isso reorganizou o `EPIC-A05` (agora 4 áreas, não 3) e deprioriza `EPIC-A02-T4/T5` (reskins). Ver [[🌍 Mundo e Biomas#As 4 áreas da demo (dentro da Floresta Tropical)|Mundo e Biomas]] e [[Projetos/Reflorescer/🗳️ Decisões e Configuração|Decisões e Configuração]].

---

> [!success]- EPIC-A00 — Padrões Técnicos de Entrega — ✅ Concluído · 🔴 Bloqueante
> *Pronto quando*: o formato de entrega está combinado com o Gustavo, e toda arte entregue a partir daqui segue canvas consistente, pivô consistente e a convenção de flip por personagem.
>
> Ler antes de desenhar qualquer frame — não é conteúdo, é a base que evita retrabalho em todo o resto.
>
> > [!success]- Task A00-T1 — Formato e consistência
> > - [x] Formato de entrega combinado com o Gustavo: sequência de frames numerados (`kairo_idle_00.png`, `kairo_idle_01.png`...) **ou** spritesheet única fatiada no editor do Godot (`SpriteFrames` → "Add frames from Sprite Sheet").
> > - [x] Todo frame de um mesmo personagem usa o **mesmo tamanho de canvas** (senão ele "pula" de posição ao trocar de animação).
> > - [x] Ponto de pivô/ancoragem consistente entre animações (recomendado: base dos pés).
> > - [x] Nomes de animação exatos e minúsculos, batendo com a tabela de cada Épico abaixo (case-sensitive).
> >
> > - **Integração (06/10):** Canvas verificados: Kairo 180×170 e Faísca 32×32. Offset visual uniforme de Kairo ajustado para (-45,-65), alinhando os pés à colisão sem mudar física. Aprovação visual pendente. Ver `docs/art-review/README.md` e capturas.
> >
> > - **Aprovação (11/10):** formato adotado na prática = sequência de frames numerados (`kairo_faisca_idle_00.png`...). Pés alinhados à colisão em todas as animações, sem "pulo" na troca, conferido jogando a demo inteira.
> >
> > Status: ✅ concluída (11/10) — Aprovado por Gustavo em 11/10, depois da partida completa da demo.
>
> > [!success]- Task A00-T2 — Flip por personagem
> > - [x] Kairo é espelhado por **escala** (`Visual.Scale.X = ±1`), não por flip de textura — evitar elementos fortemente assimétricos que fiquem estranhos espelhados.
> > - [x] Inimigos e boss usam `FlipH` tradicional (espelha só a textura) — menos restritivo.
> >
> > - **Integração (06/10):** Faísca removida dos quadros de Kairo e integrada como companheiro independente. Flip de Kairo testado no Godot .NET. Inimigos/boss não revalidados. Ver `docs/art-review/README.md` e capturas.
> >
> > - **Aprovação (11/10):** flip dos inimigos conferido nos dois sentidos (`EnemyIntegrationCheck` do Higor e `tools/qa/CombatCheck.gd`); Kairo espelhado com a espada sem distorção.
> >
> > Status: ✅ concluída (11/10) — Aprovado por Gustavo em 11/10, depois da partida completa da demo.

> [!success]- EPIC-A01 — Kairo (protagonista) — ✅ Concluído (19/10) · 🔴 Bloqueante
> *Pronto quando*: as 7 animações núcleo substituem o placeholder, a Espada de Grama tem os 2 visuais (ativada/desativada), e wall grab/dash têm animação assim que a mecânica correspondente existir no código.
>
> Prioridade máxima — desbloqueia testar o jogo inteiro com arte real.
>
> > [!success]- Task A01-T1 — Animações núcleo
> > - [x] `idle` (parado)
> > - [x] `run` (correndo)
> > - [x] `jump` (pulando)
> > - [x] `fall` (caindo)
> > - [x] `attack` (atacando)
> > - [x] `hurt` (tomando dano)
> > - [x] `dead` (morrendo)
> > - **Nota:** ao entregar, rodar de novo a Task C01-T2 do [[ROADMAP_QA_CODIGO]] — nenhum comportamento de física deve mudar, só o visual.
> > - **Achado da integração (28/09) — Faísca embutida no sprite do Kairo:** os quadros `kairo_faisca_*` têm a Faísca desenhada junto. No jogo, ela troca de lado a cada virada do Kairo (flip por escala) e aparece duplicada, porque a Faísca também é um personagem separado (`Faisca.tscn`) que segue o Kairo. Pedido: redesenhar o Kairo sem a Faísca e entregar a Faísca em sprites próprios (A03). Fora isso, as 7 animações núcleo integraram sem regressão (ver [[ROADMAP_QA_INTEGRACAO]] A01-T1).
> >
> > - **Integração (06/10):** 11 PNGs limpos; sete nomes, contagens, FPS e loops preservados. Corrigido `run_01`. Compilação, renderização e teste dirigido dos estados núcleo passaram. A regressão completa C01-T2 fica registrada no roadmap de QA.
> >
> > Status: ✅ concluída (06/10) — pacote núcleo entregue e integrado. Redesenho sem a Faísca embutida aprovado por Gustavo em 11/10 (partida completa, sem regressão de física).
>
> > [!success]- Task A01-T2 — Espada de Grama (2 visuais)
> > - [x] Espada empunhada (ativada), sprite separado no braço direito.
> > - [x] "Bandagem" no pulso (desativada), `RightHand/Bandage`.
> > - **Dependência:** não depende de código pra ser desenhado, só pra ser testado em jogo — depende de [[ROADMAP_DEV]] `EPIC-C03` (toggle da espada) estar implementado antes de validar a troca em tempo real.
> >
> > - **Aprovação (11/10):** aprovada a versão atual — lâmina longa v11 e empunhadura de corrida v12 (`docs/art-review/ground-and-grip-v1.md`, `sword-long-v11-*.png`, `sword-run-grip-v12.gif`).
> > - **Revisão (12/10):** espada ativa e guardada conferidas em todos os quadros de idle, corrida, salto, queda, dash, parede e ataque. A lâmina permanece presa ao punho, fica atrás da mão/braço e a bandagem volta ao guardar; `SwordHudCheck.tscn` passou sem falhas.
> >
> > Status: ✅ concluída (11/10) — Aprovado por Gustavo em 11/10, depois da partida completa da demo. Histórico: (08/10) a v9 mantém o cabo na mão direita em todos os quadros, aprofunda a guarda no punho durante o ataque e troca a camada da lâmina para que ela permaneça legível sem criar uma segunda mão. Ver `docs/art-review/kairo-korrag-correction-v3.md`.
>
> > [!success]- Task A01-T3 — Wall grab / wall jump
> > - [x] Animação `wall_slide` integrada como pose própria de apoio na parede.
> > - **Dependência resolvida (08/10):** [[ROADMAP_DEV]] `EPIC-C05` está concluído e o código procura o estado real de wall slide.
> >
> > - **Ajuste pedido (11/10, Gustavo):** no jogo o wall slide lê como uma queda comum ao lado da parede — o Kairo usa a pose de `fall` e as raspas do contato não aparecem na escala real (captura: `docs/art-review/qa-ingame-wallslide-2026-10-11.png`, tronco do Dossel Vivo). Pedido: deixar o apoio legível — **pose própria** de apoio (mão/pés na parede, coerente com o spritesheet do Kairo) **ou** efeito de contato bem mais visível (raspas/poeira/folhas caindo). A mecânica não muda.
> >
> > **Revisão (12/10, após retorno do Higor):** apoio inclina Kairo inteiro, inclusive a espada, para encostar a garra dianteira e o pé dobrado no tronco. Raspas e fragmentos nascem na altura desses pontos; funcionamento conferido dos dois lados da parede. Mecânica e dash preservados. Capturas e testes em `docs/art-review/wall-contact-v3.md`.
> > - **Ajuste v4 (12/10):** folhas e raspas passam a cair continuamente a partir da garra e do pé reais do sprite, acompanhando a inclinação e o espelhamento da pose. Ver `docs/art-review/wall-contact-v4.md`.
> >
> > Status: ✅ concluída (19/10) — validado jogando pelo Gustavo em 19/10.
>
> > [!success]- Task A01-T4 — Variação visual do dash
> > - [x] Animação `dash` integrada com os quadros normais de corrida e squash do impulso.
> > - **Dependência resolvida (08/10):** [[ROADMAP_DEV]] `EPIC-C04` está concluído e o código já procura `dash`.
> >
> > Status: ✅ concluída (11/10) — Aprovado por Gustavo em 11/10, depois da partida completa da demo (captura no jogo: `docs/art-review/qa-ingame-dash-2026-10-11.png`). Histórico: (08/10) `dash` preserva o Kairo normal, remove a coloração verde e acrescenta linhas de impulso e poeira. Entrada, desbloqueio, estado e animação passaram no teste de execução. Ver `docs/art-review/kairo-korrag-correction-v3.md`.

> [!success]- EPIC-A02 — Inimigos Comuns (3 Arquétipos) — ✅ Concluído, incluindo reskins pós-demo
> **Ajuste solicitado (07/10):** sprites dos três arquétipos ampliados mais 12% em relação à revisão anterior (escala 0,56, total +34,4% ante 0,4166667). Pés alinhados; colisões e stats preservados. Regressão dirigida de combate: 160 verificações passaram. Aprovação visual do novo tamanho pendente.
> *Pronto quando*: os 3 arquétipos têm o conjunto completo de animação, reaproveitados **sem reskin** nas 4 áreas da demo (mesmo bioma, Floresta Tropical). Reskins de Deserto/Tundra (A02-T4/T5) ficam pro jogo completo, fora do escopo desta demo (decidido em 29/09/2026).
>
> Prioridade máxima, junto com o Kairo. Os 3 arquétipos reaproveitam **exatamente os mesmos nomes de animação** — só muda o desenho por trás: `idle` (parado), `walk` (patrulhando/perseguindo), `detect` (detectou o Kairo), `attack` (atacando), `hurt` (tomando dano), `dead` (morrendo).
>
> > [!success]- Task A02-T1 — Voador (Floresta Tropical: Vespa-Asiática Gigante)
> > - [x] Conjunto completo (idle/walk/detect/attack/hurt/dead).
> >
> > Status: ✅ concluída (06/10) — design aprovado; idle/walk/attack com quatro quadros, detect/hurt com dois e dead com pose + fade. Voo, estados, flip e combate passaram no teste dirigido. Ver `docs/art-review/enemy-animation-delivery.md`.
>
> > [!success]- Task A02-T2 — Rápido (Floresta Tropical: Formiga-Lava-Pé)
> > - [x] Conjunto completo.
> >
> > Status: ✅ concluída (06/10) — ciclos integrados; patrulha, detecção, ataque nos dois sentidos, interrupção e morte verificados no Godot .NET. Tamanho visual ajustado sem alterar stats nem formas de colisão.
>
> > [!success]- Task A02-T3 — Robusto (Floresta Tropical: Tartaruga-de-Orelha-Vermelha)
> > - [x] Conjunto completo.
> >
> > Status: ✅ concluída (06/10) — ciclos lentos integrados; ataque respeita o cooldown por área. A regra InterruptOnHit=false foi preservada: hurt existe no recurso, mas o dano não interrompe o ataque.
>
> > [!success]- Task A02-T4 — Reskins Deserto
> > - [x] Abelha-Africanizada (Voador).
> > - [x] Rato-Preto (Rápido).
> > - [x] Burro Selvagem (Robusto).
> > - **Nota (29/09):** fora do escopo da demo — o Deserto não entra mais no percurso (ver [[🎯 Escopo da Demo]]). Fica pro jogo completo.
> > - **Entrega (15/10):** três espécies próprias em `assets/sprites/enemies/post_demo/`, com ciclos de quatro quadros (`idle`/`walk`/`attack`), estados `detect`/`hurt` e pose de morte. Cenas herdadas dos arquétipos, sem mudar lógica ou stats. Ver `docs/art-review/post-demo-reskins.md`.
> > - **Aprovação (15/10):** Higor aprovou os seis designs e animações pós-demo apresentados no chat.
> >
> > Status: ✅ concluída (15/10) — arte aprovada e integrada; teste contextual na futura área do Deserto segue no QA de integração.
>
> > [!success]- Task A02-T5 — Reskins Tundra
> > - [x] Mosquito-Ártico (Voador).
> > - [x] Lebre-Europeia (Rápido).
> > - [x] Ganso-das-Neves (Robusto).
> > - **Nota (29/09):** fora do escopo da demo — a Tundra não entra mais no percurso (ver [[🎯 Escopo da Demo]]). Fica pro jogo completo.
> > - **Entrega (15/10):** três espécies próprias com os seis estados esperados, alinhadas ao canvas 128×96 e integradas por cenas herdadas. Ver `docs/art-review/post-demo-reskins.md`.
> > - **Aprovação (15/10):** Higor aprovou os seis designs e animações pós-demo apresentados no chat.
> >
> > Status: ✅ concluída (15/10) — arte aprovada e integrada; teste contextual na futura área da Tundra segue no QA de integração.

> [!success]- EPIC-A03 — Faísca (vaga-lume companheiro) — ✅ Concluído na demo (A03-T2 adiada) · 🟠 Alta
> *Pronto quando*: `fly` e `investigate` estão entregues e a troca de animação acompanha a troca de estado sem travar num frame parado.
>
> > [!success]- Task A03-T1 — Animações
> > - [x] `idle` (parada/flutuando) — já existe.
> > - [x] `fly` (voando, seguindo o Kairo).
> > - [x] `investigate` (investigando).
> >
> > - **Integração (06/10):** 12 frames entregues, quatro por `idle`/`fly`/`investigate`; luz independente. Os três estados e o avanço de frames foram verificados no Godot .NET.
> >
> > Status: ✅ concluída (06/10) — animações entregues e integradas à máquina de estados.
>
> > [!todo]- Task A03-T2 — Faísca fortalecida pela restauração
> > - [ ] Visual da Faísca em cada nível de força (1, 2 e 3 áreas restauradas), conforme o que for definido ([[ROADMAP_DEV]] C14-T4).
> > - **Decisão do grupo (19/10):** fica fora da demo e vai pro jogo completo — a demo fecha com a cura e o +10 de vida máxima por área. Ver [[🗺️ Roadmap]] (pós-demo).
> >
> > Status: ⏭️ adiado (19/10) — fora da demo, vai pro jogo completo.

> [!success]- EPIC-A04 — Boss KORRAG — ✅ Concluído (19/10) · 🟠 Alta
> *Pronto quando*: `intro`, `charge_windup`, `phase2_transition` e `defeated` estão entregues — escopo já simplificado, fases 1 e 2 compartilham `idle`.
>
> Por último — pacote mais isolado e com o escopo já reduzido de propósito (decisão do Gustavo: manter as 2 fases já modeladas por dados, mas simplificar a arte pra caber no tempo).
>
> > [!success]- Task A04-T1 — Obrigatórios pra demo
> > - [x] `idle` (parado, fase 1 e 2 compartilhado) — já existe.
> > - [x] `attack` — já existe.
> > - [x] `hurt` — já existe.
> > - [x] `dead` — já existe.
> > - [x] `intro` (entrada da luta).
> > - [x] `charge_windup` (preparando investida).
> > - [x] `phase2_transition` (transição pra fase 2).
> > - [x] `defeated` (derrotado, pós-morte).
> > - **Nota de timing:** `charge_windup` precisa caber (ou ser cortável) em ~1s fixo — combinar com o Gustavo se a animação final não bater com esse corte (ver [[ROADMAP_DEV]] `C06-T4`).
> > - **Nota:** hoje o boss vira invisível e é destruído 0.3s depois de `Dead` — se `defeated` for mais longa que isso, o Gustavo ajusta esse tempo no código.
> > - **Correção (08/10):** o spritesheet anterior tinha contaminação de outros personagens, recortes incompletos e estados de um quadro. A v2 entrega 13 estados limpos; `charge` e `stomp` ganharam animações e chamadas próprias, transições e morte receberam sequências, escala aumentada para 0,9 e pés alinhados ao chão.
> >
> > Status: ✅ concluída (11/10) — Aprovado por Gustavo em 11/10, depois da partida completa da demo e da luta com o Korrag (folha dos 13 estados: `docs/art-review/korrag-animation-v3.png`).
>
> > [!success]- Task A04-T2 — Opcional (só se sobrar tempo)
> > - [x] `enraged_transition`.
> > - **Decisão do Gustavo (confirmada):** as 2 fases do Korrag ficam mantidas (o balanceamento por dados já está pronto), mas a arte de transição fica como "se sobrar tempo" em vez de obrigatória pra demo — só `idle` muda visualmente entre fase 1 e 2 no escopo mínimo.
> >
> > - **Integração (06/10):** o recurso contém `enraged_transition` e `BossEnragedState` a toca ao entrar no estado.
> >
> > Status: ✅ concluída (11/10) — Aprovado por Gustavo em 11/10, depois da partida completa da demo. Transição com dois quadros e brilho vermelho controlado.
>
> > [!success]- Task A04-T3 — `attack` do Korrag virando de lado no meio
> > - [x] Os quadros 1 e 2 do `attack` (regiões `Atlas_20` e `Atlas_21`) são espelhados na exibição; o 3º e as demais animações mantêm a orientação original. Conferido para ataque à esquerda e à direita.
> > - **Achado (11/10):** ao corrigir o Korrag que investia de costas ([[ROADMAP_DEV]] C06-T5), o código passou a assumir arte virada pra esquerda (`SpriteFacesLeft`). Com isso, o golpe comum mostra a cabeça pro lado errado nos dois primeiros quadros.
> > - **Entrega (12/10):** `KorragAttackOrientation` corrige os dois quadros no `AnimatedSprite2D` da cena real, sem alterar os outros estados. Seis capturas e verificações de direção: `docs/art-review/roadmap-visual-2026-10-12.md`.
> >
> > Status: ✅ concluída (19/10) — validado jogando pelo Gustavo em 19/10.

> [!success]- EPIC-A05 — Cenário das 4 Áreas da Demo (Floresta Tropical) — ✅ Concluído na demo (A05-T6 adiada) · 🔴 Bloqueante
> *Pronto quando*: as 4 áreas têm tileset, background e props temáticos entregues, com identidade visual reconhecível sem precisar de texto, e coerentes entre si (mesmo bioma, mesma paleta-base) — ver [[🌍 Mundo e Biomas#As 4 áreas da demo (dentro da Floresta Tropical)|Mundo e Biomas]].
>
> Renomeado em 29/09/2026 (era "Cenário das 3 Áreas da Demo": Floresta Tropical/Deserto/Tundra) — a demo agora fica só na Floresta Tropical, em 4 áreas.
>
> > [!success]- Task A05-T1 — Sopé da Mata (entrada, prioridade)
> > - [x] Sistema modular de chão/plataformas — vegetação densa mas saudável.
> > - [x] Background/parallax.
> > - [x] Props temáticos ODS15 (alinhados com [[ROADMAP_DEV]] `EPIC-C02`) — degradação **leve** aqui, é a área de entrada.
> > - [x] Visual do ponto de restauração.
> >
> > - **Integração (06/10):** Protótipo integrado ao Sopé da Mata: chão grama/terra/raízes/pedra, parallax, partículas e restauração seco→verde. Interação E e crescimento passaram no Godot .NET. Tileset modular, percurso completo e medição de FPS pendentes. Ver `docs/art-review/README.md` e capturas.
> >
> > Status: ✅ entrega visual concluída (08/10) — `TropicalBackdrop.tscn` fornece fundo com Parallax2D; o ponto de restauração alterna chão seco/verde e brotos na cena real. Higor aprovou as bordas laterais de 24 px (`docs/art-review/sope-ground-borders-v1.md`), a variação dos módulos centrais (`docs/art-review/sope-ground-modular-v1.md`) e os props de samambaia, arbusto e toco (`docs/art-review/sope-props-v1.md`). As colisões permanecem intactas. O sistema modular comum às áreas está documentado em `docs/art-review/modular-terrain-v1.md`. Percurso jogado e FPS permanecem no roadmap de QA.
>
> > [!success]- Task A05-T2 — Dossel Vivo (copa das árvores)
> > - [x] Plataformas modulares com leitura clara de platforming vertical (silhuetas de galhos/plataformas soltas).
> > - [x] Background/parallax com sensação de altura.
> > - [x] Props: degradação **moderada** no ponto de restauração.
> >
> > Status: ✅ entrega visual concluída (08/10) — Higor aprovou galhos, troncos, fundo de copa e restauração moderada (`docs/art-review/dossel-branches-v1.md`, `docs/art-review/dossel-canopy-restoration-v1.md`). A base modular de terreno é compartilhada com o Sopé (`docs/art-review/modular-terrain-v1.md`) e acompanha as colisões existentes. A travessia jogada da subida permanece no roadmap de QA.
>
> > [!success]- Task A05-T3 — Igarapé Sufocado (mata alagada)
> > - [x] Faixa modular de mata baixa alagada/degradada.
> > - [x] Visual do hazard de lama ([[ROADMAP_DEV]] `EPIC-C13-T3`): poças de lodo integradas aos três volumes de lentidão.
> > - [x] Props: o contraste "antes/depois" **mais forte** das 3 áreas no ponto de restauração — três tocos e lixo → duas mudas, água limpa e vitória-régia.
> >
> > Status: ✅ entrega visual concluída (08/10) — Higor aprovou fundo, margem, troncos, lama e restauração (`docs/art-review/igarape-visual-v1.md`). A margem agora usa módulos de terreno sem esticar a ilustração (`docs/art-review/modular-terrain-v1.md`). A travessia jogada e a leitura da lentidão permanecem no roadmap de QA.
>
> > [!success]- Task A05-T4 — Covil de Korrag (arena do boss)
> > - [x] Chão revirado pelas escavações do javali — sem ponto de restauração aqui.
> > - [x] Ambientação de arena de boss (mais fechada, foco na luta).
> >
> > Status: ✅ entrega visual concluída (08/10) — Higor aprovou fundo de arena e chão revirado (`docs/art-review/covil-visual-v1.md`). O chão revirado passou a usar a mesma base modular das áreas (`docs/art-review/modular-terrain-v1.md`). Teste jogado da luta e FPS permanecem no roadmap de QA.
>
> > [!success]- Task A05-T5 — Marco de checkpoint (único no Dossel Vivo)
> > - [x] Visual em 2 estados: **apagado** (ainda não tocado) e **ativado** (depois de tocar), sem os `ColorRect` azuis de placeholder.
> > - [x] Totem de raiz e pedra com broto aceso, pulso de luz e faíscas breves ao salvar.
> > - [x] Base na linha do chão, conferida na escala real (a área de colisão é 32×32).
> > - **Origem (11/10):** achado na revisão de arte do Gustavo — o placeholder aparece no Sopé, Dossel, Igarapé (2) e Covil, 5 checkpoints no total, e não estava em nenhum roadmap. Captura: `docs/art-review/qa-ingame-dash-2026-10-11.png` (bandeira azul à esquerda).
> > - **Código:** o `Checkpoint.cs` já tem a propriedade `Activated`; a troca de visual entre os 2 estados entra junto com a arte ([[ROADMAP_DEV]]).
> >
> > - **Mudança de escopo (11/10) — agora é uma bandeira só:** o grupo decidiu deixar só a do começo do Dossel Vivo (`Interactables/Checkpoint1`, `cp_dossel_01`); as do Sopé, Igarapé e Covil saíram do jogo ([[ROADMAP_DEV]] C09-T4). Ela agora ativa sozinha quando o Kairo passa, então o estado **ativado** precisa ficar bem visível na hora.
> > - **Entrega (12/10):** a cena do Dossel usa `CheckpointTotemVisual`, e `Checkpoint` sincroniza a aparência com `Activated` na entrada, ativação e desativação. Capturas no cenário real: `docs/art-review/roadmap-visual-2026-10-12.md`.
> >
> > Status: ✅ concluída (19/10) — validado jogando pelo Gustavo em 19/10.
>
> > [!todo]- Task A05-T6 — Vinhas/plataformas que crescem ao restaurar
> > - [ ] Arte do caminho novo que aparece em cada área restaurada (Sopé, Dossel, Igarapé), conforme o que for definido ([[ROADMAP_DEV]] C14-T5): estado "fechado" (seco/ausente) e "aberto" (crescido), na escala das plataformas atuais.
> > - **Decisão do grupo (19/10):** fica fora da demo e vai pro jogo completo — a demo fecha com a cura e o +10 de vida máxima por área. Ver [[🗺️ Roadmap]] (pós-demo).
> >
> > Status: ⏭️ adiado (19/10) — fora da demo, vai pro jogo completo.

> [!success]- EPIC-A06 — UI — ✅ Concluído (19/10) · 🟡 Média
> *Pronto quando*: barra de vida, ícone de habilidade e prompt de interação têm arte final substituindo os placeholders do Godot.
>
> Apoio, menor prioridade — não bloqueia jogabilidade.
>
> > [!success]- Task A06-T1 — Barra de vida
> > - [x] Visual final: moldura vegetal escura, preenchimento verde e marcas de leitura.
> >
> > Status: ✅ concluída (08/10) — Higor aprovou a revisão da HUD.
>
> > [!success]- Task A06-T4 — Indicador compacto da Espada de Grama (pedido de Higor)
> > - [x] Indicador de 156×32 abaixo da vida, com espada/ATIVA e bandagem/GUARDADA.
> > - [x] Tecla Q visível; texto e desenho distinguem os estados, além da cor.
> > - [x] Realce de borda por 0,28 s na troca, sem animação contínua.
> > - [x] Integrado ao toggle real; área de avisos de habilidade preservada.
> > - [x] Aprovação visual de Higor sobre as capturas no Sopé (07/10).
> >
> > Status: ✅ concluída (07/10) — indicador aprovado por Higor e integrado. Ver `docs/art-review/sword-hud-delivery.md`. A bandagem final da A01-T2 foi aprovada em 08/10; segue acabamento modular do Sopé (A05-T1).
>
> > [!success]- Task A06-T5 — Tela de título animada (pedido de Higor, 09/10)
> > - [x] Cenário exclusivo, tipografia e menu com a identidade de Reflorescer.
> > - [x] Névoa, vaga-lumes, água, movimento suave de cenário e fades.
> > - [x] Integração com Novo jogo, Continuar e Sair; espaço para futuras opções.
> > - [x] Revisão visual em 1280×720 e 960×540; navegação testada com saves isolados.
> > - [x] Aprovação visual (Gustavo, 11/10).
> > - [x] Ajuste de contraste entregue (12/10): dica do canto inferior direito com fonte maior e faixa escura.
> > - [x] Revisão pedida por Higor (12/10): W / S e Enter destacados em molduras de tecla, com "Navegar" e "Selecionar" em cor distinta. W/S agora movem o foco no menu; revisado em duas resoluções.
> >
> > Status: ✅ concluída (19/10) — validado jogando pelo Gustavo em 19/10.
>
> > [!success]- Task A06-T6 — Tela de fim da demo (11/10)
> > - [x] Acabamento visual da tela de encerramento depois do Korrag ("Áreas restauradas: X/3", mensagem e 3 botões). A tela usa fonte Cormorant, floresta animada e gradação de seco a verde conforme 0–3 áreas restauradas, mantendo as ações existentes.
> > - [x] Capturas em 1280×720 e 960×540, com as quatro contagens e foco de teclado conferidos em execução (`docs/art-review/roadmap-visual-2026-10-12.md`).
> >
> > Status: ✅ concluída (19/10) — validado jogando pelo Gustavo em 19/10.

> [!warning]- EPIC-A08 — Deserto pós-demo — 🔄 1ª área jogável no ar (19/10); áreas e cobra em produção · 🟡 Pós-demo
> *Pronto quando*: o caminho começa com sol, passa gradualmente pela tarde e termina numa arena noturna com uma cobra grande, com a arte aprovada no jogo.
>
> **Direção aprovada pelo Higor (15/10):** fazer o Deserto antes da Tundra; mostrar o sol inicialmente, avançar para tarde e terminar à noite; o chefe final deve ser uma cobra grande do deserto. Também foram aprovados bote/mordida frontal, varrida de cauda e mergulho/emergência na areia. Ver `docs/art-review/desert-direction-v1.md`.
>
> > [!success]- Task A08-T1 — Céu e passagem do tempo
> > - [x] Fundos diurno, vespertino e noturno com marcos visuais coerentes.
> > - [x] Transição gradual conforme Kairo avança, com iluminação correspondente no primeiro plano.
> > - **Entrega (15/10):** `DesertSkyCycle` e `DesertLighting` na cena isolada `DesertReview.tscn`; três capturas reais em `docs/art-review/desert-ingame-*-v1.png`.
> >
> > Status: ✅ aprovado (15/10) — funcionamento verificado e direção visual aprovada pelo Higor; a integração ao mapa jogável cabe à C15-T1.
>
> > [!success]- Task A08-T2 — Terreno e habitantes do Deserto
> > - [x] Faixa de areia e arenito com borda caminhável alinhada à colisão.
> > - [x] Abelha-Africanizada, Rato-Preto e Burro Selvagem apresentados no percurso de revisão.
> > - [x] Vento ambiente autoral integrado à prévia.
> > - **Entrega (15/10):** arte e colisão em cena isolada, sem tocar no percurso da demo. O desenho completo de fases, saídas e pontos de restauração ainda não foi definido.
> >
> > Status: ✅ arte aprovada (15/10) — terreno, habitantes e vento aprovados; desenho e integração do mapa jogável completo ficam na C15-T1.
>
> > [!warning]- Task A08-T3 — Cobra chefe do Deserto
> > - [x] Silhueta grande e seis estados visuais (`idle`, `slither`, `windup`, `attack`, `hurt`, `dead`).
> > - [x] Cena visual na arena noturna, com sibilo no início do bote.
> > - [x] Criar poses e efeitos distintos para a varrida de cauda e para entrar/sair da areia (`tail_sweep`, `burrow_enter`, `burrow_exit`), com preparação legível e ponto de contato estável; o `windup`/`attack` atual serve como base do bote frontal.
> > - [ ] Revisar no Godot o timing dos novos estados com o protótipo de combate do Gustavo, sem deslocamento de canvas, cabeça, corpo ou cauda.
> > - **Entrega (15/10):** `Boss_CobraDesertoVisual.tscn` é uma apresentação animada sem IA de combate; não reutiliza indevidamente os golpes do Korrag.
> >
> > **Entrega (21/10):** nove quadros novos em canvas 512×320, integrados ao `SandViperSpriteFrames.tres`; a prévia percorre bote, cauda, entrada e saída da areia. Folha e GIF atualizados em `docs/art-review/desert-snake-boss-v1.*`.
> >
> > Status: 🟡 parcial (21/10) — os novos ciclos foram aprovados pelo Higor em 21/10; falta revisar o encaixe com hitboxes e timing quando o protótipo C15-T2 existir.

> > [!success]- Task A08-T4 — Arte das áreas do Deserto
> > - [x] Plataformas, rochas e ruínas pra verticalidade (cânions), no padrão da faixa de chão atual.
> > - [x] Visual do hazard (areia movediça) e do ponto de restauração seco → vivo.
> > - [x] Fundos ou variações por área conforme a proposta visual da C15-T4: Portal do Sol, Garganta das Ruínas, Oásis Soterrado e Ninho da Serpente.
> >
> > **Entrega (21/10):** oito módulos transparentes de arenito em `assets/environment/desert/modules/`; areia movediça com ciclo `bubble` de quatro quadros e restauração em quatro estágios em `assets/environment/desert/hazards/`. Folha e GIF em `docs/art-review/desert-area-kit-v1.*`; canvases e recursos conferidos pelo `DesertReviewCheck.gd`.
> >
> > **Fundos (21/10):** quatro plates 1672×941 em `assets/environment/desert/areas/`, com composições distintas para entrada diurna, cânion vertical ao entardecer, oásis ramificado no crepúsculo e arena noturna. Proposta de conexões e folha de contraste em `docs/art-review/desert-areas-v1.md` e `desert-area-backgrounds-v1.png`.
> >
> > **Aprovação (21/10):** Higor aprovou os quatro fundos, os nomes Portal do Sol, Garganta das Ruínas, Oásis Soterrado e Ninho da Serpente, além do loop de conexões proposto.
> >
> > Status: ✅ concluída (21/10) — pacote visual aprovado; a montagem jogável continua na C15-T4.
>
> > [!success]- Task A08-T5 — Marco de checkpoint e portais do Deserto
> > - [x] Marco de checkpoint próprio do Deserto, com estado apagado/aceso.
> > - [x] Visual próprio da passagem Covil ↔ Deserto, substituindo os retângulos coloridos por arcos de arenito animados e uma variante com raízes no lado da floresta.
> >
> > **Entrega (21/10):** sprites transparentes em `assets/environment/desert/desert_checkpoint_*` e `desert_portal_*`; brilho, grãos de areia e estado bloqueado continuam animados em código. Capturas em `docs/art-review/desert-entry-checkpoint-*-v2.png`.
> >
> > Status: ✅ aprovado pelo Higor (21/10).
>
> > [!success]- Task A08-T6 — Luz noturna não escurece chão e personagens
> > - [x] À noite céu, chão, inimigos, cobra e Kairo recebem a mesma luz azul do `DesertLighting` na cena jogável.
> > - **Medição (26/10):** a luz é aplicada no primeiro plano (brilho do chão cai de 0,71 de dia pra 0,31 à noite; Kairo e inimigos também escurecem). Ao lado do céu azul a areia ainda lê dourada; pra uma noite mais marcada, escurecer `NIGHT_LIGHT` no `DesertLighting.gd`.
> >
> > Status: ✅ corrigida (21/10) — validada no renderer em `docs/art-review/desert-area-night-v2.png` e pelo `DesertReviewCheck.gd`.

> [!success]- EPIC-A09 — Cartografia da Floresta Tropical — ✅ Concluído (19/10) · 🟡 Alta
> *Pronto quando*: o mapa aberto no jogo comunica as passagens entre as quatro áreas e oculta cada trecho não explorado, com leitura clara em 1280×720.
>
> > [!success]- Task A09-T1 — Visual do mapa e névoa de exploração
> > - [x] Tela escura com moldura orgânica, cores próprias por área, nomes apenas das regiões visitadas e marcador de Kairo.
> > - [x] Contornos de salas e passagens revelados célula a célula; nenhuma silhueta de área futura aparece antes da visita.
> > - [x] Capturas reais do Godot em `docs/art-review/world-map-fog-v1.png` e `world-map-complete-v1.png` (esta última força a revelação somente para revisão).
> > - [x] Higor avaliar legibilidade, escala, ornamentos e identidade visual no jogo.
> > - **Ajuste (15/10):** cantos circulares da primeira versão substituídos por ramos e folhas espelhados, orientados para dentro e sem invadir os textos inferiores.
> >
> > Status: ✅ concluída (19/10) — validado jogando pelo Gustavo em 19/10.

## O que falta (12/10/2026)

As 9 etapas da ordem de entrega original (Kairo, inimigos, Faísca, espada, cenário, Korrag, wall jump/dash, UI e `enraged_transition`) foram entregues e aprovadas, com os seguintes ajustes e entregas ainda em acompanhamento:

```
1. Wall slide legível (A01-T3)            ← efeito entregue em 12/10; aguarda aprovação visual
2. Marco de checkpoint (A05-T5)           ← entregue em 12/10; falta avaliação visual e teste jogado
3. Contraste da dica da tela de título (A06-T5) ← entregue em 12/10
4. Korrag: orientar quadros 1 e 2 do `attack` (A04-T3) ← entregue em 12/10; falta avaliação visual
5. Tela de fim da demo (A06-T6)           ← entregue em 12/10; falta avaliação visual e percurso jogado
6. Dependem de decisões do grupo: Faísca fortalecida (A03-T2) e caminhos que crescem (A05-T6)
+ Validações do Higor no ROADMAP_QA_INTEGRACAO: playtest do indicador da espada (A06-T4) e do menu pelo teclado (A06-T5)

Pós-demo (jogo completo): reskins Deserto/Tundra dos 3 arquétipos (A02-T4/T5) entregues e aprovados em 15/10; teste nas futuras áreas pendente no QA de integração.
```
