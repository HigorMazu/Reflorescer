extends SceneTree
## C17-T2 / C15-T1: ligação Covil → Deserto depois do Korrag (decisão de 19/10/2026).
## Novo jogo → Korrag derrotado de verdade → tela de fim com a dica do Deserto → "Continuar explorando"
## → portal do leste abre → Deserto jogável (spawn, chão, inimigos, bandeira, mapa, vento) → volta ao Covil.
## O save_0.json é guardado e devolvido.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/DesertLinkCheck.gd
const MENU := "res://scenes/ui/MainMenu.tscn"
const COVIL := "res://scenes/areas/CovilDeKorrag.tscn"
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

func _player() -> CharacterBody2D:
	return get_first_node_in_group("Player") as CharacterBody2D

func _run() -> void:
	if FileAccess.file_exists(SAVE_FILE):
		_backup = FileAccess.get_file_as_string(SAVE_FILE)
	_probe = load("res://tools/qa/QaProbe.cs").new()
	root.add_child(_probe)

	# Novo jogo e Covil com o Korrag vivo
	root.get_node("SceneManager").call("LoadScene", MENU)
	await scene_changed
	await _frames(10)
	current_scene.get_node("Box/NewGameButton").emit_signal("pressed")
	await scene_changed
	await _frames(10)
	change_scene_to_file(COVIL)
	await scene_changed
	await _frames(10)
	_player().get("Health").call("SetInvulnerable", true)
	var portal := current_scene.get_node("Transitions/ToDeserto")
	_check(portal.get("IsLocked"), "portal do Deserto fechado com o Korrag vivo")

	# Korrag cai → tela de fim → Continuar explorando
	_probe.call("Damage", current_scene.get_node("Boss_Javali").get("Health"), 100000)
	for i in 330:
		await process_frame
	var panel := get_first_node_in_group("HUD").get_node_or_null("DemoEnd") as Control
	_check(panel != null and panel.visible, "tela de fim aparece depois do Korrag")
	var hint := panel.get_node_or_null("Center/Card/Margin/Box/DesertHint") as Label if panel else null
	_check(hint != null and "Deserto" in hint.text, "tela de fim avisa do caminho pro Deserto")
	panel.get_node("Center/Card/Margin/Box/ExploreButton").emit_signal("pressed")
	await _frames(5)
	_check(not paused, "'Continuar explorando' devolve o controle")
	_check(not portal.get("IsLocked"), "portal do Deserto abre depois do Korrag")
	_check(is_equal_approx((portal.get_node("PortalVisual") as CanvasItem).modulate.a, 1.0), "portal aberto aparece aceso")

	# Entra no Deserto
	_player().global_position = portal.global_position
	await scene_changed
	await _frames(30)
	_check(current_scene.name == "Deserto", "portal leva ao Deserto")
	var p := _player()
	p.get("Health").call("SetInvulnerable", true)
	_check(absf(p.global_position.x - 140.0) < 40.0, "Kairo nasce na entrada do Deserto (x=%d)" % p.global_position.x)
	_check(p.is_on_floor(), "Kairo pisa no chão do Deserto")
	var music := root.get_node("AudioManager/MusicPlayer") as AudioStreamPlayer
	await create_timer(1.2).timeout
	_check(music.playing and music.stream != null and music.stream.resource_path.ends_with("desert_wind.wav"), "vento do Deserto toca no lugar da música do Covil")
	var enemies := current_scene.get_node("Enemies").get_children()
	_check(enemies.size() == 3, "três inimigos do Deserto na área")
	for e in enemies:
		_check(e.get("StatsResource") != null and e.get("StatsResource").resource_path.ends_with("_Deserto.tres"), "%s usa os stats do Deserto" % e.name)
	_check(current_scene.get_node_or_null("Dressing/DesertSkyCycle") != null or current_scene.get_node("Dressing").get_child_count() > 2, "céu e chão do Deserto montados")

	# Bandeira do Deserto e mapa
	var cp := current_scene.get_node("Interactables/CheckpointDeserto")
	p.global_position = cp.global_position + Vector2(0, -20)
	await _frames(15)
	_check(cp.get("Activated"), "bandeira do Deserto ativa ao passar")
	_check(root.get_node("SaveManager").get("LastCheckpointId") == "cp_deserto_01", "morrer agora volta pra bandeira do Deserto")
	await _frames(5)
	_check(root.get_node("SaveManager").call("IsMapCellRevealed", "Deserto:0:0"), "mapa revela a entrada do Deserto")
	var ev := InputEventKey.new()
	ev.physical_keycode = KEY_M
	ev.pressed = true
	root.push_input(ev, true)
	await process_frame
	await process_frame  # o overlay acende no _process seguinte
	_check(paused and current_scene.get_node("HUD/WorldMapOverlay").visible, "M abre o mapa no Deserto")
	root.push_input(ev, true)
	await process_frame
	_check(not paused, "M fecha o mapa no Deserto")

	# A arena da cobra fica fechada até a luta existir (C15-T2)
	p.global_position = Vector2(3100, 440)
	p.velocity = Vector2.ZERO
	Input.action_press("move_right")
	await _frames(60)
	Input.action_release("move_right")
	_check(p.global_position.x < 3200.0, "barreira segura Kairo antes da cobra (x=%d)" % p.global_position.x)

	# Volta ao Covil
	p.global_position = current_scene.get_node("Transitions/ToCovilDeKorrag").global_position
	await scene_changed
	await _frames(20)
	_check(current_scene.name == "CovilDeKorrag", "portal de volta leva ao Covil")
	_check(absf(_player().global_position.x - 480.0) < 40.0, "Kairo volta ao lado do portal do Deserto")
	_check(current_scene.get_node_or_null("Boss_Javali") == null or not current_scene.get_node("Boss_Javali").visible or current_scene.get_node("Boss_Javali").is_queued_for_deletion(), "Korrag continua derrotado ao voltar")

	paused = false
	if _backup != null:
		var f := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		f.store_string(_backup)
		f.close()
	print("DESERT_LINK_RESULT: %d ok, %d falha(s)" % [_passes, _failures])
	quit(1 if _failures > 0 else 0)
