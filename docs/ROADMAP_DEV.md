---
tipo: projeto
categoria: roadmap
status: ativo
criado: 2026-09-15
atualizado: 2026-09-28
tags: [projeto, roadmap]
projeto: "[[🗂️ Reflorescer]]"
area: codigo
papel: execucao
responsavel: Gustavo
par: "[[ROADMAP_QA_CODIGO]]"
descricao: Backlog de implementação da demo (código)
epicos_total: 12
epicos_concluidos: 0
tasks_total: 44
tasks_prontas: 17
tasks_bloqueadas: 7
tasks_liberadas: 13
---
# Roadmap de Desenvolvimento — Código (Gustavo)

Backlog de execução da demo: Épicos (fases, ordenados pela dependência real entre as partes — fundação técnica antes de qualquer mecânica nova, restauração ODS15 e boss logo depois porque são os gaps de maior prioridade) que abrem em Tasks, que abrem em Sub-tasks. Cada sub-task é um checkbox — marcar conforme for implementado. Sempre que uma Task gerar uma nota de desenvolvimento (bugfix, decisão, mudança de escopo) durante o trabalho, ela é registrada ali mesmo, no formato **Tipo (DD/MM) — resumo**, logo abaixo do checklist da Task. Se a nota for uma decisão de design (não só um detalhe técnico de implementação), ela também é registrada no GDD do Obsidian: [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (o quê/porquê) e uma linha no [[Projetos/Reflorescer/🗓️ Log de Atualizações|🗓️ Log de Atualizações]] (quando). A ordem dos épicos é uma sugestão, ajustável a qualquer momento já que o código é do Gustavo.

Cada Épico aqui tem um Épico **gêmeo de mesmo ID** no [[ROADMAP_QA_CODIGO]] (ex: `EPIC-C03`), com os casos de teste que definem "pronto" de cada Task. Fluxo: implementar as sub-tasks de uma Task → rodar as sub-tasks de verificação da Task equivalente no QA → escrever a linha `Status:` → seguir pra próxima.

Marcação de disponibilidade na linha `Status:` de cada Task (avaliação de 28/09, após o pull do commit `6153f13`): 🟢 liberado — dá pra começar agora · 🟡 aguarda — depende de outra Task deste roadmap · ⛔ bloqueado — depende de decisão de design, arte/cena ou áudio fora deste roadmap.

Ordem recomendada: `EPIC-C01` → `EPIC-C02` → `EPIC-C06` → `EPIC-C04` → `EPIC-C03` → `EPIC-C05` → `EPIC-C07` → `EPIC-C08` → `EPIC-C09` → `EPIC-C10` → `EPIC-C11` → `EPIC-C12`. C03, C05 e C07 não têm dependência forte entre si — dá pra reordenar ou paralelizar, desde que C01 já esteja fechado.

---

> [!info]- EPIC-C01 — Fundação Técnica — 🔄 Em andamento · 🔴 Bloqueante
> *Pronto quando*: o Input Map bate 100% com os controles de [[🕹️ Sistemas de Jogo#Movimentação|Sistemas de Jogo]], só existe um sistema de movimento rodando (a `PlayerStateMachine`/`PlayerStates.cs` antiga foi removida ou virou a única fonte), e o double jump tem uma única fonte de verdade.
>
> Bloqueante — fazer antes de tudo. O Input Map real não bate com a documentação confirmada, e existem dois sistemas de movimento rodando em paralelo (`PlayerController.HandlePrototypeMovement` + `PlayerStateMachine`/`PlayerStates.cs`) — cada mecânica nova escrita em cima disso dobra o risco de bug.
>
> > [!info]- Task C01-T1 — Corrigir o Input Map
> > - [x] Remapear em Project Settings → Input Map pra bater com os controles de [[🕹️ Sistemas de Jogo#Movimentação|Sistemas de Jogo]]: A/D mover, Espaço pular, Shift dash, Q espada, E interagir, K ataque básico, segurar J especial.
> > - [x] Grep em `scripts/` por `IsActionJustPressed`/`IsActionPressed` com os nomes antigos de ação e corrigir qualquer referência que sobrar.
> > - **Achado da auditoria (15/09):** hoje `attack`=J+clique, `dash`=K+seta-baixo, `jump`=Espaço/W/seta-cima, `ability`=L (sem uso). Sem S nem Q mapeados.
> > - **Nota (28/09) — mapeamento aplicado:** `move_left`=A, `move_right`=D, `jump`=Espaço, `dash`=Shift, `interact`=E, `attack`=K, `toggle_sword`=Q (nova), `special`=J (nova, segurar), `pause`=Esc. Saíram as teclas extras (setas, W, clique, Ctrl) e a ação `ability` (L, sem uso). Nenhum script lia `ability`; `toggle_sword` e `special` ainda não têm consumidor (C03-T1 e futuro especial).
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot.
>
> > [!info]- Task C01-T2 — Unificar o sistema de movimento
> > - [x] Decidir: manter o caminho direto (`HandlePrototypeMovement`, já testado) e apagar `PlayerStateMachine`/`PlayerStates.cs`, **ou** migrar de vez pra state machine e remover o caminho direto.
> > - [x] Executar a remoção/migração escolhida.
> > - [ ] Rodar a Task C01-T2 do [[ROADMAP_QA_CODIGO]] (regressão completa de movimento e física) antes de seguir.
> > - **Recomendação:** manter o caminho direto — é o de menor risco a essa altura do projeto.
> > - **Decisão do Gustavo (28/09):** mantido o caminho direto (`HandlePrototypeMovement`). Removidos `PlayerStateMachine.cs`, `PlayerStates.cs` (+ `.uid`), o nó `PlayerStateMachine` do `Player.tscn` e o `ApplyHorizontalMovement` (só os estados usavam). Animações `hurt`/`dead` passaram pro `PlayerController` (`_hurtTimer` e `OnDied`).
> > - **Bugfix CRÍTICO (28/09) — física rodava 2x por frame:** além do ataque duplicado, cada estado antigo chamava `ApplyGravity` + `MoveAndSlide`, então o Kairo andava/caía com o dobro de deslocamento e gravidade por frame. Na prática, o jogo testado até hoje rodava ~2x mais rápido que os valores do `PlayerStatsResource`. Depois da remoção os valores documentados (200px/s, -460, etc.) passam a ser reais, e o movimento vai parecer **mais lento**. Pra recuperar a sensação antiga: velocidades ×2 e gravidade/acelerações ×4 no `.tres` — decisão de tuning, não aplicada.
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot.
>
> > [!info]- Task C01-T3 — Fonte única de verdade pro Double Jump
> > - [x] Escolher uma fonte entre `PlayerController.InitDoubleJump()` (fallback hardcoded) e `AbilityManager.UnlockPrototypeAbilities()` — recomendo `AbilityManager`, é o sistema que vai crescer com o Dash.
> > - [x] Remover o fallback duplicado do `PlayerController`.
> > - **Nota (28/09):** fonte única = `AbilityManager`. O `PlayerController` consulta `AbilityManager.Instance.HasAbility(DoubleJump)` na hora do pulo (sem cache, sem fallback). O `AbilityManager` já nasce com `DoubleJump` no conjunto, sem emitir `AbilityUnlocked` (não é um desbloqueio de gameplay, então não dispara notificação nem save).
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot.

> [!info]- EPIC-C02 — Mecânica de Restauração ODS15 — 🔄 Em andamento · 🔴 Bloqueante
> *Pronto quando*: existe pelo menos 1 ponto de restauração interagível em cada uma das 3 áreas da demo, com efeito visual próprio, evento dedicado no `EventBus`, e persistência em save.
>
> Prioridade máxima de conteúdo — sem isso a demo perde a conexão com o pitch (ODS 15). Hoje `Checkpoint` é só um save point genérico, sem nenhum conceito de "restaurar" o bioma.
>
> > [!info]- Task C02-T1 — Definir e implementar a interação
> > - [x] Decidir se a restauração é o próprio `Checkpoint` reaproveitado (com um estado temático a mais) ou uma classe nova (`RestorationPoint : IInteractable`) separada do save point.
> > - [x] Implementar a interação escolhida.
> > - **Decisão do Gustavo (28/09) — classe separada:** `RestorationPoint : Area2D, IInteractable` (`scripts/world/RestorationPoint.cs`), independente do `Checkpoint`. Restaurar não é salvar, então cada um tem evento e persistência próprios. Interage uma vez (`Restored`), prompt "Restaurar". Os ganchos de C02-T2 (efeito) e C02-T3 (evento/save) estão marcados no `Interact()`. **Registrar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].**
> >
> > Status: implementado (28/09), compila — ⏳ aguarda QA no Godot. Um ponto de teste (`RestorationPoint1`) está no `TestLevel`, perto do spawn.
>
> > [!todo]- Task C02-T2 — Efeito visual/de mundo
> > - [ ] Implementar o efeito da restauração (mesmo simples pra demo: partícula + mudança de cor no tile ao redor, ou sprite "antes/depois").
> >
> > Status: não iniciado — 🟢 liberado (C02-T1 implementado).
>
> > [!todo]- Task C02-T3 — Evento dedicado e persistência
> > - [ ] Emitir um evento próprio no `EventBus` (ex: `AreaRestored(string pointId)`) — não reaproveitar `CheckpointActivated`.
> > - [ ] Persistir pontos restaurados no save, igual `ActivatedCheckpoints` já persiste.
> >
> > Status: não iniciado — 🟢 liberado (C02-T1 implementado).
>
> > [!failure]- Task C02-T4 — Posicionar nas 3 áreas da demo
> > - [ ] Definir quantos pontos por área (mínimo 1 por bioma) e posicionar nas cenas de Floresta Tropical, Deserto e Tundra.
> >
> > Status: não iniciado — ⛔ bloqueado: as cenas de Floresta Tropical, Deserto e Tundra não existem (só `TestLevel` e `BossArena`) — depende do [[ROADMAP_ARTE]] `EPIC-A05`.

> [!info]- EPIC-C03 — Combate e Espada de Grama — 🔄 Em andamento · 🟠 Alta
> *Pronto quando*: Q alterna a espada em tempo real (visual + `HasSword`), o jogador tem stats diferentes com a espada ativada/desativada, e não dá pra atacar com ela desativada.
>
> > [!todo]- Task C03-T0 — Regressão de combate básico (antes de mexer)
> > - [ ] Rodar a Task C03-T0 do [[ROADMAP_QA_CODIGO]] (ataque, hitbox/hurtbox, cooldown, invulnerabilidade) pra confirmar a base antes de alterar.
> >
> > Status: não iniciado — 🟢 liberado. Rodar agora: depois da C01-T2 o ataque sai 1 vez só e a física ficou diferente.
>
> > [!todo]- Task C03-T1 — Wire do input Q (ativar/desativar)
> > - [ ] Ligar a ação `Q` (pós `EPIC-C01-T1`) a um método novo `ToggleSword()` em `PlayerController`, chamando `HasSword = !HasSword` e `SetSwordVisible(HasSword)`.
> > - **Achado (28/09, pós-pull) — sprite do Kairo sem variante de espada:** os `ColorRect` placeholder da espada (`Blade`/`Handle`) foram escondidos no commit `6153f13`, e o sprite novo (`KairoFaiscaSpriteFrames.tres`) não tem espada visível nem variante com e sem espada. O toggle de lógica funciona, mas o visual precisa de arte nova (ou de uma camada separada da espada). Também: a Faísca está desenhada dentro do sprite do Kairo e duplica com o `Faisca.tscn` — levar pro Higor ([[ROADMAP_ARTE]] `EPIC-A01`).
> >
> > Status: não iniciado — 🟢 liberado pra lógica (a ação `toggle_sword`/Q já existe). O visual depende de arte (ver nota acima).
>
> > [!info]- Task C03-T2 — Stats duplos (ativada/desativada)
> > - [x] Criar a variante "espada desativada": +velocidade, +pulo, sem ataque.
> > - **Opção simples:** dois blocos de valores no `PlayerStatsResource` (`MoveSpeedSwordOn`/`Off`, etc.) ou um segundo `.tres`.
> > - **Nota (28/09):** opção simples aplicada: `MoveSpeedNoSword` (240) e `JumpVelocityNoSword` (-520) no `PlayerStatsResource`. O `PlayerController` usa `CurrentMoveSpeed`/`CurrentJumpVelocity`, que leem `HasSword`. O "sem ataque" já vem do `CanAttack()`. **Os valores são chute (+20% e +13%): decidir os definitivos e registrar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].**
> >
> > Status: implementado (28/09), compila — ⏳ só dá pra testar no Godot depois do toggle (C03-T1).
>
> > [!warning]- Task C03-T3 — Bloqueio e teste em combate real
> > - [ ] Bloquear `CanAttack()` quando `HasSword == false` (parcialmente já verdade hoje).
> > - [ ] Testar a troca em pleno combate/movimento — sem travar animação/input no meio da troca.
> > - [ ] Rodar de novo a Task C03-T0 pra garantir que nada regrediu.
> >
> > Status: não iniciado — 🟡 aguarda C03-T1/T2.

> [!info]- EPIC-C04 — Dash e Desbloqueio Pós-Korrag — 🔄 Em andamento · 🔴 Bloqueante
> *Pronto quando*: derrotar o Korrag desbloqueia o dash uma única vez, e apertar Shift com o dash desbloqueado move o Kairo com um impulso rápido na direção que ele olha.
>
> Depende de `EPIC-C01` (Input Map correto).
>
> > [!info]- Task C04-T1 — Corrigir o evento BossDefeated duplicado
> > - [x] `BossBase.OnDied()` chama `EmitDefeated()` diretamente **e** `BossDeadState.Enter()` chama de novo — remover uma das duas (sugestão: manter só a de `BossDeadState.Enter()`).
> > - **Fazer antes da Task C04-T3**, senão o dash pode ser "desbloqueado" duas vezes sem causar bug visível, mas é sujeira desnecessária.
> > - Mesma correção referenciada em `EPIC-C06-T2` — fazer uma vez só.
> > - **Nota (28/09):** removido o `EmitDefeated()` de `BossBase.OnDied()`. A fonte única agora é `BossDeadState.Enter()`.
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot.
>
> > [!todo]- Task C04-T2 — Implementar o movimento do dash
> > - [ ] Impulso horizontal rápido na direção que o Kairo está olhando, com cooldown e talvez i-frames curtos (típico de Metroidvania).
> >
> > Status: não iniciado — 🟢 liberado (Shift mapeado e movimento unificado).
>
> > [!warning]- Task C04-T3 — Ligar ao AbilityManager e ao evento do boss
> > - [ ] Input: `Input.IsActionJustPressed("dash") && AbilityManager.Instance.HasAbility(AbilityId.Dash)`.
> > - [ ] Assinar `EventBus.BossDefeated` (sugestão: no próprio `AbilityManager`, ou um `DemoProgressionManager` novo) e chamar `UnlockAbility(AbilityId.Dash)` quando `bossId == "boss_javali"`.
> >
> > Status: não iniciado — 🟡 aguarda C04-T1 e C04-T2.
>
> > [!info]- Task C04-T4 — Notificação de habilidade desbloqueada
> > - [x] Dar um retorno visual mínimo (texto na tela por 2-3s) — hoje `HUDController.ShowAbilityUnlockNotification` só imprime no console.
> > - **Nota (28/09):** `HUDController` cria um `Label` centralizado no topo ("Nova habilidade: X"), 2.5s na tela e 0.4s de fade. A `BossArena` não tinha HUD, então ganhou uma instância de `HUD.tscn`: é lá que o dash vai ser desbloqueado.
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot.

> [!todo]- EPIC-C05 — Wall Grab / Wall Jump — ⬜ Não iniciado · 🟠 Alta
> *Pronto quando*: segurar A/D contra uma parede no ar reduz a queda, e pular nessa condição empurra o Kairo pro lado oposto.
>
> > [!todo]- Task C05-T1 — Detecção de parede
> > - [ ] Detectar colisão lateral com parede no ar (`IsOnWall()` do Godot ou raycast lateral) em `PlayerController`.
> >
> > Status: não iniciado — 🟢 liberado (movimento unificado).
>
> > [!warning]- Task C05-T2 — Agarrar / slide
> > - [ ] Enquanto segurando A/D contra a parede no ar, reduzir a velocidade de queda (não necessariamente travar em 0).
> >
> > Status: não iniciado — 🟡 aguarda C05-T1.
>
> > [!warning]- Task C05-T3 — Pulo de parede
> > - [ ] Pular estando agarrado empurra pro lado oposto da parede, impulso vertical semelhante ao pulo normal.
> >
> > Status: não iniciado — 🟡 aguarda C05-T2.
>
> > [!failure]- Task C05-T4 — Decisão de gating
> > - [ ] Decidir se é liberado desde o início ou gated por uma `AbilityId` (`WallJump` já existe no enum) — **pendência a decidir com o Higor/documentar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]]** antes de travar o design final.
> >
> > Status: não iniciado — ⛔ bloqueado: decisão de design pendente (Higor + [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]]).

> [!info]- EPIC-C06 — Boss Korrag (IA e Identidade) — 🔄 Em andamento · 🔴 Bloqueante
> *Pronto quando*: o Korrag usa Charge e Stomp em algum momento da luta (não só o ataque genérico), o `BossName` no código é "Korrag, o Javali", e o evento de derrota dispara uma única vez.
>
> **Achado da auditoria:** a troca de fase por HP já funciona sozinha (`BossStateMachine`). O trabalho real é só **conectar os ataques que já existem** (`StartCharge`/`PerformStomp` em `JavaliBoss.cs`) — hoje nenhum estado os chama.
>
> > [!todo]- Task C06-T0 — Regressão do núcleo do boss (antes de mexer)
> > - [ ] Rodar a Task C06-T0 do [[ROADMAP_QA_CODIGO]] (ativação, intro, fases automáticas, contato, morte em 8 hits).
> >
> > Status: não iniciado — 🟢 liberado (rodar de novo agora que o sprite real do Korrag entrou, com novo posicionamento/escala).
>
> > [!info]- Task C06-T1 — Loop de decisão de ataque
> > - [x] Em `BossPhase1State`/`BossPhase2State`/`BossEnragedState`, trocar a chamada genérica `Boss.PerformAttack()` por uma decisão entre `PerformAttack()` (melee), `StartCharge()` (investida) e `PerformStomp()` (pisada em área).
> > - **Sugestão de implementação:** método virtual `ChooseAttack()` em `BossBase`, sobrescrito por `JavaliBoss` — pode começar simples (sorteio ponderado ou por distância).
> > - **Nota (28/09) — implementação:** `BossBase.ChooseAttack()` virtual, sobrescrito em `JavaliBoss`. Stomp (35%, 50% no enraged) quando o jogador está a até `StompRadius`; Charge (60% na fase 1, 80% depois) a partir de `ChargeMinDistance` (180px); no meio-termo, 40% Charge e o resto melee. O Charge agora tem fim (`ChargeDuration` 0.9s ou bater na parede) e aplica `ChargeDamage`. O Stomp ganhou windup de 0.4s. O `MoveAndSlide` saiu do `ExecuteCharge` (o estado já chama). O melee vira a hitbox pro lado do jogador (antes só acertava à direita) e desliga depois de 0.25s.
> > - **Decisão do Gustavo (28/09) — super armor e timers:** o boss tem `InvulnerabilityDuration = 0` e todo golpe o jogava em `Hurt`, o que zerava o timer de ataque e cancelava o windup do Charge. Na prática, batendo sem parar, ele nunca atacava. Agora: (1) durante Charge/Stomp (`IsBusy`) o dano entra, mas não interrompe nem empurra; (2) o timer de ataque sobrevive às idas ao `Hurt`; (3) as transições de fase 2/enraged tocam uma vez só (antes repetiam a cada hit). **Registrar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].**
> > - **Achado (28/09), não corrigido:** o enraged chama `SetPhase(2)`, mas `JavaliStatsResource` só tem 2 fases (índices 0 e 1), então no enraged o boss cai nos defaults (cooldown 1.5, velocidade 100, dano 20). Fica mais fraco que na fase 2. Precisa de um `JavaliPhase3.tres` ou de ajuste no índice.
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot.
>
> > [!info]- Task C06-T2 — Corrigir o evento BossDefeated duplicado
> > - [x] Mesma correção da `EPIC-C04-T1` — fazer uma vez só, referenciar aqui.
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot. Mesma correção da C04-T1.
>
> > [!info]- Task C06-T3 — Corrigir o nome oficial
> > - [x] Atualizar `BossName` em `resources/bosses/JavaliStatsResource.tres` de `"Javali das Ruínas"` para `"Korrag, o Javali"`.
> > - **Nota (28/09):** trocado no `.tres` e no fallback hardcoded do `JavaliBoss._Ready()`.
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot.
>
> > [!todo]- Task C06-T4 — Ajustar timing com a arte real
> > - [ ] Testar o `charge_windup` (1s) contra a animação real assim que a arte chegar (cruza com [[ROADMAP_QA_INTEGRACAO]] `EPIC-A04`).
> > - **Nota (28/09, pós-pull):** `KorragSpriteFrames.tres` já tem `charge_windup` (1 frame, 5 fps) e todos os nomes de animação chamados em `BossStateMachine.cs`/`JavaliBoss.cs` existem no recurso (`walk` existe mas nenhum script chama ainda).
> >
> > Status: não iniciado — 🟢 liberado (C06-T1 implementado e arte do `charge_windup` no repo).

> [!info]- EPIC-C07 — Inimigos Comuns (Arquétipos) — 🔄 Em andamento · 🟠 Alta
> *Pronto quando*: existem 3 arquétipos (Voador/Rápido/Robusto) com comportamento distinto de fato, cada um com `.tres` de stats pra cada uma das 3 áreas da demo.
>
> > [!todo]- Task C07-T0 — Regressão do comportamento base (antes de mexer)
> > - [ ] Rodar a Task C07-T0 do [[ROADMAP_QA_CODIGO]] (idle/patrol/detect/chase/attack/hurt/dead do `EnemyBase` genérico).
> >
> > Status: não iniciado — 🟢 liberado.
>
> > [!info]- Task C07-T1 — Voador
> > - [x] Criar `EnemyVoador : EnemyBase` — sobrescrever gravidade/colisão pra voar (ignorar `ApplyGravity` ou usar versão sem gravidade, `Y` fixo/oscilante).
> > - **Nota (28/09):** `EnemyVoador : EnemyBase` (`MotionMode = Floating`). Sobrescreve `ApplyGravity` (flutua oscilando em Y), `ChasePlayer` (persegue nos dois eixos) e `AtPatrolEdge` (sempre `false`). No `EnemyBase`, `ApplyGravity`, `ChasePlayer` e `AtPatrolEdge` viraram `virtual`. Cena herdada `Enemy_Voador.tscn` + `EnemyVoadorStats.tres`.
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot. `Voador1` está no `TestLevel` sobre o vão entre `Floor1` e `Platform1`.
>
> > [!info]- Task C07-T2 — Rápido
> > - [x] Avaliar se basta um `.tres` novo (`ChaseSpeed`/`PatrolSpeed` mais altos, já 100% data-driven) ou se precisa de subclasse de código.
> > - **Nota (28/09) — avaliação:** basta `.tres`, sem subclasse. `EnemyRapidoStats.tres` (Chase 190, Patrol 90, HP 50, cooldown 0.6) + cena herdada `Enemy_Rapido.tscn`.
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot. `Rapido1` está no `TestLevel` (Floor2).
>
> > [!info]- Task C07-T3 — Robusto
> > - [x] `.tres` com `MaxHealth`/`KnockbackResistance` mais altos; avaliar se precisa de código extra (ex: ignorar knockback abaixo de um threshold).
> > - **Nota (28/09) — avaliação:** sem subclasse. Um campo novo no `EnemyStatsResource`, `InterruptOnHit` (`false` = não entra em `Hurt` ao tomar dano), mantém tudo data-driven. `EnemyRobustoStats.tres` (HP 150, KnockbackResistance 0.9, `InterruptOnHit = false`) + cena herdada `Enemy_Robusto.tscn`. **Os valores dos 3 arquétipos são chute: validar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].**
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot. `Robusto1` está no `TestLevel` (Floor1).
>
> > [!warning]- Task C07-T4 — Dados por arquétipo × área da demo
> > - [ ] Criar os `.tres` de `EnemyStatsResource` pra cada arquétipo × cada uma das 3 áreas (Floresta Tropical primeiro).
> >
> > Status: não iniciado — 🟡 aguarda C07-T1/T2/T3 e os valores por área em [[👾 Bestiário]] (os `.tres` não dependem das cenas existirem).
>
> > [!info]- Task C07-T5 — (baixa prioridade) AtPatrolEdge()
> > - [x] Hoje sempre retorna `false` — corrigir só se o comportamento "parar depois de patrulhar" for desejado. Aceitável pra demo como está.
> > - **Nota (28/09):** `AtPatrolEdge()` faz um raycast pra baixo 20px à frente (máscara World). No `EnemyPatrolState`, na beira o inimigo vira (`Flip`) e entra em `Idle`, retomando a patrulha no sentido oposto.
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot.

> [!info]- EPIC-C08 — Transição de Área — 🔄 Em andamento · 🟠 Alta
> *Pronto quando*: o jogador chega na cena nova numa posição consistente com de onde veio, não sempre no spawn padrão.
>
> > [!todo]- Task C08-T0 — Regressão básica do trigger
> > - [ ] Rodar a Task C08-T0 do [[ROADMAP_QA_CODIGO]] (trigger carrega a cena, erro tratado se a cena não existe).
> >
> > Status: não iniciado — 🟢 liberado.
>
> > [!info]- Task C08-T1 — Spawn direcional
> > - [x] **Opção A:** implementar o uso de `TargetArea`/`SpawnOffset` (já existem exportados em `TransitionTrigger`, não são lidos) pra posicionar o jogador no spawn correspondente.
> > - [ ] **Opção B** (mais simples se o tempo apertar): nomear spawn points por origem (`SpawnFromFlorestaTropical`, `SpawnFromDeserto`) e escolher pelo nome de onde o jogador veio.
> > - **Nota (28/09) — Opção A aplicada:** `TargetArea` = nome de um `Marker2D` no grupo `SpawnPoints` da cena de destino; `SpawnOffset` soma na posição. `SceneManager.LoadSceneAtSpawn()` guarda o destino, e o `PlayerController` consome no `_Ready` (deferred) e reseta a câmera. Markers criados: `SpawnFromBossArena` (TestLevel, perto do portal) e `SpawnFromTestLevel` (BossArena).
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot. Testável indo e voltando entre `TestLevel` e `BossArena`.

> [!info]- EPIC-C09 — Save, Checkpoint e Respawn — 🔄 Em andamento · 🟡 Média
> *Pronto quando*: uma habilidade desbloqueada persiste em disco mesmo sem passar por um checkpoint depois, e existe uma decisão explícita (implementada ou documentada como "fora de escopo") sobre o fluxo de Continuar.
>
> > [!todo]- Task C09-T0 — Regressão básica (antes de mexer)
> > - [ ] Rodar a Task C09-T0 do [[ROADMAP_QA_CODIGO]] (ativar checkpoint, salvar, morrer/respawnar).
> > - **Bugfix CRÍTICO (28/09) — E não ativava checkpoint:** `TryInteract` procurava o `IInteractable` no *pai* da área sobreposta, mas o `Checkpoint` é a própria `Area2D`. Agora testa a área e depois o pai. A auditoria de 15/09 marcou esse caso como ✅ sem rodar.
> > - **Bugfix (28/09) — respawn no checkpoint errado:** `OnDied` pegava o primeiro nó do grupo `Checkpoints`, ativado ou não. Agora o `PlayerController` guarda a posição do último `CheckpointActivated` e, se não houver, respawna perto de onde morreu.
> >
> > Status: não iniciado — 🟢 liberado. Dois bugs da baseline foram corrigidos no código (ver notas); rodar de novo.
>
> > [!info]- Task C09-T1 — Persistir unlock de habilidade imediatamente
> > - [x] Hoje só atualiza a lista em memória até o próximo save por checkpoint/boss/item — garantir `SaveGame()` também no unlock, ou documentar a decisão de não fazer isso pra demo.
> > - **Nota (28/09):** `SaveManager.OnAbilityUnlocked` chama `SaveGame()` na hora (cria o save do slot 0 se ainda não existir). `ApplySaveData` passou a restaurar as habilidades sem emitir `AbilityUnlocked`, pra carregar um save não disparar notificação nem um save extra.
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot.
>
> > [!failure]- Task C09-T2 — Fluxo de "Continuar"
> > - [ ] Se a demo tiver esse menu: chamar `SaveManager.LoadGame(slot)` e, em `ApplySaveData`, trocar de cena pra `save.CurrentScene` antes de restaurar posição/HP (hoje não troca).
> > - [ ] Se a demo **não** vai ter esse menu (sempre começa do zero): documentar essa decisão em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].
> >
> > Status: não iniciado — ⛔ bloqueado: decisão pendente (a demo terá menu Continuar?).

> [!todo]- EPIC-C10 — HUD e UX — ⬜ Não iniciado · 🟡 Média
> *Pronto quando*: um prompt visual aparece ao chegar perto de um `IInteractable` (sem precisar apertar nada), e a decisão sobre Game Over foi tomada e implementada.
>
> > [!todo]- Task C10-T0 — Regressão básica de HUD
> > - [ ] Rodar a Task C10-T0 do [[ROADMAP_QA_CODIGO]] (barra de vida, pausa).
> >
> > Status: não iniciado — 🟢 liberado.
>
> > [!todo]- Task C10-T1 — Prompt de interação contínuo
> > - [ ] Fazer o `PlayerController` (ou um `InteractionDetector` dedicado) chamar `HUDController.ShowInteractionPrompt()`/`HideInteractionPrompt()` continuamente com base em estar ou não sobrepondo um `IInteractable` — hoje só reage ao apertar E.
> >
> > Status: não iniciado — 🟢 liberado (movimento unificado).
>
> > [!failure]- Task C10-T2 — Game Over
> > - [ ] Decidir se a demo precisa de tela de Game Over real ou se o respawn automático (já funcional) basta. Se precisar, implementar o que `ShowGameOver()` deveria de fato disparar.
> >
> > Status: não iniciado — ⛔ bloqueado: decisão pendente (tela de Game Over ou só respawn?).

> [!info]- EPIC-C11 — Áudio — 🔄 Em andamento · 🟡 Média
> *Pronto quando*: as pastas/buses de áudio existem, os 3 sons já esperados pelo código tocam de verdade, e a demo inteira tem pelo menos uma música por bioma e feedback sonoro nos hits principais.
>
> Responsabilidade do Gustavo.
>
> > [!info]- Task C11-T1 — Estrutura de pastas e buses
> > - [x] Criar `res://assets/audio/music/` e `res://assets/audio/sfx/`.
> > - [x] Configurar os buses "Music" e "SFX" no Godot (Audio → Buses) — o código já assume que existem.
> > - **Nota (28/09):** pastas criadas com `.gitkeep`. Buses em `res://default_bus_layout.tres` (Master → Music, SFX). Conferir em Audio → Buses no editor.
> >
> > Status: implementado (28/09), compila — ⏳ aguarda rodar o QA no Godot.
>
> > [!failure]- Task C11-T2 — Arquivos já esperados pelo código
> > - [ ] Produzir/conseguir os 3 arquivos já chamados no código (nomes exatos, case-sensitive): `checkpoint_activate.wav`, `boss_stomp.wav`, `boss_defeat.wav`.
> >
> > Status: não iniciado — ⛔ bloqueado: arquivos de áudio ainda não existem. Lista completa esperada pelo código agora (em `sfx/`): `checkpoint_activate.wav`, `boss_stomp.wav`, `boss_defeat.wav`, `player_attack.wav`, `player_jump.wav`, `player_hurt.wav`, `enemy_hit.wav`, `enemy_death.wav`.
>
> > [!warning]- Task C11-T3 — Chamadas que faltam
> > - [x] Adicionar `AudioManager.PlaySfx(...)` pro ataque do jogador, hit em inimigo/jogador, morte de inimigo comum, pulo (opcional).
> > - [ ] Música de fundo por bioma via `AudioManager.PlayMusic(...)` (fade in/out já pronto no código).
> > - **Nota (28/09):** `PlaySfx` no ataque, pulo e dano do Kairo e no hit/morte de inimigo comum. `AreaController` ganhou `MusicTrack` (exportado, vazio por padrão), tocado no `_Ready` via `PlayMusic`. O `AudioManager` passou a logar arquivo faltando uma vez só por caminho.
> >
> > Status: parcial (28/09) — chamadas de SFX implementadas e gancho de música pronto; som real aguarda C11-T2.

> [!todo]- EPIC-C12 — Regressão Final / Release da Demo — ⬜ Não iniciado · 🔴 Bloqueante
> *Pronto quando*: o [[ROADMAP_QA_CODIGO]] inteiro passa, o [[ROADMAP_QA_INTEGRACAO]] inteiro passa, e a demo roda de ponta a ponta sem crash na frente de alguém de fora do grupo.
>
> Só começa depois que todos os épicos 🔴/🟠 acima estiverem concluídos.
>
> > [!failure]- Task C12-T1 — Regressão completa de código
> > - [ ] Rodar o [[ROADMAP_QA_CODIGO]] inteiro, de ponta a ponta, e corrigir qualquer regressão.
> >
> > Status: não iniciado — ⛔ bloqueado: só depois dos épicos 🔴/🟠.
>
> > [!failure]- Task C12-T2 — Handoff pro Higor
> > - [ ] Entregar os blocos de mecânica que faltavam pra ele terminar a arte condicional (ex: como fica visualmente o wall grab, pra desenhar a animação certa).
> >
> > Status: não iniciado — ⛔ bloqueado: só depois dos épicos 🔴/🟠.
>
> > [!warning]- Task C12-T3 — Integração incremental
> > - [ ] Rodar o [[ROADMAP_QA_INTEGRACAO]] junto com o Higor assim que a arte de cada bloco chegar — não esperar tudo pronto pra testar.
> >
> > Status: não iniciado — 🟡 pode começar por bloco: Kairo e Korrag já têm arte no repo.
