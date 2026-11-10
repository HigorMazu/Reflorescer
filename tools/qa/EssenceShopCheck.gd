extends SceneTree
## C20-T5: essência, upgrades e loja no totem. O save_0.json é guardado e devolvido.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/EssenceShopCheck.gd
const SAVE_FILE := "user://save_0.json"
const DOSSEL := "res://scenes/areas/DosselVivo.tscn"
const SOPE := "res://scenes/areas/SopeDaMata.tscn"
const IGARAPE := "res://scenes/areas/IgarapeSufocado.tscn"
const COVIL := "res://scenes/areas/CovilDeKorrag.tscn"
const DAMAGE := 0 # UpgradeId.Damage
const ATTACK_SPEED := 1
const HEALTH := 2
const DEFENSE := 3
const MOVE := 4

var _failures := 0
var _passes := 0
var _backup = null

func _initialize() -> void:
	_run.call_deferred()

func _check(ok: bool, what: String) -> void:
	if ok: _passes += 1
	else: _failures += 1
	print("  ", "ok    " if ok else "FALHA ", what)

func _saves() -> Node:
	return root.get_node("SaveManager")

func _run() -> void:
	if FileAccess.file_exists(SAVE_FILE):
		_backup = FileAccess.get_file_as_string(SAVE_FILE)
	await _test_economy()
	await _test_legacy_save()
	await _test_effects()
	await _test_drop()
	await _test_totem()
	await _test_shop()
	_finish()

func _frames(n: int) -> void:
	for i in n:
		await physics_frame

func _player() -> CharacterBody2D:
	return get_first_node_in_group("Player") as CharacterBody2D

func _hp(p: Node) -> Array:
	var health = p.get("Health")
	return [health.get("CurrentHealth"), health.get("MaxHealth")]

func _find_enemy(reward: int) -> Node:
	for e in get_nodes_in_group("Enemies"):
		if e.get("StatsResource").get("ExperienceReward") == reward: return e
	return null

func _kill(probe: Node, enemy: Node) -> void:
	probe.call("Damage", enemy.get("Health"), 99999)

func _test_drop() -> void:
	print("== Mobs soltam essência")
	var saves := _saves()
	var probe: Node = load("res://tools/qa/QaProbe.cs").new()
	root.add_child(probe)
	saves.call("StartNewGame", 0)
	await scene_changed
	await _frames(10)
	var p := _player()
	p.get("Health").call("SetInvulnerable", true)
	var robusto := _find_enemy(25)
	_check(robusto != null, "Sopé tem um Robusto")
	var label := get_first_node_in_group("HUD").find_child("EssenceLabel", true, false) as Label
	_check(label != null and label.text == "✦ 0", "contador da HUD começa em 0")
	p.global_position = robusto.global_position + Vector2(-90, -20)
	await _frames(2)
	_kill(probe, robusto)
	_check(saves.get("Essence") == 0, "saldo só muda quando a luz chega")
	await _frames(150)
	_check(saves.get("Essence") == 25, "Robusto vale 25 (%d)" % saves.get("Essence"))
	_check(label != null and label.text == "✦ 25", "contador acompanha o saldo")
	var voador := _find_enemy(10)
	p.global_position = voador.global_position + Vector2(-90, 0)
	await _frames(2)
	_kill(probe, voador)
	await _frames(150)
	_check(saves.get("Essence") == 35, "Voador vale 10 (%d)" % saves.get("Essence"))

	print("== Luz no ar durante a troca de área")
	var other := _find_enemy(10)
	_kill(probe, other)
	await process_frame
	change_scene_to_file(DOSSEL)
	await scene_changed
	await _frames(120)
	_check(saves.get("Essence") == 45, "credita uma vez só (%d)" % saves.get("Essence"))
	probe.queue_free()

func _send(action: String) -> void:
	var ev := InputEventAction.new()
	ev.action = action
	ev.pressed = true
	root.push_input(ev)

func _send_echo_e() -> void:
	var ev := InputEventKey.new()
	ev.physical_keycode = KEY_E
	ev.keycode = KEY_E
	ev.pressed = true
	ev.echo = true
	root.push_input(ev)

func _card_text(shop: Node, index: int) -> String:
	var text := ""
	for l in shop.find_child("Card%d" % index, true, false).find_children("*", "Label", true, false):
		text += (l as Label).text + "|"
	return text

func _test_shop() -> void:
	print("== Loja no totem")
	var saves := _saves()
	var gm := root.get_node("GameManager")
	saves.call("StartNewGame", 0)
	await scene_changed
	change_scene_to_file(IGARAPE)
	await scene_changed
	await _frames(10)
	for e in get_nodes_in_group("Enemies"): e.queue_free()
	var cp := current_scene.get_node("Interactables/Checkpoint1") as Node2D
	var p := _player()
	_check(cp.call("CanInteract") and cp.call("GetInteractionPrompt") == "Ativar checkpoint", "antes de ativar")
	p.global_position = cp.global_position + Vector2(0, -20)
	await _frames(10)
	_check(cp.call("CanInteract") and cp.call("GetInteractionPrompt") == "Loja", "prompt da loja depois de ativar")
	saves.call("AddEssence", 25)
	cp.call("Interact", p)
	await process_frame
	var shop := get_first_node_in_group("HUD").find_child("ShopOverlay", true, false)
	_check(shop != null and shop.get("IsOpen") and paused, "abre e pausa")
	if shop == null:
		paused = false
		return
	var card := _card_text(shop, 0)
	_check("Dano" in card and "25 → 30" in card and "✦ 20" in card, "carta mostra nome, efeito e preço (%s)" % card)
	_send_echo_e()
	_check(saves.get("Essence") == 25, "E repetido não compra")
	_send("ui_right")
	_check(shop.get("SelectedIndex") == 1, "navega para o lado")
	_send("ui_left")
	_send("ui_left")
	_check(shop.get("SelectedIndex") == 4, "dá a volta")
	_send("ui_right")
	_send("interact")
	_check(saves.get("Essence") == 5 and saves.call("GetUpgradeLevel", DAMAGE) == 1, "E compra")
	_check("40" in _card_text(shop, 0), "carta atualiza o preço")
	var price := shop.find_child("Card0", true, false).find_child("Price", true, false) as Label
	_check(price.get_theme_color("font_color").r > 0.7 and price.get_theme_color("font_color").g < 0.6, "preço vermelho sem saldo")
	_send("interact")
	_check(saves.get("Essence") == 5, "sem saldo não compra")
	saves.call("AddEssence", 200)
	_send("ui_accept")
	_check(saves.call("GetUpgradeLevel", DAMAGE) == 2, "A compra")
	_send("ui_accept")
	_check("MÁX" in _card_text(shop, 0), "carta mostra MÁX no limite")
	_send("map")
	_check(not gm.call("IsWorldMapOpen"), "mapa não abre com a loja aberta")
	_send("pause")
	_check(not shop.get("IsOpen") and not gm.get("IsGamePaused") and not paused, "Esc fecha só a loja")
	cp.call("Interact", p)
	await process_frame
	_send("ui_cancel")
	_check(not shop.get("IsOpen") and not paused, "B fecha a loja")

func _test_totem() -> void:
	print("== Totem no começo do Igarapé")
	var saves := _saves()
	var dossel: Node = (load(DOSSEL) as PackedScene).instantiate()
	_check(dossel.get_node_or_null("Interactables/Checkpoint1") == null, "Dossel não tem mais totem")
	_check(dossel.get_node_or_null("Tutorial/TutCheckpoint") == null, "dica do totem saiu do Dossel")
	dossel.free()
	saves.call("StartNewGame", 0)
	await scene_changed
	change_scene_to_file(DOSSEL)
	await scene_changed
	await _frames(10)
	_player().call("RespawnAtLastCheckpoint")
	await scene_changed
	_check(current_scene.scene_file_path == SOPE, "morrer antes de ativar volta ao Sopé")
	change_scene_to_file(IGARAPE)
	await scene_changed
	await _frames(10)
	var cp := current_scene.get_node_or_null("Interactables/Checkpoint1") as Node2D
	_check(cp != null and cp.get("CheckpointId") == "cp_igarape_01", "Igarapé tem o totem")
	if cp == null: return
	_check(cp.global_position.x >= 240 and cp.global_position.x <= 420, "totem entre o portal e a lama (x=%d)" % cp.global_position.x)
	_check(current_scene.get_node_or_null("Tutorial/TutCheckpoint") != null, "dica do totem no Igarapé")
	var p := _player()
	p.get("Health").call("SetInvulnerable", true)
	p.global_position = cp.global_position + Vector2(0, -20)
	await _frames(10)
	_check(saves.get("LastCheckpointId") == "cp_igarape_01", "passar ativa o totem")
	change_scene_to_file(COVIL)
	await scene_changed
	await _frames(10)
	_player().call("RespawnAtLastCheckpoint")
	await scene_changed
	_check(current_scene.scene_file_path == IGARAPE, "morrer no Covil volta ao Igarapé")

func _test_effects() -> void:
	print("== Efeitos dos upgrades no Kairo")
	var saves := _saves()
	var probe: Node = load("res://tools/qa/QaProbe.cs").new()
	root.add_child(probe)
	saves.call("StartNewGame", 0)
	await scene_changed
	await _frames(10)
	for e in get_nodes_in_group("Enemies"): e.queue_free()
	var p := _player()
	saves.call("AddEssence", 999)
	var base_damage: int = p.get("CurrentAttackDamage")
	var base_speed: float = p.get("CurrentMoveSpeed")
	var base_cooldown: float = p.get("CurrentAttackCooldown")
	_check(base_damage == 25 and is_equal_approx(base_cooldown, 0.3), "sem upgrade, valores base (%d, %.2f)" % [base_damage, base_cooldown])

	saves.call("TryBuyUpgrade", DAMAGE)
	_check(p.get("CurrentAttackDamage") == base_damage + 5, "dano +5")
	saves.call("TryBuyUpgrade", ATTACK_SPEED)
	_check(is_equal_approx(p.get("CurrentAttackCooldown"), 0.27), "ataque 0,27 s")
	saves.call("TryBuyUpgrade", ATTACK_SPEED)
	saves.call("TryBuyUpgrade", ATTACK_SPEED)
	_check(is_equal_approx(p.get("CurrentAttackCooldown"), 0.21), "ataque para em 0,21 s")
	saves.call("TryBuyUpgrade", MOVE)
	_check(is_equal_approx(p.get("CurrentMoveSpeed"), base_speed * 1.05), "movimento +5%")

	var health = p.get("Health")
	probe.call("Damage", health, 30)
	var before := _hp(p)
	saves.call("TryBuyUpgrade", HEALTH)
	_check(_hp(p) == [before[0] + 15, before[1] + 15], "vida atual e máxima +15 (%s)" % [_hp(p)])

	saves.call("TryBuyUpgrade", DEFENSE)
	health.call("Reset")
	var full: int = health.get("CurrentHealth")
	probe.call("Damage", health, 20)
	_check(health.get("CurrentHealth") == full - 18, "defesa tira 2 de um golpe de 20")
	var now: int = health.get("CurrentHealth")
	probe.call("Damage", health, 1)
	_check(health.get("CurrentHealth") == now - 1, "golpe nunca cai abaixo de 1")

	print("== Defesa não reduz queda")
	health.call("SetInvulnerable", false)
	health.call("Reset")
	await _frames(5)
	var floor_y := p.global_position.y
	var hp0: int = health.get("CurrentHealth")
	p.global_position = Vector2(p.global_position.x, floor_y - 420)
	p.velocity = Vector2.ZERO
	await _frames(120)
	var fall: int = p.get("LastFallDamage")
	_check(fall > 0 and health.get("CurrentHealth") == hp0 - fall, "queda tira o valor integral (%d)" % fall)

	print("== Vida comprada sobrevive ao Continuar")
	health.call("Reset")
	var max_before: int = health.get("MaxHealth")
	saves.call("SaveCurrentGame", 0)
	_check(saves.call("ContinueGame", 0), "continua o jogo")
	await scene_changed
	await _frames(10)
	var p2 := _player()
	_check(p2.get("Health").get("MaxHealth") == max_before, "vida máxima igual depois do Continuar (%d)" % max_before)
	_check(p2.get("Health").get("FlatDefense") == 2, "defesa volta depois do Continuar")
	_check(p2.get("CurrentAttackDamage") == base_damage + 5, "dano volta depois do Continuar")
	probe.queue_free()

func _test_economy() -> void:
	print("== Economia: saldo, preço e limite de níveis")
	var saves := _saves()
	saves.call("StartNewGame", 0)
	await scene_changed
	_check(saves.get("Essence") == 0, "novo jogo começa sem essência")
	_check(saves.call("GetUpgradePrice", DAMAGE) == 20, "nível 1 custa 20")
	saves.call("AddEssence", 25)
	_check(saves.get("Essence") == 25, "soma essência")
	saves.call("AddEssence", -5)
	_check(saves.get("Essence") == 25, "valor negativo é ignorado")
	_check(saves.call("TryBuyUpgrade", DAMAGE), "compra nível 1")
	_check(saves.get("Essence") == 5 and saves.call("GetUpgradeLevel", DAMAGE) == 1, "cobra 20")
	_check(saves.call("GetUpgradePrice", DAMAGE) == 40, "nível 2 custa 40")
	_check(not saves.call("TryBuyUpgrade", DAMAGE), "sem saldo não compra")
	_check(saves.get("Essence") == 5 and saves.call("GetUpgradeLevel", DAMAGE) == 1, "falha não altera nada")
	saves.call("AddEssence", 200)
	saves.call("TryBuyUpgrade", DAMAGE)
	_check(saves.call("GetUpgradePrice", DAMAGE) == 70, "nível 3 custa 70")
	saves.call("TryBuyUpgrade", DAMAGE)
	_check(saves.call("GetUpgradeLevel", DAMAGE) == 3, "chega ao nível 3")
	_check(saves.call("GetUpgradePrice", DAMAGE) == -1, "sem preço no máximo")
	var left: int = saves.get("Essence")
	_check(not saves.call("TryBuyUpgrade", DAMAGE) and saves.get("Essence") == left, "limite de 3 níveis")
	_check(left == 95, "saldo final confere (%d)" % left)
	saves.call("StartNewGame", 0)
	await scene_changed
	_check(saves.get("Essence") == 0 and saves.call("GetUpgradeLevel", DAMAGE) == 0, "novo jogo zera saldo e níveis")

func _test_legacy_save() -> void:
	print("== Save antigo com o totem no Dossel")
	var saves := _saves()
	var legacy := {
		"Slot": 0,
		"CurrentScene": "res://scenes/areas/DosselVivo.tscn",
		"CurrentArea": "DosselVivo",
		"PlayerHealth": 100,
		"PlayerMaxHealth": 100,
		"LastCheckpointId": "cp_dossel_01",
		"LastCheckpointScene": "res://scenes/areas/DosselVivo.tscn",
		"ActivatedCheckpoints": ["cp_dossel_01"],
	}
	var file := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
	file.store_string(JSON.stringify(legacy))
	file.close()
	_check(saves.call("ContinueGame", 0), "save antigo carrega")
	await scene_changed
	_check(saves.get("LastCheckpointId") == "", "última bandeira antiga foi limpa")
	_check(saves.get("LastCheckpointScene") == "", "cena da bandeira antiga foi limpa")
	_check(not saves.call("IsCheckpointActivated", "cp_dossel_01"), "bandeira antiga não fica ativa")
	_check(saves.get("Essence") == 0 and saves.call("GetUpgradeLevel", DAMAGE) == 0, "save antigo começa sem essência")
	saves.call("AddEssence", 7)
	var parsed = JSON.parse_string(FileAccess.get_file_as_string(SAVE_FILE))
	_check(parsed != null and int(parsed.get("Essence", -1)) == 7, "essência é gravada no save em disco")

func _finish() -> void:
	if _backup != null:
		var file := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		file.store_string(_backup)
		file.close()
	elif FileAccess.file_exists(SAVE_FILE):
		DirAccess.remove_absolute(SAVE_FILE)
	print("ESSENCE_SHOP_RESULT: ", _passes, " ok, ", _failures, " falha(s)")
	quit(1 if _failures > 0 else 0)
