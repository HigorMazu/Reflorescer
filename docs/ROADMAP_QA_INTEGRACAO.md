---
tipo: projeto
categoria: roadmap
status: ativo
criado: 2026-09-15
atualizado: 2026-10-15
tags: [projeto, roadmap]
projeto: "[[🗂️ Reflorescer]]"
area: arte
papel: qa
responsavel: Gustavo e Higor
par: "[[ROADMAP_ARTE]]"
descricao: Casos de teste da integração código + arte
epicos_total: 10
epicos_concluidos: 2
tasks_total: 32
tasks_prontas: 17
tasks_bloqueadas: 4
---
# Roadmap de QA — Integração (Código + Arte do Higor)

Casos de teste que confirmam a integração entre o código do Gustavo e a arte do Higor — mesma hierarquia Épico → Task → Subtask do [[ROADMAP_ARTE]], mesmos IDs (`EPIC-Axx` / `Axx-Ty`), espelhados 1:1. Só entra em jogo **depois** que a Task equivalente de código já passa no [[ROADMAP_QA_CODIGO]] com arte placeholder — este arquivo não testa mecânica nova, só confirma que a arte final não quebrou o que já funcionava. Cada Task fecha com uma linha `Status:` que reflete o estado real de hoje (a maioria ainda bloqueada, porque a arte ainda não chegou) — atualizar o checkbox, o ícone e a linha `Status:` a cada entrega do Higor, e registrar em [[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]] qualquer decisão nova que a integração revelar.

Legenda de status: ✅ passa · 🔴 ainda não dá pra testar (falta arte ou código) · 🟡 parcial.

---

> [!success]- EPIC-A00 — Padrões Técnicos — ✅ Passa · 🔴 Bloqueante
> *Pronto quando*: todo `SpriteFrames` entregue segue canvas e pivô consistentes, e o flip (por escala no Kairo, `FlipH` em inimigos/boss) não distorce a arte.
>
> > [!success]- Task A00-T1 — Formato e consistência
> > - [x] Cada `SpriteFrames` entregue usa o mesmo tamanho de canvas em todos os frames de um personagem.
> > - [x] Pivô/ancoragem consistente entre animações do mesmo personagem — sem "pulo" de posição ao trocar.
> > - **Verificação (28/09, Claude, pelos arquivos):** Kairo: os 11 quadros têm 180×170. Korrag: os 15 quadros têm 260×180 (as folhas `_sheet.png` são só referência, não entram no `SpriteFrames`). Os dois `SpriteFrames` usam `centered = false` com um offset fixo no nó, então canvas igual = mesma âncora. Um "pulo" só aconteceria se o desenho estiver deslocado dentro do canvas, e isso precisa de olho humano.
> >
> > - **Integração (06/10):** Canvas verificados: Kairo 180×170 e Faísca 32×32. Offset visual uniforme de Kairo ajustado para (-45,-65), alinhando os pés à colisão sem mudar física. Aprovação visual pendente. Ver `docs/art-review/README.md` e capturas.
> >
> > Status: ✅ passa (11/10) — sem "pulo" de posição nas trocas de animação, conferido por Gustavo jogando a demo inteira; aprovado.
>
> > [!success]- Task A00-T2 — Flip
> > - [x] Kairo: flip por escala não distorce elementos assimétricos da arte final (ex: espada só de um lado).
> > - [x] Inimigos/boss: `FlipH` espelha corretamente sem distorcer asas/antenas/elementos assimétricos.
> > - **Achado (28/09):** o sprite do Kairo (`kairo_faisca_*.png`) tem a Faísca desenhada no canto superior direito. Com o flip por escala, ela pula pro outro lado do Kairo a cada virada. Além disso, o companheiro `Faisca.tscn` continua desenhando o placeholder (corpo, asas e luz) e segue o Kairo, então aparecem **duas Faíscas**. Proposta pro Higor: Kairo sem a Faísca no sprite, e a arte da Faísca entregue separada pro `Faisca.tscn` (ver [[ROADMAP_ARTE]] A01-T1 e A03). Flip de inimigos/boss por `FlipH`: ainda não conferido.
> >
> > - **Integração (06/10):** Faísca removida dos quadros de Kairo e integrada como companheiro independente. Flip de Kairo testado no Godot .NET. Inimigos/boss não revalidados. Ver `docs/art-review/README.md` e capturas.
> >
> > Status: ✅ passa (11/10) — Kairo com a espada nos dois sentidos aprovado por Gustavo; inimigos conferidos nos dois sentidos (`EnemyIntegrationCheck`, `tools/qa/CombatCheck.gd`).

> [!warning]- EPIC-A01 — Kairo — 🟡 Parcial · 🔴 Bloqueante
> *Pronto quando*: as animações núcleo substituem o placeholder sem regressão de física, e a Espada de Grama alterna visual corretamente em jogo.
>
> > [!success]- Task A01-T1 — Animações núcleo
> > - [x] Trocar o placeholder pelo pacote idle/run/jump/fall/attack/hurt/dead e rodar de novo a Task C01-T2 do [[ROADMAP_QA_CODIGO]] — nenhum comportamento de física deve mudar, só o visual.
> > - **Verificação (28/09):** idle 2 quadros/5 fps, run 2/8, jump 1, fall 1, attack 3/10 fps (0.3s, igual ao `AttackCooldown`), hurt 1, dead 1. Os nomes batem com o que o `PlayerController` toca.
> >
> > - **Integração (06/10):** 11 PNGs limpos; sete nomes, contagens, FPS e loops preservados. Corrigido run_01. Corrida, salto e ataque passaram no teste C#; C01-T2 completo e aprovação artística ainda pendentes. Ver `docs/art-review/README.md` e capturas.
> >
> > Status: ✅ passa (11/10) — partida completa sem regressão de movimento (Gustavo) e `tools/qa/DemoRunCheck.gd` 89/89; arte aprovada.
>
> > [!success]- Task A01-T2 — Espada de Grama
> > - [x] Sprite da espada visível quando ativada, "bandagem" visível quando desativada — nunca os dois ao mesmo tempo.
> > - [x] Animação de ataque: janela de dano sincronizada com o impacto da estocada, de 0,10 s a 0,20 s após o início do golpe.
> > - **Dependência dupla:** só testável depois que [[ROADMAP_DEV]] `EPIC-C03` (toggle da espada) **e** [[ROADMAP_ARTE]] `A01-T2` (os 2 visuais) estiverem prontos.
> >
> > Status: ✅ passa (11/10) — versão atual (lâmina v11, corrida v12) aprovada por Gustavo; janela de dano 0,10–0,20 s e alcance simétrico conferidos em `tools/qa/CombatCheck.gd`. Histórico: (08/10) v9 mantém o punho único, encaixa a guarda durante o ataque e preserva a leitura da lâmina nos três quadros; toggle, bandagem e hitbox seguem aprovados. Ver `docs/art-review/kairo-korrag-correction-v3.md`.
>
> > [!failure]- Task A01-T3 — Wall grab / wall jump
> > - [x] Animação integrada e sincronizada com a mecânica ([[ROADMAP_DEV]] `EPIC-C05`).
> >
> > - [ ] Depois do ajuste do Higor: no jogo, o apoio na parede se distingue de uma queda comum (conferir no tronco do Dossel Vivo, mesma posição da captura). **(entregue em 12/10; pendente: aprovação visual)**
> >
> > **Verificação (12/10):** 20 verificações de execução passaram: dash, parede à direita/esquerda, inclinação do corpo, garra e pé dianteiro ancorados no tronco, velocidade de descida, salto de saída e remoção do efeito. Capturas reais no Dossel revisadas; ver `docs/art-review/wall-contact-v4.md`.
> >
> > Status: 🟡 parcial (12/10) — efeito ampliado entregue, aguardando aprovação visual. Revisão anterior (11/10): mecânica e integração ok, mas no jogo o apoio não se distingue de uma queda (pose de `fall`, raspas invisíveis na escala real). Ver [[ROADMAP_ARTE]] A01-T3 e `docs/art-review/qa-ingame-wallslide-2026-10-11.png`. Histórico: (08/10) o estado real usa o Kairo normal com raspas discretas; entrada, colisão, estado, animação e efeito passaram em `MovementAnimationRuntimeCheck.tscn`. Ver `docs/art-review/kairo-korrag-correction-v3.md`.
>
> > [!success]- Task A01-T4 — Dash
> > - [x] Variação visual integrada e sincronizada com a mecânica ([[ROADMAP_DEV]] `EPIC-C04`).
> >
> > Status: ✅ passa (11/10) — aprovado por Gustavo; captura no jogo em `docs/art-review/qa-ingame-dash-2026-10-11.png`. Histórico: (08/10) o estado real usa o Kairo normal, sem coloração verde, com linhas e poeira; entrada, desbloqueio, estado, animação e efeito passaram em `MovementAnimationRuntimeCheck.tscn`. Ver `docs/art-review/kairo-korrag-correction-v3.md`.

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
> > [!warning]- Task A02-T4/T5 — Reskins Deserto e Tundra
> > - [x] Repetir os testes da Task A02-T1/T2/T3 pra cada reskin.
> > - **Nota (29/09):** fora do escopo da demo (Deserto/Tundra não entram mais no percurso — ver [[🎯 Escopo da Demo]]). Fica pro jogo completo.
> > - **Verificação (15/10):** seis novas cenas herdadas passaram, junto aos três arquétipos originais, em 478 verificações dirigidas: canvas/pivô, progressão de quadros, patrulha, detecção, perseguição, ataque dos dois lados, dano/interrupção, morte e fade. Log em `docs/art-review/post-demo-enemy-validation.txt`. As futuras áreas ainda não existem para teste de percurso contextual.
> > - **Aprovação visual (15/10):** Higor aprovou os seis designs e animações no chat.
> >
> > Status: 🟡 parcial (15/10) — integração isolada e visual aprovados; falta teste nas áreas quando existirem.

> [!warning]- EPIC-A03 — Faísca — 🟡 Parcial (Faísca fortalecida) · 🟠 Alta
> *Pronto quando*: `fly`/`investigate` acompanham a troca de estado sem travar num frame parado, e a luz não "pisca" ao trocar de direção.
>
> > [!success]- Task A03-T1
> > - [x] Com `fly`/`investigate` adicionadas, a troca de animação acompanha a troca de estado (`FollowState`/`IdleFaiscaState`/`InvestigateState`/`ReturnToPlayerState`) sem travar num frame parado.
> > - [x] A luz (`PointLight2D`) pulsa e o flip de sprite não "pisca" de forma estranha ao trocar de direção.
> >
> > - **Integração (06/10):** 12 frames entregues, quatro por `idle`/`fly`/`investigate`; os três estados e o avanço de frames passaram em teste dirigido no Godot .NET. A luz pulsa em nó irmão do sprite, portanto não é espelhada pela virada.
> >
> > Status: ✅ passa (06/10) — animações, estados e luz independentes verificados.
>
> > [!failure]- Task A03-T2 — Faísca fortalecida
> > - [ ] Cada nível da Faísca se distingue no jogo e acompanha a troca de área e o Continuar.
> >
> > Status: 🔴 bloqueado (11/10) — depende de [[ROADMAP_ARTE]] A03-T2 e [[ROADMAP_DEV]] C14-T4.

> [!warning]- EPIC-A04 — Boss KORRAG — 🟡 Parcial (revisão jogada do `attack`) · 🟠 Alta
> *Pronto quando*: as transições de fase são claras pro jogador mesmo com `idle` compartilhado, o timing de `charge_windup` bate com o código, e o nome exibido é "Korrag, o Javali".
>
> > [!success]- Task A04-T1 — Obrigatórios
> > - [x] Transições de fase ([[ROADMAP_QA_CODIGO]] `C06-T0`) comunicam claramente ao jogador que o boss mudou de fase, mesmo com `idle` compartilhado.
> > - [x] Quando `StartCharge()`/`PerformStomp()` forem ligados ([[ROADMAP_DEV]] `EPIC-C06-T1`): `charge_windup` cabe no ~1s fixo antes da investida disparar.
> > - [x] Animação `defeated` aparece após `dead`, durante o fade, sem emitir vitória novamente.
> > - [x] Nome exibido em qualquer UI usa "Korrag, o Javali", não "Javali das Ruínas".
> > - **Verificação (28/09):** `charge_windup` tem 1 quadro. Mantido o windup de 1s com pulso vermelho de aviso, aprovado no playtest ([[ROADMAP_DEV]] C06-T4). Nome: nenhuma UI exibe o `BossName` hoje (a barra do Korrag mostra só a vida); o recurso diz "Korrag, o Javali".
> > - **Mudança de código (28/09):** a morte não é mais "invisível + destruir em 0.3s". Agora toca `dead`, espera 0.6s e some em fade de 0.6s. A animação `defeated` existe no `SpriteFrames` mas **não é tocada** por nenhum estado: decidir se ela substitui `dead` ou entra depois dela.
> > - **Correção (08/10):** `phase2_transition` agora tem 3 quadros; `enraged_transition`, 2; `dead`, 3. `charge` e `stomp` são animações próprias chamadas pelas ações correspondentes. O quadro com a perna traseira solta foi removido de `charge_windup` e `stomp`. Todos os 13 estados passaram em `KorragAnimationCheck.gd`.
> > - **Integração (06/10):** os visuais obrigatórios estão em `KorragSpriteFrames.tres`. A sequência agora é dead por 0,6 s → defeated durante fade de 0,6 s → remoção. Seis verificações passaram em `BossDefeatCheck.tscn`, incluindo emissão única de vitória. Falta o playtest visual das transições de fase.
> >
> > Status: ✅ passa (11/10) — luta jogada por Gustavo (transições de fase legíveis) e v3 aprovada; fases 2/enraged e vitória única também em `tools/qa/DemoRunCheck.gd`. Ver `docs/art-review/kairo-korrag-correction-v3.md`.
>
> > [!success]- Task A04-T2 — Opcional
> > - [x] `enraged_transition` integrada, se entregue.
> > - **Nota:** opcional por decisão do Gustavo — só testar se [[ROADMAP_ARTE]] `A04-T2` for de fato produzida.
> >
> > - **Verificação (06/10):** a animação existe no recurso ligado à cena e `BossEnragedState` chama `PlayAnimation("enraged_transition")`.
> >
> > Status: ✅ passa (06/10) — recurso e chamada de estado presentes.
>
> > [!warning]- Task A04-T3 — `attack` do Korrag sem virar de lado
> > - [x] Golpe comum pros dois lados: os 3 quadros com a cabeça virada pro Kairo, conferidos na cena real em execução.
> > - [ ] Higor conferir a continuidade dos três quadros durante a luta.
> >
> > Status: 🟡 parcial (12/10) — seis combinações de quadro/direção passaram em `RoadmapArtReview.gd`; falta a avaliação jogada do Higor. Ver `docs/art-review/roadmap-visual-2026-10-12.md`.

> [!warning]- EPIC-A05 — Cenário — 🟡 Parcial (revisão do marco e caminhos pendentes) · 🟠 Alta
> *Pronto quando*: as 4 áreas têm identidade visual reconhecível sem texto, os pontos de entrada de cena fazem sentido, e a performance se mantém estável no `gl_compatibility`.
>
> Atualizado em 29/09/2026 — a demo passou a ser 4 áreas dentro da Floresta Tropical (era Floresta Tropical/Deserto/Tundra); `TargetArea`/`SpawnOffset` já funcionam desde [[ROADMAP_QA_CODIGO]] `C08-T1` (concluída em 28/09), então a ressalva de posicionamento manual não se aplica mais.
>
> > [!success]- Task A05-T1 — Sopé da Mata
> > - [x] Identidade visual clara — floresta úmida reconhecível nas capturas, com chão, vegetação e restauração aprovados por Higor.
> > - [x] Transição de área: Kairo nasce em `SpawnInicial` e retorna do Dossel em `SpawnFromDosselVivo`, via `SceneManager`.
> > - [x] Performance: tileset/background + pulsos de luz da Faísca + barras de vida flutuantes (com `Tween`) sem queda perceptível de FPS no renderer `gl_compatibility`.
> >
> > - **Integração (06/10):** Protótipo integrado ao Sopé da Mata: chão grama/terra/raízes/pedra, parallax, partículas e restauração seco→verde. Interação E e crescimento passaram no Godot .NET. Tileset modular, percurso completo e medição de FPS pendentes. Ver `docs/art-review/README.md` e capturas.
> > - **Revisão visual (08/10):** Higor aprovou as bordas de 24 px e a variação do miolo nas sete faixas saudáveis (`docs/art-review/sope-ground-modular-v1.md`); os dois visuais da restauração continuam com bordas preservadas. Capturas da cena real em `docs/art-review/sope-ground-borders-v1.md`; colisões não foram editadas.
> > - **Terreno modular (08/10):** a mesma abordagem de pontas e miolos foi consolidada nas quatro áreas. Igarapé e Covil deixaram de esticar uma ilustração inteira; os testes de área continuam aprovados. Ver `docs/art-review/modular-terrain-v1.md`.
> >
> > - **QA dirigido (08/10):** spawn inicial, câmera, restauração/inimigos presentes e retorno do Dossel passaram na cena real; três trechos renderizados mais a volta ao início sustentaram 60 FPS após aquecimento no `gl_compatibility`/GTX 1650. Ver `docs/art-review/sope-area-qa-2026-10-08.md`.
> >
> > Status: ✅ passa (11/10) — Gustavo jogou a demo inteira em 11/10 e validou (travessia com combate e fluidez); `tools/qa/FpsCheck.gd` em 11/10 (RTX 3050, gl_compatibility, vsync): média 60 nas 4 áreas, 1% piores 54–56 FPS.
>
> > [!success]- Task A05-T2 — Dossel Vivo
> > - [x] Repetir os 3 testes da Task A05-T1, mais: leitura clara das plataformas de pulo vertical.
> >
> > - **QA dirigido (08/10):** entrada pelo `Marker2D`, câmera, doze colisões dos galhos, restauração e inimigos passaram. Os vãos dos onze degraus consecutivos cabem no salto base segundo as dimensões e velocidades atuais; a travessia em jogo ainda não foi executada. FPS após aquecimento: 60 nas partes média/alta; base 58,3 (mín. 56) na primeira amostra e 60 ao revisitar. Ver `docs/art-review/dossel-canopy-restoration-v1.md`.
> >
> > Status: ✅ passa (11/10) — Gustavo jogou a demo inteira em 11/10 e validou; os 12 saltos alcançáveis só com pulo simples (`tools/qa/DosselReachCheck.gd`); `tools/qa/FpsCheck.gd` em 11/10 (RTX 3050, gl_compatibility, vsync): média 60 nas 4 áreas, 1% piores 54–56 FPS.
>
> > [!success]- Task A05-T3 — Igarapé Sufocado
> > - [x] Repetir os 3 testes da Task A05-T1, mais: a lama é visualmente clara (o jogador entende por que ficou mais lento).
> >
> > Status: ✅ passa (11/10) — Gustavo jogou a demo inteira em 11/10 e validou (lentidão na lama perceptível); as 3 poças dão x0,55 (`tools/qa/DemoRunCheck.gd`); `tools/qa/FpsCheck.gd` em 11/10 (RTX 3050, gl_compatibility, vsync): média 60 nas 4 áreas, 1% piores 54–56 FPS.
>
> > [!success]- Task A05-T4 — Covil de Korrag
> > - [x] Ambientação de arena de boss clara, sem elementos de exploração (ponto de restauração, etc.) — é só a luta.
> >
> > Status: ✅ passa (11/10) — Gustavo jogou a demo inteira em 11/10 e validou (luta com o Korrag); `tools/qa/FpsCheck.gd` em 11/10 (RTX 3050, gl_compatibility, vsync): média 60 nas 4 áreas, 1% piores 54–56 FPS.
>
> > [!warning]- Task A05-T5 — Marco de checkpoint
> > - [x] O marco único do Dossel Vivo mostra a arte final no estado **apagado** antes de tocar e **ativado** depois — nenhum `ColorRect` azul sobrando. *(Eram 5 checkpoints até 11/10; ver [[ROADMAP_DEV]] C09-T4.)*
> > - [x] Base alinhada ao chão e conferida na escala real.
> > - [x] Ao continuar um save, o marco já ativado nasce no estado ativado (`CheckpointVisualCheck.gd`).
> > - **Origem (11/10):** gêmea da [[ROADMAP_ARTE]] A05-T5 (achado da revisão de arte). Código da troca de estado: [[ROADMAP_DEV]] C09-T3.
> >
> > Status: 🟡 parcial (12/10) — dois estados, base, ativação e Continue passam em `RoadmapArtReview.gd` + `CheckpointVisualCheck.gd`; falta avaliação visual do Higor. Ver `docs/art-review/roadmap-visual-2026-10-12.md`.
>
> > [!failure]- Task A05-T6 — Caminhos que crescem ao restaurar
> > - [ ] O caminho fechado não parece uma plataforma usável; o aberto é legível e alcançável; colisão bate com a arte.
> >
> > Status: 🔴 bloqueado (11/10) — depende de [[ROADMAP_ARTE]] A05-T6 e [[ROADMAP_DEV]] C14-T5.

> [!warning]- EPIC-A06 — UI — 🟡 Parcial (validações do Higor pendentes) · 🟡 Média
> *Pronto quando*: barra de vida, ícone de habilidade e prompt de interação têm arte final integrada e funcional em jogo.
>
> > [!success]- Task A06-T1 — Barra de vida
> > - [x] Barra de vida com textura final.
> >
> > Status: ✅ passa (11/10) — Gustavo jogou a demo inteira em 11/10 e validou; a barra cai com dano real (100 → 90) em `tools/qa/DemoRunCheck.gd`.
>
> > [!success]- Task A06-T2 — Ícone de habilidade
> > - [x] Ícone de habilidade desbloqueada (quando a notificação real existir — [[ROADMAP_QA_CODIGO]] `C04-T4`). *(Substituído pela notificação "Nova habilidade: Dash", decisão do Higor.)*
> >
> > Status: ✅ concluída — Higor decidiu não usar indicador persistente; a notificação de desbloqueio real já existe.
>
> > [!success]- Task A06-T3 — Prompt de interação
> > - [x] Prompt de interação com estilo definido (quando [[ROADMAP_QA_CODIGO]] `C10-T1` for resolvido).
> >
> > Status: ✅ passa (11/10) — Gustavo jogou a demo inteira em 11/10 e validou; `[E] Restaurar` aparece perto dos 3 pontos de restauração e some depois de usar (`tools/qa/DemoRunCheck.gd`).

> > [!warning]- Task A06-T4 — Indicador compacto da espada
> > - [x] Estado inicial antes/depois de criar o jogador e ao recriar o HUD.
> > - [x] Toggle real e sete trocas rápidas; combate bloqueado com espada guardada.
> > - [x] Texto de habilidade preservado; HideAll/ShowAll incluem indicador.
> > - [x] Tamanho fixo 156×32 e estado preservado ao pausar.
> > - [x] Capturas dos dois estados na cena real do Sopé, em 1280×720.
> > - [x] Aprovação visual do indicador por Higor (07/10).
> > - [ ] Playtest manual de Higor. **(pendente de validação do Higor)**
> > - **Nota (11/10):** do lado do código está tudo verificado — Gustavo jogou a demo inteira e `tools/qa/CombatCheck.gd` cobre Q, lâmina, bandagem e HUD (35/35). Falta só o Higor jogar e marcar o item acima.
> >
> > Status: 🟡 parcial (07/10, revisto em 11/10) — aguarda só o playtest do Higor — 20 verificações passaram no Godot .NET, incluindo extensão/recolhimento, reversão rápida, ausência de tint e lâmina reta no ataque; build sem erros/avisos. Teste dirigido não substitui percurso completo, teste de teclado nem carregamento de save real. Evidência: `docs/art-review/sword-hud-validation.txt`.

> > [!warning]- Task A06-T5 — Tela de título animada
> > - [x] Novo jogo, Continuar (só com save) e Sair funcionam; cliques repetidos bloqueados durante o fade (`TitleFlowCheck.gd` do Higor).
> > - [x] Visual aprovado por Gustavo (11/10).
> > - [x] Ajuste de contraste e dica "W / S Navegar · Enter Selecionar" entregue e conferido em 1280×720 e 960×540; W/S testados com eventos de teclado ([[ROADMAP_ARTE]] A06-T5).
> > - [ ] Playtest manual de Higor navegando o menu só pelo teclado. **(pendente de validação do Higor)**
> > - **Origem (11/10):** gêmea da [[ROADMAP_ARTE]] A06-T5, que não tinha par de QA.
> >
> > Status: 🟡 parcial (12/10) — contraste, separação tecla/ação e W/S revisados em 1280×720 e 960×540. Falta o playtest do Higor e a avaliação visual desta revisão.

> > [!warning]- Task A06-T6 — Tela de fim da demo
> > - [x] Quatro contagens (0–3) e legibilidade conferidas em 1280×720 e 960×540, inclusive foco inicial do teclado.
> > - [ ] Higor conferir a aparição após derrotar Korrag e as três ações durante a partida.
> >
> > Status: 🟡 parcial (12/10) — acabamento integrado, oito capturas passaram em `RoadmapArtReview.gd`; falta percurso jogado e avaliação visual do Higor. Ver `docs/art-review/roadmap-visual-2026-10-12.md`.

> [!failure]- EPIC-A07 — Validação Final (Playtest Completo) — 🔴 Bloqueado · 🔴 Bloqueante
> *Pronto quando*: a demo roda de ponta a ponta com áudio, arte e código integrados, sem crash, softlock ou animação quebrada, validada por alguém de fora do grupo.
>
> Só roda depois que todos os Épicos 🔴 acima (deste arquivo e do [[ROADMAP_QA_CODIGO]]) estiverem ✅.
>
> > [!warning]- Task A07-T1 — Áudio integrado
> > - [x] Com os primeiros arquivos de SFX/música em `res://assets/audio/`, jogar a demo do início ao fim e confirmar que nenhum som falta silenciosamente (checar o console). *(14/10: `DemoRunCheck` 89/89, `AudioCheck` 12/12 e `MusicCheck` 5/5; nenhuma faixa ou SFX faltando.)*
> > - [x] Volume relativo entre música de fundo e efeitos. *(14/10: Music -12 dB; SFX -4 dB.)*
> > - [x] Som de ataque/hit sincronizado com o frame de impacto da animação. *(O `enemy_hit` toca no mesmo quadro do dano; o `player_attack` toca no início da estocada, como o sopro do golpe.)*
> > - [ ] Gustavo ouvir os placeholders jogando e anotar os que incomodam (entram primeiro na troca da [[ROADMAP_DEV]] C11-T5).
> >
> > Status: 🟡 parcial (14/10) — música, SFX, volume e mistura verificados; falta ouvir os efeitos durante um playtest real e decidir a substituição dos placeholders.
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

> [!warning]- EPIC-A08 — Deserto pós-demo — 🟡 Direção visual aprovada; integração pendente · 🟡 Pós-demo
> *Pronto quando*: a passagem dia→tarde→noite, o terreno, os inimigos e a cobra são legíveis na partida e a luta final passa no percurso completo.
>
> > [!success]- Task A08-T1 — Passagem do tempo
> > - [x] Entrada mostra sol; seção intermediária mostra entardecer; trecho da cobra é noturno.
> > - [x] Chão e personagens mudam de luz junto ao fundo; os três estados foram capturados no renderer.
> > - [x] Higor aprovou a direção dia → tarde → noite apresentada na cena isolada.
> > - **Verificação (15/10):** `DesertReviewCheck.gd` passou; capturas em `docs/art-review/desert-ingame-*-v1.png`.
> >
> > Status: ✅ passa para a prévia (15/10) — execução confirmada e direção aprovada; o percurso final será testado na C15-T1.
>
> > [!warning]- Task A08-T2 — Terreno, habitantes e áudio
> > - [x] Chão colide na mesma altura da arte; os três reskins aparecem no percurso.
> > - [x] Vento começa na cena e o arquivo de sibilo carrega.
> > - [ ] Confirmar num mapa completo as transições, dificuldades e posições finais.
> > - **Verificação (15/10):** cena isolada abriu sem erro; a demo existente não foi redirecionada.
> >
> > Status: 🟡 parcial (15/10) — arte da prévia aprovada; mapa completo e posicionamento final ainda não existem (C15-T1).
>
> > [!warning]- Task A08-T3 — Cobra chefe
> > - [x] Seis animações têm canvas 512×320 e a cobra aparece em escala grande à noite.
> > - [x] Sibilo dispara com o bote na prévia.
> > - [ ] Varrida de cauda e entrada/saída da areia têm animações próprias e antecipação legível, sem saltos entre quadros.
> > - [ ] Validar hitboxes, dano, fases, morte e saída no confronto real quando a mecânica existir.
> > - **Verificação (15/10):** `Boss_CobraDesertoVisual.tscn` apresenta a arte, sem script de combate.
> >
> > Status: 🟡 parcial (15/10) — direção visual e três ataques aprovados; faltam quadros específicos de cauda/areia (A08-T3) e luta funcional (C15-T2/C15-T3).

> [!warning]- EPIC-A09 — Cartografia da Floresta Tropical — 🟡 Revisão visual pendente · 🟡 Alta
> *Pronto quando*: o mapa mantém leitura e contraste em partida real, sem mostrar trechos não visitados, e a arte acompanha a revelação do sistema C16.
>
> > [!warning]- Task A09-T1 — Mapa e névoa em jogo
> > - [x] Captura com save novo mostra somente a primeira sala do Sopé; captura de revisão completa mostra a conexão Sopé → Dossel → Igarapé → Covil.
> > - [x] Nomes e passagens só aparecem após visitar as células correspondentes; marcador de Kairo permanece visível no trecho atual.
> > - [ ] Higor testar visualmente o mapa com `M` durante o percurso e aprovar ou apontar ajustes.
> > - **Verificação (15/10):** `WorldMapCheck.gd` passou sem falhas e gerou capturas no Godot.
> > - **Ajuste visual (15/10):** quatro cantos refeitos com ornamentos vegetais simétricos; capturas de névoa e mapa completo atualizadas.
> >
> > Status: 🟡 parcial (15/10) — comportamento e capturas verificados; falta revisão visual jogada do Higor.
