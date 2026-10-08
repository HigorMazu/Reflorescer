extends SceneTree
## Exercises the actual Sopé→Dossel entrance and samples three vertical views.

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
	var exit_trigger := current_scene.get_node("Transitions/ToDosselVivo")
	manager.call("LoadSceneAtSpawn", exit_trigger.get("TargetScene"),
		exit_trigger.get("TargetArea"), exit_trigger.get("SpawnOffset"))
	await scene_changed
	await process_frame
	await process_frame
	var area := current_scene
	var player := area.get_node("Player") as Node2D
	var entry := area.get_node("SpawnFromSopeDaMata") as Marker2D
	_check(area.scene_file_path == DOSSEL and player.global_position.distance_to(entry.global_position) < 2.0,
		"Sopé exit enters Dossel at SpawnFromSopeDaMata")
	var camera := player.get_node("Camera2D") as Camera2D
	_check(camera.limit_left == 0 and camera.limit_top == -900 and camera.limit_right == 1600 and camera.limit_bottom == 540,
		"Dossel camera bounds are applied")
	var geometry := area.get_node("Geometry")
	var branch_count := 0
	for body in geometry.get_children():
		if body.name.begins_with("Branch") or body.name == "TopBranch":
			branch_count += 1
			_check(body.get_node_or_null("CollisionShape2D") != null,
				"%s retains its collision" % body.name)
	_check(branch_count == 12, "Dossel has eleven climbing branches and the top branch")
	_check(_route_fits_base_jump(geometry),
		"branch edge gaps fit the base jump's ballistic height and horizontal reach")
	_check(area.get_node("Interactables/RestorationPoint1") != null and area.get_node("Enemies").get_child_count() == 5,
		"Dossel has restoration and five enemies")
	if "--benchmark" in OS.get_cmdline_user_args():
		await _benchmark(player)
	print("DOSSEL_AREA_DONE failures=%d" % failures)
	quit(failures)

func _route_fits_base_jump(geometry: Node) -> bool:
	var stats := load("res://resources/player/PlayerStatsResource.tres") as Resource
	var jump_speed := -float(stats.get("JumpVelocity"))
	var gravity := float(stats.get("Gravity"))
	var move_speed := float(stats.get("MoveSpeed"))
	var route := ["Branch1", "Branch2", "Branch3", "Branch4", "Branch5", "Branch6",
		"Branch7", "Branch8", "Branch9", "Branch10", "Branch11", "TopBranch"]
	for index in range(route.size() - 1):
		var from: StaticBody2D = geometry.get_node(route[index])
		var to: StaticBody2D = geometry.get_node(route[index + 1])
		var from_shape := (from.get_node("CollisionShape2D") as CollisionShape2D).shape as RectangleShape2D
		var to_shape := (to.get_node("CollisionShape2D") as CollisionShape2D).shape as RectangleShape2D
		var rise := from.position.y - to.position.y
		var discriminant := jump_speed * jump_speed - 2.0 * gravity * rise
		if discriminant < 0.0:
			return false
		var landing_time := (jump_speed + sqrt(discriminant)) / gravity
		var edge_gap := maxf(0.0, absf(from.position.x - to.position.x) -
			(from_shape.size.x + to_shape.size.x) * 0.5)
		if edge_gap > move_speed * landing_time - 20.0:
			return false
	return true

func _benchmark(player: Node2D) -> void:
	player.set_physics_process(false)
	await create_timer(1.5).timeout
	for location in [Vector2(110, 460), Vector2(720, 210), Vector2(1380, -450), Vector2(110, 460)]:
		player.global_position = location
		var camera := player.get_node("Camera2D") as Camera2D
		camera.reset_smoothing()
		await create_timer(0.5).timeout
		var samples: Array[float] = []
		for frame in range(180):
			await process_frame
			if frame >= 60:
				samples.append(Engine.get_frames_per_second())
		var total := 0.0
		var minimum := INF
		for fps in samples:
			total += fps
			minimum = minf(minimum, fps)
		print("DOSSEL_FPS x=%d y=%d avg=%.1f min=%.1f renderer=%s" % [location.x,
			location.y, total / samples.size(), minimum,
			ProjectSettings.get_setting("rendering/renderer/rendering_method")])

func _check(condition: bool, description: String) -> void:
	if condition:
		print("DOSSEL_AREA_PASS: " + description)
	else:
		failures += 1
		push_error("DOSSEL_AREA_FAIL: " + description)
