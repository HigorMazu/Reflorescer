# Roadmap de QA — Código (Reflorescer)

Casos de teste que definem "pronto" pra cada Task do `ROADMAP_DEV.md` — mesma hierarquia Épico → Task → Subtask, mesmos IDs (`EPIC-Cxx` / `Cxx-Ty`), espelhados 1:1. A diferença de conteúdo: lá as subtasks são passos de implementação, aqui são **casos de teste**. Cada Task fecha com uma linha `Status:` — mas aqui ela reflete o resultado real da auditoria de código de 15/09/2026 (✅ passa hoje / 🔴 falha, funcionalidade não existe / 🟡 parcial ou implementado com bug conhecido), não "não iniciado", porque isso é o estado atual do jogo, não um checklist de trabalho. Sempre que rodar essa Task de novo depois de uma mudança e o resultado for diferente do registrado aqui, atualizar o checkbox, o ícone de status e a linha `Status:` — e, se a mudança revelar uma decisão nova (não só um bug), registrar também no Notion (regra do topo da página lá).

Testa **só a parte do Gustavo** (código/mecânicas), com arte placeholder — nada aqui depende de sprite final do Higor, isso é o `ROADMAP_QA_INTEGRACAO.md`.

Legenda de status dos casos de teste: ✅ já passa hoje · 🔴 vai falhar, não existe ainda · 🟡 parcial/bug conhecido. Legenda de prioridade do Épico: 🔴 Bloqueante pra demo · 🟠 Alta · 🟡 Média · 🟢 Baixa/pós-demo.

---

<details>
<summary><strong>EPIC-C01 — Fundação Técnica</strong> 🔴 Falha · 🔴 Bloqueante</summary>

*Pronto quando*: todos os casos de teste abaixo passam ✅ — Input Map bate 100% com a seção 7 do Notion, ataque nunca dispara duas vezes, e só existe uma fonte de verdade pro double jump.

<details>
<summary>Task C01-T1 — Input Map bate com a documentação</summary>

- [ ] Todas as ações lidas em `scripts/` (`move_left`, `move_right`, `jump`, `attack`, `dash`, `interact`, `ability`) correspondem às teclas da seção 7 do Notion.
- [ ] Nenhum script ficou lendo uma ação com o nome antigo depois do remapeamento.
- **Achado da auditoria (15/09):** hoje `attack`=J+clique, `dash`=K+seta-baixo, `jump`=Espaço/W/seta-cima, `ability`=L (sem uso). Sem S nem Q mapeados — bate com nada da seção 7.

Status: 🔴 falha — Input Map real não bate com a documentação confirmada.

</details>

<details>
<summary>Task C01-T2 — Regressão de movimento após unificar</summary>

- [x] Segurar mover esquerda/direita no chão → acelera até 200px/s, desacelera ao soltar.
- [x] Pular parado (chão) → `Velocity.Y = -460` instantânea.
- [x] Soltar o botão de pular cedo → corta a velocidade pra metade se `Velocity.Y < -80` (jump cut).
- [x] Sair de uma plataforma sem pular → ainda dá pra pular por ~0.15s (coyote time).
- [x] Apertar pular ~0.1s antes de tocar o chão → pulo executa ao tocar (jump buffer, 0.12s).
- [x] Cair de queda longa → `Velocity.Y` nunca passa de 650; gravidade de queda é 1.6x a de subida.
- [x] Virar de direção rapidamente → visual vira instantaneamente, sem input travado.
- [ ] Apertar ataque uma única vez → `PerformAttack()`/`EnableHitbox()` executa **1 vez só**.
- **Achado da auditoria (15/09):** os 7 primeiros casos passam hoje. O último é risco real — `PlayerController.HandlePrototypeMovement()` e a `PlayerStateMachine`/`PlayerStates.cs` antiga rodam em paralelo, e `AttackState.Enter()` também chama `Player.PerformAttack()`, então um único input pode disparar o ataque 2x.
- [ ] Depois da `EPIC-C01-T2` do Dev (remoção do sistema duplicado): repetir todos os casos acima — nada pode ter regredido.
- **Achado (28/09) — os 7 casos de física não rodavam com esses valores:** a state machine antiga também chamava `ApplyGravity` + `MoveAndSlide`, então a física rodava 2x por frame (deslocamento e gravidade dobrados). A `EPIC-C01-T2` do Dev removeu isso. Os números acima (200px/s, -460, 650) passam a valer de verdade só agora, e o movimento vai parecer mais lento que no playtest antigo. Repetir todos os casos.

Status: 🟡 parcial — física de movimento passa, mas ataque duplicado é risco real enquanto os dois sistemas coexistirem.

</details>

<details>
<summary>Task C01-T3 — Double Jump com fonte única</summary>

- [ ] Pular de novo no ar sem tocar o chão → disponível vindo de **uma única fonte** de verdade.
- **Achado da auditoria (15/09):** hoje sempre disponível, mas por duas fontes concorrentes ao mesmo tempo — `PlayerController.InitDoubleJump()` (fallback hardcoded, intencional só pro protótipo) e `AbilityManager.UnlockPrototypeAbilities()`. Funciona por acidente, não por design.

Status: 🟡 parcial — double jump funciona, mas sem fonte única de verdade.

</details>

</details>

<details>
<summary><strong>EPIC-C02 — Mecânica de Restauração ODS15</strong> 🔴 Falha · 🔴 Bloqueante</summary>

*Pronto quando*: todos os casos de teste abaixo passam ✅ — ponto de restauração interagível nas 3 áreas, efeito visual próprio, evento dedicado no `EventBus`, e persistência em save.

<details>
<summary>Task C02-T1 — Interação de restauração existe</summary>

- [ ] Existe um ponto de restauração interagível (planta/cura/reativa) em pelo menos 1 lugar de cada uma das 3 áreas da demo.
- **Achado da auditoria (15/09):** não existe — `Checkpoint` é só save point genérico, sem nenhum conceito de "restaurar" o bioma.

Status: 🔴 falha — mecânica não implementada.

</details>

<details>
<summary>Task C02-T2 — Efeito visual/de mundo</summary>

- [ ] Interagir com o ponto dispara um efeito visual/de mundo perceptível (partícula, mudança de cor no tile, sprite antes/depois).

Status: 🔴 falha — depende da Task C02-T1 existir primeiro.

</details>

<details>
<summary>Task C02-T3 — Evento dedicado e persistência</summary>

- [ ] Emite um evento próprio no `EventBus` (não reaproveita `CheckpointActivated`).
- [ ] Pontos restaurados persistem no save (fechar/reabrir o jogo mantém o estado restaurado).

Status: 🔴 falha — nada disso existe hoje.

</details>

<details>
<summary>Task C02-T4 — Cobertura nas 3 áreas da demo</summary>

- [ ] Pelo menos 1 ponto de restauração ativo e testável em Floresta Tropical, Deserto e Tundra.

Status: 🔴 falha — nenhuma área tem pontos de restauração ainda.

</details>

</details>

<details>
<summary><strong>EPIC-C03 — Combate e Espada de Grama</strong> 🔴 Falha · 🟠 Alta</summary>

*Pronto quando*: todos os casos de teste abaixo passam ✅ — Q alterna a espada em tempo real, stats diferentes com espada ativada/desativada, e não dá pra atacar com ela desativada.

<details>
<summary>Task C03-T0 — Regressão de combate básico (baseline antes de mexer)</summary>

- [x] Atacar sem inimigo por perto → hitbox ativa 0.14s, sem dano, sem erro no console.
- [x] Atacar um inimigo no alcance → recebe 25 de dano, flash vermelho, knockback, hitstop (`TimeScale=0.15` por 0.06s).
- [x] Atacar repetidamente sem soltar o botão → cada inimigo só recebe dano **uma vez** por ativação de hitbox.
- [x] Atacar durante o cooldown (0.30s) → `CanAttack()` falso, ataque não sai.
- [x] Tomar dano de um inimigo → HP cai, knockback, 1s de invulnerabilidade, shake visual.
- [x] Tomar dano durante a invulnerabilidade pós-hit → segundo hit ignorado.

Status: ✅ passa — baseline de combate sólida, segura pra construir o toggle da espada em cima.

</details>

<details>
<summary>Task C03-T1 — Toggle da espada (Q)</summary>

- [ ] Apertar Q alterna `HasSword` e a visibilidade do sprite da espada/bandagem.
- **Achado da auditoria (15/09):** `SetSwordVisible()` existe mas nunca é chamado depois do `_Ready()` — não há input ligado a ele ainda.

Status: 🔴 falha — toggle não existe, `HasSword=true` é hardcoded.

</details>

<details>
<summary>Task C03-T2 — Stats duplos</summary>

- [ ] Com a espada desativada: +velocidade, +pulo, sem ataque.
- **Achado da auditoria (15/09):** só existe um conjunto de stats em `PlayerStatsResource`, sem variante "sem espada".

Status: 🔴 falha — não existe a variante de stats.

</details>

<details>
<summary>Task C03-T3 — Bloqueio e regressão final</summary>

- [ ] Tentar atacar com a espada desativada → bloqueado.
- [ ] Alternar a espada em pleno combate/movimento → sem travar animação/input.
- [ ] Repetir a Task C03-T0 → nada regrediu.

Status: 🔴 bloqueado — não testável até C03-T1/T2 existirem.

</details>

</details>

<details>
<summary><strong>EPIC-C04 — Dash e Desbloqueio Pós-Korrag</strong> 🔴 Falha · 🔴 Bloqueante</summary>

*Pronto quando*: todos os casos de teste abaixo passam ✅ — `BossDefeated` dispara uma única vez, dash tem movimento real, é desbloqueado só ao derrotar o Korrag, e a UI avisa o jogador.

<details>
<summary>Task C04-T1 — Evento BossDefeated único</summary>

- [ ] Derrotar o Korrag emite `BossDefeated` **uma vez só**.
- **Achado da auditoria (15/09):** dispara duas vezes — `BossBase.OnDied()` e `BossDeadState.Enter()` chamam `EmitDefeated()` cada um, de forma independente. Mesmo achado documentado em `EPIC-C06-T2` — é a mesma correção, contar uma vez só no cômputo geral.

Status: 🟡 parcial — o evento existe e funciona, mas duplicado.

</details>

<details>
<summary>Task C04-T2 — Movimento do dash existe</summary>

- [ ] Apertar Shift (pós remapeamento) com a habilidade desbloqueada → impulso horizontal rápido na direção que o Kairo olha.
- **Achado da auditoria (15/09):** nenhuma implementação de movimento associada ao dash — `AbilityId.Dash` existe só como enum, sem código de movimento atrás.

Status: 🔴 falha — dash não implementado.

</details>

<details>
<summary>Task C04-T3 — Desbloqueio ao derrotar o Korrag</summary>

- [ ] Derrotar o Korrag → `AbilityManager.UnlockAbility(AbilityId.Dash)` é chamado.
- [ ] Antes de derrotar o Korrag → apertar dash não faz nada (habilidade ainda bloqueada).
- **Achado da auditoria (15/09):** nenhum script escuta `EventBus.BossDefeated` pra isso. Quando for implementado, o listener precisa ser resistente ao evento disparar 2x até a Task C04-T1 ser corrigida — senão risco de desbloquear com efeito colateral duplicado.

Status: 🔴 falha — desbloqueio não implementado.

</details>

<details>
<summary>Task C04-T4 — Notificação de UI</summary>

- [ ] Ao desbloquear o dash → alguma notificação visual aparece na tela.
- **Achado da auditoria (15/09):** só `GD.Print` no console hoje, nenhuma UI real.

Status: 🔴 falha — sem feedback visual.

</details>

</details>

<details>
<summary><strong>EPIC-C05 — Wall Grab / Wall Jump</strong> 🔴 Falha · 🟠 Alta</summary>

*Pronto quando*: todos os casos de teste abaixo passam ✅ — segurar A/D contra a parede no ar reduz a queda, e pular nessa condição empurra pro lado oposto.

<details>
<summary>Task C05-T1 — Detecção de parede</summary>

- [ ] Personagem no ar encostando numa parede lateral → estado de "encostado" é detectado.
- **Achado da auditoria (15/09):** não existe nenhuma detecção de parede em `PlayerController` hoje.

Status: 🔴 falha — mecânica não implementada.

</details>

<details>
<summary>Task C05-T2 — Agarrar parede / slide</summary>

- [ ] Segurar A/D contra uma parede no ar → reduz velocidade de queda.
- **Achado da auditoria (15/09):** não existe — o personagem só cai normalmente.

Status: 🔴 falha — mecânica não implementada.

</details>

<details>
<summary>Task C05-T3 — Pulo de parede</summary>

- [ ] Pular estando "agarrado" → empurra pro lado oposto da parede.
- **Achado da auditoria (15/09):** não existe.

Status: 🔴 falha — depende de C05-T1/T2 existirem primeiro.

</details>

<details>
<summary>Task C05-T4 — Gating da habilidade</summary>

- [ ] Se gated por `AbilityId.WallJump`: usar antes de desbloqueado não faz nada. Se liberado desde o início: funciona sem pré-requisito.
- **Nota:** decisão de design ainda em aberto (ver `ROADMAP_DEV.md` C05-T4) — este caso só é testável depois que a decisão for tomada e documentada no Notion.

Status: 🔴 bloqueado — decisão de escopo pendente.

</details>

</details>

<details>
<summary><strong>EPIC-C06 — Boss Korrag (IA e Identidade)</strong> 🔴 Falha · 🔴 Bloqueante</summary>

*Pronto quando*: todos os casos de teste abaixo passam ✅ — o Korrag usa Charge e Stomp durante a luta, o `BossName` é "Korrag, o Javali", e o evento de derrota dispara uma única vez.

<details>
<summary>Task C06-T0 — Regressão do núcleo do boss (baseline antes de mexer)</summary>

- [x] Jogador chega a <500px do Korrag → ativa automaticamente, barra de vida aparece.
- [x] Entrada da luta → estado `Intro` por 2s antes de lutar de verdade.
- [x] HP cai a 100 (50%) → transição automática pra fase 2 (`SetPhase(1)`).
- [x] HP cai a 40 (20%) → transição automática pra "Enraged" (`SetPhase(2)`).
- [x] Contato direto com o corpo do boss (raio 55px) → 22 de dano a cada ~0.9s se não invulnerável.
- [x] Boss recebe 8 ataques básicos do jogador (25×8=200) → morre exatamente no 8º hit.
- **Correção de uma nota antiga da auditoria:** a troca de fase por HP já funciona sozinha via `BossStateMachine` (`BossPhase1State`/`BossPhase2State`/`BossEnragedState` chamam `SetPhase()` automaticamente) — não é um gap, ao contrário do que uma versão anterior deste documento registrava.

Status: ✅ passa — núcleo do boss (ativação, intro, fases, dano, morte) está sólido.

</details>

<details>
<summary>Task C06-T1 — Loop de decisão de ataque</summary>

- [ ] O boss usa a investida (Charge) em algum momento da luta.
- [ ] O boss usa a pisada em área (Stomp) em algum momento.
- [ ] O boss ainda usa o ataque melee genérico em alguma situação (variedade, não só os ataques especiais).
- **Achado da auditoria (15/09):** `StartCharge()`/`ExecuteCharge()`/`PerformStomp()` já existem em `JavaliBoss.cs`, mas nenhum estado os chama — só `BossBase.PerformAttack()` (hitbox melee genérica) está em uso.

Status: 🔴 falha — ataques especiais existem no código mas nunca são acionados.

</details>

<details>
<summary>Task C06-T2 — Evento BossDefeated único</summary>

- [ ] Ver Task C04-T1 — mesmo critério e mesmo achado, não duplicar o teste.

Status: 🟡 parcial — ver C04-T1.

</details>

<details>
<summary>Task C06-T3 — Nome oficial</summary>

- [ ] `BossName` no recurso é `"Korrag, o Javali"`.
- **Achado da auditoria (15/09):** ainda `"Javali das Ruínas"` em `JavaliStatsResource.tres`.

Status: 🔴 falha — nome desatualizado no `.tres`.

</details>

<details>
<summary>Task C06-T4 — Timing contra a arte real</summary>

- [ ] `charge_windup` cabe (ou é cortável) em ~1s fixo, testado contra a animação final.
- **Nota:** este caso só roda depois que `EPIC-C06-T1` conectar o Charge **e** a arte de `charge_windup` chegar — cruza com `ROADMAP_QA_INTEGRACAO.md` `EPIC-A04-T1`.

Status: 🔴 bloqueado — depende de C06-T1 e da arte.

</details>

</details>

<details>
<summary><strong>EPIC-C07 — Inimigos Comuns (Arquétipos)</strong> 🔴 Falha · 🟠 Alta</summary>

*Pronto quando*: todos os casos de teste abaixo passam ✅ — 3 arquétipos com comportamento distinto de fato, cada um com dados por área da demo.

<details>
<summary>Task C07-T0 — Regressão do comportamento base (baseline antes de mexer)</summary>

- [x] Inimigo parado, jogador longe → `Idle` 1-3s, depois `Patrol`.
- [x] Patrulhando entre dois pontos → anda entre `PatrolStart`/`PatrolEnd`, vira nos limites.
- [x] Jogador entra no raio de detecção (150px) → `DetectPlayer` (0.5s), depois `Chase` ou `Attack`.
- [x] Jogador no alcance de ataque (30px) → para, ataca a cada 1s, 15 de dano por hitbox.
- [x] Contato direto (sem atacar) → a cada ≥1s aplica dano de contato (15) se não invulnerável.
- [x] 3 hits básicos do jogador (25×3=75) → morre no 3º hit.
- [x] Morre → some da tela, colisão desligada, emite `EnemyDefeated`, `QueueFree()` em 0.3s.
- [x] Jogador sai do alcance durante `Chase` → volta pra `Patrol`.

Status: ✅ passa — `EnemyBase` genérico está sólido, seguro pra derivar arquétipos em cima.

</details>

<details>
<summary>Task C07-T1 — Arquétipo Voador</summary>

- [ ] Comportamento de voo (ignora gravidade/colisão de chão) distinto do inimigo base.
- **Achado da auditoria (15/09):** não existe — todo `EnemyBase` usa a mesma física de chão hoje.

Status: 🔴 falha — arquétipo não implementado.

</details>

<details>
<summary>Task C07-T2 — Arquétipo Rápido</summary>

- [ ] Velocidade claramente maior que o inimigo base, mesmo padrão de comportamento.
- **Achado da auditoria (15/09):** só existe 1 inimigo genérico configurável — sem `.tres` nem subclasse dedicada ao arquétipo Rápido ainda.

Status: 🔴 falha — arquétipo não implementado.

</details>

<details>
<summary>Task C07-T3 — Arquétipo Robusto</summary>

- [ ] HP/resistência a knockback claramente maior que o inimigo base.
- **Achado da auditoria (15/09):** mesmo gap do C07-T2 — genérico único, sem variante Robusto.

Status: 🔴 falha — arquétipo não implementado.

</details>

<details>
<summary>Task C07-T4 — Dados por arquétipo × área da demo</summary>

- [ ] Existe um `.tres` de stats por arquétipo × cada uma das 3 áreas da demo (Floresta Tropical primeiro).

Status: 🔴 falha — depende de C07-T1/T2/T3 existirem primeiro.

</details>

<details>
<summary>Task C07-T5 — AtPatrolEdge() (baixa prioridade)</summary>

- [ ] Inimigo patrulhando chega numa borda sem parede (beira de plataforma) → reage de alguma forma (parar, virar) em vez de andar pro vazio.
- **Achado da auditoria (15/09):** `AtPatrolEdge()` sempre retorna `false` — stub morto. Aceitável pra demo como está, corrigir só se sobrar tempo.

Status: 🟡 parcial — comportamento aceitável pra demo, gap conhecido e de baixa prioridade.

</details>

</details>

<details>
<summary><strong>EPIC-C08 — Transição de Área</strong> 🔴 Falha · 🟠 Alta</summary>

*Pronto quando*: todos os casos de teste abaixo passam ✅ — o jogador chega na cena nova numa posição consistente com de onde veio.

<details>
<summary>Task C08-T0 — Regressão básica do trigger (baseline antes de mexer)</summary>

- [x] Jogador entra num `TransitionTrigger` → carrega `TargetScene`.
- [x] Cena de destino não existe/vazia → loga erro, não trava o jogo.

Status: ✅ passa — carregamento de cena básico é confiável.

</details>

<details>
<summary>Task C08-T1 — Spawn direcional</summary>

- [ ] Jogador chega na cena nova numa posição consistente com de onde veio (ex: sai pela direita da Floresta Tropical, entra pela esquerda do Deserto).
- **Achado da auditoria (15/09):** `TargetArea`/`SpawnOffset` existem como campos exportados em `TransitionTrigger` mas nunca são lidos em `TransitionToTarget()` — sempre cai no spawn padrão da cena.

Status: 🔴 falha — spawn direcional não implementado.

</details>

</details>

<details>
<summary><strong>EPIC-C09 — Save, Checkpoint e Respawn</strong> 🟡 Parcial · 🟡 Média</summary>

*Pronto quando*: todos os casos de teste abaixo passam ✅ — habilidade desbloqueada persiste em disco imediatamente, e existe uma decisão explícita sobre o fluxo de Continuar.

<details>
<summary>Task C09-T0 — Regressão básica (baseline antes de mexer)</summary>

- [x] Interagir (E) com um checkpoint pela primeira vez → ativa, toca `"activate"`, emite `CheckpointActivated`, salva automaticamente no slot 0.
- [x] Interagir de novo com o mesmo checkpoint → não reativa.
- [x] Fechar/reabrir o jogo depois de ativar um checkpoint → `save_0.json` existe com posição, HP, habilidades, checkpoints.
- [x] HP do Kairo chega a 0 → estado `Dead`, para de processar input, emite `PlayerDied`.
- [x] 1s depois de morrer → respawna no último checkpoint ativado (ou perto de onde morreu, se nenhum ativado).
- [x] Respawn → HP restaurado ao máximo, `Velocity` zerada, volta a processar input.

- **Achado (28/09) — dois casos acima não passavam:** (1) apertar E perto de um checkpoint não fazia nada, porque `TryInteract` só procurava o `IInteractable` no pai da área, e o `Checkpoint` é a própria área; (2) o respawn usava o primeiro checkpoint do grupo, ativado ou não. Os dois foram corrigidos no código (ver `ROADMAP_DEV.md` C09-T0). Repetir a Task inteira.

Status: 🟡 parcial — a auditoria de 15/09 marcou ✅ sem rodar; dois bugs corrigidos em 28/09, aguardando reteste no Godot.

</details>

<details>
<summary>Task C09-T1 — Persistência imediata de habilidade</summary>

- [ ] Desbloquear uma habilidade sem passar por um checkpoint depois → ainda assim é salva em disco.
- **Achado da auditoria (15/09):** só fica em memória até o próximo `SaveGame()` disparado por checkpoint/boss/item — se o jogo fechar antes disso, o unlock se perde.

Status: 🟡 parcial — habilidade funciona em memória, mas não persiste imediatamente.

</details>

<details>
<summary>Task C09-T2 — Fluxo de Continuar</summary>

- [ ] Existe uma forma de carregar um save (`LoadGame()`) através de alguma UI/fluxo do jogo.
- [ ] Carregar um save feito em outra área troca de cena automaticamente.
- **Achado da auditoria (15/09):** `LoadGame()` existe isolado, nada o chama. `ApplySaveData()` não chama `SceneManager.LoadScene()`.

Status: 🔴 falha — fluxo de Continuar não existe, nem a decisão de ter ou não foi documentada ainda.

</details>

</details>

<details>
<summary><strong>EPIC-C10 — HUD e UX</strong> 🔴 Falha · 🟡 Média</summary>

*Pronto quando*: todos os casos de teste abaixo passam ✅ — prompt de interação contínuo funcionando, e decisão sobre Game Over tomada e implementada.

<details>
<summary>Task C10-T0 — Regressão básica de HUD (baseline antes de mexer)</summary>

- [x] HP do jogador muda → barra anima suavemente (tween 0.3s), texto atualiza.
- [x] Pausar o jogo → `GetTree().Paused = true`, menu de pausa aparece.

Status: ✅ passa — barra de vida e pausa funcionam.

</details>

<details>
<summary>Task C10-T1 — Prompt de interação contínuo</summary>

- [ ] Chegar perto de um `IInteractable` sem apertar nada → aparece um prompt visual.
- **Achado da auditoria (15/09):** `ShowInteractionPrompt()`/`HideInteractionPrompt()` existem no `HUDController` mas nada os chama continuamente hoje.

Status: 🔴 falha — prompt não aparece automaticamente.

</details>

<details>
<summary>Task C10-T2 — Game Over</summary>

- [ ] Decisão tomada e implementada (ou explicitamente descartada em favor do respawn automático, já funcional).
- **Achado da auditoria (15/09):** `ShowGameOver()` só imprime no console, nada chama.

Status: 🔴 falha — nem implementado nem a decisão de escopo foi documentada.

</details>

</details>

<details>
<summary><strong>EPIC-C11 — Áudio</strong> 🔴 Falha · 🟡 Média</summary>

*Pronto quando*: todos os casos de teste abaixo passam ✅ — pastas/buses existem, os 3 sons já esperados pelo código tocam de verdade, e dá pra sobrepor música + SFX sem cortar um ao outro.

<details>
<summary>Task C11-T1 — Estrutura de pastas e buses</summary>

- [ ] `res://assets/audio/music/` e `res://assets/audio/sfx/` existem.
- [ ] Buses "Music" e "SFX" configurados no Godot (Audio → Buses).
- **Achado da auditoria (15/09):** nenhuma das duas pastas existe hoje. Toda chamada de `PlaySfx`/`PlayMusic` cai no `if (!ResourceLoader.Exists(path))` e só loga erro — silencioso, não quebra o jogo, mas confirma trabalho 100% em aberto.

Status: 🔴 falha — estrutura de áudio não existe.

</details>

<details>
<summary>Task C11-T2 — Arquivos já esperados pelo código</summary>

- [ ] Nome bate exatamente (case-sensitive) com o esperado pelo código: `checkpoint_activate.wav`, `boss_stomp.wav`, `boss_defeat.wav` — ou o código foi ajustado junto.

Status: 🔴 falha — depende de C11-T1 e dos arquivos existirem.

</details>

<details>
<summary>Task C11-T3 — Chamadas que faltam</summary>

- [ ] 8 players de SFX simultâneos + bus de música separado → dá pra tocar música de fundo + vários SFX sem cortar um ao outro.
- [ ] Ataque do jogador, hit em inimigo/jogador, morte de inimigo comum têm som.

Status: 🔴 falha — chamadas ainda não existem no código de gameplay.

</details>

</details>

<details>
<summary><strong>EPIC-C12 — Regressão Final / Release da Demo</strong> 🔴 Bloqueado · 🔴 Bloqueante</summary>

*Pronto quando*: todos os casos de teste abaixo passam ✅ — todo o `ROADMAP_QA_CODIGO.md` acima está ✅, e a demo roda de ponta a ponta sem crash/softlock.

Só é testável depois que todos os Épicos 🔴/🟠 acima estiverem ✅.

<details>
<summary>Task C12-T1 — Regressão completa de código</summary>

- [ ] Todos os Épicos 🔴/🟠 acima com todas as Tasks ✅.
- [ ] Rodar o `ROADMAP_QA_CODIGO.md` inteiro de ponta a ponta depois de qualquer mudança grande — nenhuma regressão.

Status: 🔴 bloqueado — a maioria dos épicos ainda falha, não testável ainda.

</details>

<details>
<summary>Task C12-T2 — Playtest de ponta a ponta (só código)</summary>

- [ ] Floresta Tropical → matar exemplares dos 3 arquétipos → derrotar Korrag → dash desbloqueado → seguir até o ponto de encerramento da demo, sem crash/softlock. Arte placeholder ainda ok aqui.

Status: 🔴 bloqueado — depende de C04, C06 e C07 estarem ✅ primeiro.

</details>

<details>
<summary>Task C12-T3 — Integração incremental com o Higor</summary>

- [ ] Rodar `ROADMAP_QA_INTEGRACAO.md` junto com o Higor assim que a arte de cada bloco chegar, sem esperar tudo pronto.

Status: 🔴 bloqueado — depende da arte começar a chegar.

</details>

</details>

---

## Resumo executivo (o que está bloqueando a demo hoje, em ordem de aparição no fluxo do jogador)

🔴 1. Input Map não bate com a documentação (`C01-T1`). 2. Dois sistemas de movimento em paralelo, risco de ataque duplicado (`C01-T2`). 3. Restauração ODS15 inexistente (`EPIC-C02`). 4. Espada sem toggle nem stats duplos (`C03-T1`/`T2`). 5. Korrag nunca usa Charge/Stomp (`C06-T1`). 6. Dash não implementado nem desbloqueado (`C04-T2`/`T3`). 7. Wall grab/wall jump inexistentes (`EPIC-C05`).
