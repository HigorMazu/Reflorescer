extends SceneTree

const SAVE_PATH := "user://save_0.json"
var _original_save := PackedByteArray()
var _had_save := false
var _failures := 0


func _initialize() -> void:
	_had_save = FileAccess.file_exists(SAVE_PATH)
	if _had_save:
		_original_save = FileAccess.get_file_as_bytes(SAVE_PATH)
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	if condition:
		print("PASS: ", message)
	else:
		_failures += 1
		push_error("FAIL: " + message)


func _run() -> void:
	change_scene_to_file("res://scenes/areas/SopeDaMata.tscn")
	for index in 8:
		await process_frame
	var save_manager := root.get_node("SaveManager")
	var game_manager := root.get_node("GameManager")
	var map := current_scene.get_node("HUD/WorldMapOverlay")
	_check(save_manager.IsMapCellRevealed("SopeDaMata:0:1"), "ponto inicial revelado")
	_check(not save_manager.IsMapCellRevealed("CovilDeKorrag:0:1"), "Covil oculto antes da visita")
	if "--capture-full" in OS.get_cmdline_user_args():
		for area in map.AREAS:
			var definition: Dictionary = map.AREAS[area]
			for row in definition.rows:
				for col in definition.cols:
					save_manager.RevealMapCell("%s:%d:%d" % [area, col, row])
	_press_key(KEY_M)
	await process_frame
	_check(map.visible and paused, "M abre o mapa e pausa a partida")
	if ("--capture" in OS.get_cmdline_user_args() or "--capture-full" in OS.get_cmdline_user_args()) and DisplayServer.get_name() != "headless":
		for index in 3:
			await process_frame
		var image := root.get_texture().get_image()
		if image != null:
			image.save_png("res://docs/art-review/world-map-%s-v1.png" % ("complete" if "--capture-full" in OS.get_cmdline_user_args() else "fog"))
	_press_key(KEY_M)
	await process_frame
	_check(not map.visible and not paused, "M fecha o mapa e retoma a partida")
	_press_key(KEY_M)
	await process_frame
	_press_key(KEY_ESCAPE)
	await process_frame
	_check(not map.visible and not paused, "Esc fecha o mapa sem abrir pausa")

	# Decisão de 19/10: revelar trecho grava só o mapa, sem mudar o ponto do Continuar.
	if FileAccess.file_exists(SAVE_PATH):
		var before := _read_save()
		var walker := current_scene.get_node("Player") as Node2D
		walker.global_position = Vector2(1500, walker.global_position.y)
		for index in 4:
			await process_frame
		var after := _read_save()
		_check(save_manager.IsMapCellRevealed("SopeDaMata:3:1") and "SopeDaMata:3:1" in after.get("ExploredMapCells", []), "trecho novo entra no save")
		_check(str(after.get("PlayerPosition")) == str(before.get("PlayerPosition")), "revelar trecho não muda a posição do Continuar")

	change_scene_to_file("res://scenes/areas/CovilDeKorrag.tscn")
	for index in 8:
		await process_frame
	map = current_scene.get_node("HUD/WorldMapOverlay")
	_check(save_manager.IsMapCellRevealed("CovilDeKorrag:0:1"), "Covil revelado ao entrar")
	var portal := current_scene.get_node("Transitions/ToIgarapeSufocado")
	_check(portal.IsLocked, "portal do Covil fechado com o Korrag vivo")
	var guest := current_scene.get_node("Player") as Node2D
	guest.global_position = portal.global_position
	for index in 10:
		await physics_frame
	_check(current_scene.name == "CovilDeKorrag", "portal fechado não troca de cena")
	_check(portal.TargetScene == "res://scenes/areas/IgarapeSufocado.tscn", "Covil retorna ao Igarapé")
	_check(portal.TargetArea == "SpawnFromCovilDeKorrag", "retorno usa entrada própria")
	root.get_node("SceneManager").LoadSceneAtSpawn(portal.TargetScene, portal.TargetArea, Vector2.ZERO)
	for index in 8:
		await process_frame
	_check(current_scene.name == "IgarapeSufocado", "transição de retorno carrega o Igarapé")
	var player := current_scene.get_node("Player") as Node2D
	_check(absf(player.global_position.x - 2660.0) < 1.0, "retorno posiciona Kairo fora do portal")
	var file := FileAccess.open(SAVE_PATH, FileAccess.READ)
	var json := JSON.parse_string(file.get_as_text()) as Dictionary
	file.close()
	_check(json.has("ExploredMapCells") and "CovilDeKorrag:0:1" in json["ExploredMapCells"], "descoberta persistida no save")
	game_manager.RestartGame()
	for index in 8:
		await process_frame
	_check(not save_manager.IsMapCellRevealed("CovilDeKorrag:0:1"), "novo jogo apaga a exploração anterior")
	_finish()


func _finish() -> void:
	paused = false
	if _had_save:
		var file := FileAccess.open(SAVE_PATH, FileAccess.WRITE)
		file.store_buffer(_original_save)
		file.close()
	else:
		DirAccess.remove_absolute(ProjectSettings.globalize_path(SAVE_PATH))
	print("WorldMapCheck: ", _failures, " falhas")
	quit(0 if _failures == 0 else 1)


func _read_save() -> Dictionary:
	var file := FileAccess.open(SAVE_PATH, FileAccess.READ)
	var data := JSON.parse_string(file.get_as_text()) as Dictionary
	file.close()
	return data


func _press_key(key: Key) -> void:
	var event := InputEventKey.new()
	event.physical_keycode = key
	event.pressed = true
	root.push_input(event, true)
