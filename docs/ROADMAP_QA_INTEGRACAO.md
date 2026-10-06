---
tipo: projeto
categoria: roadmap
status: ativo
criado: 2026-09-15
atualizado: 2026-10-06
tags: [projeto, roadmap]
projeto: "[[🗂️ Reflorescer]]"
area: arte
papel: qa
responsavel: Gustavo e Higor
par: "[[ROADMAP_ARTE]]"
descricao: Casos de teste da integração código + arte
epicos_total: 8
epicos_concluidos: 2
tasks_total: 21
tasks_prontas: 3
tasks_bloqueadas: 13
---
# Roadmap de QA — Integração (Código + Arte do Higor)

Casos de teste que confirmam a integração entre o código do Gustavo e a arte do Higor — mesma hierarquia Épico → Task → Subtask do [[ROADMAP_ARTE]], mesmos IDs (`EPIC-Axx` / `Axx-Ty`), espelhados 1:1. Só entra em jogo **depois** que a Task equivalente de código já passa no [[ROADMAP_QA_CODIGO]] com arte placeholder — este arquivo não testa mecânica nova, só confirma que a arte final não quebrou o que já funcionava. Cada Task fecha com uma linha `Status:` que reflete o estado real de hoje (a maioria ainda bloqueada, porque a arte ainda não chegou) — atualizar o checkbox, o ícone e a linha `Status:` a cada entrega do Higor, e registrar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] qualquer decisão nova que a integração revelar.

Legenda de status: ✅ passa · 🔴 ainda não dá pra testar (falta arte ou código) · 🟡 parcial.

---

> [!warning]- EPIC-A00 — Padrões Técnicos — 🟡 Parcial · 🔴 Bloqueante
> *Pronto quando*: todo `SpriteFrames` entregue segue canvas e pivô consistentes, e o flip (por escala no Kairo, `FlipH` em inimigos/boss) não distorce a arte.
>
> > [!warning]- Task A00-T1 — Formato e consistência
> > - [x] Cada `SpriteFrames` entregue usa o mesmo tamanho de canvas em todos os frames de um personagem.
> > - [ ] Pivô/ancoragem consistente entre animações do mesmo personagem — sem "pulo" de posição ao trocar.
> > - **Verificação (28/09, Claude, pelos arquivos):** Kairo: os 11 quadros têm 180×170. Korrag: os 15 quadros têm 260×180 (as folhas `_sheet.png` são só referência, não entram no `SpriteFrames`). Os dois `SpriteFrames` usam `centered = false` com um offset fixo no nó, então canvas igual = mesma âncora. Um "pulo" só aconteceria se o desenho estiver deslocado dentro do canvas, e isso precisa de olho humano.
> >
> > - **Integração (06/10):** Canvas verificados: Kairo 180×170 e Faísca 32×32. Offset visual uniforme de Kairo ajustado para (-45,-65), alinhando os pés à colisão sem mudar física. Aprovação visual pendente. Ver `docs/art-review/README.md` e capturas.
> >
> > Status: 🟡 parcial (06/10) — integração local verificada; aprovação e verificações restantes pendentes.
>
> > [!warning]- Task A00-T2 — Flip
> > - [ ] Kairo: flip por escala não distorce elementos assimétricos da arte final (ex: espada só de um lado).
> > - [ ] Inimigos/boss: `FlipH` espelha corretamente sem distorcer asas/antenas/elementos assimétricos.
> > - **Achado (28/09):** o sprite do Kairo (`kairo_faisca_*.png`) tem a Faísca desenhada no canto superior direito. Com o flip por escala, ela pula pro outro lado do Kairo a cada virada. Além disso, o companheiro `Faisca.tscn` continua desenhando o placeholder (corpo, asas e luz) e segue o Kairo, então aparecem **duas Faíscas**. Proposta pro Higor: Kairo sem a Faísca no sprite, e a arte da Faísca entregue separada pro `Faisca.tscn` (ver [[ROADMAP_ARTE]] A01-T1 e A03). Flip de inimigos/boss por `FlipH`: ainda não conferido.
> >
> > - **Integração (06/10):** Faísca removida dos quadros de Kairo e integrada como companheiro independente. Flip de Kairo testado no Godot .NET. Inimigos/boss não revalidados. Ver `docs/art-review/README.md` e capturas.
> >
> > Status: 🟡 parcial (06/10) — integração local verificada; aprovação e verificações restantes pendentes.

> [!warning]- EPIC-A01 — Kairo — 🟡 Parcial · 🔴 Bloqueante
> *Pronto quando*: as animações núcleo substituem o placeholder sem regressão de física, e a Espada de Grama alterna visual corretamente em jogo.
>
> > [!warning]- Task A01-T1 — Animações núcleo
> > - [ ] Trocar o placeholder pelo pacote idle/run/jump/fall/attack/hurt/dead e rodar de novo a Task C01-T2 do [[ROADMAP_QA_CODIGO]] — nenhum comportamento de física deve mudar, só o visual.
> > - **Verificação (28/09):** idle 2 quadros/5 fps, run 2/8, jump 1, fall 1, attack 3/10 fps (0.3s, igual ao `AttackCooldown`), hurt 1, dead 1. Os nomes batem com o que o `PlayerController` toca.
> >
> > - **Integração (06/10):** 11 PNGs limpos; sete nomes, contagens, FPS e loops preservados. Corrigido run_01. Corrida, salto e ataque passaram no teste C#; C01-T2 completo e aprovação artística ainda pendentes. Ver `docs/art-review/README.md` e capturas.
> >
> > Status: 🟡 parcial (06/10) — integração local verificada; aprovação e verificações restantes pendentes.
>
> > [!failure]- Task A01-T2 — Espada de Grama
> > - [ ] Sprite da espada visível quando ativada, "bandagem" visível quando desativada — nunca os dois ao mesmo tempo.
> > - [ ] Animação de ataque: a janela de dano da hitbox é fixa em 0.14s hoje, independente da duração da animação — ajustar o tempo da hitbox (ou o `AttackCooldown`) se a animação final não bater com esse corte.
> > - **Dependência dupla:** só testável depois que [[ROADMAP_DEV]] `EPIC-C03` (toggle da espada) **e** [[ROADMAP_ARTE]] `A01-T2` (os 2 visuais) estiverem prontos.
> >
> > Status: 🔴 bloqueado só por arte (28/09) — o código está pronto ([[ROADMAP_DEV]] EPIC-C03 concluído); faltam os 2 visuais (espada / bandagem). Hoje o estado aparece no HUD e num tom verde provisório.
>
> > [!failure]- Task A01-T3 — Wall grab / wall jump
> > - [ ] Animação integrada e sincronizada com a mecânica ([[ROADMAP_DEV]] `EPIC-C05`).
> >
> > Status: 🔴 bloqueado só por arte (28/09) — mecânica pronta ([[ROADMAP_DEV]] EPIC-C05 concluído). O código já toca `wall_slide` se existir no `SpriteFrames`; senão usa `fall`.
>
> > [!failure]- Task A01-T4 — Dash
> > - [ ] Variação visual integrada e sincronizada com a mecânica ([[ROADMAP_DEV]] `EPIC-C04`).
> >
> > Status: 🔴 bloqueado só por arte (28/09) — mecânica pronta ([[ROADMAP_DEV]] EPIC-C04 concluído). O código já toca `dash` se existir no `SpriteFrames`; senão usa `run` com tom esverdeado.

> [!success]- EPIC-A02 — Inimigos Comuns — ✅ Passa no escopo da demo
> *Pronto quando*: os três arquétipos usam recursos próprios com a mesma máquina de estados. Reskins de Deserto/Tundra ficam fora da demo.
>
> > [!success]- Task A02-T1/T2/T3 — Arquétipos da Floresta Tropical
> > - [x] Recursos de cada arquétipo integrados sem lógica específica por desenho. Correções compartilhadas de integração registradas abaixo.
> > - [x] Voador permanece suspenso sem gravidade; os dois terrestres patrulham apoiados no chão.
> > - [x] `FlipH` espelha corretamente os três arquétipos sem deslocar o eixo do sprite.
> >
> > - **Integração (06/10):** 160 verificações dirigidas passaram na master: canvas, avanço de quadros, patrulha/detecção naturais, duração/reinício do ataque nos nove recursos de área, dano nos dois sentidos, interrupção conforme stats, morte por colisão real e fade. Corrigidos escala/pivô, direção da hitbox, fechamento da janela de ataque e desativação segura de colisões. Cena `EnemyReview.tscn` conferida no renderer Compatibility. Logs em `docs/art-review/enemy-validation.txt`.
> >
> > Status: ✅ passa (06/10) — integração dirigida concluída. Percurso completo, dificuldade e áudio seguem no A07; não foram cobertos por este teste.
>
> > [!failure]- Task A02-T4/T5 — Reskins Deserto e Tundra
> > - [ ] Repetir os testes da Task A02-T1/T2/T3 pra cada reskin.
> > - **Nota (29/09):** fora do escopo da demo (Deserto/Tundra não entram mais no percurso — ver [[🎯 Escopo da Demo]]). Fica pro jogo completo.
> >
> > Status: 🔴 bloqueado — pós-demo, sem urgência.

> [!success]- EPIC-A03 — Faísca — ✅ Passa · 🟠 Alta
> *Pronto quando*: `fly`/`investigate` acompanham a troca de estado sem travar num frame parado, e a luz não "pisca" ao trocar de direção.
>
> > [!success]- Task A03-T1
> > - [x] Com `fly`/`investigate` adicionadas, a troca de animação acompanha a troca de estado (`FollowState`/`IdleFaiscaState`/`InvestigateState`/`ReturnToPlayerState`) sem travar num frame parado.
> > - [x] A luz (`PointLight2D`) pulsa e o flip de sprite não "pisca" de forma estranha ao trocar de direção.
> >
> > - **Integração (06/10):** 12 frames entregues, quatro por `idle`/`fly`/`investigate`; os três estados e o avanço de frames passaram em teste dirigido no Godot .NET. A luz pulsa em nó irmão do sprite, portanto não é espelhada pela virada.
> >
> > Status: ✅ passa (06/10) — animações, estados e luz independentes verificados.

> [!warning]- EPIC-A04 — Boss KORRAG — 🟡 Parcial · 🟠 Alta
> *Pronto quando*: as transições de fase são claras pro jogador mesmo com `idle` compartilhado, o timing de `charge_windup` bate com o código, e o nome exibido é "Korrag, o Javali".
>
> > [!warning]- Task A04-T1 — Obrigatórios
> > - [ ] Transições de fase ([[ROADMAP_QA_CODIGO]] `C06-T0`) comunicam claramente ao jogador que o boss mudou de fase, mesmo com `idle` compartilhado.
> > - [x] Quando `StartCharge()`/`PerformStomp()` forem ligados ([[ROADMAP_DEV]] `EPIC-C06-T1`): `charge_windup` cabe no ~1s fixo antes da investida disparar.
> > - [x] Animação `defeated` aparece após `dead`, durante o fade, sem emitir vitória novamente.
> > - [x] Nome exibido em qualquer UI usa "Korrag, o Javali", não "Javali das Ruínas".
> > - **Verificação (28/09):** `charge_windup` tem 1 quadro. Mantido o windup de 1s com pulso vermelho de aviso, aprovado no playtest ([[ROADMAP_DEV]] C06-T4). Nome: nenhuma UI exibe o `BossName` hoje (a barra do Korrag mostra só a vida); o recurso diz "Korrag, o Javali".
> > - **Mudança de código (28/09):** a morte não é mais "invisível + destruir em 0.3s". Agora toca `dead`, espera 0.6s e some em fade de 0.6s. A animação `defeated` existe no `SpriteFrames` mas **não é tocada** por nenhum estado: decidir se ela substitui `dead` ou entra depois dela.
> > - **Observação pra conferência visual:** `phase2_transition` tem 1 quadro (0.25s), mas o estado segura 1.5s nela. `phase1_idle` e `enraged_transition` também têm 1 quadro.
> > - **Integração (06/10):** os visuais obrigatórios estão em `KorragSpriteFrames.tres`. A sequência agora é dead por 0,6 s → defeated durante fade de 0,6 s → remoção. Seis verificações passaram em `BossDefeatCheck.tscn`, incluindo emissão única de vitória. Falta o playtest visual das transições de fase.
> >
> > Status: 🟡 parcial (06/10) — integração de recursos e chamadas validada; playtest visual pendente.
>
> > [!success]- Task A04-T2 — Opcional
> > - [x] `enraged_transition` integrada, se entregue.
> > - **Nota:** opcional por decisão do Gustavo — só testar se [[ROADMAP_ARTE]] `A04-T2` for de fato produzida.
> >
> > - **Verificação (06/10):** a animação existe no recurso ligado à cena e `BossEnragedState` chama `PlayAnimation("enraged_transition")`.
> >
> > Status: ✅ passa (06/10) — recurso e chamada de estado presentes.

> [!warning]- EPIC-A05 — Cenário — 🟡 Parcial · 🟠 Alta
> *Pronto quando*: as 4 áreas têm identidade visual reconhecível sem texto, os pontos de entrada de cena fazem sentido, e a performance se mantém estável no `gl_compatibility`.
>
> Atualizado em 29/09/2026 — a demo passou a ser 4 áreas dentro da Floresta Tropical (era Floresta Tropical/Deserto/Tundra); `TargetArea`/`SpawnOffset` já funcionam desde [[ROADMAP_QA_CODIGO]] `C08-T1` (concluída em 28/09), então a ressalva de posicionamento manual não se aplica mais.
>
> > [!warning]- Task A05-T1 — Sopé da Mata
> > - [ ] Identidade visual clara — dá pra saber que bioma é só de olhar, sem ler texto.
> > - [ ] Transição de área: o Kairo entra no `Marker2D` certo vindo do menu/novo jogo.
> > - [ ] Performance: tileset/background + pulsos de luz da Faísca + barras de vida flutuantes (com `Tween`) sem queda perceptível de FPS no renderer `gl_compatibility`.
> >
> > - **Integração (06/10):** Protótipo integrado ao Sopé da Mata: chão grama/terra/raízes/pedra, parallax, partículas e restauração seco→verde. Interação E e crescimento passaram no Godot .NET. Tileset modular, percurso completo e medição de FPS pendentes. Ver `docs/art-review/README.md` e capturas.
> >
> > Status: 🟡 parcial (06/10) — integração local verificada; aprovação e verificações restantes pendentes.
>
> > [!failure]- Task A05-T2 — Dossel Vivo
> > - [ ] Repetir os 3 testes da Task A05-T1, mais: leitura clara das plataformas de pulo vertical.
> >
> > Status: 🔴 bloqueado — depende de [[ROADMAP_ARTE]] `A05-T2` e de [[ROADMAP_DEV]] `EPIC-C13-T2`.
>
> > [!failure]- Task A05-T3 — Igarapé Sufocado
> > - [ ] Repetir os 3 testes da Task A05-T1, mais: a lama é visualmente clara (o jogador entende por que ficou mais lento).
> >
> > Status: 🔴 bloqueado — depende de [[ROADMAP_ARTE]] `A05-T3` e de [[ROADMAP_DEV]] `EPIC-C13-T3`.
>
> > [!failure]- Task A05-T4 — Covil de Korrag
> > - [ ] Ambientação de arena de boss clara, sem elementos de exploração (ponto de restauração, etc.) — é só a luta.
> >
> > Status: 🔴 bloqueado — depende de [[ROADMAP_ARTE]] `A05-T4` e de [[ROADMAP_DEV]] `EPIC-C13-T4`.

> [!failure]- EPIC-A06 — UI — 🔴 Bloqueado · 🟡 Média
> *Pronto quando*: barra de vida, ícone de habilidade e prompt de interação têm arte final integrada e funcional em jogo.
>
> > [!failure]- Task A06-T1 — Barra de vida
> > - [ ] Barra de vida com textura final.
> >
> > Status: 🔴 bloqueado — depende de [[ROADMAP_ARTE]] `A06-T1`.
>
> > [!failure]- Task A06-T2 — Ícone de habilidade
> > - [ ] Ícone de habilidade desbloqueada (quando a notificação real existir — [[ROADMAP_QA_CODIGO]] `C04-T4`).
> >
> > Status: 🔴 bloqueado — depende de código e arte, nenhum dos dois pronto ainda.
>
> > [!failure]- Task A06-T3 — Prompt de interação
> > - [ ] Prompt de interação com estilo definido (quando [[ROADMAP_QA_CODIGO]] `C10-T1` for resolvido).
> >
> > Status: 🔴 bloqueado — depende de código e arte, nenhum dos dois pronto ainda.

> [!failure]- EPIC-A07 — Validação Final (Playtest Completo) — 🔴 Bloqueado · 🔴 Bloqueante
> *Pronto quando*: a demo roda de ponta a ponta com áudio, arte e código integrados, sem crash, softlock ou animação quebrada, validada por alguém de fora do grupo.
>
> Só roda depois que todos os Épicos 🔴 acima (deste arquivo e do [[ROADMAP_QA_CODIGO]]) estiverem ✅.
>
> > [!failure]- Task A07-T1 — Áudio integrado
> > - [ ] Com os primeiros arquivos de SFX/música em `res://assets/audio/`, jogar a demo do início ao fim e confirmar que nenhum som falta silenciosamente (checar o console).
> > - [ ] Volume relativo entre música de fundo e efeitos.
> > - [ ] Som de ataque/hit sincronizado com o frame de impacto da animação.
> >
> > Status: 🔴 bloqueado — depende de [[ROADMAP_DEV]] `EPIC-C11`.
>
> > [!failure]- Task A07-T2 — Ponta a ponta
> > - [ ] Demo completa sem parar: Sopé da Mata → Dossel Vivo → Igarapé Sufocado → Covil de Korrag → matar 1 de cada arquétipo pelo caminho → derrotar o Korrag → dash desbloqueado (ver [[🎯 Escopo da Demo]]) — sem crash, sem softlock, sem animação quebrada.
> > - [ ] Repetir morrendo de propósito em cada área (valida respawn/checkpoint com arte e áudio reais).
> >
> > Status: 🔴 bloqueado — depende de todos os Épicos anteriores, de código e de arte.
>
> > [!failure]- Task A07-T3 — Playtest cego
> > - [ ] Alguém de fora do grupo joga sem explicação prévia — se entender controles e objetivo, a demo está pronta pra apresentação.
> >
> > Status: 🔴 bloqueado — última etapa, depende de tudo acima.
