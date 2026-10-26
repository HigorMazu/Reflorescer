extends SceneTree
## Playtest de 26/10: tela de abertura (história + controles) no Novo jogo, faixas de dica no cenário
## e dano de queda proporcional à altura. O save_0.json é guardado e devolvido.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/TutorialFallCheck.gd
const MENU := "res://scenes/ui/MainMenu.tscn"
const SAVE_FILE := "user://save_0.json"

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

func _overlay() -> Node:
	return get_first_node_in_group("TutorialOverlay")

func _run() -> void:
	if FileAccess.file_exists(SAVE_FILE):
		_backup = FileAccess.get_file_as_string(SAVE_FILE)
	var saves := root.get_node("SaveManager")

	# --- Tela de abertura ---
	saves.set("IntroEnabled", true)
	root.get_node("SceneManager").call("LoadScene", MENU)
	await scene_changed
	await _frames(10)
	current_scene.get_node("Box/NewGameButton").emit_signal("pressed")
	await scene_changed
	await _frames(15)
	var overlay := _overlay()
	_check(overlay != null and overlay.get("IsIntroOpen"), "Novo jogo abre a tela de abertura")
	_check(paused, "o jogo espera o jogador ler")
	var intro := overlay.get_node("Intro")
	var text := ""
	for l in intro.find_children("*", "Label", true, false):
		text += (l as Label).text + "\n"
	_check("última semente" in text and "OBJETIVO" in text, "abertura conta o propósito (semente + objetivo)")
	for key in ["A / D", "Espaço", "K", "Q", "E", "M", "Esc"]:
		_check(key in text, "abertura mostra a tecla %s" % key)
	var start := intro.get_node("Center/Card/Margin/Box/StartButton") as Button
	_check(root.gui_get_focus_owner() == start, "botão Começar já vem com foco (Enter)")
	start.emit_signal("pressed")
	await _frames(3)
	_check(not overlay.get("IsIntroOpen") and not paused, "Começar fecha a abertura e solta o jogo")
	saves.set("IntroEnabled", false)

	# --- Faixas de dica ---
	var p := _player()
	p.get("Health").call("SetInvulnerable", true)
	for e in get_nodes_in_group("Enemies"): e.queue_free()
	var zone := current_scene.get_node("Tutorial/TutAttack") as Node2D
	p.global_position = Vector2(zone.global_position.x, 440)
	await _frames(10)
	_check(overlay.get("IsBannerVisible") and "atacar" in str(overlay.get("BannerText")), "faixa de dica do ataque aparece no trecho certo")
	p.global_position = Vector2(current_scene.get_node("Tutorial/TutRestore").global_position.x, 440)
	await _frames(10)
	_check("restaurar" in str(overlay.get("BannerText")), "faixa troca pra dica de restaurar")
	p.global_position = Vector2(1800, 440)
	await _frames(150)
	_check(not overlay.get("IsBannerVisible"), "faixa some depois de sair do trecho")
	var count := current_scene.get_node("Tutorial").get_child_count()
	_check(count >= 5, "Sopé tem %d dicas (andar/pular, atacar, restaurar, espada, mapa)" % count)

	# --- Dano de queda ---
	p.get("Health").call("SetInvulnerable", false)
	p.get("Health").call("Reset")
	await _frames(5)
	var floor_y := p.global_position.y
	var hp0: int = p.get("Health").get("CurrentHealth")
	p.global_position = Vector2(1800, floor_y - 150)
	p.velocity = Vector2.ZERO
	await _frames(90)
	_check(p.get("Health").get("CurrentHealth") == hp0, "queda curta (150 px) não machuca")
	p.global_position = Vector2(1800, floor_y - 420)
	p.velocity = Vector2.ZERO
	await _frames(120)
	var hp1: int = p.get("Health").get("CurrentHealth")
	_check(hp1 < hp0, "queda alta (420 px) machuca (%d → %d, %d px)" % [hp0, hp1, p.get("LastFallHeight")])
	var dmg_mid: int = p.get("LastFallDamage")
	p.get("Health").call("Reset")
	p.global_position = Vector2(1800, floor_y - 900)
	p.velocity = Vector2.ZERO
	await _frames(180)
	var dmg_high: int = p.get("LastFallDamage")
	_check(dmg_high > dmg_mid and dmg_high <= 40, "queda maior machuca mais, até 40 (%d → %d)" % [dmg_mid, dmg_high])

	paused = false
	if _backup != null:
		var f := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		f.store_string(_backup)
		f.close()
	print("TUTORIAL_FALL_RESULT: %d ok, %d falha(s)" % [_passes, _failures])
	quit(1 if _failures > 0 else 0)
