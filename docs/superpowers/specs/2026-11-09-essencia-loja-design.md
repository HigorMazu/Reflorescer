---
tipo: projeto
categoria: spec
status: aprovado
criado: 2026-11-09
tags: [projeto, spec]
projeto: "[[🗂️ Reflorescer]]"
descricao: Design da essência dos mobs, da loja no totem e da mudança do totem para o Igarapé
---
# Essência e loja no totem

Refere-se à Task `C20-T5` do [[ROADMAP_DEV]].

## Por que

No playtest de 09/11, o Gustavo notou que matar mobs não dá nenhuma recompensa. A decisão de 06/11 já era "essência + upgrades na bandeira". Este documento fecha o formato.

## Objetivos

- Todo mob morto paga uma recompensa visível.
- O jogador escolhe como ficar mais forte antes do Korrag.
- Nenhum sprite novo: tudo desenhado por código ou com a UI do Godot.

## Fora de escopo

- Agachar no S (adiado em 09/11, sem sprite).
- O restante da `C20-T4`: chefes mais agressivos, cura limitada e padrões novos dos mobs.
- Vários totens na Floresta. Continua existindo **um só**.
- Mudar a recompensa de restauração (+10 de vida máxima e cura), que segue como está.

## O que não pode acontecer

- Save antigo travar ou deixar o jogador sem renascer.
- Estocada visivelmente cortada na velocidade máxima de ataque.
- Jogo destravado ou entrada duplicada com a loja aberta.
- Essência perdida ou duplicada ao trocar de área, morrer ou recarregar.

## Comportamento decidido

### Essência

| Item | Decisão | Origem |
|---|---|---|
| Forma | Luz ligada à Faísca, desenhada por código | Gustavo aprovou no pacote |
| Valor | `ExperienceReward` do inimigo: Robusto 25, demais 10 | Gustavo aprovou no pacote |
| Coleta | Sai do corpo do mob e voa até o Kairo; soma ao chegar | Gustavo aprovou no pacote |
| Ao morrer | Não perde essência | Gustavo aprovou no pacote |
| Respawn de mobs | Continuam voltando ao recarregar a cena, permitindo acumular | Decisão do Gustavo |
| Save | Saldo e níveis comprados entram no save | Gustavo aprovou no pacote |
| HUD | Contador `✦ N` | Gustavo aprovou no pacote |

Matar os 16 mobs das áreas 1 a 3 uma vez rende 220 de essência.

### Totem

- O totem sai do Dossel Vivo e vai para o **começo do Igarapé Sufocado**, depois do spawn `SpawnFromDosselVivo` (x=110) e fora da arte do portal `ToDosselVivo`.
- Cena do Dossel: remover `Interactables/Checkpoint1` e a zona `Tutorial/TutCheckpoint`. A dica de totem vai para o Igarapé.
- Regras mantidas: ativa ao passar, cura ao passar toda vez, morrer em qualquer área volta para ele, e morrer antes de ativá-lo volta para o começo do Sopé da Mata.
- Depois de ativado, o totem oferece `[E] Loja`. O prompt continua existindo após a ativação.
- O totem do Deserto também abre a loja, porque usa o mesmo `Checkpoint`.

### Upgrades

Cada upgrade tem 3 níveis, com preços de **20, 40 e 70**. O custo de completar tudo é 650.

| Upgrade | Base | Por nível | No máximo |
|---|---|---|---|
| Dano | 25 | +5 | 40 |
| Velocidade de ataque | 0,30 s (valor real do `PlayerStatsResource.tres`) | -0,03 s | 0,21 s (mesmos 30% de ganho combinados; a animação acelera junto) |
| Vida | 100 | +15 | +45 |
| Defesa | 0 | -2 de dano por golpe, mínimo 1 | -6 |
| Velocidade de movimento | 200 com espada, 240 sem | +5% | +15% |

- Comprar Vida aumenta a vida atual na mesma quantidade.
- Defesa vale para golpe e contato; **não** reduz dano de queda.
- Bônus de Vida soma com o bônus de restauração.

### Tela da loja (formato C, faixa de cartas)

- Painel na parte de cima da tela, com o Kairo e o totem visíveis embaixo, nas cores da HUD (`0.06, 0.12, 0.1` e borda `0.67, 0.77, 0.46`).
- Título "Totem da Floresta" e saldo `✦ N` no canto.
- Cinco cartas lado a lado. Cada uma mostra nome, 3 bolinhas de nível, efeito do próximo nível e preço.
- Preço em vermelho quando falta essência; `MÁX` no nível 3.
- Navegação para os lados; comprar com `interact`/`E` ou A no controle; sair com Esc ou B.
- O jogo fica pausado com a loja aberta. Comprar sem saldo não faz nada além de um retorno sonoro já existente.

## Pontos técnicos que orientam a implementação

- **Valores base:** não alterar `PlayerStatsResource`. O `.tres` é compartilhado; aplicar bônus nos getters do `PlayerController` (`CurrentAttackDamage`, `CurrentMoveSpeed`, cooldown em `:289`).
- **Vida:** `PlayerController.cs:115` calcula vida máxima a partir da base e das restaurações; somar os níveis de Vida ali.
- **Dash:** `PlayerController.cs:531` usa `StatsResource.MoveSpeed` ao terminar o dash; decidir no plano se passa por `CurrentMoveSpeed`, sem mudar a velocidade do dash em si.
- **Defesa:** é nova. Aplicar só em dano vindo de golpe ou contato, separado do dano de queda (`PlayerController.cs:699`).
- **Essência:** escutar a morte em `EnemyBase.cs:309`. O sinal atual `EnemyDefeated` só leva o id, então o drop precisa também da posição e do valor.
- **Checkpoint:** `Checkpoint.CanInteract()` hoje retorna falso depois de ativado, e o prompt muda junto.
- **Pausa:** seguir o padrão do mapa em `GameManager.cs:67`; a loja precisa de `ProcessMode` que funcione pausado.
- **EventBus:** toda assinatura em nó de cena exige `-=` em `_ExitTree()`.
- **Save antigo:** `SaveData` ganha campos com valor padrão. Um save com `cp_dossel_01` ativo e última bandeira no Dossel precisa renascer de forma válida.

## Testes que precisam mudar

Dependem do `Interactables/Checkpoint1` no Dossel: `CheckpointVisualCheck`, `AudioCheck`, `RewardCheck`, `RoadmapArtReview` e `DosselPortalLayerCheck`. Conferir também o `DemoRunCheck`, que interage com bandeira e restauração.

Teste novo cobrindo: drop e soma por tipo de mob, saldo e níveis no save, preço e limite de 3 níveis, compra sem saldo, efeito de cada upgrade, Defesa fora do dano de queda, loja só com totem ativado, pausa e save antigo.

## Documentação a atualizar ao implementar

- `🕹️ Sistemas de Jogo.md`: seção "Morte, bandeira e vida na demo" passa a falar do Igarapé.
- `🗳️ Decisões e Configuração.md` e `🗓️ Log de Atualizações.md`: totem movido e formato da loja.
- `ROADMAP_DEV.md` e `ROADMAP_QA_CODIGO.md`: `C20-T5` e contadores.
- `🌍 Mundo e Biomas.md`, se citar a bandeira no Dossel.

## Decidido pelo Claude, para revisão

- Botões da loja no controle (A compra, B sai).
- Título "Totem da Floresta" também no Deserto; pode virar "Totem do Deserto" sem custo.
- Comprar Vida aumenta também a vida atual.
- Novo id `cp_igarape_01` para o totem movido.
