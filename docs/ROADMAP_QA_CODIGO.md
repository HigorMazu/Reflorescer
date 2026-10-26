---
tipo: projeto
categoria: roadmap
status: ativo
criado: 2026-09-15
atualizado: 2026-10-26
tags: [projeto, roadmap]
projeto: "[[🗂️ Reflorescer]]"
area: codigo
papel: qa
responsavel: Gustavo
par: "[[ROADMAP_DEV]]"
descricao: Casos de teste que definem pronto pra cada Task de código
epicos_total: 18
epicos_concluidos: 15
tasks_total: 80
tasks_prontas: 66
tasks_bloqueadas: 4
---
# Roadmap de QA — Código (Reflorescer)

Casos de teste que definem "pronto" pra cada Task do [[ROADMAP_DEV]] — mesma hierarquia Épico → Task → Subtask, mesmos IDs (`EPIC-Cxx` / `Cxx-Ty`), espelhados 1:1. A diferença de conteúdo: lá as subtasks são passos de implementação, aqui são **casos de teste**. Cada Task fecha com uma linha `Status:` — mas aqui ela reflete o resultado real da auditoria de código de 15/09/2026 (✅ passa hoje / 🔴 falha, funcionalidade não existe / 🟡 parcial ou implementado com bug conhecido), não "não iniciado", porque isso é o estado atual do jogo, não um checklist de trabalho. Sempre que rodar essa Task de novo depois de uma mudança e o resultado for diferente do registrado aqui, atualizar o checkbox, o ícone de status e a linha `Status:` — e, se a mudança revelar uma decisão nova (não só um bug), registrar também em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].

Testa **só a parte do Gustavo** (código/mecânicas), com arte placeholder — nada aqui depende de sprite final do Higor, isso é o [[ROADMAP_QA_INTEGRACAO]].

Legenda de status dos casos de teste: ✅ já passa hoje · 🔴 vai falhar, não existe ainda · 🟡 parcial/bug conhecido. Legenda de prioridade do Épico: 🔴 Bloqueante pra demo · 🟠 Alta · 🟡 Média · 🟢 Baixa/pós-demo.

---

> [!success]- EPIC-C01 — Fundação Técnica — ✅ Passa · 🔴 Bloqueante
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — Input Map bate 100% com os controles de [[🕹️ Sistemas de Jogo#Movimentação|Sistemas de Jogo]], ataque nunca dispara duas vezes, e só existe uma fonte de verdade pro double jump.
>
> > [!success]- Task C01-T1 — Input Map bate com a documentação
> > - [x] Todas as ações lidas em `scripts/` (`move_left`, `move_right`, `jump`, `attack`, `dash`, `interact`, `ability`) correspondem às teclas de [[🕹️ Sistemas de Jogo#Movimentação|Sistemas de Jogo]].
> > - [x] Nenhum script ficou lendo uma ação com o nome antigo depois do remapeamento.
> > - **Achado da auditoria (15/09):** hoje `attack`=J+clique, `dash`=K+seta-baixo, `jump`=Espaço/W/seta-cima, `ability`=L (sem uso). Sem S nem Q mapeados — bate com nada da seção 7.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C01-T2 — Regressão de movimento após unificar
> > - [x] Segurar mover esquerda/direita no chão → acelera até 200px/s, desacelera ao soltar.
> > - [x] Pular parado (chão) → `Velocity.Y = -460` instantânea.
> > - [x] Soltar o botão de pular cedo → corta a velocidade pra metade se `Velocity.Y < -80` (jump cut).
> > - [x] Sair de uma plataforma sem pular → ainda dá pra pular por ~0.15s (coyote time).
> > - [x] Apertar pular ~0.1s antes de tocar o chão → pulo executa ao tocar (jump buffer, 0.12s).
> > - [x] Cair de queda longa → `Velocity.Y` nunca passa de 650; gravidade de queda é 1.6x a de subida.
> > - [x] Virar de direção rapidamente → visual vira instantaneamente, sem input travado.
> > - [x] Apertar ataque uma única vez → `PerformAttack()`/`EnableHitbox()` executa **1 vez só**.
> > - **Achado da auditoria (15/09):** os 7 primeiros casos passam hoje. O último é risco real — `PlayerController.HandlePrototypeMovement()` e a `PlayerStateMachine`/`PlayerStates.cs` antiga rodam em paralelo, e `AttackState.Enter()` também chama `Player.PerformAttack()`, então um único input pode disparar o ataque 2x.
> > - [x] Depois da `EPIC-C01-T2` do Dev (remoção do sistema duplicado): repetir todos os casos acima — nada pode ter regredido.
> > - **Achado (28/09) — os 7 casos de física não rodavam com esses valores:** a state machine antiga também chamava `ApplyGravity` + `MoveAndSlide`, então a física rodava 2x por frame (deslocamento e gravidade dobrados). A `EPIC-C01-T2` do Dev removeu isso. Os números acima (200px/s, -460, 650) passam a valer de verdade só agora, e o movimento vai parecer mais lento que no playtest antigo. Repetir todos os casos.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C01-T3 — Double Jump com fonte única
> > - [x] Pular de novo no ar sem tocar o chão → disponível vindo de **uma única fonte** de verdade.
> > - **Achado da auditoria (15/09):** hoje sempre disponível, mas por duas fontes concorrentes ao mesmo tempo — `PlayerController.InitDoubleJump()` (fallback hardcoded, intencional só pro protótipo) e `AbilityManager.UnlockPrototypeAbilities()`. Funciona por acidente, não por design.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.

> [!success]- EPIC-C02 — Mecânica de Restauração ODS15 — ✅ Passa · 🔴 Bloqueante
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — ponto de restauração interagível em 3 das 4 áreas da demo (Sopé da Mata, Dossel Vivo, Igarapé Sufocado), efeito visual próprio, evento dedicado no `EventBus`, e persistência em save.
>
> > [!success]- Task C02-T1 — Interação de restauração existe
> > - [x] Existe um ponto de restauração interagível (planta/cura/reativa) em pelo menos 1 lugar de cada uma das 3 áreas da demo.
> > - **Achado da auditoria (15/09):** não existe — `Checkpoint` é só save point genérico, sem nenhum conceito de "restaurar" o bioma.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C02-T2 — Efeito visual/de mundo
> > - [x] Interagir com o ponto dispara um efeito visual/de mundo perceptível (partícula, mudança de cor no tile, sprite antes/depois).
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C02-T3 — Evento dedicado e persistência
> > - [x] Emite um evento próprio no `EventBus` (não reaproveita `CheckpointActivated`).
> > - [x] Pontos restaurados persistem no save (fechar/reabrir o jogo mantém o estado restaurado).
> > - **Nota (28/09):** o segundo caso (fechar/reabrir) depende do fluxo de Continuar (C09-T2), que ainda não existe. Por enquanto, testar: (1) restaurar e ir pro `BossArena` e voltar → continua restaurado; (2) o `save_0.json` (em `%APPDATA%/Godot/app_userdata/Joguim - Metroidvania/`) lista o ponto em `RestoredPoints`.
> > - **Nota (28/09):** o fluxo de Continuar (C09-T2) existe agora, então o caso "fechar/reabrir mantém o estado restaurado" é testável: restaurar, fechar o jogo, abrir e escolher Continuar.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo): evento, persistência na sessão e fechar/reabrir (via Continuar) mantêm o ponto restaurado.
>
> > [!success]- Task C02-T4 — Cobertura no Sopé da Mata, Dossel Vivo e Igarapé Sufocado
> > - [x] Pelo menos 1 ponto de restauração ativo e testável em cada uma das 3 áreas (sem ponto no Covil de Korrag).
> > - **Nota (29/09):** escopo atualizado pras 4 áreas da demo (ver [[ROADMAP_DEV]], decisão no topo do arquivo).
> > - **Nota (05/10):** as 3 cenas novas têm 1 ponto cada (`restoration_sope_01`, `restoration_dossel_01`, `restoration_igarape_01`, ver [[ROADMAP_DEV]] C02-T4). Conferido headless: os `NodePath` de antes/depois resolvem.
> >
> > Status: ✅ passa — teste em execução por script no Godot (05/10, Claude): nos 3 pontos, já com a arte do Higor, os secos somem e os verdes crescem até a escala final.

> [!success]- EPIC-C03 — Combate e Espada de Grama — ✅ Passa · 🟠 Alta
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — Q alterna a espada em tempo real, stats diferentes com espada ativada/desativada, e não dá pra atacar com ela desativada.
>
> > [!success]- Task C03-T0 — Regressão de combate básico (baseline antes de mexer)
> > - [x] Atacar sem inimigo por perto → a janela de dano abre depois da preparação (~0,10 s) e dura ~0,10 s, sem dano, sem erro no console. *(Era 0,14 s antes da estocada do Higor, 07/10.)*
> > - [x] Atacar um inimigo no alcance → recebe 25 de dano, flash vermelho, knockback, hitstop (`TimeScale=0.15` por 0.06s).
> > - [x] Alcance igual virado pra direita e pra esquerda (95 px centro a centro), e inimigo colado nas costas não é atingido. *(Novo em 11/10, depois do bugfix da hitbox — ver [[ROADMAP_DEV]] C03-T3.)*
> > - [x] Atacar repetidamente sem soltar o botão → cada inimigo só recebe dano **uma vez** por ativação de hitbox.
> > - [x] Atacar durante o cooldown (0.30s) → `CanAttack()` falso, ataque não sai.
> > - [x] Tomar dano de um inimigo → HP cai, knockback, 1s de invulnerabilidade, shake visual.
> > - [x] Tomar dano durante a invulnerabilidade pós-hit → segundo hit ignorado.
> >
> > Status: ✅ passa — baseline revalidada em 28/09 depois da EPIC-C03 (Gustavo). Revalidada em 11/10 com a estocada nova, por script (`tools/qa/CombatCheck.gd`, 35 verificações, 0 falhas em 3 execuções); a hitbox espelhada pela metade foi corrigida nesta rodada.
>
> > [!success]- Task C03-T1 — Toggle da espada (Q)
> > - [x] Apertar Q alterna `HasSword` e a visibilidade do sprite da espada/bandagem.
> > - **Achado da auditoria (15/09):** `SetSwordVisible()` existe mas nunca é chamado depois do `_Ready()` — não há input ligado a ele ainda.
> >
> > - **Critério atualizado (11/10):** o tom verde no sprite saiu (Higor, 07/10). O estado agora aparece na lâmina (estende/recolhe), na bandagem no pulso (só com a espada guardada) e no indicador do HUD.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo). Revalidado em 11/10 por script (`tools/qa/CombatCheck.gd`, 35 verificações, 0 falhas em 3 execuções): Q recolhe a lâmina, mostra a bandagem, atualiza o HUD, sem tom verde, estado guardado no `GameManager`.
>
> > [!success]- Task C03-T2 — Stats duplos
> > - [x] Com a espada desativada: +velocidade, +pulo, sem ataque.
> > - **Achado da auditoria (15/09):** só existe um conjunto de stats em `PlayerStatsResource`, sem variante "sem espada".
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo). Espada guardada: visivelmente mais rápido e mais alto, sem ataque. Revalidado em 11/10 por script: corrida 240 contra 200, pulo -520 contra -460.
>
> > [!success]- Task C03-T3 — Bloqueio e regressão final
> > - [x] Tentar atacar com a espada desativada → bloqueado.
> > - [x] Alternar a espada em pleno combate/movimento → sem travar animação/input.
> > - [x] Apertar ataque e guardar a espada durante a preparação → o golpe é cancelado, sem dano. *(Novo em 11/10: antes o golpe saía mesmo com a espada guardada; corrigido, ver [[ROADMAP_DEV]] C03-T3.)*
> > - [x] Repetir a Task C03-T0 → nada regrediu.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo). Troca repetida em combate sem travar; C03-T0 repetida sem regressão. Revalidado em 11/10 por script (`tools/qa/CombatCheck.gd`, 35 verificações, 0 falhas em 3 execuções), incluindo o caso novo da preparação.
>
> > [!success]- Task C03-T4 — Hitbox do Kairo
> > - [x] Inimigo baixo (formiga, tartaruga) **no chão** é atingido pelo golpe, dos dois lados.
> > - [x] Inimigo encostando nas pernas ou na cabeça do Kairo causa dano (hurtbox do corpo inteiro).
> >
> > Status: ✅ passa (11/10, Claude) — `tools/qa/CombatCheck.gd` (inimigos no chão) 35/35, `RewardCheck.gd` 11/11, `DemoRunCheck.gd` 89/89.

> [!success]- EPIC-C04 — Dash e Desbloqueio Pós-Korrag — ✅ Passa · 🔴 Bloqueante
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — `BossDefeated` dispara uma única vez, dash tem movimento real, é desbloqueado só ao derrotar o Korrag, e a UI avisa o jogador.
>
> > [!success]- Task C04-T1 — Evento BossDefeated único
> > - [x] Derrotar o Korrag emite `BossDefeated` **uma vez só**.
> > - **Achado da auditoria (15/09):** dispara duas vezes — `BossBase.OnDied()` e `BossDeadState.Enter()` chamam `EmitDefeated()` cada um, de forma independente. Mesmo achado documentado em `EPIC-C06-T2` — é a mesma correção, contar uma vez só no cômputo geral.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C04-T2 — Movimento do dash existe
> > - [x] Apertar Shift (pós remapeamento) com a habilidade desbloqueada → impulso horizontal rápido na direção que o Kairo olha.
> > - **Achado da auditoria (15/09):** nenhuma implementação de movimento associada ao dash — `AbilityId.Dash` existe só como enum, sem código de movimento atrás.
> > - **Nota (28/09):** validados também: parado e andando pros dois lados; no ar não funciona (dash só no chão, decisão registrada no Dev); atravessar um inimigo durante o impulso não causa dano.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C04-T3 — Desbloqueio ao derrotar o Korrag
> > - [x] Derrotar o Korrag → `AbilityManager.UnlockAbility(AbilityId.Dash)` é chamado.
> > - [x] Antes de derrotar o Korrag → apertar dash não faz nada (habilidade ainda bloqueada).
> > - **Achado da auditoria (15/09):** nenhum script escuta `EventBus.BossDefeated` pra isso. Quando for implementado, o listener precisa ser resistente ao evento disparar 2x até a Task C04-T1 ser corrigida — senão risco de desbloquear com efeito colateral duplicado.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo).
>
> > [!success]- Task C04-T4 — Notificação de UI
> > - [x] Ao desbloquear o dash → alguma notificação visual aparece na tela.
> > - **Achado da auditoria (15/09):** só `GD.Print` no console hoje, nenhuma UI real.
> > - **Achado (28/09) — falhou no teste do Gustavo:** a notificação não aparecia quando se chegava à arena pelo portal. Reproduzido no Godot: o HUD destruído do `TestLevel` continuava inscrito no `EventBus` e a exceção dele cortava a cadeia do evento. Corrigido no código (ver [[ROADMAP_DEV]] C04-T4). Com a correção, o teste automatizado (headless: TestLevel → BossArena → 8 golpes) mostra o label visível com "Nova habilidade: Dash".
> >
> > Status: ✅ passa — revalidado no Godot em 28/09 (Gustavo): entrando pelo portal, derrotar o Korrag mostra "Nova habilidade: Dash"; a barra de vida do Kairo também atualiza na arena.

> [!success]- EPIC-C05 — Wall Grab / Wall Jump — ✅ Passa · 🟠 Alta
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — segurar A/D contra a parede no ar reduz a queda, e pular nessa condição empurra pro lado oposto.
>
> > [!success]- Task C05-T1 — Detecção de parede
> > - [x] Personagem no ar encostando numa parede lateral → estado de "encostado" é detectado.
> > - **Achado da auditoria (15/09):** não existe nenhuma detecção de parede em `PlayerController` hoje.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo): agarra só no ar, encostado e segurando a direção da parede; no chão não agarra.
>
> > [!success]- Task C05-T2 — Agarrar parede / slide
> > - [x] Segurar A/D contra uma parede no ar → reduz velocidade de queda.
> > - **Achado da auditoria (15/09):** não existe — o personagem só cai normalmente.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo): desliza devagar; sem segurar, cai normalmente.
>
> > [!success]- Task C05-T3 — Pulo de parede
> > - [x] Pular estando "agarrado" → empurra pro lado oposto da parede.
> > - **Achado da auditoria (15/09):** não existe.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo): empurra pro lado oposto mesmo com A segurado; wall coyote funciona; pulo duplo volta depois de agarrar.
>
> > [!success]- Task C05-T4 — Gating da habilidade
> > - [x] Se gated por `AbilityId.WallJump`: usar antes de desbloqueado não faz nada. Se liberado desde o início: funciona sem pré-requisito.
> > - **Nota:** decisão de design ainda em aberto (ver [[ROADMAP_DEV]] C05-T4) — este caso só é testável depois que a decisão for tomada e documentada em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]].
> > - **Nota (28/09):** decidido "liberado desde o início" (ver [[ROADMAP_DEV]] C05-T4). O caso a testar é o segundo: funciona sem pré-requisito, logo no começo da fase.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo): liberado desde o início, sem pré-requisito. Regressão de pulo, pulo duplo, dash e ataque ok.

> [!success]- EPIC-C06 — Boss Korrag (IA e Identidade) — ✅ Passa · 🔴 Bloqueante
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — o Korrag usa Charge e Stomp durante a luta, o `BossName` é "Korrag, o Javali", e o evento de derrota dispara uma única vez.
>
> > [!success]- Task C06-T0 — Regressão do núcleo do boss (baseline antes de mexer)
> > - [x] Jogador chega a <500px do Korrag → ativa automaticamente, barra de vida aparece.
> > - [x] Entrada da luta → estado `Intro` por 2s antes de lutar de verdade.
> > - [x] HP cai a 100 (50%) → transição automática pra fase 2 (`SetPhase(1)`).
> > - [x] HP cai a 40 (20%) → transição automática pra "Enraged" (`SetPhase(2)`).
> > - [x] Contato direto com o corpo do boss (raio 55px) → 22 de dano a cada ~0.9s se não invulnerável.
> > - [x] Boss recebe 8 ataques básicos do jogador (25×8=200) → morre exatamente no 8º hit.
> > - **Correção de uma nota antiga da auditoria:** a troca de fase por HP já funciona sozinha via `BossStateMachine` (`BossPhase1State`/`BossPhase2State`/`BossEnragedState` chamam `SetPhase()` automaticamente) — não é um gap, ao contrário do que uma versão anterior deste documento registrava.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C06-T1 — Loop de decisão de ataque
> > - [x] O boss usa a investida (Charge) em algum momento da luta.
> > - [x] O boss usa a pisada em área (Stomp) em algum momento.
> > - [x] O boss ainda usa o ataque melee genérico em alguma situação (variedade, não só os ataques especiais).
> > - **Achado da auditoria (15/09):** `StartCharge()`/`ExecuteCharge()`/`PerformStomp()` já existem em `JavaliBoss.cs`, mas nenhum estado os chama — só `BossBase.PerformAttack()` (hitbox melee genérica) está em uso.
> > - **Nota (05/10) — revalidar o enraged:** o enraged caía em valores padrão (mais fraco que a fase 2). Corrigido com `JavaliPhase3.tres` (ver [[ROADMAP_DEV]] C06-T1). Ao rodar a C06-T0 de novo, conferir que abaixo de 20% de HP o Korrag fica visivelmente mais rápido e ataca com mais frequência que na fase 2.
> > - **Revalidado (11/10, script):** no Covil, o enraged entra com ≤20% e usa cooldown 1,0 s / velocidade 140, contra 1,2 s / 120 da fase 2 (fase 1: 2,0 s). Falta só ver na luta jogada se a diferença é perceptível.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C06-T2 — Evento BossDefeated único
> > - [x] Ver Task C04-T1 — mesmo critério e mesmo achado, não duplicar o teste.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C06-T3 — Nome oficial
> > - [x] `BossName` no recurso é `"Korrag, o Javali"`.
> > - **Achado da auditoria (15/09):** ainda `"Javali das Ruínas"` em `JavaliStatsResource.tres`.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C06-T4 — Timing contra a arte real
> > - [x] `charge_windup` cabe (ou é cortável) em ~1s fixo, testado contra a animação final.
> > - **Nota:** este caso só roda depois que `EPIC-C06-T1` conectar o Charge **e** a arte de `charge_windup` chegar — cruza com [[ROADMAP_QA_INTEGRACAO]] `EPIC-A04-T1`.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo): windup de 1s com pulso vermelho, tempo aprovado.
>
> > [!success]- Task C06-T5 — Korrag de frente e hitboxes
> > - [x] Investindo/andando pra direita e pra esquerda, o Korrag está **de frente** (cabeça na direção do movimento).
> > - [x] A investida machuca pelas presas, sem o Kairo precisar estar "dentro" do javali.
> > - [x] Golpe do Kairo na cabeça e no traseiro tira vida (antes só o centro).
> > - [x] Conferir jogando: a luta ficou justa com as hitboxes novas (o Korrag ficou maior pra acertar e pra encostar).
> >
> > Status: ✅ passa (11/10) — por script (investida de frente nos dois sentidos, 40 de dano; cabeça e traseiro 25 cada) e luta jogada e validada pelo Gustavo.

> [!success]- EPIC-C07 — Inimigos Comuns (Arquétipos) — ✅ Passa · 🟠 Alta
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — 3 arquétipos com comportamento distinto de fato, cada um com dados pras 4 áreas da demo.
>
> > [!success]- Task C07-T0 — Regressão do comportamento base (baseline antes de mexer)
> > - [x] Inimigo parado, jogador longe → `Idle` 1-3s, depois `Patrol`.
> > - [x] Patrulhando entre dois pontos → anda entre `PatrolStart`/`PatrolEnd`, vira nos limites.
> > - [x] Jogador entra no raio de detecção (150px) → `DetectPlayer` (0.5s), depois `Chase` ou `Attack`.
> > - [x] Jogador no alcance de ataque (30px) → para, ataca a cada 1s, 15 de dano por hitbox.
> > - [x] Contato direto (sem atacar) → a cada ≥1s aplica dano de contato (15) se não invulnerável.
> > - [x] 3 hits básicos do jogador (25×3=75) → morre no 3º hit.
> > - [x] Morre → some da tela, colisão desligada, emite `EnemyDefeated`, `QueueFree()` em 0.3s.
> > - [x] Jogador sai do alcance durante `Chase` → volta pra `Patrol`.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C07-T1 — Arquétipo Voador
> > - [x] Comportamento de voo (ignora gravidade/colisão de chão) distinto do inimigo base.
> > - **Achado da auditoria (15/09):** não existe — todo `EnemyBase` usa a mesma física de chão hoje.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C07-T2 — Arquétipo Rápido
> > - [x] Velocidade claramente maior que o inimigo base, mesmo padrão de comportamento.
> > - **Achado da auditoria (15/09):** só existe 1 inimigo genérico configurável — sem `.tres` nem subclasse dedicada ao arquétipo Rápido ainda.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C07-T3 — Arquétipo Robusto
> > - [x] HP/resistência a knockback claramente maior que o inimigo base.
> > - **Achado da auditoria (15/09):** mesmo gap do C07-T2 — genérico único, sem variante Robusto.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C07-T4 — Dados por arquétipo × área da demo
> > - [x] Existe um `.tres` de stats por arquétipo × cada uma das 4 áreas da demo (Sopé da Mata primeiro — já são os valores atuais).
> >
> > - **Nota (05/10):** 9 `.tres` em `resources/enemies/areas/` (`<Arquétipo>_<Área>.tres`; o Covil de Korrag não tem inimigo comum). Os valores batem com a tabela do [[👾 Bestiário]] e são provisórios.
> >
> > - **Nota (11/10):** valores oficializados e escalados (os 3 arquétipos sobem a cada área), ver [[ROADMAP_DEV]] C07-T4. O que muda pra este teste: os valores agora batem com a tabela oficial do [[👾 Bestiário]] de 11/10. A sensação de dificuldade é testada na C09-T4.
> >
> > Status: ✅ passa — verificado headless em 05/10 (Claude): os 9 arquivos existem e cada inimigo das cenas do `EPIC-C13` usa o `.tres` da própria área.
>
> > [!success]- Task C07-T5 — AtPatrolEdge() (baixa prioridade)
> > - [x] Inimigo patrulhando chega numa borda sem parede (beira de plataforma) → reage de alguma forma (parar, virar) em vez de andar pro vazio.
> > - **Achado da auditoria (15/09):** `AtPatrolEdge()` sempre retorna `false` — stub morto. Aceitável pra demo como está, corrigir só se sobrar tempo.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C07-T6 — Dano dos inimigos (ataque + toque)
> > - [x] O ataque animado de cada arquétipo acerta o Kairo (antes a formiga e a tartaruga nunca acertavam).
> > - [x] Encostar no corpo visível dá metade do dano; perto mas sem encostar, nada (sem aura invisível).
> > - [x] Conferir jogando: a dificuldade continua boa com o ataque acertando de verdade.
> >
> > Status: ✅ passa (11/10) — por script (acertos de ataque em 10 s: 7 / 4 / 7; contato 5 / 12 / 6) e validado jogando pelo Gustavo.

> [!success]- EPIC-C08 — Transição de Área — ✅ Passa · 🟠 Alta
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — o jogador chega na cena nova numa posição consistente com de onde veio.
>
> > [!success]- Task C08-T0 — Regressão básica do trigger (baseline antes de mexer)
> > - [x] Jogador entra num `TransitionTrigger` → carrega `TargetScene`.
> > - [x] Cena de destino não existe/vazia → loga erro, não trava o jogo.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C08-T1 — Spawn direcional
> > - [x] Jogador chega na cena nova numa posição consistente com de onde veio (ex: sai pela direita da Floresta Tropical, entra pela esquerda do Deserto).
> > - **Achado da auditoria (15/09):** `TargetArea`/`SpawnOffset` existem como campos exportados em `TransitionTrigger` mas nunca são lidos em `TransitionToTarget()` — sempre cai no spawn padrão da cena.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.

> [!success]- EPIC-C13 — Construção das 4 Áreas da Demo (Floresta Tropical) — ✅ Passa · 🔴 Bloqueante
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — as 4 cenas existem, encadeadas na ordem certa, e o novo jogo começa no Sopé da Mata.
>
> > [!success]- Task C13-T1 — Sopé da Mata (entrada)
> > - [x] A cena existe, o Kairo spawna no `Marker2D` de entrada, câmera com limites corretos, inimigos e ponto de restauração presentes.
> >
> > Status: ✅ passa — por script no Godot em 11/10 (Claude, `tools/qa/DemoRunCheck.gd`, 89 verificações, 0 falhas): Novo jogo pelo botão do menu nasce no `SpawnInicial` em pé no chão; câmera (0, -200, 2400, 720); 2 Voadores, 2 Rápidos, 1 Robusto; prompt `[E] Restaurar` aparece, E restaura e o prompt some.
>
> > [!success]- Task C13-T2 — Dossel Vivo (verticalidade)
> > - [x] A cena existe, plataforming vertical navegável, mais Voadores que o Sopé da Mata, ponto de restauração presente.
> > - [x] Chegando do Sopé da Mata, o Kairo entra no `Marker2D` certo.
> >
> > Status: ✅ passa — por script no Godot em 11/10 (Claude). Alcance: um bot com a física real (`tools/qa/DosselReachCheck.gd`) subiu os 12 saltos do chão ao `TopBranch` **só com pulo simples**; a saída pro Igarapé fica em cima do galho do topo. Percurso (`tools/qa/DemoRunCheck.gd`, 89 verificações, 0 falhas): entra no `SpawnFromSopeDaMata`, 4 Voadores contra 2, restauração ok, ida e volta pro Sopé caem nos markers certos e o ponto continua restaurado.
>
> > [!success]- Task C13-T3 — Igarapé Sufocado (hazard de lama)
> > - [x] A cena existe, o `MudHazard` reduz a velocidade do Kairo dentro da área e para de reduzir ao sair.
> > - [x] Mais Robustos/Rápidos que as áreas anteriores, ponto de restauração com contraste "antes/depois" visível.
> > - [x] Chegando do Dossel Vivo, o Kairo entra no `Marker2D` certo.
> >
> > Status: ✅ passa — por script no Godot em 11/10 (Claude, `tools/qa/DemoRunCheck.gd`, 89 verificações, 0 falhas): as 3 poças dão x0,55 dentro e x1,0 fora; 3 Robustos + 2 Rápidos (contra 3 no Sopé e 1 no Dossel); restauração ok (contraste visual aprovado pelo Higor em 08/10); ida e volta pelo Dossel nos markers certos.
>
> > [!success]- Task C13-T4 — Covil de Korrag (arena do boss)
> > - [x] A cena existe, sem inimigos comuns, o Korrag ativa normalmente (`EPIC-C06-T0`), chegando do Igarapé Sufocado o Kairo entra no `Marker2D` certo.
> >
> > Status: ✅ passa — por script no Godot em 11/10 (Claude, `tools/qa/DemoRunCheck.gd`, 89 verificações, 0 falhas): entra no `SpawnFromIgarapeSufocado` em pé no chão; sem inimigos comuns nem restauração; o Korrag ativa com o Kairo por perto. A luta foi feita com dano aplicado pelo script, não com a espada; a luta jogada fica no C12-T2.
>
> > [!success]- Task C13-T5 — Encadear e validar a demo inteira
> > - [x] Novo jogo → Sopé da Mata → Dossel Vivo → Igarapé Sufocado → Covil de Korrag → derrotar o Korrag → dash desbloqueado, sem cair fora do mundo nem spawn errado em nenhuma transição.
> > - **Achado (11/10) — erro do motor a cada transição:** `Removing a CollisionObject node during a physics callback` (o `TransitionTrigger` trocava de cena dentro do `AreaEntered`). Corrigido com `CallDeferred` (ver [[ROADMAP_DEV]] C13-T5); o percurso roda sem nenhum erro do motor.
> >
> > Status: ✅ passa — por script no Godot em 11/10 (Claude, `tools/qa/DemoRunCheck.gd`, 89 verificações, 0 falhas): Novo jogo → Sopé → Dossel → Igarapé → Covil pelos portais reais, com as voltas Dossel→Sopé e Igarapé→Dossel; em todas as 7 transições o Kairo nasce no x do marker, em pé no chão e dentro do mundo. Korrag: fase 2 com ≤50%, enraged com ≤20%, `BossDefeated` uma vez, Dash desbloqueado uma vez, notificação "Nova habilidade: Dash". Teste dirigido: não mede dificuldade, sensação nem FPS (C12-T2/A07).

> [!success]- EPIC-C14 — Cura e Recompensas de Restauração — ✅ Passa na demo (T4/T5 adiados pro jogo completo) · 🔴 Bloqueante
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — o Kairo recupera vida restaurando e passando na bandeira, e cada restauração dá recompensas visíveis.
>
> > [!success]- Task C14-T1 — Restaurar cura e dá +10 de vida máxima
> > - [x] Restaurar: vida cheia e máxima +10, com aviso no HUD.
> > - [x] A vida máxima continua depois de trocar de área, morrer e Continuar.
> >
> > Status: ✅ passa (11/10, Claude) — `tools/qa/RewardCheck.gd` 11/11.
>
> > [!success]- Task C14-T2 — Bandeira cura ao passar
> > - [x] Passar ferido pela bandeira enche a vida e avisa "Vida restaurada"; passar com a vida cheia não faz nada.
> >
> > Status: ✅ passa (11/10, Claude) — `RewardCheck.gd`.
>
> > [!success]- Task C14-T3 — Registro de áreas restauradas no fim da demo
> > - [x] Derrotar o Korrag com 0, 1, 2 e 3 áreas restauradas mostra o número certo, e a mensagem muda com 3/3.
> > - [x] Jogo pausado com a tela aberta; "Continuar explorando" despausa com o Dash liberado; "Menu inicial" volta ao menu despausado.
> >
> > Status: ✅ passa (11/10, Claude) — `tools/qa/DemoEndCheck.gd` 15/15 (0, 1 e 3 áreas restauradas de verdade, Korrag derrotado).
>
> > [!todo]- Task C14-T4 — Faísca fica mais forte ao restaurar
> > - [ ] Cada restauração muda a Faísca de um jeito perceptível, e o efeito continua depois de trocar de área e Continuar.
> >
> > Status: ⏭️ adiado (19/10) — fora da demo ([[ROADMAP_DEV]] C14-T4).
>
> > [!todo]- Task C14-T5 — Caminho novo ao restaurar
> > - [ ] Antes de restaurar, o caminho não existe; depois, aparece e é alcançável; ao voltar pra área (ou Continuar), continua aberto.
> >
> > Status: ⏭️ adiado (19/10) — fora da demo ([[ROADMAP_DEV]] C14-T5).

> [!success]- EPIC-C09 — Save, Checkpoint e Respawn — ✅ Passa (19/10) · 🟡 Média
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — habilidade desbloqueada persiste em disco imediatamente, e existe uma decisão explícita sobre o fluxo de Continuar.
>
> > [!success]- Task C09-T0 — Regressão básica (baseline antes de mexer)
> > - [x] Interagir (E) com um checkpoint pela primeira vez → ativa, toca `"activate"`, emite `CheckpointActivated`, salva automaticamente no slot 0.
> > - [x] Interagir de novo com o mesmo checkpoint → não reativa.
> > - [x] Fechar/reabrir o jogo depois de ativar um checkpoint → `save_0.json` existe com posição, HP, habilidades, checkpoints.
> > - [x] HP do Kairo chega a 0 → estado `Dead`, para de processar input, emite `PlayerDied`.
> > - [x] 1s depois de morrer → respawna no último checkpoint ativado (ou perto de onde morreu, se nenhum ativado).
> > - [x] Respawn → HP restaurado ao máximo, `Velocity` zerada, volta a processar input.
> >
> > - **Achado (28/09) — dois casos acima não passavam:** (1) apertar E perto de um checkpoint não fazia nada, porque `TryInteract` só procurava o `IInteractable` no pai da área, e o `Checkpoint` é a própria área; (2) o respawn usava o primeiro checkpoint do grupo, ativado ou não. Os dois foram corrigidos no código (ver [[ROADMAP_DEV]] C09-T0). Repetir a Task inteira.
> > - **Mudança de comportamento (28/09):** os dois casos de respawn acima agora passam pela tela de Game Over (C10-T2). Morrer abre "Você caiu", e "Tentar de novo" faz o respawn no último checkpoint com HP cheio. O respawn automático de 1s só existe em cena sem HUD.
> > - **Nota (28/09):** o `Checkpoint` passou a herdar de `Area2D` (antes `Node2D`, causava `InvalidCastException` no laço de interação). Revalidar "E na bandeira ativa o checkpoint".
> >
> > Status: ✅ passa — revalidado em 28/09 (Gustavo) com o respawn pela tela de Game Over.
>
> > [!success]- Task C09-T1 — Persistência imediata de habilidade
> > - [x] Desbloquear uma habilidade sem passar por um checkpoint depois → ainda assim é salva em disco.
> > - **Achado da auditoria (15/09):** só fica em memória até o próximo `SaveGame()` disparado por checkpoint/boss/item — se o jogo fechar antes disso, o unlock se perde.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo): Dash desbloqueado, salvo na arena, volta ao Continuar.
>
> > [!success]- Task C09-T2 — Fluxo de Continuar
> > - [x] Existe uma forma de carregar um save (`LoadGame()`) através de alguma UI/fluxo do jogo.
> > - [x] Carregar um save feito em outra área troca de cena automaticamente.
> > - **Achado da auditoria (15/09):** `LoadGame()` existe isolado, nada o chama. `ApplySaveData()` não chama `SceneManager.LoadScene()`.
> > - **Nota (28/09):** decidido "menu inicial com Continuar". Testar: com save, o menu mostra Continuar; Continuar volta à cena e posição salvas; salvar na arena e continuar abre a arena (troca de cena automática).
> > - [x] Salvar → Menu inicial → Continuar **no mesmo processo** (sem fechar o jogo) não trava.
> > - **Correção (11/10, mais tarde):** a hipótese abaixo estava errada. O mesmo crash apareceu no teste de combate e foi isolado: é uma corrida do runtime .NET do Godot 4.7.2 ao descarregar e recarregar os `.tres` com script C# (troca de cena, inimigos morrendo). Corrigido com o autoload `ResourceKeeper`. Ver C12-T4.
> > - **Investigação (11/10) — crash `gchandle.is_released()` relatado pelo Higor (`docs/art-review/title-save-reload-diagnostic.txt`):** não reproduz no projeto real. Testado no Godot .NET 4.7.2 com renderização e `GC.Collect()` forçado a cada troca de cena: (1) Novo jogo → Pausa → Salvar → Menu inicial → Continuar, 5 ciclos; (2) salvar → menu/cena vazia → `ContinueGame`, 10 ciclos; (3) as 4 áreas, 2 voltas. Zero travamentos e zero erros. Hipótese: efeito do projeto-cópia isolado (`ReflorescerTitleQA`) que o Higor usou no teste, não do jogo. Regressão automatizada em `tools/qa/SaveReloadCheck.gd` (faz backup e devolve o `save_0.json`). Se voltar a aparecer jogando, anotar a sequência exata aqui.
> > - **Nota (11/10):** depois de um `git pull` com arte nova, rodar `--headless --import` (ou abrir o editor) antes dos testes por script; sem isso os PNGs novos não carregam e o `Player.tscn` falha.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo): menu com Continuar; Continuar volta à cena/posição salvas, inclusive na arena (sem o Korrag, com dash) e depois de fechar/reabrir o jogo. Revalidado em 11/10 (Claude, por script): save/recarga no mesmo processo nas 4 áreas sem crash.
>
> > [!success]- Task C09-T3 — Visual do checkpoint apagado/ativado
> > - [x] Antes de tocar: checkpoint apagado. Ao tocar: muda pra ativado na hora e salva.
> > - [x] Fechar e Continuar: os checkpoints já ativados nascem ativados; os outros, apagados.
> > - [x] `SaveReloadCheck.gd` e `DemoRunCheck.gd` continuam passando.
> >
> > - **Nota (11/10):** desde a C09-T4 é uma bandeira só (`cp_dossel_01`).
> >
> > Status: ✅ passa (19/10) — validado jogando pelo Gustavo em 19/10.
>
> > [!success]- Task C09-T4 — Bandeira única e vida que não enche ao trocar de área
> > - [x] O projeto compila no editor sem erro.
> > - [x] Só existe uma bandeira na demo, no começo do Dossel Vivo; passar por ela ativa sozinha (som de checkpoint + save).
> > - [x] Tomar dano no Sopé e passar pro Dossel: a barra chega com a mesma vida (não enche). Idem Dossel → Igarapé → Covil.
> > - [x] Morrer no Dossel depois da bandeira → "Tentar de novo" → renasce na bandeira com a vida cheia.
> > - [x] Morrer no Igarapé e no Covil → "Tentar de novo" → carrega o Dossel Vivo e renasce na bandeira com a vida cheia; a bandeira aparece ativada (sem prompt de "Ativar").
> > - [x] Morrer no Sopé (antes da bandeira) → volta pro começo do Sopé.
> > - [x] Novo jogo depois de morrer começa com a vida cheia; Continuar depois de salvar no Igarapé e morrer volta pra bandeira do Dossel.
> > - [x] O caminho até o Korrag ficou difícil mas justo (playtest do Gustavo): dá pra chegar no Korrag com vida razoável sem perfeição.
> > - [x] `DemoRunCheck.gd`, `SaveReloadCheck.gd` e `CombatCheck.gd` continuam passando.
> > - **Origem (11/10):** decisão do grupo de deixar a demo mais difícil até o Korrag ([[ROADMAP_DEV]] C09-T4).
> >
> > Status: ✅ passa (19/10) — compila sem erro; regra jogada e aprovada pelo Gustavo em 11/10; `DemoRunCheck`, `SaveReloadCheck` e `CombatCheck` verdes em 19/10.
>
> > [!success]- Task C09-T5 — Continuar volta com a vida salva
> > - [x] Salvar ferido, sair pro menu e Continuar: volta com a mesma vida, não cheia.
> >
> > Status: ✅ passa (11/10, Claude) — `RewardCheck.gd`.

> [!success]- EPIC-C10 — HUD e UX — ✅ Passa · 🟡 Média
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — prompt de interação contínuo funcionando, e decisão sobre Game Over tomada e implementada.
>
> > [!success]- Task C10-T0 — Regressão básica de HUD (baseline antes de mexer)
> > - [x] HP do jogador muda → barra anima suavemente (tween 0.3s), texto atualiza.
> > - [x] Pausar o jogo → `GetTree().Paused = true`, menu de pausa aparece.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C10-T1 — Prompt de interação contínuo
> > - [x] Chegar perto de um `IInteractable` sem apertar nada → aparece um prompt visual.
> > - **Achado da auditoria (15/09):** `ShowInteractionPrompt()`/`HideInteractionPrompt()` existem no `HUDController` mas nada os chama continuamente hoje.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo): prompt aparece perto da bandeira e do ponto de restauração, some depois de usar e ao se afastar.
>
> > [!success]- Task C10-T2 — Game Over
> > - [x] Decisão tomada e implementada (ou explicitamente descartada em favor do respawn automático, já funcional).
> > - **Achado da auditoria (15/09):** `ShowGameOver()` só imprime no console, nada chama.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo): tela "Você caiu"; Esc não despausa; Tentar de novo leva ao checkpoint; Menu inicial funciona.

> [!success]- EPIC-C11 — Áudio — ✅ Passa (19/10) · 🟡 Média
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — pastas/buses existem, os 3 sons já esperados pelo código tocam de verdade, e dá pra sobrepor música + SFX sem cortar um ao outro.
>
> > [!success]- Task C11-T1 — Estrutura de pastas e buses
> > - [x] `res://assets/audio/music/` e `res://assets/audio/sfx/` existem.
> > - [x] Buses "Music" e "SFX" configurados no Godot (Audio → Buses).
> > - **Achado da auditoria (15/09):** nenhuma das duas pastas existe hoje. Toda chamada de `PlaySfx`/`PlayMusic` cai no `if (!ResourceLoader.Exists(path))` e só loga erro — silencioso, não quebra o jogo, mas confirma trabalho 100% em aberto.
> >
> > Status: ✅ passa — validado no Godot em 28/09 (Gustavo), depois das mudanças da mesma data.
>
> > [!success]- Task C11-T2 — Arquivos já esperados pelo código
> > - [x] Nome bate exatamente (case-sensitive) com o esperado pelo código: `checkpoint_activate.wav`, `boss_stomp.wav`, `boss_defeat.wav` — ou o código foi ajustado junto.
> >
> > - [x] Os 12 sons tocam pelo caminho real do jogo (não só os 3 originais).
> >
> > Status: ✅ passa (11/10, Claude) — `tools/qa/AudioCheck.gd` 12/12 com os placeholders da [[ROADMAP_DEV]] C11-T2; nenhum "SFX not found" no `DemoRunCheck`.
>
> > [!success]- Task C11-T3 — Chamadas que faltam
> > - [x] 8 players de SFX simultâneos + bus de música separado → dá pra tocar música de fundo + vários SFX sem cortar um ao outro.
> > - [x] Ataque do jogador, hit em inimigo/jogador, morte de inimigo comum têm som.
> >
> > Status: ✅ passa (14/10) — `MusicCheck.gd` manteve a música enquanto três players reais de SFX tocavam; `AudioCheck.gd` segue 12/12 pelo caminho real.

> > [!success]- Task C11-T4 — Música por área e tela de título
> > - [x] Cada área e a tela de título tocam a própria faixa em loop; a troca de área faz fade de uma pra outra, sem corte seco.
> > - [x] Música + vários SFX ao mesmo tempo (ex.: luta com o Korrag) sem cortar nenhum.
> >
> > Status: ✅ passa (14/10) — cinco loops autorais carregam nas cenas de produção, tocam em sequência e passam em `MusicCheck.gd` com save preservado.

> > [!success]- Task C11-T5 — Sons finais no lugar dos placeholders
> > - [x] `tools/qa/AudioCheck.gd` 12/12 com os sons finais.
> > - [x] `assets/audio/CREDITOS.md` lista origem/licença de todo som que não for do grupo.
> > - **Decisão do grupo (19/10):** os 12 SFX atuais e as 5 músicas autorais ficam como o áudio final da demo (já existe som pra cada ação). Se criarem algo melhor, trocam o arquivo mantendo o nome e atualizam aqui.
> >
> > Status: ✅ passa (19/10) — áudio atual aceito como final da demo; `AudioCheck.gd` 12/12 e `MusicCheck.gd` 5/5.

> [!success]- EPIC-C12 — Regressão Final / Release da Demo — ✅ Passa (19/10; build `.exe` adiada) · 🔴 Bloqueante
> *Pronto quando*: todos os casos de teste abaixo passam ✅ — todo o [[ROADMAP_QA_CODIGO]] acima está ✅, e a demo roda de ponta a ponta sem crash/softlock.
>
> Só é testável depois que todos os Épicos 🔴/🟠 acima estiverem ✅.
>
> > [!success]- Task C12-T1 — Regressão completa de código
> > - [x] Todos os Épicos 🔴/🟠 acima com todas as Tasks ✅.
> > - [x] Rodar o [[ROADMAP_QA_CODIGO]] inteiro de ponta a ponta depois de qualquer mudança grande — nenhuma regressão.
> > - [x] Bateria automática atual: `CombatCheck --stress`, `DosselReachCheck`, `GcStressCheck`, `AudioCheck`, `MusicCheck`, `DemoRunCheck` e `FpsCheck` passaram; os testes que atravessam a bandeira automática preservam o `save_0.json`.
> >
> > Status: ✅ passa (19/10) — bateria completa verde em 19/10 depois das mudanças do dia: 12 scripts de `tools/qa/` (incl. `WorldMapCheck` 16/16 e `DesertLinkCheck` 24/24) + checks de arte.
>
> > [!success]- Task C12-T2 — Playtest de ponta a ponta (só código)
> > - [x] Floresta Tropical → matar exemplares dos 3 arquétipos → derrotar Korrag → dash desbloqueado → seguir até o ponto de encerramento da demo, sem crash/softlock. Arte placeholder ainda ok aqui.
> >
> > - [x] *(11/10)* Sopé → Dossel → Igarapé → Covil → Korrag → Dash jogado pelo Gustavo, sem crash/softlock; `tools/qa/DemoRunCheck.gd` 89/89.
> >
> > Status: ✅ passa (11/10) — partida completa do Gustavo + percurso automatizado. O playtest final com arte/áudio completos e alguém de fora fica no [[ROADMAP_QA_INTEGRACAO]] A07.
>
> > [!success]- Task C12-T3 — Integração incremental com o Higor
> > - [x] Rodar [[ROADMAP_QA_INTEGRACAO]] junto com o Higor assim que a arte de cada bloco chegar, sem esperar tudo pronto.
> >
> > - [x] Rodada de 11/10: entregas do Higor de 05/10 a 09/10 integradas e [[ROADMAP_QA_INTEGRACAO]] rodado (A00–A06 passam, exceto pendências cadastradas).
> >
> > Status: ✅ passa (19/10) — entregas integradas e validadas jogando pelo Gustavo.

> > [!success]- Task C12-T4 — Sem travamento do runtime .NET
> > - [x] `tools/qa/CombatCheck.gd -- --stress` roda 5 vezes seguidas sem `FATAL: gchandle.is_released()`.
> > - [x] `tools/qa/GcStressCheck.gd` (troca de área + combate, sem GC forçado) completa em várias execuções seguidas.
> > - [x] `SaveReloadCheck.gd` e `DemoRunCheck.gd` continuam passando.
> > - [ ] Repetir o `GcStressCheck` no jogo exportado quando houver build.
> >
> > Status: ✅ passa (11/10, Claude) — com o autoload `ResourceKeeper`: `CombatCheck --stress` 5/5 (35/35 cada), `GcStressCheck` 8/8 com 24 ciclos (antes 6/6 travavam), `DemoRunCheck` 89/89 sem erro do motor, `SaveReloadCheck` e `DosselReachCheck` sem falhas. Ver [[ROADMAP_DEV]] C12-T4.
>
> > [!todo]- Task C12-T5 — Build exportada da demo
> > - [ ] O `.exe` abre num computador sem Godot e roda menu → 4 áreas → Korrag → tela de fim sem crash.
> > - [ ] Novo jogo, Continuar e o save em `%APPDATA%` funcionam no exportado.
> > - [ ] `GcStressCheck` (ou uma partida longa trocando de área) sem `FATAL: gchandle.is_released()` no exportado.
> >
> > Status: ⏭️ adiado (19/10) — fora da demo ([[ROADMAP_DEV]] C12-T5).

> [!warning]- EPIC-C15 — Deserto pós-demo e chefe cobra — 🟡 1ª área testável · 🟢 Pós-demo
> *Pronto quando*: a área completa e a luta passam nos casos abaixo, além da verificação de integração visual A08. A prévia visual isolada de 15/10 não satisfaz estes testes de mecânica.
>
> > [!warning]- Task C15-T1 — Área jogável do Deserto
> > - [ ] Percorrer entrada → trecho ensolarado → tarde → arena noturna → saída sem buracos, softlock ou colisão desalinhada; os três habitantes aparecem em posições finais e interagem com Kairo.
> > - [ ] A iluminação acompanha o progresso com transições contínuas; morte, retorno e reload restauram o estado visual adequado ao ponto do percurso.
> > - [ ] Entrada, câmera, checkpoints e saída respeitam as decisões aprovadas; as quatro áreas da demo e seus saves continuam funcionando.
> >
> > Status: 🟡 parcial (19/10) — 1ª área passa no `DesertLinkCheck.gd` (spawn, chão, vento, inimigos com stats do Deserto, bandeira, mapa, barreira antes da cobra); o percurso completo depende da C15-T4.
>
> > [!warning]- Task C15-T2 — Bote, cauda e areia da cobra
> > - [x] Cada golpe tem aviso, execução e recuperação distinguíveis; a hitbox só causa dano na janela ativa e não alcança além do desenho, em ambos os lados da arena.
> > - [x] Bote/mordida frontal, varrida de cauda e mergulho/emergência na areia podem ser provocados em teste; dano, invulnerabilidade e colisão do jogador obedecem às regras existentes.
> > - [x] Durante o mergulho, o corpo e a colisão seguem o estado subterrâneo sem golpe invisível; retorno à superfície não prende nem teleporta Kairo.
> > - [x] Hurt/death encerram os golpes com segurança; a luta termina uma vez, sem novos ataques após a morte. O sibilo não se repete por quadro.
> >
> > Status: 🟡 parcial (26/10) — `SerpentBossCheck.gd` 24/24 (arena fecha, Kairo acerta o corpo, os 3 golpes com aviso, mergulho invulnerável que sai embaixo do Kairo, fase 2, morte salva sem tela de fim). Falta jogar.
>
> > [!failure]- Task C15-T3 — Persistência e saída pós-chefe
> > - [ ] Após a derrota, a recompensa/desbloqueio aprovado ocorre uma vez; reload e retorno à arena mantêm o estado correto.
> > - [ ] A transição aprovada para o próximo trecho não quebra saves anteriores nem o encerramento da demo; checkpoints retornam ao lugar correto.
> > - [ ] Percurso completo da entrada do Deserto até a saída, com derrota do boss, termina sem crash, softlock ou regressão na demo.
> >
> > Status: 🔴 pendente (15/10) — depende da decisão de recompensa/destino e da [[ROADMAP_DEV]] C15-T3.

> > [!failure]- Task C15-T4 — Mapa do Deserto em áreas
> > - [ ] Percorrer todas as áreas aprovadas nos dois sentidos, com spawn certo, câmera, mapa e save.
> > - [ ] O hazard do bioma atrasa/afunda sem prender o Kairo e reseta ao morrer.
> >
> > Status: 🔴 pendente de implementação (21/10) — quatro áreas e conexões aprovadas pelo Higor; depende agora da execução da [[ROADMAP_DEV]] C15-T4.
>
> > [!failure]- Task C15-T5 — Restauração no Deserto
> > - [ ] Cada ponto restaura uma vez, persiste no save, cura e conta pra recompensa do bioma.
> >
> > Status: 🔴 bloqueado (19/10) — depende do gancho ODS 15 ([[ROADMAP_DEV]] C15-T5).
>
> > [!failure]- Task C15-T6 — Música e stats do Deserto
> > - [ ] A faixa do Deserto toca na área e troca sem corte com o Covil.
> > - [ ] Os três inimigos usam os stats aprovados e o percurso fica difícil mas justo.
> >
> > Status: 🔴 pendente (19/10) — depende da [[ROADMAP_DEV]] C15-T6.

> [!success]- EPIC-C16 — Mapa explorável da Floresta Tropical — ✅ Passa · 🟡 Alta
> *Pronto quando*: a cartografia revela somente posições atravessadas, persiste no save e todas as transições atuais funcionam nos dois sentidos.
>
> > [!success]- Task C16-T1 — Revelação, entrada e save
> > - [x] Save novo mostra só a célula inicial; as células futuras não aparecem até Kairo chegar nelas.
> > - [x] `M` abre e fecha o mapa com a partida pausada; `Esc` fecha sem abrir o menu de pausa.
> > - [x] A descoberta entra em `ExploredMapCells` no arquivo de save e saves antigos sem o campo carregam com lista vazia.
> > - [x] `SaveReloadCheck.gd` continua sem falhas após adicionar o campo.
> > - **Verificação (15/10):** `WorldMapCheck.gd` passou; captura real em `docs/art-review/world-map-fog-v1.png`.
> >
> > Status: ✅ passa (15/10) — comportamento e persistência verificados; revisão estética pertence à A09-T1.
>
> > [!success]- Task C16-T2 — Passagens bidirecionais
> > - [x] `DemoRunCheck.gd` continua 89/89 na rota Sopé ↔ Dossel ↔ Igarapé → Covil.
> > - [x] Portal do Covil aponta ao Igarapé; a chegada usa marcador próprio, longe do gatilho de retorno.
> > - [x] Retorno Covil → Igarapé carrega a cena, posiciona Kairo e revela o trecho certo.
> > - **Verificação (15/10):** `WorldMapCheck.gd` passou 12 verificações sem falha.
> >
> > Status: ✅ passa (15/10) — rede atual é bidirecional e o movimento vertical já ocorre no Dossel; expansão para novas ramificações requer salas adicionais.
>
> > [!success]- Task C16-T3 — Ajustes da revisão do mapa
> > - [x] O título aparece inteiro ("MAPA DE REFLORESCER") em 1280×720 e 960×540.
> > - [x] Andar por células novas e usar **Continuar**: Kairo volta no ponto combinado na decisão da [[ROADMAP_DEV]] C16-T3, e o mapa explorado continua lá.
> > - [x] Apertar `M` numa cena sem mapa não pausa o jogo.
> > - [x] Portal do Covil se comporta conforme a decisão (fechado ou aberto) durante a luta com o Korrag.
> >
> > Status: ✅ passa (19/10) — `WorldMapCheck.gd` 16/16: título inteiro, Continuar não muda com o mapa, M sem efeito fora das áreas e portal fechado com o Korrag vivo.

> [!warning]- EPIC-C18 — Playtest de 26/10: tutorial, dificuldade e hitboxes — 🟡 Testes automáticos passam; falta jogar · 🔴 Bloqueante
> *Pronto quando*: os testes abaixo passam e o Gustavo joga a demo inteira com as mudanças.
>
> > [!warning]- Task C18-T1 — Tela de abertura e dicas
> > - [x] Novo jogo abre a abertura com propósito, objetivo e as teclas A/D, Espaço, K, Q, E, M e Esc; o botão já vem com foco e solta o jogo.
> > - [x] As faixas aparecem no trecho certo, trocam entre trechos e somem ao sair.
> > - [ ] Alguém que nunca jogou entende o que fazer só com isso (A07-T3).
> >
> > Status: 🟡 parcial (26/10) — `TutorialFallCheck.gd` 19/19.
>
> > [!success]- Task C18-T2 — Dano de queda
> > - [x] Queda curta não dói; queda alta dói; mais alta dói mais, com teto de 40.
> > - [x] Bateria antiga (DemoRun, Dossel, Combat) continua passando com o dano de queda ligado.
> >
> > Status: ✅ passa (26/10) — `TutorialFallCheck.gd` + bateria completa verde.
>
> > [!warning]- Task C18-T3 — Korrag e inimigos mais difíceis
> > - [x] `DemoRunCheck` 89/89, `CombatCheck` 35/35, `DemoEndCheck` 15/15 com os valores novos.
> > - [ ] Playtest do Gustavo: o Korrag ficou rápido, ágil e forte; a demo ficou bem mais difícil, mas dá pra vencer.
> >
> > Status: 🟡 parcial (26/10) — falta o playtest.
>
> > [!warning]- Task C18-T4 — Hitboxes dos inimigos
> > - [x] Medição automática: cada hitbox cobre ~90% do corpo visível (antes Burro, Ganso e Lebre tinham só metade).
> > - [x] `EnemyIntegrationCheck` (9 espécies) e `CombatCheck` verdes.
> > - [ ] Playtest: golpe que encosta no desenho acerta, nos 9 inimigos.
> >
> > Status: 🟡 parcial (26/10) — falta conferir jogando.

> [!failure]- EPIC-C17 — Fundação do jogo completo (mundo conectado) — 🔴 Ainda não testável · 🟢 Pós-demo
> *Pronto quando*: uma área de teste nova entra no mapa, nas transições e no save só por configuração, sem regressão na demo.
>
> > [!failure]- Task C17-T1 — Mapa do mundo configurável
> > - [ ] Uma área nova aparece no mapa só com a definição dela, sem editar o `WorldMapOverlay.gd`.
> > - [ ] `WorldMapCheck.gd` 16/16 nas 4 áreas da demo.
> >
> > Status: 🔴 pendente (19/10) — depende da [[ROADMAP_DEV]] C17-T1.
>
> > [!success]- Task C17-T2 — Ligação entre biomas
> > - [x] Ir e voltar entre a Floresta e o bioma seguinte, com spawn certo, mapa e save.
> >
> > Status: ✅ passa (19/10) — `DesertLinkCheck.gd` 24/24: portal fechado com o Korrag vivo, tela de fim avisa do Deserto, "Continuar explorando" abre o portal, Deserto jogável e volta ao Covil.
>
> > [!failure]- Task C17-T3 — Habilidade por boss
> > - [ ] Derrotar cada boss libera a habilidade certa uma vez só, persiste no save e abre os caminhos previstos.
> >
> > Status: 🔴 bloqueado (19/10) — depende da definição ([[ROADMAP_DEV]] C17-T3).

---

## Resumo executivo (o que falta pra demo, em 11/10/2026)

> [!info] Retrato de 11/10/2026 — 10 de 13 épicos passam
> O resumo de 15/09 (Input Map, movimento duplicado, restauração inexistente, espada, Korrag, dash, parede) está todo resolvido. O que ainda falta do lado do código:
> 1. **C09-T3** — visual do checkpoint apagado/ativado (espera a arte do Higor, [[ROADMAP_ARTE]] A05-T5).
> 2. **C11-T4** — música por área e na tela de título (faltam as faixas).
> 3. **C11-T5** — trocar os 12 SFX placeholder pelos sons finais.
> 4. **C12-T1** — regressão final, depois dos três itens acima.
>
> Bateria automática que cobre o resto: `tools/qa/` — rodar com o console do Godot (`--headless --fixed-fps 60 --path . --script res://tools/qa/<Teste>.gd`; o `SaveReloadCheck` e o `FpsCheck` rodam sem `--headless`). Depois de `git pull` com arte nova, rodar `--headless --import` antes.
