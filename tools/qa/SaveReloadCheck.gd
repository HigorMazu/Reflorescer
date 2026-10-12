extends SceneTree
## Regressão de save/recarga no mesmo processo (investigação de 11/10/2026).
## Percorre: Menu -> Novo jogo -> Pausa/Salvar/Menu inicial/Continuar (x3) e, nas 4 áreas,
## salvar -> menu -> Continuar (x2), forçando o GC do .NET a cada troca de cena.
## O save_0.json do usuário é guardado no início e devolvido no fim.
## Rodar: <godot_console.exe> --path . --script res://tools/qa/SaveReloadCheck.gd
const MENU := "res://scenes/ui/MainMenu.tscn"
const AREAS := ["res://scenes/areas/SopeDaMata.tscn", "res://scenes/areas/DosselVivo.tscn", "res://scenes/areas/IgarapeSufocado.tscn", "res://scenes/areas/CovilDeKorrag.tscn"]
const SAVE_FILE := "user://save_0.json"

var _gc: Node
var _backup = null
var _failures := 0

func _initialize() -> void:
	_run.call_deferred()

func _wait(s: float) -> void:
	await create_timer(s).timeout

func _check(ok: bool, what: String) -> void:
	if not ok:
		_failures += 1
		push_error("SAVE_RELOAD_FAIL: " + what)

func _settle() -> void:
	await _wait(0.5)
	_gc.call("Collect")
	await process_frame

func _run() -> void:
	if FileAccess.file_exists(SAVE_FILE):
		_backup = FileAccess.get_file_as_string(SAVE_FILE)
	_gc = load("res://tools/qa/GcProbe.cs").new()
	root.add_child(_gc)
	var saves := root.get_node("SaveManager")
	var manager := root.get_node("SceneManager")

	manager.call("LoadScene", MENU)
	await scene_changed
	await _wait(0.5)
	current_scene.get_node("Box/NewGameButton").emit_signal("pressed")
	await scene_changed
	await _settle()
	for i in range(3):
		root.get_node("GameManager").call("TogglePause")
		var hud := get_first_node_in_group("HUD")
		hud.get_node("PauseMenu/VBoxContainer/SaveButton").emit_signal("pressed")
		hud.get_node("PauseMenu/VBoxContainer/MainMenuButton").emit_signal("pressed")
		await scene_changed
		await _settle()
		_check(current_scene.get_node("Box/ContinueButton").visible, "Continuar visível após salvar (ciclo %d)" % i)
		current_scene.get_node("Box/ContinueButton").emit_signal("pressed")
		await scene_changed
		await _settle()
		_check(current_scene.scene_file_path == AREAS[0], "Continuar volta ao Sopé (ciclo %d)" % i)
		_check(not paused, "jogo despausado após Continuar (ciclo %d)" % i)

	for rep in range(2):
		for area in AREAS:
			manager.call("LoadScene", area)
			await scene_changed
			await _settle()
			saves.call("SaveGame", 0)
			manager.call("LoadScene", MENU)
			await scene_changed
			await _settle()
			saves.call("ContinueGame", 0)
			await scene_changed
			await _settle()
			_check(current_scene.scene_file_path == area, "Continuar volta a " + area)

	if _backup != null:
		var f := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		f.store_string(_backup)
		f.close()
	else:
		DirAccess.remove_absolute(SAVE_FILE)
	print("SAVE_RELOAD_RESULT: %d falha(s)" % _failures)
	quit(1 if _failures > 0 else 0)
