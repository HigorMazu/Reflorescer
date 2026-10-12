extends SceneTree
## C14-T3: tela de fim da demo. Pra 0, 1 e 3 áreas restauradas (restaurando de verdade, com E):
## derrota o Korrag e confere "Áreas restauradas: X/3", a mensagem, a pausa e os botões.
## Com renderização (sem --headless), salva a captura da tela com 3/3 em user://demo_end_3de3.png.
## O save_0.json é guardado e devolvido.
## Rodar: <godot_console.exe> --fixed-fps 60 --path . --script res://tools/qa/DemoEndCheck.gd
const MENU := "res://scenes/ui/MainMenu.tscn"
const AREAS := ["res://scenes/areas/SopeDaMata.tscn", "res://scenes/areas/DosselVivo.tscn", "res://scenes/areas/IgarapeSufocado.tscn"]
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

func _new_game() -> void:
	paused = false
	root.get_node("SceneManager").call("LoadScene", MENU)
	await scene_changed
	await _frames(10)
	current_scene.get_node("Box/NewGameButton").emit_signal("pressed")
	await scene_changed
	await _frames(20)

func _go(scene: String) -> void:
	change_scene_to_file(scene)
	await scene_changed
	await _frames(10)
	for e in get_nodes_in_group("Enemies"): e.queue_free()
	get_first_node_in_group("Player").get("Health").call("SetInvulnerable", true)

func _restore_here() -> void:
	var p := get_first_node_in_group("Player") as Node2D
	var rp := get_nodes_in_group("RestorationPoints")[0] as Node2D
	p.global_position = rp.global_position + Vector2(0, -4)
	await _frames(20)
	Input.action_press("interact")
	await physics_frame
	Input.action_release("interact")
	await _frames(20)

func _defeat_korrag_and_wait() -> Control:
	await _go(COVIL)
	var boss := current_scene.get_node("Boss_Javali")
	_probe.call("Damage", boss.get("Health"), 100000)
	for i in 330:  # 5,5 s: fade do Korrag + aviso do Dash + atraso da tela
		await process_frame
	return get_first_node_in_group("HUD").get_node_or_null("DemoEnd") as Control

func _run() -> void:
	if FileAccess.file_exists(SAVE_FILE):
		_backup = FileAccess.get_file_as_string(SAVE_FILE)
	_probe = load("res://tools/qa/QaProbe.cs").new()
	root.add_child(_probe)

	for n in [0, 1, 3]:
		print("== %d área(s) restaurada(s)" % n)
		await _new_game()
		for i in n:
			await _go(AREAS[i])
			await _restore_here()
		var panel := await _defeat_korrag_and_wait()
		_check(panel != null and panel.visible, "tela de fim aparece depois do Korrag")
		if panel == null: continue
		var count := (panel.get_node("Box/Count") as Label).text
		var msg := (panel.get_node("Box/Message") as Label).text
		_check(count == "Áreas restauradas: %d/3" % n, "contagem '%s'" % count)
		print("     mensagem: ", msg)
		_check(paused, "jogo pausado com a tela aberta")
		if n == 3:
			_check("toda a mata" in msg, "mensagem especial com 3/3")
			if DisplayServer.get_name() != "headless":
				await RenderingServer.frame_post_draw
				root.get_viewport().get_texture().get_image().save_png("user://demo_end_3de3.png")
				print("     captura: ", ProjectSettings.globalize_path("user://demo_end_3de3.png"))
		else:
			_check(not ("toda a mata" in msg), "mensagem comum (não a de 3/3)")
		if n == 1:
			panel.get_node("Box/ExploreButton").emit_signal("pressed")
			await _frames(5)
			_check(not paused and not panel.visible, "'Continuar explorando' fecha a tela e despausa")
			_check(root.get_node("AbilityManager").call("HasAbility", 1), "Dash liberado pra explorar")
		elif n == 0:
			panel.get_node("Box/MenuButton").emit_signal("pressed")
			await scene_changed
			await _frames(5)
			_check(current_scene.scene_file_path == MENU and not paused, "'Menu inicial' volta ao menu despausado")

	paused = false
	if _backup != null:
		var f := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		f.store_string(_backup)
		f.close()
	print("DEMO_END_RESULT: %d ok, %d falha(s)" % [_passes, _failures])
	quit(1 if _failures > 0 else 0)
