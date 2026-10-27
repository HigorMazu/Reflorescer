extends SceneTree
## Controle (26/10/2026): cada ação tem botão no layout padrão, o Kairo anda/pula/ataca pelo controle,
## Start pausa e o menu de pausa navega com A, View abre o mapa, e as dicas da tela trocam pros nomes
## do controle (Xbox/PlayStation) e voltam pro teclado. O save_0.json é guardado e devolvido.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/GamepadCheck.gd
const SOPE := "res://scenes/areas/SopeDaMata.tscn"
const MENU := "res://scenes/ui/MainMenu.tscn"
const SAVE_FILE := "user://save_0.json"
const ACTIONS := ["move_left", "move_right", "jump", "attack", "dash", "interact", "toggle_sword", "special", "pause", "map"]

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

func _button(index: int, pressed: bool) -> void:
	var e := InputEventJoypadButton.new()
	e.device = 0
	e.button_index = index
	e.pressed = pressed
	Input.parse_input_event(e)

func _tap(index: int) -> void:
	_button(index, true)
	await _frames(2)
	_button(index, false)
	await _frames(2)

func _axis(axis: int, value: float) -> void:
	var e := InputEventJoypadMotion.new()
	e.device = 0
	e.axis = axis
	e.axis_value = value
	Input.parse_input_event(e)

func _run() -> void:
	if FileAccess.file_exists(SAVE_FILE):
		_backup = FileAccess.get_file_as_string(SAVE_FILE)
	var gm := root.get_node("GameManager")

	for a in ACTIONS:
		var has_pad := false
		for e in InputMap.action_get_events(a):
			if e is InputEventJoypadButton or e is InputEventJoypadMotion: has_pad = true
		_check(has_pad, "ação %s tem botão no controle" % a)

	change_scene_to_file(SOPE)
	await scene_changed
	await _frames(20)
	for e in get_nodes_in_group("Enemies"): e.queue_free()
	var p := get_first_node_in_group("Player") as CharacterBody2D
	p.get("Health").call("SetInvulnerable", true)
	await _frames(5)

	# Andar com o analógico e com o D-pad
	var x0 := p.global_position.x
	_axis(0, 1.0)
	await _frames(30)
	_axis(0, 0.0)
	_check(p.global_position.x > x0 + 40, "analógico pra direita anda (%d → %d)" % [x0, p.global_position.x])
	x0 = p.global_position.x
	_button(13, true)
	await _frames(30)
	_button(13, false)
	_check(p.global_position.x < x0 - 40, "D-pad pra esquerda anda (%d → %d)" % [x0, p.global_position.x])
	await _frames(10)

	# Pular com A, atacar com X
	_button(0, true)
	await _frames(6)
	_check(not p.is_on_floor() and p.velocity.y < 0, "A pula")
	_button(0, false)
	await _frames(60)
	_button(2, true)
	await _frames(2)
	_check(Input.is_action_pressed("attack"), "X ataca")
	_button(2, false)
	await _frames(20)

	# Dicas trocam pro controle
	_check(gm.call("GetInputHint", "jump") == "A" and gm.call("GetInputHint", "attack") == "X", "dicas mostram os botões do controle (A pula, X ataca)")
	var hud := get_first_node_in_group("HUD")
	_check((hud.get_node("MarginContainer/VBoxContainer/TopRow/MapHint") as Label).text.begins_with("View"), "dica do mapa vira 'View'")
	var sword_keys := []
	for l in hud.find_children("*", "Label", true, false):
		sword_keys.append((l as Label).text)
	_check("LB" in sword_keys, "indicador da espada mostra LB")
	var zone := current_scene.get_node("Tutorial/TutAttack") as Node2D
	p.global_position = Vector2(zone.global_position.x, 440)
	await _frames(10)
	var overlay := get_first_node_in_group("TutorialOverlay")
	_check(overlay.get("BannerKeys") == "X", "faixa de dica do ataque mostra X")
	gm.call("SetInputHintsForTest", true, 1)
	await _frames(2)
	_check(overlay.get("BannerKeys") == "Quadrado" and gm.call("GetInputHint", "jump") == "Cruz", "controle de PlayStation usa os nomes dele (Quadrado, Cruz)")
	var key := InputEventKey.new()
	key.physical_keycode = KEY_K
	key.pressed = true
	Input.parse_input_event(key)
	await _frames(3)
	key.pressed = false
	Input.parse_input_event(key)
	await _frames(3)
	_check(overlay.get("BannerKeys") == "K" and gm.call("GetInputHint", "jump") == "Espaço", "voltar pro teclado volta as dicas (K, Espaço)")

	# Start pausa, menu de pausa navega com A
	await _tap(6)
	_check(paused and gm.get("IsGamePaused"), "Start pausa")
	var resume := hud.get_node("PauseMenu/VBoxContainer/ResumeButton") as Button
	_check(root.gui_get_focus_owner() == resume, "menu de pausa abre com foco no Continuar")
	await _tap(0)
	_check(not paused and not gm.get("IsGamePaused"), "A no Continuar despausa")

	# View abre e fecha o mapa
	await _tap(4)
	await _frames(1)
	_check(gm.get("IsMapOpen"), "View abre o mapa")
	await _tap(4)
	_check(not gm.get("IsMapOpen") and not paused, "View fecha o mapa")

	# Menu inicial com D-pad
	change_scene_to_file(MENU)
	await scene_changed
	await _frames(10)
	_axis(0, 0.0)
	await _tap(0)  # qualquer botão: entra no modo controle
	var focus_before := root.gui_get_focus_owner()
	await _tap(12)
	var focus_after := root.gui_get_focus_owner()
	_check(focus_after != null and focus_after != focus_before, "D-pad pra baixo troca a opção do menu")
	_check((current_scene.get_node("ControlsHint") as Label).text.begins_with("D-pad"), "dica do menu mostra D-pad e A")

	if _backup != null:
		var f := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		f.store_string(_backup)
		f.close()
	print("GAMEPAD_RESULT: %d ok, %d falha(s)" % [_passes, _failures])
	quit(1 if _failures > 0 else 0)
