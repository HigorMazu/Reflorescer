# Roadmap de Desenvolvimento — Código (Gustavo)

Backlog de execução da demo: Épicos (fases, ordenados pela dependência real entre as partes — fundação técnica antes de qualquer mecânica nova, restauração ODS15 e boss logo depois porque são os gaps de maior prioridade) que abrem em Tasks, que abrem em Sub-tasks. Cada sub-task é um checkbox — marcar conforme for implementado. Sempre que uma Task gerar uma nota de desenvolvimento (bugfix, decisão, mudança de escopo) durante o trabalho, ela é registrada ali mesmo, no formato **Tipo (DD/MM) — resumo**, logo abaixo do checklist da Task. Se a nota for uma decisão de design (não só um detalhe técnico de implementação), ela também é replicada na documentação oficial do Notion (regra do topo da página lá). A ordem dos épicos é uma sugestão, ajustável a qualquer momento já que o código é do Gustavo.

Cada Épico aqui tem um Épico **gêmeo de mesmo ID** no `ROADMAP_QA_CODIGO.md` (ex: `EPIC-C03`), com os casos de teste que definem "pronto" de cada Task. Fluxo: implementar as sub-tasks de uma Task → rodar as sub-tasks de verificação da Task equivalente no QA → escrever a linha `Status:` → seguir pra próxima.

Ordem recomendada: `EPIC-C01` → `EPIC-C02` → `EPIC-C06` → `EPIC-C04` → `EPIC-C03` → `EPIC-C05` → `EPIC-C07` → `EPIC-C08` → `EPIC-C09` → `EPIC-C10` → `EPIC-C11` → `EPIC-C12`. C03, C05 e C07 não têm dependência forte entre si — dá pra reordenar ou paralelizar, desde que C01 já esteja fechado.

---

<details>
<summary><strong>EPIC-C01 — Fundação Técnica</strong> ⬜ Não iniciado · 🔴 Bloqueante</summary>

*Pronto quando*: o Input Map bate 100% com a seção 7 do Notion, só existe um sistema de movimento rodando (a `PlayerStateMachine`/`PlayerStates.cs` antiga foi removida ou virou a única fonte), e o double jump tem uma única fonte de verdade.

Bloqueante — fazer antes de tudo. O Input Map real não bate com a documentação confirmada, e existem dois sistemas de movimento rodando em paralelo (`PlayerController.HandlePrototypeMovement` + `PlayerStateMachine`/`PlayerStates.cs`) — cada mecânica nova escrita em cima disso dobra o risco de bug.

<details>
<summary>Task C01-T1 — Corrigir o Input Map</summary>

- [ ] Remapear em Project Settings → Input Map pra bater com a seção 7 do Notion: A/D mover, Espaço pular, Shift dash, Q espada, E interagir, K ataque básico, segurar J especial.
- [ ] Grep em `scripts/` por `IsActionJustPressed`/`IsActionPressed` com os nomes antigos de ação e corrigir qualquer referência que sobrar.
- **Achado da auditoria (15/09):** hoje `attack`=J+clique, `dash`=K+seta-baixo, `jump`=Espaço/W/seta-cima, `ability`=L (sem uso). Sem S nem Q mapeados.

Status: não iniciado.

</details>

<details>
<summary>Task C01-T2 — Unificar o sistema de movimento</summary>

- [ ] Decidir: manter o caminho direto (`HandlePrototypeMovement`, já testado) e apagar `PlayerStateMachine`/`PlayerStates.cs`, **ou** migrar de vez pra state machine e remover o caminho direto.
- [ ] Executar a remoção/migração escolhida.
- [ ] Rodar a Task C01-T2 do `ROADMAP_QA_CODIGO.md` (regressão completa de movimento e física) antes de seguir.
- **Recomendação:** manter o caminho direto — é o de menor risco a essa altura do projeto.

Status: não iniciado.

</details>

<details>
<summary>Task C01-T3 — Fonte única de verdade pro Double Jump</summary>

- [ ] Escolher uma fonte entre `PlayerController.InitDoubleJump()` (fallback hardcoded) e `AbilityManager.UnlockPrototypeAbilities()` — recomendo `AbilityManager`, é o sistema que vai crescer com o Dash.
- [ ] Remover o fallback duplicado do `PlayerController`.

Status: não iniciado.

</details>

</details>

<details>
<summary><strong>EPIC-C02 — Mecânica de Restauração ODS15</strong> ⬜ Não iniciado · 🔴 Bloqueante</summary>

*Pronto quando*: existe pelo menos 1 ponto de restauração interagível em cada uma das 3 áreas da demo, com efeito visual próprio, evento dedicado no `EventBus`, e persistência em save.

Prioridade máxima de conteúdo — sem isso a demo perde a conexão com o pitch (ODS 15). Hoje `Checkpoint` é só um save point genérico, sem nenhum conceito de "restaurar" o bioma.

<details>
<summary>Task C02-T1 — Definir e implementar a interação</summary>

- [ ] Decidir se a restauração é o próprio `Checkpoint` reaproveitado (com um estado temático a mais) ou uma classe nova (`RestorationPoint : IInteractable`) separada do save point.
- [ ] Implementar a interação escolhida.

Status: não iniciado.

</details>

<details>
<summary>Task C02-T2 — Efeito visual/de mundo</summary>

- [ ] Implementar o efeito da restauração (mesmo simples pra demo: partícula + mudança de cor no tile ao redor, ou sprite "antes/depois").

Status: não iniciado.

</details>

<details>
<summary>Task C02-T3 — Evento dedicado e persistência</summary>

- [ ] Emitir um evento próprio no `EventBus` (ex: `AreaRestored(string pointId)`) — não reaproveitar `CheckpointActivated`.
- [ ] Persistir pontos restaurados no save, igual `ActivatedCheckpoints` já persiste.

Status: não iniciado.

</details>

<details>
<summary>Task C02-T4 — Posicionar nas 3 áreas da demo</summary>

- [ ] Definir quantos pontos por área (mínimo 1 por bioma) e posicionar nas cenas de Floresta Tropical, Deserto e Tundra.

Status: não iniciado.

</details>

</details>

<details>
<summary><strong>EPIC-C03 — Combate e Espada de Grama</strong> ⬜ Não iniciado · 🟠 Alta</summary>

*Pronto quando*: Q alterna a espada em tempo real (visual + `HasSword`), o jogador tem stats diferentes com a espada ativada/desativada, e não dá pra atacar com ela desativada.

<details>
<summary>Task C03-T0 — Regressão de combate básico (antes de mexer)</summary>

- [ ] Rodar a Task C03-T0 do `ROADMAP_QA_CODIGO.md` (ataque, hitbox/hurtbox, cooldown, invulnerabilidade) pra confirmar a base antes de alterar.

Status: não iniciado.

</details>

<details>
<summary>Task C03-T1 — Wire do input Q (ativar/desativar)</summary>

- [ ] Ligar a ação `Q` (pós `EPIC-C01-T1`) a um método novo `ToggleSword()` em `PlayerController`, chamando `HasSword = !HasSword` e `SetSwordVisible(HasSword)`.

Status: não iniciado.

</details>

<details>
<summary>Task C03-T2 — Stats duplos (ativada/desativada)</summary>

- [ ] Criar a variante "espada desativada": +velocidade, +pulo, sem ataque.
- **Opção simples:** dois blocos de valores no `PlayerStatsResource` (`MoveSpeedSwordOn`/`Off`, etc.) ou um segundo `.tres`.

Status: não iniciado.

</details>

<details>
<summary>Task C03-T3 — Bloqueio e teste em combate real</summary>

- [ ] Bloquear `CanAttack()` quando `HasSword == false` (parcialmente já verdade hoje).
- [ ] Testar a troca em pleno combate/movimento — sem travar animação/input no meio da troca.
- [ ] Rodar de novo a Task C03-T0 pra garantir que nada regrediu.

Status: não iniciado.

</details>

</details>

<details>
<summary><strong>EPIC-C04 — Dash e Desbloqueio Pós-Korrag</strong> ⬜ Não iniciado · 🔴 Bloqueante</summary>

*Pronto quando*: derrotar o Korrag desbloqueia o dash uma única vez, e apertar Shift com o dash desbloqueado move o Kairo com um impulso rápido na direção que ele olha.

Depende de `EPIC-C01` (Input Map correto).

<details>
<summary>Task C04-T1 — Corrigir o evento BossDefeated duplicado</summary>

- [ ] `BossBase.OnDied()` chama `EmitDefeated()` diretamente **e** `BossDeadState.Enter()` chama de novo — remover uma das duas (sugestão: manter só a de `BossDeadState.Enter()`).
- **Fazer antes da Task C04-T3**, senão o dash pode ser "desbloqueado" duas vezes sem causar bug visível, mas é sujeira desnecessária.
- Mesma correção referenciada em `EPIC-C06-T2` — fazer uma vez só.

Status: não iniciado.

</details>

<details>
<summary>Task C04-T2 — Implementar o movimento do dash</summary>

- [ ] Impulso horizontal rápido na direção que o Kairo está olhando, com cooldown e talvez i-frames curtos (típico de Metroidvania).

Status: não iniciado.

</details>

<details>
<summary>Task C04-T3 — Ligar ao AbilityManager e ao evento do boss</summary>

- [ ] Input: `Input.IsActionJustPressed("dash") && AbilityManager.Instance.HasAbility(AbilityId.Dash)`.
- [ ] Assinar `EventBus.BossDefeated` (sugestão: no próprio `AbilityManager`, ou um `DemoProgressionManager` novo) e chamar `UnlockAbility(AbilityId.Dash)` quando `bossId == "boss_javali"`.

Status: não iniciado.

</details>

<details>
<summary>Task C04-T4 — Notificação de habilidade desbloqueada</summary>

- [ ] Dar um retorno visual mínimo (texto na tela por 2-3s) — hoje `HUDController.ShowAbilityUnlockNotification` só imprime no console.

Status: não iniciado.

</details>

</details>

<details>
<summary><strong>EPIC-C05 — Wall Grab / Wall Jump</strong> ⬜ Não iniciado · 🟠 Alta</summary>

*Pronto quando*: segurar A/D contra uma parede no ar reduz a queda, e pular nessa condição empurra o Kairo pro lado oposto.

<details>
<summary>Task C05-T1 — Detecção de parede</summary>

- [ ] Detectar colisão lateral com parede no ar (`IsOnWall()` do Godot ou raycast lateral) em `PlayerController`.

Status: não iniciado.

</details>

<details>
<summary>Task C05-T2 — Agarrar / slide</summary>

- [ ] Enquanto segurando A/D contra a parede no ar, reduzir a velocidade de queda (não necessariamente travar em 0).

Status: não iniciado.

</details>

<details>
<summary>Task C05-T3 — Pulo de parede</summary>

- [ ] Pular estando agarrado empurra pro lado oposto da parede, impulso vertical semelhante ao pulo normal.

Status: não iniciado.

</details>

<details>
<summary>Task C05-T4 — Decisão de gating</summary>

- [ ] Decidir se é liberado desde o início ou gated por uma `AbilityId` (`WallJump` já existe no enum) — **pendência a decidir com o Higor/documentar no Notion** antes de travar o design final.

Status: não iniciado.

</details>

</details>

<details>
<summary><strong>EPIC-C06 — Boss Korrag (IA e Identidade)</strong> ⬜ Não iniciado · 🔴 Bloqueante</summary>

*Pronto quando*: o Korrag usa Charge e Stomp em algum momento da luta (não só o ataque genérico), o `BossName` no código é "Korrag, o Javali", e o evento de derrota dispara uma única vez.

**Achado da auditoria:** a troca de fase por HP já funciona sozinha (`BossStateMachine`). O trabalho real é só **conectar os ataques que já existem** (`StartCharge`/`PerformStomp` em `JavaliBoss.cs`) — hoje nenhum estado os chama.

<details>
<summary>Task C06-T0 — Regressão do núcleo do boss (antes de mexer)</summary>

- [ ] Rodar a Task C06-T0 do `ROADMAP_QA_CODIGO.md` (ativação, intro, fases automáticas, contato, morte em 8 hits).

Status: não iniciado.

</details>

<details>
<summary>Task C06-T1 — Loop de decisão de ataque</summary>

- [ ] Em `BossPhase1State`/`BossPhase2State`/`BossEnragedState`, trocar a chamada genérica `Boss.PerformAttack()` por uma decisão entre `PerformAttack()` (melee), `StartCharge()` (investida) e `PerformStomp()` (pisada em área).
- **Sugestão de implementação:** método virtual `ChooseAttack()` em `BossBase`, sobrescrito por `JavaliBoss` — pode começar simples (sorteio ponderado ou por distância).

Status: não iniciado.

</details>

<details>
<summary>Task C06-T2 — Corrigir o evento BossDefeated duplicado</summary>

- [ ] Mesma correção da `EPIC-C04-T1` — fazer uma vez só, referenciar aqui.

Status: não iniciado.

</details>

<details>
<summary>Task C06-T3 — Corrigir o nome oficial</summary>

- [ ] Atualizar `BossName` em `resources/bosses/JavaliStatsResource.tres` de `"Javali das Ruínas"` para `"Korrag, o Javali"`.

Status: não iniciado.

</details>

<details>
<summary>Task C06-T4 — Ajustar timing com a arte real</summary>

- [ ] Testar o `charge_windup` (1s) contra a animação real assim que a arte chegar (cruza com `ROADMAP_QA_INTEGRACAO.md` `EPIC-A04`).

Status: não iniciado.

</details>

</details>

<details>
<summary><strong>EPIC-C07 — Inimigos Comuns (Arquétipos)</strong> ⬜ Não iniciado · 🟠 Alta</summary>

*Pronto quando*: existem 3 arquétipos (Voador/Rápido/Robusto) com comportamento distinto de fato, cada um com `.tres` de stats pra cada uma das 3 áreas da demo.

<details>
<summary>Task C07-T0 — Regressão do comportamento base (antes de mexer)</summary>

- [ ] Rodar a Task C07-T0 do `ROADMAP_QA_CODIGO.md` (idle/patrol/detect/chase/attack/hurt/dead do `EnemyBase` genérico).

Status: não iniciado.

</details>

<details>
<summary>Task C07-T1 — Voador</summary>

- [ ] Criar `EnemyVoador : EnemyBase` — sobrescrever gravidade/colisão pra voar (ignorar `ApplyGravity` ou usar versão sem gravidade, `Y` fixo/oscilante).

Status: não iniciado.

</details>

<details>
<summary>Task C07-T2 — Rápido</summary>

- [ ] Avaliar se basta um `.tres` novo (`ChaseSpeed`/`PatrolSpeed` mais altos, já 100% data-driven) ou se precisa de subclasse de código.

Status: não iniciado.

</details>

<details>
<summary>Task C07-T3 — Robusto</summary>

- [ ] `.tres` com `MaxHealth`/`KnockbackResistance` mais altos; avaliar se precisa de código extra (ex: ignorar knockback abaixo de um threshold).

Status: não iniciado.

</details>

<details>
<summary>Task C07-T4 — Dados por arquétipo × área da demo</summary>

- [ ] Criar os `.tres` de `EnemyStatsResource` pra cada arquétipo × cada uma das 3 áreas (Floresta Tropical primeiro).

Status: não iniciado.

</details>

<details>
<summary>Task C07-T5 — (baixa prioridade) AtPatrolEdge()</summary>

- [ ] Hoje sempre retorna `false` — corrigir só se o comportamento "parar depois de patrulhar" for desejado. Aceitável pra demo como está.

Status: não iniciado.

</details>

</details>

<details>
<summary><strong>EPIC-C08 — Transição de Área</strong> ⬜ Não iniciado · 🟠 Alta</summary>

*Pronto quando*: o jogador chega na cena nova numa posição consistente com de onde veio, não sempre no spawn padrão.

<details>
<summary>Task C08-T0 — Regressão básica do trigger</summary>

- [ ] Rodar a Task C08-T0 do `ROADMAP_QA_CODIGO.md` (trigger carrega a cena, erro tratado se a cena não existe).

Status: não iniciado.

</details>

<details>
<summary>Task C08-T1 — Spawn direcional</summary>

- [ ] **Opção A:** implementar o uso de `TargetArea`/`SpawnOffset` (já existem exportados em `TransitionTrigger`, não são lidos) pra posicionar o jogador no spawn correspondente.
- [ ] **Opção B** (mais simples se o tempo apertar): nomear spawn points por origem (`SpawnFromFlorestaTropical`, `SpawnFromDeserto`) e escolher pelo nome de onde o jogador veio.

Status: não iniciado.

</details>

</details>

<details>
<summary><strong>EPIC-C09 — Save, Checkpoint e Respawn</strong> ⬜ Não iniciado · 🟡 Média</summary>

*Pronto quando*: uma habilidade desbloqueada persiste em disco mesmo sem passar por um checkpoint depois, e existe uma decisão explícita (implementada ou documentada como "fora de escopo") sobre o fluxo de Continuar.

<details>
<summary>Task C09-T0 — Regressão básica (antes de mexer)</summary>

- [ ] Rodar a Task C09-T0 do `ROADMAP_QA_CODIGO.md` (ativar checkpoint, salvar, morrer/respawnar).

Status: não iniciado.

</details>

<details>
<summary>Task C09-T1 — Persistir unlock de habilidade imediatamente</summary>

- [ ] Hoje só atualiza a lista em memória até o próximo save por checkpoint/boss/item — garantir `SaveGame()` também no unlock, ou documentar a decisão de não fazer isso pra demo.

Status: não iniciado.

</details>

<details>
<summary>Task C09-T2 — Fluxo de "Continuar"</summary>

- [ ] Se a demo tiver esse menu: chamar `SaveManager.LoadGame(slot)` e, em `ApplySaveData`, trocar de cena pra `save.CurrentScene` antes de restaurar posição/HP (hoje não troca).
- [ ] Se a demo **não** vai ter esse menu (sempre começa do zero): documentar essa decisão no Notion.

Status: não iniciado.

</details>

</details>

<details>
<summary><strong>EPIC-C10 — HUD e UX</strong> ⬜ Não iniciado · 🟡 Média</summary>

*Pronto quando*: um prompt visual aparece ao chegar perto de um `IInteractable` (sem precisar apertar nada), e a decisão sobre Game Over foi tomada e implementada.

<details>
<summary>Task C10-T0 — Regressão básica de HUD</summary>

- [ ] Rodar a Task C10-T0 do `ROADMAP_QA_CODIGO.md` (barra de vida, pausa).

Status: não iniciado.

</details>

<details>
<summary>Task C10-T1 — Prompt de interação contínuo</summary>

- [ ] Fazer o `PlayerController` (ou um `InteractionDetector` dedicado) chamar `HUDController.ShowInteractionPrompt()`/`HideInteractionPrompt()` continuamente com base em estar ou não sobrepondo um `IInteractable` — hoje só reage ao apertar E.

Status: não iniciado.

</details>

<details>
<summary>Task C10-T2 — Game Over</summary>

- [ ] Decidir se a demo precisa de tela de Game Over real ou se o respawn automático (já funcional) basta. Se precisar, implementar o que `ShowGameOver()` deveria de fato disparar.

Status: não iniciado.

</details>

</details>

<details>
<summary><strong>EPIC-C11 — Áudio</strong> ⬜ Não iniciado · 🟡 Média</summary>

*Pronto quando*: as pastas/buses de áudio existem, os 3 sons já esperados pelo código tocam de verdade, e a demo inteira tem pelo menos uma música por bioma e feedback sonoro nos hits principais.

Responsabilidade do Gustavo.

<details>
<summary>Task C11-T1 — Estrutura de pastas e buses</summary>

- [ ] Criar `res://assets/audio/music/` e `res://assets/audio/sfx/`.
- [ ] Configurar os buses "Music" e "SFX" no Godot (Audio → Buses) — o código já assume que existem.

Status: não iniciado.

</details>

<details>
<summary>Task C11-T2 — Arquivos já esperados pelo código</summary>

- [ ] Produzir/conseguir os 3 arquivos já chamados no código (nomes exatos, case-sensitive): `checkpoint_activate.wav`, `boss_stomp.wav`, `boss_defeat.wav`.

Status: não iniciado.

</details>

<details>
<summary>Task C11-T3 — Chamadas que faltam</summary>

- [ ] Adicionar `AudioManager.PlaySfx(...)` pro ataque do jogador, hit em inimigo/jogador, morte de inimigo comum, pulo (opcional).
- [ ] Música de fundo por bioma via `AudioManager.PlayMusic(...)` (fade in/out já pronto no código).

Status: não iniciado.

</details>

</details>

<details>
<summary><strong>EPIC-C12 — Regressão Final / Release da Demo</strong> ⬜ Não iniciado · 🔴 Bloqueante</summary>

*Pronto quando*: o `ROADMAP_QA_CODIGO.md` inteiro passa, o `ROADMAP_QA_INTEGRACAO.md` inteiro passa, e a demo roda de ponta a ponta sem crash na frente de alguém de fora do grupo.

Só começa depois que todos os épicos 🔴/🟠 acima estiverem concluídos.

<details>
<summary>Task C12-T1 — Regressão completa de código</summary>

- [ ] Rodar o `ROADMAP_QA_CODIGO.md` inteiro, de ponta a ponta, e corrigir qualquer regressão.

Status: não iniciado.

</details>

<details>
<summary>Task C12-T2 — Handoff pro Higor</summary>

- [ ] Entregar os blocos de mecânica que faltavam pra ele terminar a arte condicional (ex: como fica visualmente o wall grab, pra desenhar a animação certa).

Status: não iniciado.

</details>

<details>
<summary>Task C12-T3 — Integração incremental</summary>

- [ ] Rodar o `ROADMAP_QA_INTEGRACAO.md` junto com o Higor assim que a arte de cada bloco chegar — não esperar tudo pronto pra testar.

Status: não iniciado.

</details>

</details>
