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
epicos_concluidos: 8
tasks_total: 44
tasks_prontas: 37
tasks_bloqueadas: 5
tasks_liberadas: 0
---
# Roadmap de Desenvolvimento — Código (Gustavo)

Backlog de execução da demo: Épicos (fases, ordenados pela dependência real entre as partes — fundação técnica antes de qualquer mecânica nova, restauração ODS15 e boss logo depois porque são os gaps de maior prioridade) que abrem em Tasks, que abrem em Sub-tasks. Cada sub-task é um checkbox — marcar conforme for implementado. Sempre que uma Task gerar uma nota de desenvolvimento (bugfix, decisão, mudança de escopo) durante o trabalho, ela é registrada ali mesmo, no formato **Tipo (DD/MM) — resumo**, logo abaixo do checklist da Task. Se a nota for uma decisão de design (não só um detalhe técnico de implementação), ela também é registrada no GDD do Obsidian: [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (o quê/porquê) e uma linha no [[Projetos/Reflorescer/🗓️ Log de Atualizações|🗓️ Log de Atualizações]] (quando). A ordem dos épicos é uma sugestão, ajustável a qualquer momento já que o código é do Gustavo.

Cada Épico aqui tem um Épico **gêmeo de mesmo ID** no [[ROADMAP_QA_CODIGO]] (ex: `EPIC-C03`), com os casos de teste que definem "pronto" de cada Task. Fluxo: implementar as sub-tasks de uma Task → rodar as sub-tasks de verificação da Task equivalente no QA → escrever a linha `Status:` → seguir pra próxima.

Marcação de disponibilidade na linha `Status:` de cada Task (avaliação de 28/09, após o pull do commit `6153f13`): 🟢 liberado — dá pra começar agora · 🟡 aguarda — depende de outra Task deste roadmap · ⛔ bloqueado — depende de decisão de design, arte/cena ou áudio fora deste roadmap.

Ordem recomendada: `EPIC-C01` → `EPIC-C02` → `EPIC-C06` → `EPIC-C04` → `EPIC-C03` → `EPIC-C05` → `EPIC-C07` → `EPIC-C08` → `EPIC-C09` → `EPIC-C10` → `EPIC-C11` → `EPIC-C12`. C03, C05 e C07 não têm dependência forte entre si — dá pra reordenar ou paralelizar, desde que C01 já esteja fechado.

---

> [!success]- EPIC-C01 — Fundação Técnica — ✅ Concluído · 🔴 Bloqueante
> *Pronto quando*: o Input Map bate 100% com os controles de [[🕹️ Sistemas de Jogo#Movimentação|Sistemas de Jogo]], só existe um sistema de movimento rodando (a `PlayerStateMachine`/`PlayerStates.cs` antiga foi removida ou virou a única fonte), e o double jump tem uma única fonte de verdade.
>
> Bloqueante — fazer antes de tudo. O Input Map real não bate com a documentação confirmada, e existem dois sistemas de movimento rodando em paralelo (`PlayerController.HandlePrototypeMovement` + `PlayerStateMachine`/`PlayerStates.cs`) — cada mecânica nova escrita em cima disso dobra o risco de bug.
>
> > [!success]- Task C01-T1 — Corrigir o Input Map
> > - [x] Remapear em Project Settings → Input Map pra bater com os controles de [[🕹️ Sistemas de Jogo#Movimentação|Sistemas de Jogo]]: A/D mover, Espaço pular, Shift dash, Q espada, E interagir, K ataque básico, segurar J especial.
> > - [x] Grep em `scripts/` por `IsActionJustPressed`/`IsActionPressed` com os nomes antigos de ação e corrigir qualquer referência que sobrar.
> > - **Achado da auditoria (15/09):** hoje `attack`=J+clique, `dash`=K+seta-baixo, `jump`=Espaço/W/seta-cima, `ability`=L (sem uso). Sem S nem Q mapeados.
> > - **Nota (28/09) — mapeamento aplicado:** `move_left`=A, `move_right`=D, `jump`=Espaço, `dash`=Shift, `interact`=E, `attack`=K, `toggle_sword`=Q (nova), `special`=J (nova, segurar), `pause`=Esc. Saíram as teclas extras (setas, W, clique, Ctrl) e a ação `ability` (L, sem uso). Nenhum script lia `ability`; `toggle_sword` e `special` ainda não têm consumidor (C03-T1 e futuro especial).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C01-T2 — Unificar o sistema de movimento
> > - [x] Decidir: manter o caminho direto (`HandlePrototypeMovement`, já testado) e apagar `PlayerStateMachine`/`PlayerStates.cs`, **ou** migrar de vez pra state machine e remover o caminho direto.
> > - [x] Executar a remoção/migração escolhida.
> > - [x] Rodar a Task C01-T2 do [[ROADMAP_QA_CODIGO]] (regressão completa de movimento e física) antes de seguir.
> > - **Recomendação:** manter o caminho direto — é o de menor risco a essa altura do projeto.
> > - **Decisão do Gustavo (28/09):** mantido o caminho direto (`HandlePrototypeMovement`). Removidos `PlayerStateMachine.cs`, `PlayerStates.cs` (+ `.uid`), o nó `PlayerStateMachine` do `Player.tscn` e o `ApplyHorizontalMovement` (só os estados usavam). Animações `hurt`/`dead` passaram pro `PlayerController` (`_hurtTimer` e `OnDied`).
> > - **Bugfix CRÍTICO (28/09) — física rodava 2x por frame:** além do ataque duplicado, cada estado antigo chamava `ApplyGravity` + `MoveAndSlide`, então o Kairo andava/caía com o dobro de deslocamento e gravidade por frame. Na prática, o jogo testado até hoje rodava ~2x mais rápido que os valores do `PlayerStatsResource`. Depois da remoção os valores documentados (200px/s, -460, etc.) passam a ser reais, e o movimento vai parecer **mais lento**. Pra recuperar a sensação antiga: velocidades ×2 e gravidade/acelerações ×4 no `.tres` — decisão de tuning, não aplicada.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C01-T3 — Fonte única de verdade pro Double Jump
> > - [x] Escolher uma fonte entre `PlayerController.InitDoubleJump()` (fallback hardcoded) e `AbilityManager.UnlockPrototypeAbilities()` — recomendo `AbilityManager`, é o sistema que vai crescer com o Dash.
> > - [x] Remover o fallback duplicado do `PlayerController`.
> > - **Nota (28/09):** fonte única = `AbilityManager`. O `PlayerController` consulta `AbilityManager.Instance.HasAbility(DoubleJump)` na hora do pulo (sem cache, sem fallback). O `AbilityManager` já nasce com `DoubleJump` no conjunto, sem emitir `AbilityUnlocked` (não é um desbloqueio de gameplay, então não dispara notificação nem save).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).

> [!info]- EPIC-C02 — Mecânica de Restauração ODS15 — 🔄 Em andamento · 🔴 Bloqueante
> *Pronto quando*: existe pelo menos 1 ponto de restauração interagível em cada uma das 3 áreas da demo, com efeito visual próprio, evento dedicado no `EventBus`, e persistência em save.
>
> Prioridade máxima de conteúdo — sem isso a demo perde a conexão com o pitch (ODS 15). Hoje `Checkpoint` é só um save point genérico, sem nenhum conceito de "restaurar" o bioma.
>
> > [!success]- Task C02-T1 — Definir e implementar a interação
> > - [x] Decidir se a restauração é o próprio `Checkpoint` reaproveitado (com um estado temático a mais) ou uma classe nova (`RestorationPoint : IInteractable`) separada do save point.
> > - [x] Implementar a interação escolhida.
> > - **Decisão do Gustavo (28/09) — classe separada:** `RestorationPoint : Area2D, IInteractable` (`scripts/world/RestorationPoint.cs`), independente do `Checkpoint`. Restaurar não é salvar, então cada um tem evento e persistência próprios. Interage uma vez (`Restored`), prompt "Restaurar". Os ganchos de C02-T2 (efeito) e C02-T3 (evento/save) estão marcados no `Interact()`. **Registrar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].**
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C02-T2 — Efeito visual/de mundo
> > - [x] Implementar o efeito da restauração (mesmo simples pra demo: partícula + mudança de cor no tile ao redor, ou sprite "antes/depois").
> > - **Nota (28/09) — antes/depois data-driven:** o `RestorationPoint` ganhou `DegradedVisuals` e `RestoredVisuals` (listas de `NodePath`). Ao restaurar, os degradados somem em fade (0.6s), os restaurados brotam de baixo pra cima, e sai um burst de `CpuParticles2D` verde (`ParticleColor`). No `TestLevel`, placeholder: chão seco + toco → grama verde + broto. Quando a arte chegar, basta apontar as listas pros sprites do Higor no editor, sem mexer no código. SFX esperado: `restoration.wav`.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C02-T3 — Evento dedicado e persistência
> > - [x] Emitir um evento próprio no `EventBus` (ex: `AreaRestored(string pointId)`) — não reaproveitar `CheckpointActivated`.
> > - [x] Persistir pontos restaurados no save, igual `ActivatedCheckpoints` já persiste.
> > - **Nota (28/09):** novo sinal `EventBus.AreaRestored(string pointId)`. O `SaveManager` guarda `RestoredPoints` no `SaveData` e salva na hora (igual ao unlock de habilidade). O `ApplySaveData` restaura os pontos sem efeito nem evento. Cada `RestorationPoint` consulta `SaveManager.IsPointRestored()` no `_Ready`, então sair pro `BossArena` e voltar mantém o ponto restaurado.
> > - **Pendência (28/09) — reabrir o jogo:** o estado vai pro `save_0.json`, mas hoje nenhum fluxo carrega o save ao abrir o jogo. Isso é a decisão da C09-T2 (menu Continuar). Até ela, "fechar e reabrir mantém restaurado" não tem como passar; o que dá pra verificar é o arquivo `user://save_0.json` conter `RestoredPoints`.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!failure]- Task C02-T4 — Posicionar nas 3 áreas da demo
> > - [ ] Definir quantos pontos por área (mínimo 1 por bioma) e posicionar nas cenas de Floresta Tropical, Deserto e Tundra.
> >
> > Status: não iniciado — ⛔ bloqueado: as cenas de Floresta Tropical, Deserto e Tundra não existem (só `TestLevel` e `BossArena`) — depende do [[ROADMAP_ARTE]] `EPIC-A05`.

> [!success]- EPIC-C03 — Combate e Espada de Grama — ✅ Concluído · 🟠 Alta
> *Pronto quando*: Q alterna a espada em tempo real (visual + `HasSword`), o jogador tem stats diferentes com a espada ativada/desativada, e não dá pra atacar com ela desativada.
>
> > [!success]- Task C03-T0 — Regressão de combate básico (antes de mexer)
> > - [x] Rodar a Task C03-T0 do [[ROADMAP_QA_CODIGO]] (ataque, hitbox/hurtbox, cooldown, invulnerabilidade) pra confirmar a base antes de alterar.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C03-T1 — Wire do input Q (ativar/desativar)
> > - [x] Ligar a ação `Q` (pós `EPIC-C01-T1`) a um método novo `ToggleSword()` em `PlayerController`, chamando `HasSword = !HasSword` e `SetSwordVisible(HasSword)`.
> > - **Achado (28/09, pós-pull) — sprite do Kairo sem variante de espada:** os `ColorRect` placeholder da espada (`Blade`/`Handle`) foram escondidos no commit `6153f13`, e o sprite novo (`KairoFaiscaSpriteFrames.tres`) não tem espada visível nem variante com e sem espada. O toggle de lógica funciona, mas o visual precisa de arte nova (ou de uma camada separada da espada). Também: a Faísca está desenhada dentro do sprite do Kairo e duplica com o `Faisca.tscn` — levar pro Higor ([[ROADMAP_ARTE]] `EPIC-A01`).
> > - **Nota (28/09):** `PlayerController.ToggleSword()` na ação `toggle_sword` (Q): inverte `HasSword`, chama `SetSwordVisible`, emite o novo sinal `EventBus.SwordToggled(bool)` e toca `sword_on.wav`/`sword_off.wav`. O estado fica em `GameManager.HasSword` (autoload), porque o Player é recriado a cada troca de cena e sem isso a espada "voltava" ao entrar na arena.
> > - **UX (28/09) — indicador provisório:** até existir arte com/sem espada, o HUD mostra "Espada: ativada [Q]" / "Espada: guardada [Q]" (no `AbilityDisplay` que já existia) e o sprite do Kairo ganha um tom verde claro com a espada guardada (`SelfModulate`, pra não conflitar com os flashes de ataque/dano). Remover o tom quando a arte do Higor chegar (`ROADMAP_ARTE` A01-T2).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C03-T2 — Stats duplos (ativada/desativada)
> > - [x] Criar a variante "espada desativada": +velocidade, +pulo, sem ataque.
> > - **Opção simples:** dois blocos de valores no `PlayerStatsResource` (`MoveSpeedSwordOn`/`Off`, etc.) ou um segundo `.tres`.
> > - **Nota (28/09):** opção simples aplicada: `MoveSpeedNoSword` (240) e `JumpVelocityNoSword` (-520) no `PlayerStatsResource`. O `PlayerController` usa `CurrentMoveSpeed`/`CurrentJumpVelocity`, que leem `HasSword`. O "sem ataque" já vem do `CanAttack()`. **Os valores são chute (+20% e +13%): decidir os definitivos e registrar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].**
> > - **Nota (28/09):** testável agora: com a espada guardada, `CurrentMoveSpeed` = 240 e `CurrentJumpVelocity` = -520; ativada, 200 e -460 (conferido no Godot).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C03-T3 — Bloqueio e teste em combate real
> > - [x] Bloquear `CanAttack()` quando `HasSword == false` (parcialmente já verdade hoje).
> > - [x] Testar a troca em pleno combate/movimento — sem travar animação/input no meio da troca.
> > - [x] Rodar de novo a Task C03-T0 pra garantir que nada regrediu.
> > - **Nota (28/09):** o bloqueio já existia (`CanAttack()` exige `HasSword`). Trocar a espada não cancela um golpe em andamento: ele termina a janela de 0.14s normalmente, sem travar animação nem input (verificado no Godot: ataque + Q no mesmo frame, sem erro).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).

> [!success]- EPIC-C04 — Dash e Desbloqueio Pós-Korrag — ✅ Concluído · 🔴 Bloqueante
> *Pronto quando*: derrotar o Korrag desbloqueia o dash uma única vez, e apertar Shift com o dash desbloqueado move o Kairo com um impulso rápido na direção que ele olha.
>
> Depende de `EPIC-C01` (Input Map correto).
>
> > [!success]- Task C04-T1 — Corrigir o evento BossDefeated duplicado
> > - [x] `BossBase.OnDied()` chama `EmitDefeated()` diretamente **e** `BossDeadState.Enter()` chama de novo — remover uma das duas (sugestão: manter só a de `BossDeadState.Enter()`).
> > - **Fazer antes da Task C04-T3**, senão o dash pode ser "desbloqueado" duas vezes sem causar bug visível, mas é sujeira desnecessária.
> > - Mesma correção referenciada em `EPIC-C06-T2` — fazer uma vez só.
> > - **Nota (28/09):** removido o `EmitDefeated()` de `BossBase.OnDied()`. A fonte única agora é `BossDeadState.Enter()`.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C04-T2 — Implementar o movimento do dash
> > - [x] Impulso horizontal rápido na direção que o Kairo está olhando, com cooldown e talvez i-frames curtos (típico de Metroidvania).
> > - **Nota (28/09):** `PlayerController.StartDash()`: impulso horizontal na direção em que o Kairo olha, sem gravidade nem input durante o impulso. Termina no fim da duração ou ao bater na parede e sai com a velocidade de corrida. Parâmetros no `PlayerStatsResource`: `DashSpeed` 520, `DashDuration` 0.16s (≈83px), `DashCooldown` 0.6s. I-frames só durante o impulso, sem encurtar uma invulnerabilidade pós-dano que já esteja rodando. Anima com `dash` se o `SpriteFrames` tiver (A01-T4); senão usa `run` com um tint esverdeado. SFX esperado: `player_dash.wav`.
> > - **Decisão do Gustavo (28/09) — dash só no chão:** o enum já separa `Dash` de `AirDash`, então o dash do Korrag funciona só com o Kairo no chão; o dash aéreo fica pra uma habilidade futura. **Registrar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].**
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C04-T3 — Ligar ao AbilityManager e ao evento do boss
> > - [x] Input: `Input.IsActionJustPressed("dash") && AbilityManager.Instance.HasAbility(AbilityId.Dash)`.
> > - [x] Assinar `EventBus.BossDefeated` (sugestão: no próprio `AbilityManager`, ou um `DemoProgressionManager` novo) e chamar `UnlockAbility(AbilityId.Dash)` quando `bossId == "boss_javali"`.
> > - **Nota (28/09):** o próprio `AbilityManager` escuta `EventBus.BossDefeated` e chama `UnlockAbility(Dash)` quando `bossId == "boss_javali"`. O `UnlockAbility` já ignora repetição. O unlock dispara em cadeia a notificação do HUD (C04-T4) e o save imediato (C09-T1): as duas ficam testáveis a partir daqui.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C04-T4 — Notificação de habilidade desbloqueada
> > - [x] Dar um retorno visual mínimo (texto na tela por 2-3s) — hoje `HUDController.ShowAbilityUnlockNotification` só imprime no console.
> > - **Nota (28/09):** `HUDController` cria um `Label` centralizado no topo ("Nova habilidade: X"), 2.5s na tela e 0.4s de fade. A `BossArena` não tinha HUD, então ganhou uma instância de `HUD.tscn`: é lá que o dash vai ser desbloqueado.
> > - **Bugfix CRÍTICO (28/09) — notificação não aparecia vindo do TestLevel:** reproduzido no Godot (headless). O HUD e a Faísca do `TestLevel` se inscreviam no `EventBus` (autoload, sobrevive à troca de cena) e nunca se desinscreviam. Na `BossArena`, o handler do HUD já destruído lançava `ObjectDisposedException`, e a exceção interrompia a cadeia do evento antes de chegar ao HUD novo. O mesmo quebrava a barra de vida do Kairo na arena (`PlayerHealthChanged`). Correção: `_ExitTree()` desinscrevendo tudo em `HUDController` e `FaiscaController`. **Regra daqui pra frente:** todo nó de cena que fizer `EventBus.Instance.X += ...` precisa do `-=` no `_ExitTree()`. Só autoloads (`AbilityManager`, `SaveManager`) ficam dispensados.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo), depois do bugfix do EventBus.

> [!success]- EPIC-C05 — Wall Grab / Wall Jump — ✅ Concluído · 🟠 Alta
> *Pronto quando*: segurar A/D contra uma parede no ar reduz a queda, e pular nessa condição empurra o Kairo pro lado oposto.
>
> > [!success]- Task C05-T1 — Detecção de parede
> > - [x] Detectar colisão lateral com parede no ar (`IsOnWall()` do Godot ou raycast lateral) em `PlayerController`.
> > - **Nota (28/09):** `UpdateWallSlide()` usa o `IsOnWall()`/`GetWallNormal()` do último `MoveAndSlide`. O corpo do Kairo só colide com a camada World, então inimigo não conta como parede. "Agarrado" = no ar + encostado + segurando A/D na direção da parede.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C05-T2 — Agarrar / slide
> > - [x] Enquanto segurando A/D contra a parede no ar, reduzir a velocidade de queda (não necessariamente travar em 0).
> > - **Nota (28/09):** agarrado, a queda fica limitada a `WallSlideSpeed` (90 px/s, contra ~650 em queda livre). Agarrar devolve o pulo duplo. Anima com `wall_slide` se existir no `SpriteFrames` (A01-T3); senão usa `fall`.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C05-T3 — Pulo de parede
> > - [x] Pular estando agarrado empurra pro lado oposto da parede, impulso vertical semelhante ao pulo normal.
> > - **Nota (28/09):** o pulo de parede usa o mesmo impulso vertical do pulo normal (respeita os stats com/sem espada) + `WallJumpHorizontalSpeed` (260) pro lado oposto. O input horizontal fica travado por `WallJumpInputLock` (0.15s), senão segurar a direção da parede anularia o empurrão. Há um "wall coyote" de 0.1s pra pular logo depois de soltar a parede. Verificado no Godot: com A ainda segurado, o pulo sai com velocidade (+260, -444) e afasta da parede.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C05-T4 — Decisão de gating
> > - [x] Decidir se é liberado desde o início ou gated por uma `AbilityId` (`WallJump` já existe no enum) — **pendência a decidir com o Higor/documentar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]]** antes de travar o design final.
> > - **Decisão do Gustavo (28/09) — desde o início, deslizando devagar:** wall grab/wall jump ficam disponíveis desde o começo da demo, como já dizia a tabela de controles do [[🕹️ Sistemas de Jogo#Movimentação|Sistemas de Jogo]] (sem condição de desbloqueio; o dash continua sendo a recompensa do Korrag). Continua passando pelo `AbilityManager` (`WallJump` entra no conjunto inicial, igual ao `DoubleJump`): gatear no futuro é tirar uma linha. O agarrar é "deslizar devagar", não "grudar parado", como está no GDD. **Registrar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].**
> >
> > Status: ✅ concluído — decisão tomada em 28/09 (Gustavo).

> [!success]- EPIC-C06 — Boss Korrag (IA e Identidade) — ✅ Concluído · 🔴 Bloqueante
> *Pronto quando*: o Korrag usa Charge e Stomp em algum momento da luta (não só o ataque genérico), o `BossName` no código é "Korrag, o Javali", e o evento de derrota dispara uma única vez.
>
> **Achado da auditoria:** a troca de fase por HP já funciona sozinha (`BossStateMachine`). O trabalho real é só **conectar os ataques que já existem** (`StartCharge`/`PerformStomp` em `JavaliBoss.cs`) — hoje nenhum estado os chama.
>
> > [!success]- Task C06-T0 — Regressão do núcleo do boss (antes de mexer)
> > - [x] Rodar a Task C06-T0 do [[ROADMAP_QA_CODIGO]] (ativação, intro, fases automáticas, contato, morte em 8 hits).
> > - **UX (28/09) — Korrag "morria antes do golpe final":** o dano estava certo (morte no 8º golpe, conferido no Godot), mas o `OnDied()` escondia o boss no mesmo frame do golpe final, sem animação. Agora a colisão e o dano são desligados na hora (via `SetDeferred`, porque a morte vem de callback de física), a barra flutuante some, toca `dead`, e ele desaparece em fade (0.6s de espera + 0.6s de fade) antes do `QueueFree`.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo); morte com animação e fade revalidada no mesmo dia.
>
> > [!success]- Task C06-T1 — Loop de decisão de ataque
> > - [x] Em `BossPhase1State`/`BossPhase2State`/`BossEnragedState`, trocar a chamada genérica `Boss.PerformAttack()` por uma decisão entre `PerformAttack()` (melee), `StartCharge()` (investida) e `PerformStomp()` (pisada em área).
> > - **Sugestão de implementação:** método virtual `ChooseAttack()` em `BossBase`, sobrescrito por `JavaliBoss` — pode começar simples (sorteio ponderado ou por distância).
> > - **Nota (28/09) — implementação:** `BossBase.ChooseAttack()` virtual, sobrescrito em `JavaliBoss`. Stomp (35%, 50% no enraged) quando o jogador está a até `StompRadius`; Charge (60% na fase 1, 80% depois) a partir de `ChargeMinDistance` (180px); no meio-termo, 40% Charge e o resto melee. O Charge agora tem fim (`ChargeDuration` 0.9s ou bater na parede) e aplica `ChargeDamage`. O Stomp ganhou windup de 0.4s. O `MoveAndSlide` saiu do `ExecuteCharge` (o estado já chama). O melee vira a hitbox pro lado do jogador (antes só acertava à direita) e desliga depois de 0.25s.
> > - **Decisão do Gustavo (28/09) — super armor e timers:** o boss tem `InvulnerabilityDuration = 0` e todo golpe o jogava em `Hurt`, o que zerava o timer de ataque e cancelava o windup do Charge. Na prática, batendo sem parar, ele nunca atacava. Agora: (1) durante Charge/Stomp (`IsBusy`) o dano entra, mas não interrompe nem empurra; (2) o timer de ataque sobrevive às idas ao `Hurt`; (3) as transições de fase 2/enraged tocam uma vez só (antes repetiam a cada hit). **Registrar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].**
> > - **Achado (28/09), não corrigido:** o enraged chama `SetPhase(2)`, mas `JavaliStatsResource` só tem 2 fases (índices 0 e 1), então no enraged o boss cai nos defaults (cooldown 1.5, velocidade 100, dano 20). Fica mais fraco que na fase 2. Precisa de um `JavaliPhase3.tres` ou de ajuste no índice.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C06-T2 — Corrigir o evento BossDefeated duplicado
> > - [x] Mesma correção da `EPIC-C04-T1` — fazer uma vez só, referenciar aqui.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C06-T3 — Corrigir o nome oficial
> > - [x] Atualizar `BossName` em `resources/bosses/JavaliStatsResource.tres` de `"Javali das Ruínas"` para `"Korrag, o Javali"`.
> > - **Nota (28/09):** trocado no `.tres` e no fallback hardcoded do `JavaliBoss._Ready()`.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C06-T4 — Ajustar timing com a arte real
> > - [ ] Testar o `charge_windup` (1s) contra a animação real assim que a arte chegar (cruza com [[ROADMAP_QA_INTEGRACAO]] `EPIC-A04`).
> > - **Nota (28/09, pós-pull):** `KorragSpriteFrames.tres` já tem `charge_windup` (1 frame, 5 fps) e todos os nomes de animação chamados em `BossStateMachine.cs`/`JavaliBoss.cs` existem no recurso (`walk` existe mas nenhum script chama ainda).
> > - **Nota (28/09) — arte vs. timing:** a `charge_windup` do Higor tem 1 quadro (5 fps, sem loop), então o Korrag ficava congelado o segundo inteiro do windup. Mantido o `ChargeWindup` de 1s (tempo justo pra reagir) e adicionada telegrafia: pulso vermelho no `SelfModulate` do sprite (0.12s por meia-volta) durante o windup, desligado quando a investida começa, quando ela para ou quando o boss morre. Se 1s parecer lento, é só baixar `ChargeWindup` no inspetor do `Boss_Javali`. Se o Higor fizer um windup com mais quadros, o pulso pode sair. Cruza com [[ROADMAP_QA_INTEGRACAO]] A04.
> > - **Decisão do Gustavo (28/09):** `ChargeWindup` de 1s aprovado com o pulso vermelho de aviso.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo). Tempo de 1s aprovado.

> [!info]- EPIC-C07 — Inimigos Comuns (Arquétipos) — 🔄 Em andamento · 🟠 Alta
> *Pronto quando*: existem 3 arquétipos (Voador/Rápido/Robusto) com comportamento distinto de fato, cada um com `.tres` de stats pra cada uma das 3 áreas da demo.
>
> > [!success]- Task C07-T0 — Regressão do comportamento base (antes de mexer)
> > - [x] Rodar a Task C07-T0 do [[ROADMAP_QA_CODIGO]] (idle/patrol/detect/chase/attack/hurt/dead do `EnemyBase` genérico).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C07-T1 — Voador
> > - [x] Criar `EnemyVoador : EnemyBase` — sobrescrever gravidade/colisão pra voar (ignorar `ApplyGravity` ou usar versão sem gravidade, `Y` fixo/oscilante).
> > - **Nota (28/09):** `EnemyVoador : EnemyBase` (`MotionMode = Floating`). Sobrescreve `ApplyGravity` (flutua oscilando em Y), `ChasePlayer` (persegue nos dois eixos) e `AtPatrolEdge` (sempre `false`). No `EnemyBase`, `ApplyGravity`, `ChasePlayer` e `AtPatrolEdge` viraram `virtual`. Cena herdada `Enemy_Voador.tscn` + `EnemyVoadorStats.tres`.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C07-T2 — Rápido
> > - [x] Avaliar se basta um `.tres` novo (`ChaseSpeed`/`PatrolSpeed` mais altos, já 100% data-driven) ou se precisa de subclasse de código.
> > - **Nota (28/09) — avaliação:** basta `.tres`, sem subclasse. `EnemyRapidoStats.tres` (Chase 190, Patrol 90, HP 50, cooldown 0.6) + cena herdada `Enemy_Rapido.tscn`.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C07-T3 — Robusto
> > - [x] `.tres` com `MaxHealth`/`KnockbackResistance` mais altos; avaliar se precisa de código extra (ex: ignorar knockback abaixo de um threshold).
> > - **Nota (28/09) — avaliação:** sem subclasse. Um campo novo no `EnemyStatsResource`, `InterruptOnHit` (`false` = não entra em `Hurt` ao tomar dano), mantém tudo data-driven. `EnemyRobustoStats.tres` (HP 150, KnockbackResistance 0.9, `InterruptOnHit = false`) + cena herdada `Enemy_Robusto.tscn`. **Os valores dos 3 arquétipos são chute: validar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].**
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!failure]- Task C07-T4 — Dados por arquétipo × área da demo
> > - [ ] Criar os `.tres` de `EnemyStatsResource` pra cada arquétipo × cada uma das 3 áreas (Floresta Tropical primeiro).
> >
> > Status: não iniciado — ⛔ bloqueado: os 3 arquétipos existem (C07-T1/T2/T3); falta definir os valores por área em [[👾 Bestiário]] (os .tres não dependem das cenas existirem).
>
> > [!success]- Task C07-T5 — (baixa prioridade) AtPatrolEdge()
> > - [x] Hoje sempre retorna `false` — corrigir só se o comportamento "parar depois de patrulhar" for desejado. Aceitável pra demo como está.
> > - **Nota (28/09):** `AtPatrolEdge()` faz um raycast pra baixo 20px à frente (máscara World). No `EnemyPatrolState`, na beira o inimigo vira (`Flip`) e entra em `Idle`, retomando a patrulha no sentido oposto.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).

> [!success]- EPIC-C08 — Transição de Área — ✅ Concluído · 🟠 Alta
> *Pronto quando*: o jogador chega na cena nova numa posição consistente com de onde veio, não sempre no spawn padrão.
>
> > [!success]- Task C08-T0 — Regressão básica do trigger
> > - [x] Rodar a Task C08-T0 do [[ROADMAP_QA_CODIGO]] (trigger carrega a cena, erro tratado se a cena não existe).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C08-T1 — Spawn direcional
> > - [x] **Opção A:** implementar o uso de `TargetArea`/`SpawnOffset` (já existem exportados em `TransitionTrigger`, não são lidos) pra posicionar o jogador no spawn correspondente.
> > - [ ] **Opção B** (mais simples se o tempo apertar): nomear spawn points por origem (`SpawnFromFlorestaTropical`, `SpawnFromDeserto`) e escolher pelo nome de onde o jogador veio.
> > - **Nota (28/09) — Opção A aplicada:** `TargetArea` = nome de um `Marker2D` no grupo `SpawnPoints` da cena de destino; `SpawnOffset` soma na posição. `SceneManager.LoadSceneAtSpawn()` guarda o destino, e o `PlayerController` consome no `_Ready` (deferred) e reseta a câmera. Markers criados: `SpawnFromBossArena` (TestLevel, perto do portal) e `SpawnFromTestLevel` (BossArena).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).

> [!success]- EPIC-C09 — Save, Checkpoint e Respawn — ✅ Concluído · 🟡 Média
> *Pronto quando*: uma habilidade desbloqueada persiste em disco mesmo sem passar por um checkpoint depois, e existe uma decisão explícita (implementada ou documentada como "fora de escopo") sobre o fluxo de Continuar.
>
> > [!success]- Task C09-T0 — Regressão básica (antes de mexer)
> > - [x] Rodar a Task C09-T0 do [[ROADMAP_QA_CODIGO]] (ativar checkpoint, salvar, morrer/respawnar).
> > - **Bugfix CRÍTICO (28/09) — E não ativava checkpoint:** `TryInteract` procurava o `IInteractable` no *pai* da área sobreposta, mas o `Checkpoint` é a própria `Area2D`. Agora testa a área e depois o pai. A auditoria de 15/09 marcou esse caso como ✅ sem rodar.
> > - **Bugfix (28/09) — respawn no checkpoint errado:** `OnDied` pegava o primeiro nó do grupo `Checkpoints`, ativado ou não. Agora o `PlayerController` guarda a posição do último `CheckpointActivated` e, se não houver, respawna perto de onde morreu.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C09-T1 — Persistir unlock de habilidade imediatamente
> > - [x] Hoje só atualiza a lista em memória até o próximo save por checkpoint/boss/item — garantir `SaveGame()` também no unlock, ou documentar a decisão de não fazer isso pra demo.
> > - **Nota (28/09):** `SaveManager.OnAbilityUnlocked` chama `SaveGame()` na hora (cria o save do slot 0 se ainda não existir). `ApplySaveData` passou a restaurar as habilidades sem emitir `AbilityUnlocked`, pra carregar um save não disparar notificação nem um save extra.
> > - **Nota (28/09):** confirmado no teste automatizado do Continuar: o `Dash` desbloqueado aparece no save e volta ao continuar.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C09-T2 — Fluxo de "Continuar"
> > - [x] Se a demo tiver esse menu: chamar `SaveManager.LoadGame(slot)` e, em `ApplySaveData`, trocar de cena pra `save.CurrentScene` antes de restaurar posição/HP (hoje não troca).
> > - [ ] Se a demo **não** vai ter esse menu (sempre começa do zero): documentar essa decisão em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].
> > - **Decisão do Gustavo (28/09) — menu inicial com Continuar:** nova cena principal `scenes/ui/MainMenu.tscn` (`MainMenu.cs`) com "Continuar" (só aparece se existe `save_0.json`), "Novo jogo" e "Sair". A segunda sub-task (não ter o menu) fica descartada. **Registrar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].**
> > - **Nota (28/09) — como funciona:** `SaveManager.ContinueGame()` lê o save, restaura na hora o estado de sessão (habilidades sem notificação, `GameManager.HasSword`), troca pra `save.CurrentScene` e, no `SceneTree.SceneChanged`, aplica posição, HP, checkpoints (o último vira ponto de respawn) e áreas restauradas. `StartNewGame()` apaga o save e zera o estado dos autoloads (`AbilityManager.ResetToInitial()`, espada ativada). Boss que está em `DefeatedBosses` não é instanciado de novo (`BossBase._Ready`). O menu de pausa ganhou os botões ligados (Continuar, Salvar, Menu inicial); antes nenhum funcionava.
> > - **Bugfix CRÍTICO (28/09) — posição nunca era salva:** o `save_0.json` gravava `"PlayerPosition": {}`, porque o `Vector2` do Godot usa campos (X/Y) e o `System.Text.Json` só serializa propriedades por padrão. Correção: `IncludeFields = true` no `SaveData`. Saves antigos (com `{}`) carregam no spawn padrão da cena. O `HasSword` também passou a ir pro save.
> > - **Verificação (28/09, headless):** menu → Novo jogo → guardar espada + checkpoint + restaurar + salvar → morrer → Tentar de novo → menu → Continuar: cena, posição, espada, dash, checkpoint e área restaurada voltam; na arena, o Korrag já derrotado não reaparece.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).

> [!success]- EPIC-C10 — HUD e UX — ✅ Concluído · 🟡 Média
> *Pronto quando*: um prompt visual aparece ao chegar perto de um `IInteractable` (sem precisar apertar nada), e a decisão sobre Game Over foi tomada e implementada.
>
> > [!success]- Task C10-T0 — Regressão básica de HUD
> > - [x] Rodar a Task C10-T0 do [[ROADMAP_QA_CODIGO]] (barra de vida, pausa).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C10-T1 — Prompt de interação contínuo
> > - [x] Fazer o `PlayerController` (ou um `InteractionDetector` dedicado) chamar `HUDController.ShowInteractionPrompt()`/`HideInteractionPrompt()` continuamente com base em estar ou não sobrepondo um `IInteractable` — hoje só reage ao apertar E.
> > - **Nota (28/09):** o `PlayerController` procura, a cada frame, o primeiro `IInteractable` disponível no `InteractionDetector` (`FindInteractable()`, também usado pelo E) e emite o novo sinal `EventBus.InteractionPromptChanged(string)` só quando o texto muda. O HUD mostra `[E] <prompt>` no `InteractionPrompt` que já existia. Textos: "Ativar checkpoint", "Restaurar". Checkpoint ativado e ponto já restaurado não oferecem interação, então o prompt some depois de usar.
> > - **Bugfix CRÍTICO (28/09) — `Checkpoint` herdava de `Node2D`, mas o nó é uma `Area2D`:** ao percorrer `GetOverlappingAreas()` (tipado como `Area2D`), o C# lançava `InvalidCastException` quando o checkpoint estava na lista. Com o prompt checando a cada frame, isso quebrava todo frame (achado no teste automatizado). Correção: `Checkpoint : Area2D`. O E perto da bandeira passava pelo mesmo laço, então vale revalidar a ativação do checkpoint.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C10-T2 — Game Over
> > - [x] Decidir se a demo precisa de tela de Game Over real ou se o respawn automático (já funcional) basta. Se precisar, implementar o que `ShowGameOver()` deveria de fato disparar.
> > - **Decisão do Gustavo (28/09) — tela de Game Over:** ao morrer, 0.8s depois aparece "Você caiu" com "Tentar de novo" (renasce no último checkpoint ativado, ou perto de onde morreu) e "Menu inicial". O jogo fica pausado e o Esc não despausa (`GameManager.IsGameOver`). Substitui o respawn automático de 1s, que só continua como fallback em cena sem HUD. **Registrar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].**
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).

> [!info]- EPIC-C11 — Áudio — 🔄 Em andamento · 🟡 Média
> *Pronto quando*: as pastas/buses de áudio existem, os 3 sons já esperados pelo código tocam de verdade, e a demo inteira tem pelo menos uma música por bioma e feedback sonoro nos hits principais.
>
> Responsabilidade do Gustavo.
>
> > [!success]- Task C11-T1 — Estrutura de pastas e buses
> > - [x] Criar `res://assets/audio/music/` e `res://assets/audio/sfx/`.
> > - [x] Configurar os buses "Music" e "SFX" no Godot (Audio → Buses) — o código já assume que existem.
> > - **Nota (28/09):** pastas criadas com `.gitkeep`. Buses em `res://default_bus_layout.tres` (Master → Music, SFX). Conferir em Audio → Buses no editor.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!failure]- Task C11-T2 — Arquivos já esperados pelo código
> > - [ ] Produzir/conseguir os 3 arquivos já chamados no código (nomes exatos, case-sensitive): `checkpoint_activate.wav`, `boss_stomp.wav`, `boss_defeat.wav`.
> >
> > Status: não iniciado — ⛔ bloqueado: arquivos de áudio ainda não existem. Lista completa esperada pelo código agora (em `sfx/`): `checkpoint_activate.wav`, `boss_stomp.wav`, `boss_defeat.wav`, `player_attack.wav`, `player_jump.wav`, `player_hurt.wav`, `player_dash.wav`, `sword_on.wav`, `sword_off.wav`, `enemy_hit.wav`, `enemy_death.wav`, `restoration.wav`.
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
