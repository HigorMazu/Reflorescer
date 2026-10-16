---
tipo: projeto
categoria: roadmap
status: ativo
criado: 2026-09-15
atualizado: 2026-10-15
tags: [projeto, roadmap]
projeto: "[[🗂️ Reflorescer]]"
area: codigo
papel: execucao
responsavel: Gustavo
par: "[[ROADMAP_QA_CODIGO]]"
descricao: Backlog de implementação da demo e do Deserto pós-demo (código)
epicos_total: 16
epicos_concluidos: 11
tasks_total: 68
tasks_prontas: 56
tasks_bloqueadas: 4
tasks_liberadas: 4
---
# Roadmap de Desenvolvimento — Código (Gustavo)

Backlog de execução da demo: Épicos (fases, ordenados pela dependência real entre as partes — fundação técnica antes de qualquer mecânica nova, restauração ODS15 e boss logo depois porque são os gaps de maior prioridade) que abrem em Tasks, que abrem em Sub-tasks. Cada sub-task é um checkbox — marcar conforme for implementado. Sempre que uma Task gerar uma nota de desenvolvimento (bugfix, decisão, mudança de escopo) durante o trabalho, ela é registrada ali mesmo, no formato **Tipo (DD/MM) — resumo**, logo abaixo do checklist da Task. Se a nota for uma decisão de design (não só um detalhe técnico de implementação), ela também é registrada no GDD do Obsidian: [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (o quê/porquê) e uma linha no [[Projetos/Reflorescer/🗓️ Log de Atualizações|🗓️ Log de Atualizações]] (quando). A ordem dos épicos é uma sugestão, ajustável a qualquer momento já que o código é do Gustavo.

Cada Épico aqui tem um Épico **gêmeo de mesmo ID** no [[ROADMAP_QA_CODIGO]] (ex: `EPIC-C03`), com os casos de teste que definem "pronto" de cada Task. Fluxo: implementar as sub-tasks de uma Task → rodar as sub-tasks de verificação da Task equivalente no QA → escrever a linha `Status:` → seguir pra próxima.

Marcação de disponibilidade na linha `Status:` de cada Task (avaliação de 28/09, após o pull do commit `6153f13`): 🟢 liberado — dá pra começar agora · 🟡 aguarda — depende de outra Task deste roadmap · ⛔ bloqueado — depende de decisão de design, arte/cena ou áudio fora deste roadmap.

Ordem recomendada: `EPIC-C01` → `EPIC-C02` → `EPIC-C06` → `EPIC-C04` → `EPIC-C03` → `EPIC-C05` → `EPIC-C07` → `EPIC-C08` → `EPIC-C13` → `EPIC-C09` → `EPIC-C10` → `EPIC-C11` → `EPIC-C12`. C03, C05 e C07 não têm dependência forte entre si — dá pra reordenar ou paralelizar, desde que C01 já esteja fechado.

> [!info] Decidido em 29/09/2026 — a demo virou 4 áreas dentro da Floresta Tropical
> A demo deixou de ser Floresta Tropical → Deserto → Tundra e passou a ser **4 áreas dentro da própria Floresta Tropical** — Sopé da Mata → Dossel Vivo → Igarapé Sufocado → Covil de Korrag —, estilo Hollow Knight (regiões interligadas, mesma identidade visual). Deserto e Tundra saem do escopo da demo. Isso adicionou o `EPIC-C13` (construir as 4 cenas) e mudou o alvo de `C02-T4` e `C07-T4`. Ver [[🌍 Mundo e Biomas#As 4 áreas da demo (dentro da Floresta Tropical)|Mundo e Biomas]] e [[Projetos/Reflorescer/🗳️ Decisões e Configuração|Decisões e Configuração]].

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

> [!success]- EPIC-C02 — Mecânica de Restauração ODS15 — ✅ Concluído · 🔴 Bloqueante
> *Pronto quando*: existe pelo menos 1 ponto de restauração interagível em 3 das 4 áreas da demo (Sopé da Mata, Dossel Vivo, Igarapé Sufocado — o Covil de Korrag não tem ponto de restauração, é a arena do boss), com efeito visual próprio, evento dedicado no `EventBus`, e persistência em save.
>
> Prioridade máxima de conteúdo — sem isso a demo perde a conexão com o pitch (ODS 15). Hoje `Checkpoint` é só um save point genérico, sem nenhum conceito de "restaurar" o bioma.
>
> > [!success]- Task C02-T1 — Definir e implementar a interação
> > - [x] Decidir se a restauração é o próprio `Checkpoint` reaproveitado (com um estado temático a mais) ou uma classe nova (`RestorationPoint : IInteractable`) separada do save point.
> > - [x] Implementar a interação escolhida.
> > - **Decisão do Gustavo (28/09) — classe separada:** `RestorationPoint : Area2D, IInteractable` (`scripts/world/RestorationPoint.cs`), independente do `Checkpoint`. Restaurar não é salvar, então cada um tem evento e persistência próprios. Interage uma vez (`Restored`), prompt "Restaurar". Os ganchos de C02-T2 (efeito) e C02-T3 (evento/save) estão marcados no `Interact()`. Registrada em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (28/09).
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
> > [!success]- Task C02-T4 — Posicionar no Sopé da Mata, Dossel Vivo e Igarapé Sufocado
> > - [x] Definir quantos pontos por área (mínimo 1 cada) e posicionar nas 3 cenas — degradação leve no Sopé da Mata, moderada no Dossel Vivo, mais impactante no Igarapé Sufocado (ver [[🌍 Mundo e Biomas#As 4 áreas da demo (dentro da Floresta Tropical)|Mundo e Biomas]]). Sem ponto no Covil de Korrag.
> > - **Nota (29/09):** título e escopo atualizados — a demo trocou de Floresta Tropical/Deserto/Tundra pras 4 áreas da própria Floresta Tropical (ver decisão no topo do arquivo). Esta Task passa a depender do `EPIC-C13`, não mais do `EPIC-A05` sozinho — as cenas agora são construídas pelo Gustavo (`EPIC-C13`), a arte final entra depois por cima.
> > - **Nota (05/10) — 1 ponto por área, montado junto com as cenas do `EPIC-C13`:** `restoration_sope_01` (no degrau do Sopé da Mata: chão seco + toco → grama + broto), `restoration_dossel_01` (no galho largo do meio da subida: folhagem seca + 2 galhos mortos → folhagem verde + bromélia + broto) e `restoration_igarape_01` (no trecho entre as poças de lama: água poluída + lixo + 3 tocos → água limpa + margem verde + 2 mudas + flor). A escala de contraste segue a degradação de cada área. Tudo em `ColorRect` placeholder, trocável pelos sprites do Higor sem mexer no código.
> >
> > - **Melhoria (05/10) — arte do Higor nos 3 pontos:** os `ColorRect` viraram `Sprite2D` com os PNGs de `assets/environment/tropical/` (entrega do Higor de 05/10): faixa de chão `ground_degraded`/`ground_restored` (cortada por `region_rect` na largura de cada ponto: 192/224/320 px) e brotos `sprout_degraded`/`sprout_restored`. Os brotos têm a origem na base (`offset` 0,-16), então o crescimento do `RestorationPoint` (escala Y de 0 a 1) sobe do chão. A quantidade segue a degradação de cada área: Sopé com 1 broto seco → 2 verdes, Dossel com 2 → 3 e Igarapé com 3 secos + água parada + lixo → 3 mudas (2 maiores) + água limpa + flor. A água e o lixo continuam em `ColorRect` (não há arte deles). Nenhum código mudou.
> >
> > Status: ✅ concluído (05/10) — teste em execução por script no Godot (05/10, Claude): nas 3 áreas, ao restaurar, os visuais secos somem e os verdes crescem até a escala final. Antes/depois conferido nos prints.

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
> > - **Nota (28/09):** opção simples aplicada: `MoveSpeedNoSword` (240) e `JumpVelocityNoSword` (-520) no `PlayerStatsResource`. O `PlayerController` usa `CurrentMoveSpeed`/`CurrentJumpVelocity`, que leem `HasSword`. O "sem ataque" já vem do `CanAttack()`. **Valores iniciais (+20% e +13%) validados em playtest e registrados em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (28/09).**
> > - **Nota (28/09):** testável agora: com a espada guardada, `CurrentMoveSpeed` = 240 e `CurrentJumpVelocity` = -520; ativada, 200 e -460 (conferido no Godot).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C03-T3 — Bloqueio e teste em combate real
> > - [x] Bloquear `CanAttack()` quando `HasSword == false` (parcialmente já verdade hoje).
> > - [x] Testar a troca em pleno combate/movimento — sem travar animação/input no meio da troca.
> > - [x] Rodar de novo a Task C03-T0 pra garantir que nada regrediu.
> > - **Bugfix (11/10) — hitbox espelhada só pela metade:** a forma da `AttackHitbox` tem deslocamento próprio de +25 px e o código só espelhava a posição da área. Virado pra esquerda, o golpe alcançava pouco e acertava **as costas** (existia desde o protótipo; ficou evidente com a estocada reta do Higor). `PerformAttack()` agora espelha também a forma: alcance de 95 px (centro a centro) dos dois lados, costas protegidas. O lado direito ficou igual ao calibrado pelo Higor.
> > - **Bugfix (11/10) — golpe saía com a espada guardada:** apertar ataque e Q durante a preparação (0,10 s) ainda abria a janela de dano. Agora `ToggleSword()` encerra o ataque ao guardar e a janela só abre com `HasSword`.
> > - **Nota (28/09):** o bloqueio já existia (`CanAttack()` exige `HasSword`). Trocar a espada não cancela um golpe em andamento: ele termina a janela de 0.14s normalmente, sem travar animação nem input (verificado no Godot: ataque + Q no mesmo frame, sem erro).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C03-T4 — Hitbox do Kairo (corpo e golpe)
> > - [x] Hurtbox do Kairo cobrindo o corpo inteiro, do chão à cabeça (22×66; era 20×36 só do quadril ao pescoço — pernas e cabeça não levavam dano).
> > - [x] Área do golpe alta o bastante pra pegar inimigos baixos (50×52, do chão à cabeça), mesmo alcance de antes (até ~100 px centro a centro).
> > - **Achado (11/10, playtest do Gustavo):** a área do golpe ficava entre 56 e 28 px acima do chão e a formiga e a tartaruga têm no máximo 30 px de altura: o golpe passava por cima delas (2 px de sobreposição). Os testes não pegaram porque criavam os inimigos flutuando na altura do Kairo — corrigido no `CombatCheck.gd`.
> >
> > Status: ✅ concluído (11/10) — golpe acerta inimigos no chão dos dois lados (2/2 cada arquétipo); `tools/qa/CombatCheck.gd` (inimigos no chão) 35/35, `RewardCheck.gd` 11/11, `DemoRunCheck.gd` 89/89.

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
> > - **Decisão do Gustavo (28/09) — dash só no chão:** o enum já separa `Dash` de `AirDash`, então o dash do Korrag funciona só com o Kairo no chão; o dash aéreo fica pra uma habilidade futura. Registrada em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (28/09).
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
> > - **Decisão do Gustavo (28/09) — desde o início, deslizando devagar:** wall grab/wall jump ficam disponíveis desde o começo da demo, como já dizia a tabela de controles do [[🕹️ Sistemas de Jogo#Movimentação|Sistemas de Jogo]] (sem condição de desbloqueio; o dash continua sendo a recompensa do Korrag). Continua passando pelo `AbilityManager` (`WallJump` entra no conjunto inicial, igual ao `DoubleJump`): gatear no futuro é tirar uma linha. O agarrar é "deslizar devagar", não "grudar parado", como está no GDD. Registrada em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (28/09).
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
> > - **Decisão do Gustavo (28/09) — super armor e timers:** o boss tem `InvulnerabilityDuration = 0` e todo golpe o jogava em `Hurt`, o que zerava o timer de ataque e cancelava o windup do Charge. Na prática, batendo sem parar, ele nunca atacava. Agora: (1) durante Charge/Stomp (`IsBusy`) o dano entra, mas não interrompe nem empurra; (2) o timer de ataque sobrevive às idas ao `Hurt`; (3) as transições de fase 2/enraged tocam uma vez só (antes repetiam a cada hit). Registrada em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (28/09).
> > - **Achado (28/09), não corrigido:** o enraged chama `SetPhase(2)`, mas `JavaliStatsResource` só tem 2 fases (índices 0 e 1), então no enraged o boss cai nos defaults (cooldown 1.5, velocidade 100, dano 20). Fica mais fraco que na fase 2. Precisa de um `JavaliPhase3.tres` ou de ajuste no índice.
> > - **Bugfix (05/10) — enraged mais fraco que a fase 2:** corrigido com um `JavaliPhase3.tres` novo (o índice 2 fica, porque o `JavaliBoss.ChooseAttack` já usa `_currentPhaseIndex >= 2` pra subir o Stomp a 50%). Valores provisórios do enraged: cooldown 1.0 (×0.7 do estado = 0.7s efetivo), velocidade 140, dano 35 (igual à fase 2; o enraged escala em frequência e velocidade, não em dano, já que o Kairo tem 100 de HP). `PhaseName`/`HealthThreshold`/`IsEnraged` dos 3 `.tres` foram ajustados pra refletir o que cada um é de fato (Fase 1 = 100–50%, Fase 2 = 50–20%, Enraged = ≤20%). Esses campos são só metadado, porque os limiares reais (0.5/0.2) estão no `BossStateMachine`. Conferido headless: `JavaliStatsResource` carrega 3 fases com os valores certos.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo). Bugfix do enraged (05/10) aguarda conferência em jogo (C06-T0: luta até ≤20% de HP).
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
> > - [x] Testar o `charge_windup` (1s) contra a animação real assim que a arte chegar (cruza com [[ROADMAP_QA_INTEGRACAO]] `EPIC-A04`). *(11/10: feito com o Korrag v3 — luta jogada pelo Gustavo e arte aprovada; o windup de 1s ficou.)*
> > - **Nota (28/09, pós-pull):** `KorragSpriteFrames.tres` já tem `charge_windup` (1 frame, 5 fps) e todos os nomes de animação chamados em `BossStateMachine.cs`/`JavaliBoss.cs` existem no recurso (`walk` existe mas nenhum script chama ainda).
> > - **Nota (28/09) — arte vs. timing:** a `charge_windup` do Higor tem 1 quadro (5 fps, sem loop), então o Korrag ficava congelado o segundo inteiro do windup. Mantido o `ChargeWindup` de 1s (tempo justo pra reagir) e adicionada telegrafia: pulso vermelho no `SelfModulate` do sprite (0.12s por meia-volta) durante o windup, desligado quando a investida começa, quando ela para ou quando o boss morre. Se 1s parecer lento, é só baixar `ChargeWindup` no inspetor do `Boss_Javali`. Se o Higor fizer um windup com mais quadros, o pulso pode sair. Cruza com [[ROADMAP_QA_INTEGRACAO]] A04.
> > - **Decisão do Gustavo (28/09):** `ChargeWindup` de 1s aprovado com o pulso vermelho de aviso.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo). Tempo de 1s aprovado.
>
> > [!success]- Task C06-T5 — Korrag de frente e hitboxes do tamanho do desenho
> > - [x] Investida e caminhada de frente: a arte olha pra esquerda e o código espelhava como se olhasse pra direita (ele investia "com a bunda", nos dois sentidos). Novo `BossBase.FaceDirection()` + `SpriteFacesLeft` (exportado, pra arte futura virada pro outro lado).
> > - [x] Hurtbox do tamanho do corpo (240×150; era 70×55 no centro de um desenho de ~280 px — cabeça e traseiro não levavam dano).
> > - [x] Golpe e investida pelas presas: área 130×110 na frente, virada pro lado do Kairo (a investida acertava num raio de 60 px do centro, com o Kairo "dentro" do javali).
> > - [x] Contato = sobreposição dos corpos com metade do dano da fase (era raio de 55 px e 22 fixo). Pisada com raio de 170 px (era 100, menor que o próprio corpo).
> > - **Pendência de arte:** os quadros 1 e 2 do `attack` olham pra direita e o 3º pra esquerda — o golpe vira de lado no meio ([[ROADMAP_ARTE]] A04-T3).
> >
> > Status: ✅ concluído (11/10) — investida de frente nos dois sentidos com 40 de dano pelas presas; golpe do Kairo acerta cabeça e traseiro (25 cada); capturas em `docs/art-review/` pendentes de revisão no jogo.

> [!success]- EPIC-C07 — Inimigos Comuns (Arquétipos) — ✅ Concluído · 🟠 Alta
> *Pronto quando*: existem 3 arquétipos (Voador/Rápido/Robusto) com comportamento distinto de fato, cada um com `.tres` de stats pra cada uma das 4 áreas da demo (Sopé da Mata → Dossel Vivo → Igarapé Sufocado, com escalada leve de dificuldade; o Covil de Korrag não tem inimigo comum).
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
> > - **Nota (28/09) — avaliação:** sem subclasse. Um campo novo no `EnemyStatsResource`, `InterruptOnHit` (`false` = não entra em `Hurt` ao tomar dano), mantém tudo data-driven. `EnemyRobustoStats.tres` (HP 150, KnockbackResistance 0.9, `InterruptOnHit = false`) + cena herdada `Enemy_Robusto.tscn`. Valores provisórios registrados em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (28/09); valores por área seguem pendentes (C07-T4).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C07-T4 — Dados por arquétipo × área da demo
> > - [x] Criar os `.tres` de `EnemyStatsResource` pra cada arquétipo × cada uma das 4 áreas (Sopé da Mata primeiro — já são os valores de `EnemyVoadorStats`/`EnemyRapidoStats`/`EnemyRobustoStats` atuais, só renomear/confirmar).
> > - **Nota (29/09):** proposta de valores pra Dossel Vivo e Igarapé Sufocado já rascunhada em [[👾 Bestiário#Valores por área (Sopé da Mata → Dossel Vivo → Igarapé Sufocado)|Bestiário]] — provisória, ajustável em playtest. O Covil de Korrag não recebe `.tres` de inimigo comum.
> > - **Nota (05/10) — 9 `.tres` em `resources/enemies/areas/`, nomeados `<Arquétipo>_<Área>.tres`:** os 3 antigos (`EnemyVoadorStats`/`EnemyRapidoStats`/`EnemyRobustoStats`) viraram os `_SopeDaMata`, sem mudar valores, e as cenas `Enemy_Voador/Rapido/Robusto.tscn` passaram a apontar pra eles (são o padrão). Os do Dossel Vivo e do Igarapé Sufocado seguem a tabela do [[👾 Bestiário#Valores por área (Sopé da Mata → Dossel Vivo → Igarapé Sufocado)|Bestiário]] (vida, dano e perseguição; o resto igual ao Sopé). Cada inimigo instanciado nas cenas do `EPIC-C13` sobrescreve o `StatsResource` com o `.tres` da própria área. Valores provisórios, pra ajustar em playtest.
> >
> > - **Decisão do Gustavo (11/10) — valores oficiais, os 3 arquétipos sobem a cada área:** o rascunho passou no playtest de 11/10, mas o grupo decidiu que a dificuldade sobe a cada área até o Korrag, então **todos** os arquétipos escalam (antes só o dominante de cada área). Mudaram 4 arquivos: `Rapido_DosselVivo` (75 HP / 12 dano / perseguição 200), `Robusto_DosselVivo` (175 / 28), `Voador_IgarapeSufocado` (100 / 18 / 110) e `Rapido_IgarapeSufocado` (100 / 15). Tabela oficial em [[👾 Bestiário#Valores por área (Sopé da Mata → Dossel Vivo → Igarapé Sufocado)|Bestiário]]; decisão em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (11/10). Validar jogando junto com a C09-T4.
> >
> > Status: ✅ concluído (05/10) — conferido headless: os 9 arquivos existem e cada inimigo das 3 cenas usa o `.tres` da própria área. Valores oficializados e escalados em 11/10; o teste jogando entra na C09-T4.
>
> > [!success]- Task C07-T5 — (baixa prioridade) AtPatrolEdge()
> > - [x] Hoje sempre retorna `false` — corrigir só se o comportamento "parar depois de patrulhar" for desejado. Aceitável pra demo como está.
> > - **Nota (28/09):** `AtPatrolEdge()` faz um raycast pra baixo 20px à frente (máscara World). No `EnemyPatrolState`, na beira o inimigo vira (`Flip`) e entra em `Idle`, retomando a patrulha no sentido oposto.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C07-T6 — Hitboxes e dano dos inimigos comuns
> > - [x] **Decisão do Gustavo (11/10):** estilo Hollow Knight — o ataque (mordida/ferrão) dá o dano cheio; encostar no corpo dá metade.
> > - [x] Hurtbox e área de ataque medidas no desenho de cada arquétipo (exports novos no `EnemyBase`, valores em cada `Enemy_X.tscn`): formiga 38×24 / mandíbulas 26×22; tartaruga 40×22 / cabeça 26×18; vespa 34×30 / ferrão 24×24.
> > - [x] Contato por sobreposição de hurtboxes (`CombatGeometry`), no lugar da aura invisível de 32 px em volta do centro.
> > - **Achado (11/10, playtest do Gustavo):** os ataques da formiga e da tartaruga **nunca acertavam** (área na altura dos pés, hurtbox do Kairo do quadril pra cima): todo o dano vinha da aura de contato, que machucava 1×/s mesmo sem atacar. Medido em 10 s: formiga 0 acertos de ataque e 9 de contato; tartaruga 0 e 8.
> > - **Nota de balanceamento:** como o ataque passou a acertar, ficar parado perto do inimigo dá um pouco mais de dano no total que antes. Ajustar no playtest se pesar demais.
> >
> > Status: ✅ concluído (11/10) — acertos do ataque no Kairo em 10 s: formiga 0 → 7, tartaruga 0 → 4, vespa 4 → 7; contato agora 5/12/6; `tools/qa/CombatCheck.gd` (inimigos no chão) 35/35, `RewardCheck.gd` 11/11, `DemoRunCheck.gd` 89/89.

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
> > - ~~**Opção B** (mais simples se o tempo apertar): nomear spawn points por origem (`SpawnFromFlorestaTropical`, `SpawnFromDeserto`) e escolher pelo nome de onde o jogador veio.~~ *(descartada: a Opção A foi aplicada)*
> > - **Nota (28/09) — Opção A aplicada:** `TargetArea` = nome de um `Marker2D` no grupo `SpawnPoints` da cena de destino; `SpawnOffset` soma na posição. `SceneManager.LoadSceneAtSpawn()` guarda o destino, e o `PlayerController` consome no `_Ready` (deferred) e reseta a câmera. Markers criados: `SpawnFromBossArena` (TestLevel, perto do portal) e `SpawnFromTestLevel` (BossArena).
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).

> [!success]- EPIC-C13 — Construção das 4 Áreas da Demo (Floresta Tropical) — ✅ Concluído · 🔴 Bloqueante
> *Pronto quando*: as 4 cenas da demo existem e estão encadeadas na ordem certa — Sopé da Mata → Dossel Vivo → Igarapé Sufocado → Covil de Korrag —, cada uma com o conteúdo mínimo da sua identidade (ver [[🌍 Mundo e Biomas#As 4 áreas da demo (dentro da Floresta Tropical)|Mundo e Biomas]]), substituindo o `TestLevel`/`BossArena` isolados como o percurso real da demo.
>
> Épico novo, adicionado em 29/09/2026: a demo deixou de ser Floresta Tropical → Deserto → Tundra e virou 4 áreas dentro da própria Floresta Tropical, estilo Hollow Knight (ver [[Projetos/Reflorescer/🗳️ Decisões e Configuração|Decisões e Configuração]]). Pode começar com placeholder (TileMap simples, `ColorRect`), igual o `TestLevel` hoje — a arte final do Higor entra depois por cima ([[ROADMAP_ARTE]] `EPIC-A05`), sem exigir remontar a cena.
>
> Implementado em 05/10/2026. As 4 cenas ficam em `scenes/areas/` e todas seguem o mesmo esqueleto: raiz com `AreaController` (nome da área + limites de câmera), nós-grupo `Geometry`/`Hazards`/`Interactables`/`Transitions`/`Enemies`, e instâncias de `Player`, `Faisca` e `HUD.tscn`. A geometria é `StaticBody2D` + `ColorRect`, igual o `TestLevel`. Pra arte do Higor, basta trocar os `Visual`/`Top` de cada bloco por sprites ou um `TileMap` por cima, sem mexer em colisão. As cenas foram geradas por script (geometria consistente: degraus de até ~80px, já que o pulo simples sobe ~108px). Daqui pra frente, editar direto no Godot.
>
> **Pendência resolvida (05/10) — compilação:** o `dotnet build` era barrado pelo Controle Inteligente de Aplicativos do Windows (Smart App Control), que bloqueava o `Godot.SourceGenerators.dll` sem assinatura. O Gustavo desligou e o projeto compilou no editor (DLL das 21:17).
>
> **Bugfix (05/10) — re-save no editor apagou os limites de câmera:** as cenas foram salvas no editor quando a DLL ainda era a de 28/09, que não conhecia `UseCameraLimits`/`CameraLimits`, e o Godot descartou as duas propriedades das 4 raízes. Restaurado. O resto que sumiu no mesmo commit (`e651e40`) era só valor igual ao padrão, que o Godot omite: nada se perdeu. **Regra:** depois de adicionar `[Export]` novo, compilar antes de abrir ou salvar cenas que o usam.
>
> **Melhoria (05/10) — fundo tropical do Higor nas 4 áreas:** cada cena instancia `scenes/art/TropicalBackdrop.tscn` (parallax + partículas no ar, só visual) logo depois do `Background`, que passou a `z_index = -30` pra ficar atrás dele (o backdrop é -20). A área das partículas (`Motes`) foi ajustada ao tamanho de cada cena. Com a escala de parallax do Higor (0.18/0.12), a imagem cobre a tela inteira em todos os limites de câmera.
>
> > [!success]- Task C13-T1 — Sopé da Mata (entrada)
> > - [x] Criar `scenes/areas/SopeDaMata.tscn`: `TileMap`/geometria placeholder, limites de câmera, `Marker2D` de spawn inicial do Kairo (novo jogo começa aqui).
> > - [x] Instanciar 1-2 exemplares de cada arquétipo comum, em densidade baixa (área introdutória).
> > - [x] Instanciar 1 `RestorationPoint` com degradação leve (`DegradedVisuals`/`RestoredVisuals` com o placeholder atual).
> > - [x] `TransitionTrigger` de saída pro Dossel Vivo, com `Marker2D` `SpawnFromSopeDaMata` do lado de lá (ver `EPIC-C08`).
> > - **Nota (05/10):** 2400×720 px, horizontal: chão com um degrau (onde fica o ponto de restauração), uma vala rasa e 3 plataformas. Checkpoint `cp_sope_01` logo no começo. Inimigos: 1 Robusto, 2 Voadores, 2 Rápidos. `Marker2D`s: `SpawnInicial` e `SpawnFromDosselVivo` (perto da saída, pra quem volta).
> > - **Melhoria (05/10) — limites de câmera por área:** a câmera do Kairo não tinha limite nenhum. O `AreaController` ganhou `UseCameraLimits` + `CameraLimits` (`Rect2I`) e aplica na câmera do Player no `_Ready`. As 4 áreas usam, e o `TestLevel`/`BossArena` continuam sem limite. Os limites precisam ter pelo menos 1280×720 (o tamanho da tela).
> >
> > Status: ✅ concluído (11/10) — QA C13-T1 passa por script com a física real e a arte do Higor (`tools/qa/DemoRunCheck.gd` (89 verificações, 0 falhas)): Novo jogo nasce no `SpawnInicial` em pé no chão, câmera com limites, 5 inimigos, restauração com prompt. Sensação de jogo fica pro playtest (C12-T2/A07).
>
> > [!success]- Task C13-T2 — Dossel Vivo (verticalidade)
> > - [x] Criar `scenes/areas/DosselVivo.tscn`: geometria com foco em plataforming vertical (plataformas soltas, sem chão contínuo).
> > - [x] Maior densidade do arquétipo Voador em relação ao Sopé da Mata.
> > - [x] Instanciar 1 `RestorationPoint` com degradação moderada.
> > - [x] `TransitionTrigger`s de entrada (do Sopé da Mata) e saída (pro Igarapé Sufocado), com os `Marker2D` correspondentes.
> > - **Nota (05/10):** 1600×1440 px. Entra embaixo à esquerda e sobe em zigue-zague por 11 galhos (degraus de ~76px) até o galho do topo à direita, onde fica a saída. Tem chão de mata embaixo só pra quem cair não sair do mundo (não existe zona de morte por queda). Dois troncos (`TrunkA`/`TrunkB`) formam uma chaminé de 132px pra subir no pulo de parede (atalho opcional). Inimigos: 4 Voadores pela subida e 1 Rápido embaixo. Checkpoint `cp_dossel_01` na base.
> >
> > Status: ✅ concluído (11/10) — QA C13-T2 passa: os 12 saltos do chão ao galho do topo são alcançáveis **só com pulo simples**, testado por um bot com a física real do Kairo (`tools/qa/DosselReachCheck.gd`); entrada/volta pelos markers certos (`tools/qa/DemoRunCheck.gd` (89 verificações, 0 falhas)).
>
> > [!success]- Task C13-T3 — Igarapé Sufocado (mata alagada e degradada)
> > - [x] Criar `scenes/areas/IgarapeSufocado.tscn`: geometria de mata baixa alagada.
> > - [x] Novo componente `MudHazard` (`Area2D`): reduz `CurrentMoveSpeed` do Kairo por um multiplicador enquanto ele estiver dentro da área — primeiro hazard de terreno do jogo, código novo e pequeno.
> > - [x] Maior densidade dos arquétipos Robusto/Rápido em relação às áreas anteriores.
> > - [x] Instanciar 1 `RestorationPoint` com o maior contraste "antes/depois" das 3 (área mais degradada da demo).
> > - [x] `TransitionTrigger`s de entrada (do Dossel Vivo) e saída (pro Covil de Korrag).
> > - **Nota (05/10) — `MudHazard`:** `scripts/world/MudHazard.cs`, `Area2D` com máscara 2 (corpo do Kairo). Ao entrar chama `PlayerController.SetTerrainSpeedModifier(this, SpeedMultiplier)` e ao sair chama `ClearTerrainSpeedModifier`. `SpeedMultiplier` padrão 0.55 (provisório). Com duas poças sobrepostas vale a mais lenta. Só afeta a corrida (`CurrentMoveSpeed`): pulo, dash e pulo de parede ficam iguais, então dá pra atravessar a lama pulando ou por cima dos troncos.
> > - **Nota (05/10) — cena:** 2800×720 px, chão contínuo com 3 poças de lama e 3 troncos flutuando por cima. Inimigos: 3 Robustos, 2 Rápidos e 1 Voador. Checkpoints `cp_igarape_01` (começo) e `cp_igarape_02` (logo antes da saída pro boss).
> >
> > Status: ✅ concluído (11/10) — QA C13-T3 passa (`tools/qa/DemoRunCheck.gd` (89 verificações, 0 falhas)): as 3 poças aplicam x0,55 dentro e voltam a x1,0 fora; 5 Robustos/Rápidos contra 2 e 1 das áreas anteriores; entrada e volta pelo Dossel nos markers certos.
>
> > [!success]- Task C13-T4 — Covil de Korrag (arena do boss)
> > - [x] Decidir: renomear/adaptar a `BossArena.tscn` existente pra virar a cena final (chão revirado pelas escavações do javali, sem inimigos comuns), ou criar uma cena nova e aposentar a `BossArena` de teste.
> > - [x] `TransitionTrigger` de entrada (do Igarapé Sufocado) — sem saída, a demo termina aqui.
> > - [x] Sem `RestorationPoint` nesta área — é a arena do boss, não um trecho de exploração.
> > - **Decisão (05/10) — cena nova, `BossArena` fica como teste:** `scenes/areas/CovilDeKorrag.tscn` é a arena da demo. A `BossArena` continua ligada ao `TestLevel` como par de teste isolado, igual ele. A arena nova tem 1280 px de largura (a `BossArena` tinha 800, menos que a tela, e com o limite de câmera isso não fecharia), chão com montes de terra revirada (placeholder), checkpoint `cp_covil_01` na entrada e nenhum `TransitionTrigger` de saída.
> > - **Nota (05/10):** "entrada do Igarapé Sufocado" = o `Marker2D` `SpawnFromIgarapeSufocado`, já que não há volta. Sem trigger de retorno: depois de entrar, o jogador fica na arena até derrotar o Korrag.
> >
> > Status: ✅ concluído (11/10) — QA C13-T4 passa (`tools/qa/DemoRunCheck.gd` (89 verificações, 0 falhas)): entrada no marker certo, sem inimigos comuns nem restauração, Korrag ativa, passa pelas 3 fases, morre uma vez e libera o Dash.
>
> > [!success]- Task C13-T5 — Encadear e validar a demo inteira
> > - [x] Conferir a cadeia completa Sopé da Mata → Dossel Vivo → Igarapé Sufocado → Covil de Korrag: cada saída cai no `Marker2D` certo da cena seguinte (rodar a Task equivalente do [[ROADMAP_QA_CODIGO]]).
> > - [x] Novo jogo (menu inicial) começa no Sopé da Mata; `TestLevel` continua existindo só como cena de teste isolada, fora do percurso da demo.
> > - **Nota (05/10):** `SaveManager.NewGameScene` agora é `res://scenes/areas/SopeDaMata.tscn` e é a única fonte. `CreateNewSave`, o padrão do `SaveData` e o `GameManager.RestartGame` passaram a usar essa constante (antes cada um tinha `Main.tscn` escrito à mão). O `Main.tscn` (que embrulha o `TestLevel`) continua existindo, só saiu do fluxo do menu.
> > - **Nota (05/10) — conferência estática:** headless, as 5 transições da cadeia (ida e volta entre Sopé, Dossel e Igarapé, mais a ida pro Covil) apontam pra `Marker2D`s que existem na cena de destino. O percurso jogado de ponta a ponta continua pendente.
> >
> > - **Bugfix (11/10) — troca de cena dentro do callback de física:** o `TransitionTrigger` chamava `ChangeSceneToFile` direto no `AreaEntered`, e o Godot acusava `Removing a CollisionObject node during a physics callback` a cada transição (7 vezes num percurso). Agora a troca é adiada com `CallDeferred`. Depois disso, o percurso inteiro roda sem nenhum erro do motor.
> >
> > Status: ✅ concluído (11/10) — cadeia completa validada por script com os portais reais, ida e volta (`tools/qa/DemoRunCheck.gd` (89 verificações, 0 falhas)). A partida jogada à mão, com combate de verdade, fica no C12-T2.

> [!info]- EPIC-C14 — Cura e Recompensas de Restauração — 🔄 Em andamento · 🔴 Bloqueante
> *Pronto quando*: o Kairo tem como recuperar vida na demo e restaurar uma área dá recompensas que fazem o jogador querer restaurar.
>
> Criado em 11/10/2026 a partir do playtest do Gustavo: com a dificuldade nova (trocar de área não cura, uma bandeira só), o jogador só perdia vida. Decisões em [[Projetos/Reflorescer/🕹️ Sistemas de Jogo|🕹️ Sistemas de Jogo]].
>
> > [!success]- Task C14-T1 — Restaurar cura e dá +10 de vida máxima
> > - [x] Ao restaurar: vida cheia e vida máxima +10 (100 → 130 com as 3 áreas). `PlayerController.MaxHealthPerRestoration`.
> > - [x] A vida máxima sai dos pontos restaurados do save (`SaveManager.RestoredCount`), então vale depois de trocar de área, morrer e Continuar.
> > - [x] Aviso no HUD: "Área restaurada · Vida máxima +10" (`HUDController.ShowNotice`, reaproveitando o aviso de habilidade).
> >
> > Status: ✅ concluído (11/10) — `tools/qa/RewardCheck.gd` 11/11.
>
> > [!success]- Task C14-T2 — Bandeira cura ao passar
> > - [x] Passar pela bandeira enche a vida, toda vez (não só na ativação); aviso "Vida restaurada" só quando estava ferido.
> >
> > Status: ✅ concluído (11/10) — `tools/qa/RewardCheck.gd`.
>
> > [!success]- Task C14-T3 — Registro de áreas restauradas no fim da demo
> > - [x] Depois de derrotar o Korrag (e do aviso do Dash), uma tela de encerramento da demo mostra **"Áreas restauradas: X/3"**, com mensagem diferente pra 3/3.
> > - [x] Botões: voltar ao menu / sair. Hoje a demo não tem tela final: o jogo só segue no Covil depois do Korrag.
> > - **Implementação (11/10):** `HUDController.ShowDemoEnd()`, painel montado em código como o do Game Over: aparece 4 s depois do `BossDefeated` (timer que ignora o hitstop), pausa o jogo e bloqueia o Esc (mesma flag `IsGameOver`); se o Kairo morrer nesse intervalo, o Game Over tem prioridade. Contagem pelos pontos restaurados do save (`SaveManager.RestoredCount`, total `DemoRestorationTotal` = 3). Mensagens: 0 → "Korrag caiu, mas a mata segue ferida..."; 1–2 → "A floresta começou a se recuperar..."; 3/3 → "Você restaurou toda a mata pelo caminho. A floresta volta a respirar."
> > - **Melhoria (11/10):** além de "Menu inicial" e "Sair", um botão **"Continuar explorando"** fecha a tela pra testar o Dash recém-liberado. Só aparece uma vez: o Continuar num save com o Korrag já derrotado não mostra de novo.
> > - **Captura:** `docs/art-review/qa-demo-end-3de3-2026-10-11.png` (texto puro; o acabamento visual é do Higor, [[ROADMAP_ARTE]] A06-T6).
> > - **Arte:** tela simples com texto funciona; acabamento visual com o Higor ([[ROADMAP_ARTE]] A06-T6).
> >
> > Status: ✅ concluído (11/10) — `tools/qa/DemoEndCheck.gd` 15/15 (0, 1 e 3 áreas restauradas de verdade, Korrag derrotado); regressão (`DemoRunCheck`, `RewardCheck`, `CombatCheck`, `AudioCheck`, `SaveReloadCheck`, `GcStressCheck`) passando.
>
> > [!failure]- Task C14-T4 — Faísca fica mais forte ao restaurar
> > - [ ] Definir o que a Faísca ganha a cada área restaurada (proposta em [[Projetos/Reflorescer/🕹️ Sistemas de Jogo|🕹️ Sistemas de Jogo]]).
> > - [ ] Implementar e testar.
> >
> > Status: ⛔ bloqueado (11/10) — aguarda a definição do grupo.
>
> > [!failure]- Task C14-T5 — Caminho novo ao restaurar (atalho ou segredo)
> > - [ ] Definir, em cada área com ponto de restauração (Sopé, Dossel, Igarapé), o que cresce e o que abre (proposta em [[Projetos/Reflorescer/🕹️ Sistemas de Jogo|🕹️ Sistemas de Jogo]]).
> > - [ ] Montar na cena (plataformas/vinhas que aparecem com a restauração, usando as listas de visuais do `RestorationPoint`) e testar o alcance.
> >
> > Status: ⛔ bloqueado (11/10) — aguarda a definição do grupo e a arte ([[ROADMAP_ARTE]] A05-T6).

> [!warning]- EPIC-C09 — Save, Checkpoint e Respawn — 🔄 Em andamento (visual do checkpoint) · 🟡 Média
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
> > - ~~Se a demo **não** vai ter esse menu (sempre começa do zero): documentar essa decisão em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].~~ *(não se aplica: a demo tem o menu com Continuar)*
> > - **Decisão do Gustavo (28/09) — menu inicial com Continuar:** nova cena principal `scenes/ui/MainMenu.tscn` (`MainMenu.cs`) com "Continuar" (só aparece se existe `save_0.json`), "Novo jogo" e "Sair". A segunda sub-task (não ter o menu) fica descartada. Registrada em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (28/09).
> > - **Nota (28/09) — como funciona:** `SaveManager.ContinueGame()` lê o save, restaura na hora o estado de sessão (habilidades sem notificação, `GameManager.HasSword`), troca pra `save.CurrentScene` e, no `SceneTree.SceneChanged`, aplica posição, HP, checkpoints (o último vira ponto de respawn) e áreas restauradas. `StartNewGame()` apaga o save e zera o estado dos autoloads (`AbilityManager.ResetToInitial()`, espada ativada). Boss que está em `DefeatedBosses` não é instanciado de novo (`BossBase._Ready`). O menu de pausa ganhou os botões ligados (Continuar, Salvar, Menu inicial); antes nenhum funcionava.
> > - **Bugfix CRÍTICO (28/09) — posição nunca era salva:** o `save_0.json` gravava `"PlayerPosition": {}`, porque o `Vector2` do Godot usa campos (X/Y) e o `System.Text.Json` só serializa propriedades por padrão. Correção: `IncludeFields = true` no `SaveData`. Saves antigos (com `{}`) carregam no spawn padrão da cena. O `HasSword` também passou a ir pro save.
> > - **Verificação (28/09, headless):** menu → Novo jogo → guardar espada + checkpoint + restaurar + salvar → morrer → Tentar de novo → menu → Continuar: cena, posição, espada, dash, checkpoint e área restaurada voltam; na arena, o Korrag já derrotado não reaparece.
> >
> > Status: ✅ concluído — QA validado no Godot em 28/09 (Gustavo).
>
> > [!warning]- Task C09-T3 — Visual do checkpoint apagado/ativado
> > - [x] Trocar os `ColorRect` placeholder (`FlagPole`/`Flag`) da bandeira única do Dossel Vivo (`cp_dossel_01`) pela arte do Higor ([[ROADMAP_ARTE]] A05-T5). *(Eram 5 checkpoints até 11/10; ver C09-T4.)*
> > - [x] `Checkpoint.cs`: mostrar o estado **apagado** antes de tocar e **ativado** depois; ao continuar um save, os checkpoints em `ActivatedCheckpoints` já nascem ativados.
> > - **Origem (11/10):** achado na revisão de arte (item 4). Hoje o checkpoint funciona, mas o visual não muda ao ser tocado.
> >
> > - **Nota (11/10, C09-T4):** agora é uma bandeira só. O `Checkpoint.cs` já nasce `Activated` quando o save da sessão diz que ela foi ativada (ao voltar pra cena ou renascer nela); falta só o visual reagir.
> >
> > Status: 🟡 entregue (12/10) — `CheckpointTotemVisual` integrado ao estado salvo. `CheckpointVisualCheck.gd` confirma o ciclo novo jogo → toque → salvar → Continuar, sem alterar o save pessoal; revisão visual do Higor pendente.
>
> > [!info]- Task C09-T4 — Bandeira única e vida que não enche ao trocar de área
> > - [x] Tirar os checkpoints do Sopé da Mata, do Igarapé Sufocado (2) e do Covil de Korrag. Fica só `cp_dossel_01`, no começo do Dossel Vivo (x = 200, logo depois do spawn vindo do Sopé).
> > - [x] Bandeira do Dossel ativa sozinha quando o Kairo passa (`Checkpoint.AutoActivateOnTouch = true`, via `BodyEntered`). O E continua funcionando.
> > - [x] Morrer em qualquer área → "Tentar de novo" volta pra bandeira, recarregando a cena do Dossel se a morte foi em outra área (`SaveData.LastCheckpointScene` + `SceneManager.LoadSceneAtCheckpoint`). Sem bandeira ativada ainda → começo do Sopé da Mata. Cenas de teste fora de `res://scenes/areas/` mantêm o comportamento antigo.
> > - [x] Trocar de área não cura: `GameManager.CarriedPlayerHealth` guarda a vida e o `PlayerController` aplica ao nascer na cena nova (`Health.SetCurrentHealth`). Morrer (respawn) e Novo jogo enchem a vida.
> > - [x] `Checkpoint` nasce ativado se o save da sessão diz que foi ativado (`SaveManager.IsCheckpointActivated`).
> > - [ ] Compilar no editor e rodar a C09-T4 do [[ROADMAP_QA_CODIGO]].
> > - **Decisão do Gustavo (11/10):** a demo fica mais difícil até o Korrag: uma bandeira só na 2ª área, morrer volta pra ela, e passar de área não cura mais. Registrada em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (11/10).
> > - **Arquivos:** `scripts/world/Checkpoint.cs`, `scripts/player/PlayerController.cs`, `scripts/core/GameManager.cs`, `scripts/core/SceneManager.cs`, `scripts/save/SaveManager.cs`, `scripts/save/SaveData.cs`, `scripts/combat/Health.cs`, `scenes/areas/*.tscn` (4).
> > - **Atenção:** feito fora do Godot (sem compilar). Se o editor estiver aberto com alguma das 4 cenas, recarregar antes de salvar, senão ele regrava a versão antiga com as bandeiras.
> >
> > Status: implementado (11/10) — aguarda compilar no editor e jogar.
>
> > [!success]- Task C09-T5 — Continuar volta com a vida salva
> > - [x] `ApplySaveData` restaura a vida salva (`SetCurrentHealth`), não a vida cheia.
> > - **Bugfix (11/10):** o `SetMaxHealth(..., true)` enchia a vida e o `Heal` somava por cima, então salvar/sair/Continuar curava de graça — um atalho com a regra de 11/10 de que trocar de área não cura. A vida máxima agora é a maior entre a salva e a calculada pelas restaurações (C14-T1).
> >
> > Status: ✅ concluído (11/10) — `tools/qa/RewardCheck.gd`: Continuar volta com 100/120 como foi salvo.

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
> > - **Decisão do Gustavo (28/09) — tela de Game Over:** ao morrer, 0.8s depois aparece "Você caiu" com "Tentar de novo" (renasce no último checkpoint ativado, ou perto de onde morreu) e "Menu inicial". O jogo fica pausado e o Esc não despausa (`GameManager.IsGameOver`). Substitui o respawn automático de 1s, que só continua como fallback em cena sem HUD. Registrada em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] (28/09).
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
> > [!success]- Task C11-T2 — Arquivos já esperados pelo código
> > - [x] Produzir/conseguir os 3 arquivos já chamados no código (nomes exatos, case-sensitive): `checkpoint_activate.wav`, `boss_stomp.wav`, `boss_defeat.wav`.
> >
> > - **Entrega (11/10) — placeholders sintetizados:** os 12 `.wav` foram gerados por código (`tools/audio/generate_placeholder_sfx.py`: formas de onda + ruído filtrado, WAV 16 bits mono 44,1 kHz, pico em -3 dBFS, semente fixa). São 100% do projeto, sem licença de terceiros. Decisão do Gustavo: usar agora pra destravar o áudio e o QA, e trocar arquivo por arquivo pelos sons finais depois (C11-T5) — o código não muda, só o arquivo com o mesmo nome.
> > - **Verificação (11/10):** `tools/qa/AudioCheck.gd` dispara os 12 pelo caminho real do jogo (atacar, pular, dash, Q, dano, acertar/matar inimigo, checkpoint com E, restaurar, pisada e derrota do Korrag) e confere que o `AudioManager` tocou cada um: 12/12. `DemoRunCheck` sem nenhum "SFX not found"; `GcStressCheck` 4/4, `CombatCheck --stress` e `SaveReloadCheck` continuam passando com os sons carregados.
> >
> > Status: ✅ concluído (11/10) — com sons placeholder; os finais entram pela C11-T5. Lista completa esperada pelo código (em `sfx/`): `checkpoint_activate.wav`, `boss_stomp.wav`, `boss_defeat.wav`, `player_attack.wav`, `player_jump.wav`, `player_hurt.wav`, `player_dash.wav`, `sword_on.wav`, `sword_off.wav`, `enemy_hit.wav`, `enemy_death.wav`, `restoration.wav`.
>
> > [!warning]- Task C11-T3 — Chamadas que faltam
> > - [x] Adicionar `AudioManager.PlaySfx(...)` pro ataque do jogador, hit em inimigo/jogador, morte de inimigo comum, pulo (opcional).
> > - [x] Música de fundo por bioma via `AudioManager.PlayMusic(...)` (fade in/out já pronto no código).
> > - **Nota (28/09):** `PlaySfx` no ataque, pulo e dano do Kairo e no hit/morte de inimigo comum. `AreaController` ganhou `MusicTrack` (exportado, vazio por padrão), tocado no `_Ready` via `PlayMusic`. O `AudioManager` passou a logar arquivo faltando uma vez só por caminho.
> >
> > Status: ✅ concluído (14/10) — as chamadas de SFX e música estão integradas; `MusicCheck.gd` confirmou música e três SFX simultâneos sem corte.

> > [!success]- Task C11-T4 — Música por área e tela de título
> > - [x] Faixas em `res://assets/audio/music/` (loop): tela de título, Sopé da Mata, Dossel Vivo, Igarapé Sufocado e Covil de Korrag (luta). Opcional: variação pra fase enfurecida do Korrag.
> > - [x] Preencher o `MusicTrack` do `AreaController` de cada área (o `PlayMusic` com fade já existe) e tocar a do título no `MainMenu`.
> > - [x] Ajustar volume do bus Music × SFX (ver [[ROADMAP_QA_INTEGRACAO]] A07-T1).
> > - **Entrega (14/10):** cinco loops instrumentais autorais e reproduzíveis foram gerados por `tools/audio/generate_demo_music.py`; os créditos e a procedência ficam em `assets/audio/CREDITOS.md`. A música fica em -12 dB e os SFX em -4 dB; o player reinicia a faixa ao terminar e preserva o fade entre áreas.
> >
> > Status: ✅ concluído (14/10) — `MusicCheck.gd` confirmou as cinco faixas, transições e sobreposição com três SFX; `DemoRunCheck.gd` passou 89/89.

> > [!todo]- Task C11-T5 — Trocar os SFX placeholder pelos sons finais
> > - [ ] Substituir os 12 `.wav` de `assets/audio/sfx/` pelos sons finais, **mantendo os nomes**: `player_attack`, `player_jump`, `player_dash`, `player_hurt`, `sword_on`, `sword_off`, `enemy_hit`, `enemy_death`, `checkpoint_activate`, `restoration`, `boss_stomp`, `boss_defeat`.
> > - [ ] Registrar a origem e a licença de cada som que não for do grupo (ex.: CC0) num `assets/audio/CREDITOS.md`.
> > - [ ] Rodar `tools/qa/AudioCheck.gd` de novo (12/12) e ouvir na demo.
> > - **Origem (11/10):** os atuais são placeholders sintetizados (C11-T2). Pra ouvir todos em sequência, gerar a prévia ou abrir os arquivos da pasta.
> >
> > Status: não iniciado (11/10).

> [!info]- EPIC-C12 — Regressão Final / Release da Demo — 🔄 Em andamento · 🔴 Bloqueante
> *Pronto quando*: o [[ROADMAP_QA_CODIGO]] inteiro passa, o [[ROADMAP_QA_INTEGRACAO]] inteiro passa, e a demo roda de ponta a ponta sem crash na frente de alguém de fora do grupo.
>
> Só começa depois que todos os épicos 🔴/🟠 acima estiverem concluídos.
>
> > [!todo]- Task C12-T1 — Regressão completa de código
> > - [ ] Rodar o [[ROADMAP_QA_CODIGO]] inteiro, de ponta a ponta, e corrigir qualquer regressão.
> >
> > - **Nota (11/10):** os épicos 🔴/🟠 de código estão concluídos (falta só o visual do checkpoint, C09-T3, e a música, C11-T4). Já existe uma bateria automática que cobre boa parte do [[ROADMAP_QA_CODIGO]]: `tools/qa/` (`DemoRunCheck`, `CombatCheck`, `DosselReachCheck`, `SaveReloadCheck`, `GcStressCheck`, `AudioCheck`, `FpsCheck`).
> >
> > Status: 🟢 liberado (11/10) — pode começar; rodar de novo no fim, depois da C09-T3 e da C11-T4.
>
> > [!todo]- Task C12-T2 — Handoff pro Higor
> > - [ ] Entregar os blocos de mecânica que faltavam pra ele terminar a arte condicional (ex: como fica visualmente o wall grab, pra desenhar a animação certa).
> >
> > - **Nota (11/10):** útil agora pro ajuste do wall slide ([[ROADMAP_ARTE]] A01-T3): mostrar ao Higor como o estado `IsWallSliding` funciona (quando entra, quanto dura, de que lado fica a parede) pra ele desenhar a pose certa.
> >
> > Status: 🟢 liberado (11/10) — as mecânicas estão prontas.
>
> > [!info]- Task C12-T3 — Integração incremental
> > - [ ] Rodar o [[ROADMAP_QA_INTEGRACAO]] junto com o Higor assim que a arte de cada bloco chegar — não esperar tudo pronto pra testar.
> >
> > - **Nota (05/10) — entrega do Higor (commit `f282c1b`):** chegaram só arquivos novos: Faísca (`FaiscaSpriteFrames.tres` + 12 PNGs), cenário tropical (fundo, faixas de chão, plataformas, brotos), `TropicalBackdrop.tscn`, shader de folhagem e o painel `ArtReview`. Fundo e restauração já integrados nas 4 áreas (ver `EPIC-C13` e C02-T4). **Não vieram** as edições em arquivos existentes que o README dele descreve: os 11 PNGs do Kairo sem a Faísca embutida (por isso continua aparecendo Faísca duplicada em jogo), o `Faisca.tscn` usando o sprite novo e a textura da luz, e o `TestLevel` com o cenário. Pedir pro Higor subir.
> >
> > - **Nota (11/10) — rodada de integração das entregas de 05/10 a 09/10:** tudo o que o Higor entregou está integrado e o [[ROADMAP_QA_INTEGRACAO]] foi rodado: A00 a A06 passam, exceto as pendências abaixo. Faísca com sprite próprio, Kairo sem a Faísca embutida, inimigos, Korrag v3, espada v11/v12, dash, cenário das 4 áreas, HUD e tela de título aprovados pelo Gustavo (item 4 da revisão). Evidências: `tools/qa/DemoRunCheck.gd` (89/89), `CombatCheck.gd` (35/35), `DosselReachCheck.gd`, `FpsCheck.gd` (60 FPS médios nas 4 áreas).
> > - **Pendências abertas (cada uma cadastrada no roadmap):**
> >   - Higor entregar: wall slide legível ([[ROADMAP_ARTE]] A01-T3), contraste da dica da tela de título (A06-T5), bandeira de checkpoint (A05-T5).
> >   - Higor validar: playtest manual do indicador da espada ([[ROADMAP_QA_INTEGRACAO]] A06-T4) e do menu pelo teclado (A06-T5).
> >   - Gustavo: trocar o visual do checkpoint quando a arte chegar (C09-T3).
> >   - Áudio: nenhum dos 12 `.wav` existe ainda (C11-T2) — bloqueia o [[ROADMAP_QA_INTEGRACAO]] A07.
> >
> > Status: em andamento (11/10) — integração em dia com tudo que foi entregue até 09/10; segue aberta até chegarem as 3 entregas pendentes do Higor e o áudio.

> > [!success]- Task C12-T4 — Travamento aleatório do runtime .NET (`gchandle.is_released`)
> > - [x] Reduzir a chance do travamento no jogo (ver hipóteses abaixo) e medir de novo com `CombatCheck.gd -- --stress`.
> > - [ ] Decidir se vale reportar no GitHub do Godot com a reprodução (opcional, não bloqueia a demo).
> > - **Achado (11/10):** `FATAL: Condition "gchandle.is_released()" is true` em `GodotObject.Finalize()` / `Handle is not initialized` em `SwapGCHandleForType`. É o mesmo erro que o Higor viu ao recarregar o save (09/10) — **não** era efeito do projeto-cópia dele, como foi suposto na C09-T2. Reprodução: `tools/qa/CombatCheck.gd -- --stress` (cria e libera ~40 inimigos com combate) trava quase sempre no mesmo trecho.
> > - **Causa (11/10):** corrida entre a thread do finalizador do GC do .NET e a thread principal, no Godot 4.7.2 (última versão de manutenção; não há 4.7.3). Forçando `GC.Collect()` + `WaitForPendingFinalizers()` na thread principal a cada inimigo, 6/6 execuções passam; sem isso, quase todas travam. Segurar referências aos `.tres` de stats e `SpriteFrames` **não** resolve sozinho.
> > - **Hipóteses de mitigação:** (1) GC sincronizado em pontos seguros (troca de cena no `SceneManager`); (2) diminuir lixo de objetos Godot criados por C# (`CreateTimer(...).Timeout += ...` por golpe, `new StyleBoxFlat` a cada `_Draw` no `HealthBar`/`SwordStatusIndicator`); (3) atualizar o Godot quando sair correção. Na partida jogada de 11/10 o Gustavo não viu o travamento: é raro em jogo normal, mas precisa estar resolvido antes do playtest com gente de fora.
> >
> > - **Causa exata (11/10, depois):** são os **Resources com script C#** (`EnemyStatsResource`, `BossStatsResource`, `BossPhaseResource`, `PlayerStatsResource`; 16 `.tres` em `resources/`). Quando o último nó que usa um deles é liberado (ex.: o `BasicEnemyStats.tres` herdado do `Enemy_Base.tscn` a cada inimigo morto, ou tudo numa troca de cena), o Godot descarrega o Resource; ao carregar de novo enquanto o finalizador ainda libera o objeto C# antigo, o handle corrompe — às vezes o inimigo nasce sem `StatsResource` ("Using defaults") e logo depois trava. A hipótese (2) foi testada e descartada: trocar os `CreateTimer`/`CreateTween`/`StyleBoxFlat` por versões com `Dispose()` não mudou nada (revertido).
> > - **Correção (11/10):** autoload `ResourceKeeper` (`scripts/core/ResourceKeeper.cs`), o **primeiro** da lista, carrega todos os `.tres` de `res://resources` uma vez e os mantém vivos (trata o sufixo `.remap` do jogo exportado). Teste novo de estresse `tools/qa/GcStressCheck.gd` (troca de área + inimigos criados/mortos com combate): antes 6/6 travavam; depois 8/8 completas com 24 ciclos (3.840 golpes). Regra pra frente: **todo `.tres` com script C# novo vai em `res://resources/`**, senão fica fora da proteção.
> >
> > Status: ✅ concluído (11/10) — travamento eliminado nos testes de estresse; falta só confirmar no jogo exportado quando houver build (o `.remap` foi tratado, mas não testado).

---

> [!info]- EPIC-C15 — Deserto pós-demo e chefe cobra — 🔄 Planejado · 🟢 Pós-demo
> *Pronto quando*: após a demo, uma área jogável do Deserto progride do sol à tarde e à noite, culmina numa luta funcional contra a cobra grande e passa nos testes espelhados do [[ROADMAP_QA_CODIGO]] e do [[ROADMAP_QA_INTEGRACAO]] A08.
>
> **Contrato aprovado pelo Higor (15/10):** manter a direção de `docs/art-review/desert-direction-v1.md`. Os três golpes da cobra são bote/mordida frontal, varrida de cauda e mergulho/emergência na areia. Ataques, IA, colisões, dano, fases e progressão são trabalho de código; animações adicionais de cauda/areia estão em [[ROADMAP_ARTE]] A08-T3. A cena `DesertReview.tscn` é somente uma prévia visual e `Boss_CobraDesertoVisual.tscn` não tem combate. Nenhuma dessas tarefas altera a demo já entregue por si só.
>
> > [!todo]- Task C15-T1 — Transformar a prévia do Deserto em área jogável
> > - [ ] Construir o mapa de produção com plataformas, colisões, câmera, entrada/saída e posições definitivas dos três habitantes já apresentados; usar a faixa de terreno e a progressão dia → tarde → noite aprovadas na A08-T1/T2.
> > - [ ] Vincular o avanço de luz ao percurso real, sem mudança brusca ao morrer, recarregar ou retornar a uma seção; preservar o comportamento das quatro áreas da demo.
> > - [ ] Definir com o grupo a posição da entrada pós-demo, checkpoints, pontos de restauração e rota até a arena antes de fechar o layout; registrar essas decisões no GDD.
> >
> > Status: 🟢 liberado (15/10) — base visual aprovada e cena de revisão pronta; topologia e pontos de progressão finais ainda serão definidos pelo grupo.
>
> > [!todo]- Task C15-T2 — Combate próprio da cobra do Deserto
> > - [ ] Criar boss funcional, sem reaproveitar a IA/golpes do Korrag, com estados de preparação, execução, recuperação, dano e morte. Usar `windup`/`attack` da prévia como base visual do bote frontal.
> > - [ ] Implementar os três ataques aprovados: bote/mordida frontal, varrida de cauda e mergulho/emergência na areia. Cada um deve ter aviso legível, janela de dano delimitada e alcance coerente com a animação; o deslocamento subterrâneo não pode deixar hitbox fantasma.
> > - [ ] Integrar as futuras animações e efeitos `tail_sweep`, `burrow_enter` e `burrow_exit` da A08-T3, com fallback temporário enquanto forem produzidos; ajustar vida, dano e cadência por playtest, sem fixar números neste roadmap.
> > - [ ] Reproduzir o sibilo e demais sons de combate em momentos ligados aos golpes, mantendo os volumes compatíveis com música e vento.
> >
> > Status: 🟢 liberado (15/10) — trio de ataques aprovado; protótipo de mecânica pode começar com arte provisória e concluir após a A08-T3.
>
> > [!todo]- Task C15-T3 — Conclusão da luta e progressão pós-chefe
> > - [ ] Definir com o grupo o resultado narrativo, recompensa/desbloqueio e próximo destino após a cobra; registrar a decisão no GDD antes de implementar a transição final.
> > - [ ] Integrar morte do chefe, persistência, checkpoint/retorno e saída da arena sem duplicar recompensa nem reabrir o combate indevidamente.
> > - [ ] Fazer a passagem da demo para o Deserto e do Deserto para o próximo trecho somente quando as rotas forem aprovadas; preservar saves existentes.
> >
> > Status: ⛔ bloqueado (15/10) — falta decidir recompensa e destino depois da cobra; a luta C15-T2 e o mapa C15-T1 podem avançar independentemente.

> [!success]- EPIC-C16 — Mapa explorável da Floresta Tropical — ✅ Implementado · 🟡 Alta
> *Pronto quando*: as quatro áreas atuais aparecem numa cartografia conectada, Kairo vê apenas células percorridas, e o progresso permanece após salvar/continuar sem quebrar as transições da demo.
>
> > [!success]- Task C16-T1 — Revelação e persistência do mapa
> > - [x] Adicionar `M` para abrir/fechar mapa, pausando a partida; `Esc` também fecha o mapa.
> > - [x] Associar posição de Kairo a células de cada área e registrar somente as visitadas em `ExploredMapCells` no save, compatível com saves antigos sem esse campo.
> > - [x] Mostrar marcador atual, nomes e ligações apenas quando explorados; salvar descobertas novas automaticamente.
> > - **Verificação (15/10):** `WorldMapCheck.gd` passa abertura/fechamento, névoa e persistência; `SaveReloadCheck.gd` mantém o fluxo anterior.
> >
> > Status: ✅ concluído (15/10) — mapa funcional nas quatro áreas da demo; arte em revisão na A09-T1.
>
> > [!success]- Task C16-T2 — Rede de ida e volta entre áreas existentes
> > - [x] Preservar Sopé ↔ Dossel ↔ Igarapé e a subida/descida do Dossel.
> > - [x] Adicionar retorno Covil → Igarapé, com ponto de chegada afastado do portal para não criar loop de troca de cena.
> > - [x] Verificar o percurso original e o retorno sem regressão de spawn/save.
> > - **Verificação (15/10):** `DemoRunCheck.gd` 89/89 e `WorldMapCheck.gd` confirma destino e chegada do retorno.
> >
> > Status: ✅ concluído (15/10) — as quatro cenas atuais formam uma rota bidirecional, com deslocamento horizontal e vertical pelo Dossel. Ramificações inéditas dependem de novas salas/áreas, ainda não desenhadas.
