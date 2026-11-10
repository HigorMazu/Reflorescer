---
tipo: projeto
categoria: plano
status: aguardando-aprovacao
criado: 2026-11-09
tags: [projeto, plano]
projeto: "[[🗂️ Reflorescer]]"
descricao: Plano de implementação da essência, da loja e da mudança do totem
---
# Essência e loja no totem: plano de implementação

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** mobs soltam essência, o totem vai para o começo do Igarapé e abre uma loja com cinco upgrades de 3 níveis.

**Architecture:** regras e preços ficam numa classe pura (`UpgradeCatalog`). `SaveManager` guarda saldo e níveis. `PlayerController` aplica bônus sobre os valores base, sem tocar no `.tres`. A loja é um `Control` criado por código dentro da HUD.

**Tech Stack:** Godot 4.7.2 Mono, C#, testes GDScript em `tools/qa/`.

**Spec:** `docs/superpowers/specs/2026-11-09-essencia-loja-design.md`

## Global Constraints

- Nenhum sprite novo.
- Não alterar `PlayerStatsResource` nem os `.tres` de atributos.
- Preços 20/40/70; 3 níveis; Dano +5; ataque -0,04 s (mín. 0,28 s); Vida +15; Defesa -2 (mínimo 1 de dano); movimento +5%.
- Valor da essência = `StatsResource.ExperienceReward`.
- Morrer não perde essência; chefes não soltam essência.
- Todo `EventBus += ` em nó de cena tem `-=` em `_ExitTree()`.
- Trabalho em branch `feature/essencia-loja`, a partir de `origin/main`, levando as alterações pendentes de 09/11. Commits locais; **sem push** sem pedido do Gustavo.

Compilar e testar (a partir da pasta `💻 Código`):

```powershell
$g = "C:\Users\gusta\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe"
& $g --headless --path . --build-solutions --quit
& $g --headless --fixed-fps 60 --path . --script res://tools/qa/EssenceShopCheck.gd
```

## Review Focus

1. E segurado ao abrir a loja: a repetição da tecla não pode comprar nada.
2. Esc com a loja aberta: fecha só a loja, sem abrir o menu de pausa.
3. Essência no ar durante troca de área ou morte: é creditada exatamente uma vez.
4. Comprar Vida, morrer e usar Continuar: a vida máxima não dobra nem some.
5. Save antigo com `cp_dossel_01`: o Kairo renasce de forma válida.

---

### Task 1: Economia e save

**Files:**
- Create: `scripts/progression/UpgradeCatalog.cs`, `tools/qa/EssenceShopCheck.gd`
- Modify: `scripts/save/SaveData.cs`, `scripts/save/SaveManager.cs`, `scripts/core/EventBus.cs`

**Interfaces:**
- Produces: `enum UpgradeId { Damage, AttackSpeed, Health, Defense, MoveSpeed }`; `UpgradeCatalog.MaxLevel = 3`; `int Price(int level)` (-1 no máximo); `int DamageBonus(int)`, `float AttackCooldownReduction(int)`, `int HealthBonus(int)`, `int Defense(int)`, `float MoveSpeedMultiplier(int)`.
- Produces em `SaveManager`: `int Essence`, `void AddEssence(int)`, `int GetUpgradeLevel(UpgradeId)`, `int GetUpgradePrice(UpgradeId)`, `bool TryBuyUpgrade(UpgradeId)`.
- Produces em `EventBus`: `EssenceChanged(int total)`, `UpgradePurchased(int upgradeId)`.

- [ ] **Step 1: Criar a branch e o teste**

`EssenceShopCheck.gd` segue o molde de `BossRetryCheck.gd` (`_check`, guarda e devolve `save_0.json`, `quit(1)` se falhar). Função `_test_economy()`, depois de `StartNewGame`:

```gdscript
saves.call("AddEssence", 25)
_check(saves.get("Essence") == 25, "soma essência")
_check(saves.call("TryBuyUpgrade", DAMAGE), "compra nível 1")
_check(saves.get("Essence") == 5 and saves.call("GetUpgradeLevel", DAMAGE) == 1, "cobra 20")
_check(not saves.call("TryBuyUpgrade", DAMAGE), "sem saldo não compra")
_check(saves.get("Essence") == 5 and saves.call("GetUpgradeLevel", DAMAGE) == 1, "falha não altera nada")
saves.call("AddEssence", 200)
saves.call("TryBuyUpgrade", DAMAGE); saves.call("TryBuyUpgrade", DAMAGE)
_check(saves.call("GetUpgradeLevel", DAMAGE) == 3, "chega ao nível 3")
_check(saves.call("GetUpgradePrice", DAMAGE) == -1, "sem preço no máximo")
var left: int = saves.get("Essence")
_check(not saves.call("TryBuyUpgrade", DAMAGE) and saves.get("Essence") == left, "limite de 3 níveis")
```

Também `_test_legacy_save()`: grava em `user://save_0.json` um JSON com `LastCheckpointId` e `ActivatedCheckpoints` em `cp_dossel_01`, chama `ContinueGame(0)` e confere `LastCheckpointId == ""`, `IsCheckpointActivated("cp_dossel_01") == false` e `Essence == 0`.

- [ ] **Step 2: Rodar e ver falhar** (métodos inexistentes).

- [ ] **Step 3: Implementar**

`SaveData`: `int Essence`, `Dictionary<string,int> UpgradeLevels = new()` e `void MigrateLegacyCheckpoint()`, que, para `cp_dossel_01`, limpa `LastCheckpointId` e `LastCheckpointScene` e remove o id de `ActivatedCheckpoints`. Chamar em `ReadSave`, depois de `FromJson`.

`AddEssence` e `TryBuyUpgrade` seguem `RevealMapCell` (`:65`): `_currentSave ??= CreateNewSave(0)` e `SaveToFile` só se `HasSave`. Ignorar `AddEssence` com valor ≤ 0.

- [ ] **Step 4: Rodar e ver passar.** Saída: `0 falhas`.

- [ ] **Step 5: Commit** `feat: adiciona economia de essencia e upgrades no save`

### Task 2: Efeitos dos upgrades no Kairo

**Files:**
- Modify: `scripts/combat/DamageInfo.cs`, `scripts/combat/Health.cs`, `scripts/player/PlayerController.cs`
- Test: `tools/qa/EssenceShopCheck.gd`

**Interfaces:**
- Consumes: Task 1.
- Produces: `DamageInfo.IgnoresDefense`; `Health.FlatDefense`; `PlayerController.CurrentAttackCooldown`.

- [ ] **Step 1: Teste `_test_effects()`** num Sopé carregado, com essência suficiente:

```gdscript
var base_damage: int = p.get("CurrentAttackDamage"); var base_speed: float = p.get("CurrentMoveSpeed")
_buy(DAMAGE); _check(p.get("CurrentAttackDamage") == base_damage + 5, "dano +5")
_buy(ATTACK_SPEED); _check(is_equal_approx(p.get("CurrentAttackCooldown"), 0.36), "ataque 0,36")
_buy(MOVE); _check(is_equal_approx(p.get("CurrentMoveSpeed"), base_speed * 1.05), "movimento +5%")
var hp = _hp(); _buy(HEALTH); _check(_hp() == [hp[0] + 15, hp[1] + 15], "vida atual e máxima +15")
_buy(DEFENSE); _hit_from_enemy(p, 20); # cai 18
_hit_from_enemy(p, 1); # cai 1 (mínimo)
_fall(p, 400); # LastFallDamage integral, sem Defesa
```

`_hit_from_enemy` chama `Health.ResetInvulnerability`, se existir, ou espera a invulnerabilidade acabar, antes de bater. Incluir o caso 4 do Review Focus: comprar Vida, gravar, `ContinueGame` e conferir a mesma vida máxima.

- [ ] **Step 2: Rodar e ver falhar.**

- [ ] **Step 3: Implementar**

- `DamageInfo`: `bool IgnoresDefense`, padrão falso. O dano de queda (`PlayerController.cs:702`) marca verdadeiro.
- `Health.TakeDamage`: com `Damage > 0`, sem `IgnoresDefense` e com `FlatDefense > 0`, aplica `Max(1, Damage - FlatDefense)`.
- `CurrentAttackDamage` e `CurrentMoveSpeed` somam os bônus. `CurrentAttackCooldown` = base menos redução, e substitui o uso em `:289`.
- `:115` soma `HealthBonus`. `_Ready` define `Health.FlatDefense`.
- Assinar `UpgradePurchased`, com `-=` em `_ExitTree()`: Vida faz `SetMaxHealth(Max + 15)` e `SetCurrentHealth(atual + 15)`; Defesa atualiza `FlatDefense`.
- Animação `attack` (bloco em `:381-393`): enquanto ativa, `SpeedScale = base / CurrentAttackCooldown`; nas demais, 1. Mesmo princípio de `EnemyBase.cs:228-234`.
- O fim do dash (`:531`) fica como está.

- [ ] **Step 4: Rodar `EssenceShopCheck` e `CombatCheck`.** Saída: `0 falhas` em ambos.

- [ ] **Step 5: Commit** `feat: aplica upgrades de dano, ataque, vida, defesa e movimento`

### Task 3: Drop da essência e contador

**Files:**
- Create: `scripts/progression/EssenceOrb.cs`
- Modify: `scripts/enemies/EnemyBase.cs:309`, `scripts/ui/HUDController.cs`
- Test: `tools/qa/EssenceShopCheck.gd`

**Interfaces:**
- Consumes: `SaveManager.AddEssence`, `EssenceChanged`.
- Produces: `static EssenceOrb.Spawn(Node parent, Vector2 position, int amount)`.

- [ ] **Step 1: Teste `_test_drop()`** com Kairo próximo: matar um Robusto do Sopé soma 25 e um Voador soma 10, depois de esperar 2 s; o saldo só muda quando o orbe chega (conferir logo após a morte); o label da HUD mostra `✦ N`. Caso 3 do Review Focus: matar um mob e trocar de cena no frame seguinte credita uma vez só.

- [ ] **Step 2: Rodar e ver falhar.**

- [ ] **Step 3: Implementar**

`EssenceOrb : Node2D`, `ZIndex = 6`, desenhado em `_Draw` com círculos nas cores da Faísca. Flutua 0,15 s, voa até o grupo `Player` a 420 px/s e credita a menos de 18 px. Vida máxima de 1,5 s, ou Kairo ausente: credita. `_ExitTree` credita se ainda não creditou, com uma flag que garante uma única vez.

`EnemyBase`, junto do `EmitSignal` de `:309`: `EssenceOrb.Spawn(GetTree().CurrentScene, GlobalPosition, StatsResource.ExperienceReward)`.

`HUDController`: `Label` criado em código na `TopRow`, texto `✦ {total}`, iniciado com `SaveManager.Essence`, assinando `EssenceChanged` com `-=`.

- [ ] **Step 4: Rodar `EssenceShopCheck` e `DemoRunCheck`.** Saída: `0 falhas` e `89 ok`.

- [ ] **Step 5: Commit** `feat: mobs soltam essencia e HUD mostra o saldo`

### Task 4: Totem no Igarapé

**Files:**
- Modify: `scenes/areas/DosselVivo.tscn`, `scenes/areas/IgarapeSufocado.tscn`
- Test: `tools/qa/EssenceShopCheck.gd`, `CheckpointVisualCheck.gd`, `AudioCheck.gd`, `RewardCheck.gd`, `DosselPortalLayerCheck.gd`, `tools/art/RoadmapArtReview.gd`

**Interfaces:** Produces: `Interactables/Checkpoint1` no Igarapé, id `cp_igarape_01`, posição `(300, 480)`.

- [ ] **Step 1: Teste `_test_totem()`:** Dossel sem `Interactables/Checkpoint1`; Igarapé com ele em x entre 240 e 420 (depois da arte do portal, que acaba em x=227, e antes da lama, que começa em x=480); ao passar, `LastCheckpointId == "cp_igarape_01"`; morrer no Covil volta para o Igarapé; morrer no Dossel antes de ativar volta para o Sopé.

- [ ] **Step 2: Rodar e ver falhar.**

- [ ] **Step 3: Mover nós**

Remover do Dossel `Checkpoint1`, `Tutorial/TutCheckpoint` e os `ext_resource`/`sub_resource` sem uso. Criar no Igarapé o `Checkpoint1` (com `TotemVisual`, `AutoActivateOnTouch = true`, `z_index = 2`) e um nó `Tutorial` com `TutCheckpoint` em `(310, 380)`, `Keys = "Totem"` e `Text = "salva o progresso e abre a loja: ao morrer, o Kairo volta pra cá"`.

- [ ] **Step 4: Atualizar os testes**

`CheckpointVisualCheck`, `AudioCheck`, `RewardCheck` e `RoadmapArtReview` passam a usar o Igarapé e `cp_igarape_01`. `DosselPortalLayerCheck` perde só a checagem do totem e mantém as demais.

- [ ] **Step 5: Rodar os 5 testes acima e `DemoRunCheck`.** Saída: nenhuma falha.

- [ ] **Step 6: Commit** `feat: move o totem para o comeco do Igarape`

### Task 5: Tela da loja

**Files:**
- Create: `scripts/ui/ShopOverlay.cs`
- Modify: `scripts/world/Checkpoint.cs:55-91`, `scripts/ui/HUDController.cs`, `scripts/core/GameManager.cs`
- Test: `tools/qa/EssenceShopCheck.gd`

**Interfaces:**
- Consumes: Task 1.
- Produces: `HUDController.OpenShop()`; `GameManager.IsShopOpen`, `SetShopOpen(bool)`; `ShopOverlay.IsOpen`, `SelectedIndex`, `Open()`, `Close()`, `MoveSelection(int)`, `BuySelected() -> bool`.

- [ ] **Step 1: Teste `_test_shop()`** no Igarapé:

```gdscript
_check(cp.call("CanInteract") and cp.call("GetInteractionPrompt") == "Ativar checkpoint", "antes de ativar")
# depois de ativar:
_check(cp.call("GetInteractionPrompt") == "Loja", "prompt da loja")
cp.call("Interact", p); await process_frame
_check(shop.get("IsOpen") and paused, "abre e pausa")
_send_echo("interact"); _check(saves.get("Essence") == before, "E repetido não compra")
_send("ui_right"); _check(shop.get("SelectedIndex") == 1, "navega para o lado")
_send("ui_left"); _send("ui_left"); _check(shop.get("SelectedIndex") == 4, "dá a volta")
_send("interact"); _check(comprou, "E compra"); _send("ui_accept"); _check(comprou_outro, "A compra")
_send("pause"); _check(not shop.get("IsOpen") and not gm.get("IsGamePaused") and not paused, "Esc fecha só a loja")
_send("map") # loja aberta: mapa não abre
```

Conferir também o texto da carta: nome, efeito do próximo nível, preço e `MÁX`.

- [ ] **Step 2: Rodar e ver falhar.**

- [ ] **Step 3: `GameManager`:** `SetShopOpen` define `Paused`. `_Input` retorna logo depois de `Observe` se `IsShopOpen`. `ToggleMap` e `TogglePause` também saem cedo.

- [ ] **Step 4: `Checkpoint`:** `CanInteract()` retorna verdadeiro; prompt ativado é `"Loja"`; `Interact` ativado chama `HUDController.OpenShop()`, encontrada pelo grupo `HUDController.GroupName`.

- [ ] **Step 5: `ShopOverlay : Control`**, criado por código e sob demanda pela HUD, como `BuildGameOverPanel`. `ProcessMode = Always`.

Formato C do spec: painel no topo (x de 3% a 97%, y de 7% a 65%), título "Totem da Floresta", saldo `✦ N`, cinco cartas com os nomes `Dano`, `Vel. ataque`, `Vida`, `Defesa`, `Vel. mov.`. Cores da HUD: fundo `0.06, 0.12, 0.1`, borda `0.67, 0.77, 0.46`. Preço vermelho sem saldo; `MÁX` no nível 3.

`_Input` ignora `IsEcho()`. `ui_left`/`ui_right` navegam com volta; `interact`/`ui_accept` compram; `pause`/`ui_cancel` fecham; sempre `SetInputAsHandled`. Compra usa `checkpoint_activate.wav`; compra negada usa `sword_off.wav`. Se o `AudioManager` não tocar com o jogo pausado, dar `ProcessMode = Always` aos players de SFX.

- [ ] **Step 6: Rodar `EssenceShopCheck`, `GamepadCheck` e `DesertLinkCheck`.** Saída: nenhuma falha.

- [ ] **Step 7: Commit** `feat: adiciona loja de upgrades no totem`

### Task 6: Documentação e regressão

**Files:** `docs/ROADMAP_DEV.md`, `docs/ROADMAP_QA_CODIGO.md`, `CLAUDE.md`, `🕹️ Sistemas de Jogo.md`, `🗳️ Decisões e Configuração.md`, `🗓️ Log de Atualizações.md`, `🌍 Mundo e Biomas.md`, `✅ Próximos Passos.md`, comentário em `PlayerController.cs:641`.

- [ ] **Step 1:** `C20-T5` e sua gêmea de QA passam a `info`/`warning`, com checklist e contadores do frontmatter recontados; `EssenceShopCheck` entra na lista do `CLAUDE.md`.
- [ ] **Step 2:** Registrar no GDD o totem no Igarapé, a loja e os valores; retirar "bandeira no Dossel" das notas.
- [ ] **Step 3: Regressão completa:** todos os `tools/qa/*.gd`, usando as regras do `CLAUDE.md` para os que rodam sem `--headless`. Saída: nenhuma falha.
- [ ] **Step 4:** Listar para o Gustavo o teste jogando: pegar essência, abrir a loja com teclado e controle, comprar os 5 upgrades, ver a estocada a 0,28 s, enfrentar o Korrag com upgrades.
- [ ] **Step 5: Commit** `docs: registra essencia, loja e totem no Igarape`
