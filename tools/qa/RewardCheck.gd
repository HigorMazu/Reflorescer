extends SceneTree
## C14: recompensa de restaurar (+10 de vida máxima e vida cheia) e cura na bandeira.
## Novo jogo -> dano -> restaurar no Sopé -> Dossel (vida máxima mantida) -> dano -> bandeira cura ->
## restaurar no Dossel -> Pausa/Salvar/Menu/Continuar (vida máxima mantida).
## O save_0.json é guardado e devolvido.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/RewardCheck.gd
const MENU := "res://scenes/ui/MainMenu.tscn"
const SAVE_FILE := "user://save_0.json"

var _probe: Node
var _failures := 0
var _passes := 0
var _backup = null

func _initialize() -> void:
	_run.call_deferred()

func _check(ok: bool, what: String) -> void:
	if ok: _passes += 1
	else: _failures += 1
	print("  ", "ok    " if ok else "FALHA ", what)

func _frames(n: int) -> void:
	for i in n:
		await physics_frame

func _tap(action: String) -> void:
	Input.action_press(action)
	await physics_frame
	Input.action_release(action)

func _player() -> CharacterBody2D:
	return get_first_node_in_group("Player")

func _hp() -> Array:
	var h = _player().get("Health")
	return [h.get("CurrentHealth"), h.get("MaxHealth")]

func _notice() -> String:
	var n := get_first_node_in_group("HUD").get_node_or_null("AbilityUnlockNotification") as Label
	return n.text if n and n.visible else ""

func _hurt(amount: int) -> void:
	_probe.call("Damage", _player().get("Health"), amount)
	await _frames(80)  # passa a invulnerabilidade

func _restore() -> void:
	var rp := get_nodes_in_group("RestorationPoints")[0] as Node2D
	_player().global_position = rp.global_position + Vector2(0, -4)
	await _frames(20)
	await _tap("interact")
	await _frames(10)

func _portal(path: String) -> void:
	_player().global_position = (current_scene.get_node(path) as Node2D).global_position
	await scene_changed
	await _frames(30)

func _run() -> void:
	if FileAccess.file_exists(SAVE_FILE):
		_backup = FileAccess.get_file_as_string(SAVE_FILE)
	_probe = load("res://tools/qa/QaProbe.cs").new()
	root.add_child(_probe)
	root.get_node("SceneManager").call("LoadScene", MENU)
	await scene_changed
	await _frames(10)
	current_scene.get_node("Box/NewGameButton").emit_signal("pressed")
	await scene_changed
	await _frames(30)
	for e in get_nodes_in_group("Enemies"): e.queue_free()

	print("== Sopé: restaurar cura e dá +10 de vida máxima")
	_check(_hp() == [100, 100], "Novo jogo começa com 100/100 (%s)" % [_hp()])
	await _hurt(40)
	_check(_hp() == [60, 100], "depois de 40 de dano: 60/100 (%s)" % [_hp()])
	await _restore()
	_check(_hp() == [110, 110], "restaurar: vida cheia e máxima 110 (%s)" % [_hp()])
	_check("Vida máxima +10" in _notice(), "HUD avisa a recompensa ('%s')" % _notice())
	await _hurt(30)

	print("== Dossel: a vida máxima vem junto; a bandeira cura")
	await _portal("Transitions/ToDosselVivo")
	for e in get_nodes_in_group("Enemies"): e.queue_free()
	_check(_hp() == [80, 110], "chega no Dossel com 80/110, sem cura na troca de área (%s)" % [_hp()])
	var flag := current_scene.get_node("Interactables/Checkpoint1") as Node2D
	_player().global_position = flag.global_position + Vector2(0, -4)
	await _frames(10)
	_check(_hp() == [110, 110], "passar na bandeira enche a vida (%s)" % [_hp()])
	_check(_notice() == "Vida restaurada", "HUD avisa 'Vida restaurada' ('%s')" % _notice())
	_player().global_position = flag.global_position + Vector2(200, -60)
	await _frames(60)
	_player().global_position = flag.global_position + Vector2(0, -4)
	await _frames(10)
	_check(_hp() == [110, 110], "passar de novo com a vida cheia não quebra nada (%s)" % [_hp()])
	await _restore()
	_check(_hp() == [120, 120], "segunda restauração: máxima 120 (%s)" % [_hp()])
	await _hurt(20)

	print("== Salvar, sair pro menu e Continuar")
	root.get_node("GameManager").call("TogglePause")
	var hud := get_first_node_in_group("HUD")
	hud.get_node("PauseMenu/VBoxContainer/SaveButton").emit_signal("pressed")
	hud.get_node("PauseMenu/VBoxContainer/MainMenuButton").emit_signal("pressed")
	await scene_changed
	await _frames(20)
	current_scene.get_node("Box/ContinueButton").emit_signal("pressed")
	await scene_changed
	await _frames(30)
	_check(_hp()[1] == 120, "Continuar mantém a vida máxima 120 (%s)" % [_hp()])
	_check(_hp()[0] == 100, "Continuar mantém a vida salva, 100 (%s)" % [_hp()])

	if _backup != null:
		var f := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		f.store_string(_backup)
		f.close()
	print("REWARD_RESULT: %d ok, %d falha(s)" % [_passes, _failures])
	quit(1 if _failures > 0 else 0)
