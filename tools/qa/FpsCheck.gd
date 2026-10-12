extends SceneTree
## A05: mede FPS real (renderer gl_compatibility, sem --fixed-fps e sem --headless) nas 4 áreas.
## O Kairo atravessa cada área correndo da esquerda pra direita (portais desligados, inimigos ativos,
## Kairo invulnerável). Descarta os primeiros 60 quadros (aquecimento de shaders).
## Rodar: <godot_console.exe> --path . --script res://tools/qa/FpsCheck.gd
const AREAS := ["res://scenes/areas/SopeDaMata.tscn", "res://scenes/areas/DosselVivo.tscn", "res://scenes/areas/IgarapeSufocado.tscn", "res://scenes/areas/CovilDeKorrag.tscn"]

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	print("FPS GPU: ", RenderingServer.get_video_adapter_name(), " | vsync=", DisplayServer.window_get_vsync_mode())
	for area in AREAS:
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
		for f in range(60 + 600):
			await process_frame
			var now := Time.get_ticks_usec()
			if f >= 60: times.append((now - last) / 1000.0)
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
	quit()

func _tap_jump() -> void:
	Input.action_press("jump")
	await physics_frame
	Input.action_release("jump")
