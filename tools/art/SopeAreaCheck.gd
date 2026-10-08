extends SceneTree
## Exercises the real scene manager and area entrances without touching save slots.

const SOPE := "res://scenes/areas/SopeDaMata.tscn"
const DOSSEL := "res://scenes/areas/DosselVivo.tscn"
var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var manager := root.get_node("SceneManager")
	manager.call("LoadScene", SOPE)
	await scene_changed
	await process_frame
	var area := current_scene
	var player := area.get_node("Player") as Node2D
	var start := area.get_node("SpawnInicial") as Marker2D
	_check(player.global_position.distance_to(start.global_position) < 2.0, "new game enters at SpawnInicial")
	var camera := player.get_node("Camera2D") as Camera2D
	_check(camera.limit_left == 0 and camera.limit_top == -200 and camera.limit_right == 2400 and camera.limit_bottom == 520,
		"Sopé camera bounds are applied")
	_check(area.get_node("Interactables/RestorationPoint1") != null and area.get_node("Enemies").get_child_count() == 5,
		"Sopé has the restoration point and five enemies")

	manager.call("LoadScene", DOSSEL)
	await scene_changed
	await process_frame
	var return_trigger := current_scene.get_node("Transitions/ToSopeDaMata")
	manager.call("LoadSceneAtSpawn", return_trigger.get("TargetScene"),
		return_trigger.get("TargetArea"), return_trigger.get("SpawnOffset"))
	await scene_changed
	await process_frame
	await process_frame
	area = current_scene
	player = area.get_node("Player") as Node2D
	var return_spawn := area.get_node("SpawnFromDosselVivo") as Marker2D
	_check(player.global_position.distance_to(return_spawn.global_position) < 2.0,
		"return from Dossel enters at SpawnFromDosselVivo")

	if "--benchmark" in OS.get_cmdline_user_args():
		await _benchmark(area, player)
	print("SOPE_AREA_DONE failures=%d" % failures)
	quit(failures)

func _benchmark(area: Node2D, player: Node2D) -> void:
	await create_timer(1.5).timeout
	for x in [160.0, 1050.0, 2000.0, 160.0]:
		player.global_position = Vector2(x, 400.0)
		var camera := player.get_node("Camera2D") as Camera2D
		camera.reset_smoothing()
		await create_timer(0.5).timeout
		var samples: Array[float] = []
		for frame in range(180):
			await process_frame
			if frame >= 60:
				samples.append(Engine.get_frames_per_second())
		if samples.is_empty():
			continue
		var total := 0.0
		var minimum := INF
		for fps in samples:
			total += fps
			minimum = minf(minimum, fps)
		print("SOPE_FPS x=%d avg=%.1f min=%.1f renderer=%s" % [x, total / samples.size(), minimum,
			ProjectSettings.get_setting("rendering/renderer/rendering_method")])

func _check(condition: bool, description: String) -> void:
	if condition:
		print("SOPE_AREA_PASS: " + description)
	else:
		failures += 1
		push_error("SOPE_AREA_FAIL: " + description)
