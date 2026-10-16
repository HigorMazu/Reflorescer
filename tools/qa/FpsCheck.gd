extends SceneTree
## A05: mede FPS real (renderer gl_compatibility, sem --fixed-fps e sem --headless) nas 4 áreas.
## O Kairo atravessa cada área correndo da esquerda pra direita (portais desligados, inimigos ativos,
## Kairo invulnerável). Descarta os primeiros 60 quadros (aquecimento de shaders).
## Rodar: <godot_console.exe> --path . --script res://tools/qa/FpsCheck.gd
## Para conferir só isolamento do save, usar -- --warmup=5 --frames=30 (não é medição de performance).
const AREAS := ["res://scenes/areas/SopeDaMata.tscn", "res://scenes/areas/DosselVivo.tscn", "res://scenes/areas/IgarapeSufocado.tscn", "res://scenes/areas/CovilDeKorrag.tscn"]
const SAVE_FILE := "user://save_0.json"

var _save_backup := ""
var _had_save_backup := false

func _initialize() -> void:
	_run.call_deferred()

func _backup_user_save() -> void:
	_had_save_backup = FileAccess.file_exists(SAVE_FILE)
	if _had_save_backup:
		_save_backup = FileAccess.get_file_as_string(SAVE_FILE)

func _restore_user_save() -> void:
	if _had_save_backup:
		var file := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		file.store_string(_save_backup)
		file.close()
	elif FileAccess.file_exists(SAVE_FILE):
		DirAccess.remove_absolute(SAVE_FILE)

func _run() -> void:
	# Dossel contém uma bandeira automática. O benchmark não pode alterar o save do jogador.
	_backup_user_save()
	var warmup := 60
	var measured_frames := 600
	var requested_area := ""
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--warmup="):
			warmup = max(1, int(argument.get_slice("=", 1)))
		elif argument.begins_with("--frames="):
			measured_frames = max(1, int(argument.get_slice("=", 1)))
		elif argument.begins_with("--area="):
			requested_area = argument.get_slice("=", 1)
	var areas := AREAS
	if not requested_area.is_empty():
		areas = AREAS.filter(func(path): return path.get_file().get_basename() == requested_area)
		if areas.is_empty():
			push_error("Área desconhecida para FpsCheck: " + requested_area)
			_restore_user_save()
			quit(2)
			return
	print("FPS GPU: ", RenderingServer.get_video_adapter_name(), " | vsync=", DisplayServer.window_get_vsync_mode())
	for area in areas:
		change_scene_to_file(area)
		await scene_changed
		for t in current_scene.get_node("Transitions").get_children():
			(t as Area2D).set_deferred("monitoring", false)
		var p := get_first_node_in_group("Player") as CharacterBody2D
		p.get("Health").call("SetInvulnerable", true)
		var lim: Rect2i = current_scene.get("CameraLimits")
		Input.action_press("move_right")
		var times: Array[float] = []
		var last := Time.get_ticks_usec()
		for f in range(warmup + measured_frames):
			await process_frame
			var now := Time.get_ticks_usec()
			if f >= warmup: times.append((now - last) / 1000.0)
			last = now
			if p.is_on_wall() and p.is_on_floor(): await _tap_jump()
		Input.action_release("move_right")
		times.sort()
		var total := 0.0
		for t in times: total += t
		var avg_fps := 1000.0 / (total / times.size())
		var p1 := 1000.0 / times[int(times.size() * 0.99)]
		var worst := 1000.0 / times[-1]
		print("FPS %-16s média %.1f | 1%% piores %.1f | pior quadro %.1f | Kairo x=%d de %d" % [area.get_file().get_basename(), avg_fps, p1, worst, p.global_position.x, lim.end.x])
	_restore_user_save()
	quit()

func _tap_jump() -> void:
	Input.action_press("jump")
	await physics_frame
	Input.action_release("jump")
