---
tipo: projeto
categoria: roadmap
status: ativo
criado: 2026-09-15
atualizado: 2026-10-07
tags: [projeto, roadmap]
projeto: "[[🗂️ Reflorescer]]"
area: arte
papel: execucao
responsavel: Higor
par: "[[ROADMAP_QA_INTEGRACAO]]"
descricao: Backlog de sprites, animação e estilização da demo
epicos_total: 7
epicos_concluidos: 3
tasks_total: 22
tasks_prontas: 8
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

> [!warning]- EPIC-A00 — Padrões Técnicos de Entrega — 🔄 Em andamento · 🔴 Bloqueante
> *Pronto quando*: o formato de entrega está combinado com o Gustavo, e toda arte entregue a partir daqui segue canvas consistente, pivô consistente e a convenção de flip por personagem.
>
> Ler antes de desenhar qualquer frame — não é conteúdo, é a base que evita retrabalho em todo o resto.
>
> > [!warning]- Task A00-T1 — Formato e consistência
> > - [ ] Formato de entrega combinado com o Gustavo: sequência de frames numerados (`kairo_idle_00.png`, `kairo_idle_01.png`...) **ou** spritesheet única fatiada no editor do Godot (`SpriteFrames` → "Add frames from Sprite Sheet").
> > - [x] Todo frame de um mesmo personagem usa o **mesmo tamanho de canvas** (senão ele "pula" de posição ao trocar de animação).
> > - [ ] Ponto de pivô/ancoragem consistente entre animações (recomendado: base dos pés).
> > - [x] Nomes de animação exatos e minúsculos, batendo com a tabela de cada Épico abaixo (case-sensitive).
> >
> > - **Integração (06/10):** Canvas verificados: Kairo 180×170 e Faísca 32×32. Offset visual uniforme de Kairo ajustado para (-45,-65), alinhando os pés à colisão sem mudar física. Aprovação visual pendente. Ver `docs/art-review/README.md` e capturas.
> >
> > Status: 🟡 parcial (06/10) — integração local verificada; aprovação e verificações restantes pendentes.
>
> > [!warning]- Task A00-T2 — Flip por personagem
> > - [x] Kairo é espelhado por **escala** (`Visual.Scale.X = ±1`), não por flip de textura — evitar elementos fortemente assimétricos que fiquem estranhos espelhados.
> > - [ ] Inimigos e boss usam `FlipH` tradicional (espelha só a textura) — menos restritivo.
> >
> > - **Integração (06/10):** Faísca removida dos quadros de Kairo e integrada como companheiro independente. Flip de Kairo testado no Godot .NET. Inimigos/boss não revalidados. Ver `docs/art-review/README.md` e capturas.
> >
> > Status: 🟡 parcial (06/10) — integração local verificada; aprovação e verificações restantes pendentes.

> [!warning]- EPIC-A01 — Kairo (protagonista) — 🔄 Em andamento · 🔴 Bloqueante
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
> > Status: ✅ concluída (06/10) — pacote núcleo entregue e integrado.
>
> > [!success]- Task A01-T2 — Espada de Grama (2 visuais)
> > - [x] Espada empunhada (ativada), sprite separado no braço direito.
> > - [x] "Bandagem" no pulso (desativada), `RightHand/Bandage`.
> > - **Dependência:** não depende de código pra ser desenhado, só pra ser testado em jogo — depende de [[ROADMAP_DEV]] `EPIC-C03` (toggle da espada) estar implementado antes de validar a troca em tempo real.
> >
> > Status: 🟡 corrigida, aguardando aprovação (08/10) — a v9 mantém o cabo na mão direita em todos os quadros, aprofunda a guarda no punho durante o ataque e troca a camada da lâmina para que ela permaneça legível sem criar uma segunda mão. Ver `docs/art-review/kairo-korrag-correction-v3.md`.
>
> > [!todo]- Task A01-T3 — Wall grab / wall jump
> > - [x] Animação `wall_slide` integrada como pose própria de apoio na parede.
> > - **Dependência resolvida (08/10):** [[ROADMAP_DEV]] `EPIC-C05` está concluído e o código procura o estado real de wall slide.
> >
> > Status: 🟡 corrigida, aguardando aprovação (08/10) — `wall_slide` usa o Kairo normal e ganhou raspas discretas no ponto de contato. Entrada, colisão, estado e animação passaram no teste de execução. Ver `docs/art-review/kairo-korrag-correction-v3.md`.
>
> > [!todo]- Task A01-T4 — Variação visual do dash
> > - [x] Animação `dash` integrada com os quadros normais de corrida e squash do impulso.
> > - **Dependência resolvida (08/10):** [[ROADMAP_DEV]] `EPIC-C04` está concluído e o código já procura `dash`.
> >
> > Status: 🟡 corrigida, aguardando aprovação (08/10) — `dash` preserva o Kairo normal, remove a coloração verde e acrescenta linhas de impulso e poeira. Entrada, desbloqueio, estado e animação passaram no teste de execução. Ver `docs/art-review/kairo-korrag-correction-v3.md`.

> [!success]- EPIC-A02 — Inimigos Comuns (3 Arquétipos) — ✅ Concluído no escopo da demo
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
> > [!todo]- Task A02-T4 — Reskins Deserto
> > - [ ] Abelha-Africanizada (Voador).
> > - [ ] Rato-Preto (Rápido).
> > - [ ] Burro Selvagem (Robusto).
> > - **Nota (29/09):** fora do escopo da demo — o Deserto não entra mais no percurso (ver [[🎯 Escopo da Demo]]). Fica pro jogo completo.
> >
> > Status: não iniciado — pós-demo, sem urgência.
>
> > [!todo]- Task A02-T5 — Reskins Tundra
> > - [ ] Mosquito-Ártico (Voador).
> > - [ ] Lebre-Europeia (Rápido).
> > - [ ] Ganso-das-Neves (Robusto).
> > - **Nota (29/09):** fora do escopo da demo — a Tundra não entra mais no percurso (ver [[🎯 Escopo da Demo]]). Fica pro jogo completo.
> >
> > Status: não iniciado — pós-demo, sem urgência.

> [!success]- EPIC-A03 — Faísca (vaga-lume companheiro) — ✅ Concluído · 🟠 Alta
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

> [!warning]- EPIC-A04 — Boss KORRAG — 🟡 Corrigido, aguardando aprovação · 🟠 Alta
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
> > Status: 🟡 corrigida, aguardando aprovação (08/10) — integração e testes dirigidos passaram; revisão em `docs/art-review/kairo-korrag-correction-v2.md`.
>
> > [!success]- Task A04-T2 — Opcional (só se sobrar tempo)
> > - [x] `enraged_transition`.
> > - **Decisão do Gustavo (confirmada):** as 2 fases do Korrag ficam mantidas (o balanceamento por dados já está pronto), mas a arte de transição fica como "se sobrar tempo" em vez de obrigatória pra demo — só `idle` muda visualmente entre fase 1 e 2 no escopo mínimo.
> >
> > - **Integração (06/10):** o recurso contém `enraged_transition` e `BossEnragedState` a toca ao entrar no estado.
> >
> > Status: 🟡 corrigida, aguardando aprovação (08/10) — transição refeita com dois quadros coerentes e brilho vermelho controlado.

> [!success]- EPIC-A05 — Cenário das 4 Áreas da Demo (Floresta Tropical) — ✅ Entrega visual concluída
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
> > [!todo]- Task A05-T4 — Covil de Korrag (arena do boss)
> > - [x] Chão revirado pelas escavações do javali — sem ponto de restauração aqui.
> > - [x] Ambientação de arena de boss (mais fechada, foco na luta).
> >
> > Status: ✅ entrega visual concluída (08/10) — Higor aprovou fundo de arena e chão revirado (`docs/art-review/covil-visual-v1.md`). O chão revirado passou a usar a mesma base modular das áreas (`docs/art-review/modular-terrain-v1.md`). Teste jogado da luta e FPS permanecem no roadmap de QA.

> [!success]- EPIC-A06 — UI — ✅ Entrega visual concluída
> *Pronto quando*: barra de vida, ícone de habilidade e prompt de interação têm arte final substituindo os placeholders do Godot.
>
> Apoio, menor prioridade — não bloqueia jogabilidade.
>
> > [!todo]- Task A06-T1 — Barra de vida
> > - [x] Visual final: moldura vegetal escura, preenchimento verde e marcas de leitura.
> >
> > Status: ✅ concluída (08/10) — Higor aprovou a revisão da HUD.
>
> > [!todo]- Task A06-T2 — Ícone de habilidade
> > - [x] Sem indicador persistente de dash, por decisão de Higor (08/10); a notificação de desbloqueio existente permanece suficiente.
> > - **Dependência resolvida:** a notificação real existe em [[ROADMAP_QA_CODIGO]] `C04-T4`.
> >
> > Status: ✅ concluída (08/10) — escopo visual aprovado sem badge de dash.
>
> > [!todo]- Task A06-T3 — Prompt de interação
> > - [x] Painel compacto de leitura para o prompt contínuo `[E]`.
> > - **Dependência resolvida:** o prompt contínuo existe em [[ROADMAP_QA_CODIGO]] `C10-T1`.
> >
> > Status: ✅ concluída (08/10) — Higor aprovou a revisão da HUD.

---

### A06-T4 — Indicador compacto da Espada de Grama (pedido de Higor)

- [x] Indicador de 156×32 abaixo da vida, com espada/ATIVA e bandagem/GUARDADA.
- [x] Tecla Q visível; texto e desenho distinguem os estados, além da cor.
- [x] Realce de borda por 0,28 s na troca, sem animação contínua.
- [x] Integrado ao toggle real; área de avisos de habilidade preservada.
- [x] Aprovação visual de Higor sobre as capturas no Sopé (07/10).

Status: ✅ concluída (07/10) — indicador aprovado por Higor e integrado. Ver `docs/art-review/sword-hud-delivery.md`. A bandagem final da A01-T2 foi aprovada em 08/10; segue acabamento modular do Sopé (A05-T1).

## Ordem de entrega recomendada (resumo)

```
1. Kairo núcleo (A01-T1)                        ← desbloqueia testar o jogo inteiro
2. Inimigos comuns, os 3 arquétipos (A02-T1/T2/T3) — reaproveitados sem reskin nas 4 áreas
3. Faísca (A03-T1)
4. Espada do Kairo (A01-T2) — pode ser em paralelo com o item 2
5. Cenário das 4 áreas (A05-T1 Sopé da Mata → A05-T2 Dossel Vivo → A05-T3 Igarapé Sufocado → A05-T4 Covil de Korrag)
6. Boss Korrag (A04-T1)
7. Kairo: wall grab/wall jump/dash (A01-T3/T4, conforme o código for ficando pronto)
8. UI (A06)
9. enraged_transition do boss (A04-T2) — só se sobrar tempo

Pós-demo (jogo completo, fora desta entrega): reskins Deserto/Tundra dos 3 arquétipos (A02-T4/T5).
```
