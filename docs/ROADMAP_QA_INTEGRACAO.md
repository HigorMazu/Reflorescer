# Roadmap de QA — Integração (Código + Arte do Higor)

Casos de teste que confirmam a integração entre o código do Gustavo e a arte do Higor — mesma hierarquia Épico → Task → Subtask do `ROADMAP_ARTE.md`, mesmos IDs (`EPIC-Axx` / `Axx-Ty`), espelhados 1:1. Só entra em jogo **depois** que a Task equivalente de código já passa no `ROADMAP_QA_CODIGO.md` com arte placeholder — este arquivo não testa mecânica nova, só confirma que a arte final não quebrou o que já funcionava. Cada Task fecha com uma linha `Status:` que reflete o estado real de hoje (a maioria ainda bloqueada, porque a arte ainda não chegou) — atualizar o checkbox, o ícone e a linha `Status:` a cada entrega do Higor, e registrar no Notion qualquer decisão nova que a integração revelar (regra do topo da página lá).

Legenda de status: ✅ passa · 🔴 ainda não dá pra testar (falta arte ou código) · 🟡 parcial.

---

<details>
<summary><strong>EPIC-A00 — Padrões Técnicos</strong> 🔴 Bloqueado · 🔴 Bloqueante</summary>

*Pronto quando*: todo `SpriteFrames` entregue segue canvas e pivô consistentes, e o flip (por escala no Kairo, `FlipH` em inimigos/boss) não distorce a arte.

<details>
<summary>Task A00-T1 — Formato e consistência</summary>

- [ ] Cada `SpriteFrames` entregue usa o mesmo tamanho de canvas em todos os frames de um personagem.
- [ ] Pivô/ancoragem consistente entre animações do mesmo personagem — sem "pulo" de posição ao trocar.

Status: 🔴 bloqueado — nenhuma arte final entregue ainda para verificar.

</details>

<details>
<summary>Task A00-T2 — Flip</summary>

- [ ] Kairo: flip por escala não distorce elementos assimétricos da arte final (ex: espada só de um lado).
- [ ] Inimigos/boss: `FlipH` espelha corretamente sem distorcer asas/antenas/elementos assimétricos.

Status: 🔴 bloqueado — depende da primeira entrega de arte com elementos assimétricos.

</details>

</details>

<details>
<summary><strong>EPIC-A01 — Kairo</strong> 🔴 Bloqueado · 🔴 Bloqueante</summary>

*Pronto quando*: as animações núcleo substituem o placeholder sem regressão de física, e a Espada de Grama alterna visual corretamente em jogo.

<details>
<summary>Task A01-T1 — Animações núcleo</summary>

- [ ] Trocar o placeholder pelo pacote idle/run/jump/fall/attack/hurt/dead e rodar de novo a Task C01-T2 do `ROADMAP_QA_CODIGO.md` — nenhum comportamento de física deve mudar, só o visual.

Status: 🔴 bloqueado — depende de `ROADMAP_ARTE.md` `A01-T1`.

</details>

<details>
<summary>Task A01-T2 — Espada de Grama</summary>

- [ ] Sprite da espada visível quando ativada, "bandagem" visível quando desativada — nunca os dois ao mesmo tempo.
- [ ] Animação de ataque: a janela de dano da hitbox é fixa em 0.14s hoje, independente da duração da animação — ajustar o tempo da hitbox (ou o `AttackCooldown`) se a animação final não bater com esse corte.
- **Dependência dupla:** só testável depois que `ROADMAP_DEV.md` `EPIC-C03` (toggle da espada) **e** `ROADMAP_ARTE.md` `A01-T2` (os 2 visuais) estiverem prontos.

Status: 🔴 bloqueado — depende de código e arte, nenhum dos dois pronto ainda.

</details>

<details>
<summary>Task A01-T3 — Wall grab / wall jump</summary>

- [ ] Animação integrada e sincronizada com a mecânica (`ROADMAP_DEV.md` `EPIC-C05`).

Status: 🔴 bloqueado — mecânica de código ainda não existe.

</details>

<details>
<summary>Task A01-T4 — Dash</summary>

- [ ] Variação visual integrada e sincronizada com a mecânica (`ROADMAP_DEV.md` `EPIC-C04`).

Status: 🔴 bloqueado — mecânica de código ainda não existe.

</details>

</details>

<details>
<summary><strong>EPIC-A02 — Inimigos Comuns</strong> 🔴 Bloqueado · 🔴 Bloqueante</summary>

*Pronto quando*: os 3 arquétipos da Floresta Tropical rodam com arte final sem tocar em script, e os reskins de Deserto/Tundra repetem o mesmo resultado.

<details>
<summary>Task A02-T1/T2/T3 — Arquétipos da Floresta Tropical</summary>

- [ ] Trocar o `SpriteFrames` do inimigo base pelo de cada arquétipo, **sem tocar em nenhum script**, e confirmar que tudo funciona igual (valida a decisão de design "reskin sem mudar código").
- [ ] Quando as subclasses de arquétipo existirem (`ROADMAP_DEV.md` `EPIC-C07`): confirmar que o Voador de fato ignora colisão de chão/voa — isso é física, não só arte.
- [ ] `FlipH` espelha corretamente os 3 arquétipos.

Status: 🔴 bloqueado — depende de `ROADMAP_ARTE.md` `A02-T1/T2/T3` e, pro segundo caso, de `ROADMAP_DEV.md` `EPIC-C07`.

</details>

<details>
<summary>Task A02-T4/T5 — Reskins Deserto e Tundra</summary>

- [ ] Repetir os testes da Task A02-T1/T2/T3 pra cada reskin.

Status: 🔴 bloqueado — depende da Task A02-T1/T2/T3 passar primeiro.

</details>

</details>

<details>
<summary><strong>EPIC-A03 — Faísca</strong> 🔴 Bloqueado · 🟠 Alta</summary>

*Pronto quando*: `fly`/`investigate` acompanham a troca de estado sem travar num frame parado, e a luz não "pisca" ao trocar de direção.

<details>
<summary>Task A03-T1</summary>

- [ ] Com `fly`/`investigate` adicionadas, a troca de animação acompanha a troca de estado (`FollowState`/`IdleFaiscaState`/`InvestigateState`/`ReturnToPlayerState`) sem travar num frame parado.
- [ ] A luz (`PointLight2D`) pulsa e o flip de sprite não "pisca" de forma estranha ao trocar de direção.

Status: 🔴 bloqueado — depende de `ROADMAP_ARTE.md` `A03-T1`.

</details>

</details>

<details>
<summary><strong>EPIC-A04 — Boss KORRAG</strong> 🔴 Bloqueado · 🟠 Alta</summary>

*Pronto quando*: as transições de fase são claras pro jogador mesmo com `idle` compartilhado, o timing de `charge_windup` bate com o código, e o nome exibido é "Korrag, o Javali".

<details>
<summary>Task A04-T1 — Obrigatórios</summary>

- [ ] Transições de fase (`ROADMAP_QA_CODIGO.md` `C06-T0`) comunicam claramente ao jogador que o boss mudou de fase, mesmo com `idle` compartilhado.
- [ ] Quando `StartCharge()`/`PerformStomp()` forem ligados (`ROADMAP_DEV.md` `EPIC-C06-T1`): `charge_windup` cabe no ~1s fixo antes da investida disparar.
- [ ] Animação `defeated`: hoje o boss vira invisível e é destruído 0.3s depois de `Dead` — se `defeated` for mais longa, ajustar esse tempo no código.
- [ ] Nome exibido em qualquer UI usa "Korrag, o Javali", não "Javali das Ruínas".

Status: 🔴 bloqueado — depende de `ROADMAP_ARTE.md` `A04-T1` e de `ROADMAP_DEV.md` `C06-T1`/`C06-T3`.

</details>

<details>
<summary>Task A04-T2 — Opcional</summary>

- [ ] `enraged_transition` integrada, se entregue.
- **Nota:** opcional por decisão do Gustavo — só testar se `ROADMAP_ARTE.md` `A04-T2` for de fato produzida.

Status: não aplicável ainda — condicional a sobrar tempo pra arte opcional.

</details>

</details>

<details>
<summary><strong>EPIC-A05 — Cenário</strong> 🔴 Bloqueado · 🟠 Alta</summary>

*Pronto quando*: as 3 áreas têm identidade visual reconhecível sem texto, o ponto de entrada de cena faz sentido apesar do spawn direcional não existir ainda, e a performance se mantém estável no `gl_compatibility`.

<details>
<summary>Task A05-T1 — Floresta Tropical</summary>

- [ ] Identidade visual clara — dá pra saber que bioma é só de olhar, sem ler texto.
- [ ] Transição de área: como o código hoje **não** usa `TargetArea`/`SpawnOffset` (`ROADMAP_QA_CODIGO.md` `C08-T1`), o ponto de entrada da cena precisa ser posicionado manualmente de forma que faça sentido.
- [ ] Performance: tileset/background + pulsos de luz da Faísca + barras de vida flutuantes (com `Tween`) sem queda perceptível de FPS no renderer `gl_compatibility`.

Status: 🔴 bloqueado — depende de `ROADMAP_ARTE.md` `A05-T1`.

</details>

<details>
<summary>Task A05-T2/T3 — Deserto e Tundra</summary>

- [ ] Repetir os 3 testes da Task A05-T1.

Status: 🔴 bloqueado — depende da Task A05-T1 passar primeiro.

</details>

</details>

<details>
<summary><strong>EPIC-A06 — UI</strong> 🔴 Bloqueado · 🟡 Média</summary>

*Pronto quando*: barra de vida, ícone de habilidade e prompt de interação têm arte final integrada e funcional em jogo.

<details>
<summary>Task A06-T1 — Barra de vida</summary>

- [ ] Barra de vida com textura final.

Status: 🔴 bloqueado — depende de `ROADMAP_ARTE.md` `A06-T1`.

</details>

<details>
<summary>Task A06-T2 — Ícone de habilidade</summary>

- [ ] Ícone de habilidade desbloqueada (quando a notificação real existir — `ROADMAP_QA_CODIGO.md` `C04-T4`).

Status: 🔴 bloqueado — depende de código e arte, nenhum dos dois pronto ainda.

</details>

<details>
<summary>Task A06-T3 — Prompt de interação</summary>

- [ ] Prompt de interação com estilo definido (quando `ROADMAP_QA_CODIGO.md` `C10-T1` for resolvido).

Status: 🔴 bloqueado — depende de código e arte, nenhum dos dois pronto ainda.

</details>

</details>

<details>
<summary><strong>EPIC-A07 — Validação Final (Playtest Completo)</strong> 🔴 Bloqueado · 🔴 Bloqueante</summary>

*Pronto quando*: a demo roda de ponta a ponta com áudio, arte e código integrados, sem crash, softlock ou animação quebrada, validada por alguém de fora do grupo.

Só roda depois que todos os Épicos 🔴 acima (deste arquivo e do `ROADMAP_QA_CODIGO.md`) estiverem ✅.

<details>
<summary>Task A07-T1 — Áudio integrado</summary>

- [ ] Com os primeiros arquivos de SFX/música em `res://assets/audio/`, jogar a demo do início ao fim e confirmar que nenhum som falta silenciosamente (checar o console).
- [ ] Volume relativo entre música de fundo e efeitos.
- [ ] Som de ataque/hit sincronizado com o frame de impacto da animação.

Status: 🔴 bloqueado — depende de `ROADMAP_DEV.md` `EPIC-C11`.

</details>

<details>
<summary>Task A07-T2 — Ponta a ponta</summary>

- [ ] Demo completa sem parar: Floresta Tropical → matar 1 de cada arquétipo → derrotar o Korrag → dash desbloqueado → ponto de encerramento definido (seção 13 do Notion) — sem crash, sem softlock, sem animação quebrada.
- [ ] Repetir morrendo de propósito em cada área (valida respawn/checkpoint com arte e áudio reais).

Status: 🔴 bloqueado — depende de todos os Épicos anteriores, de código e de arte.

</details>

<details>
<summary>Task A07-T3 — Playtest cego</summary>

- [ ] Alguém de fora do grupo joga sem explicação prévia — se entender controles e objetivo, a demo está pronta pra apresentação.

Status: 🔴 bloqueado — última etapa, depende de tudo acima.

</details>

</details>
