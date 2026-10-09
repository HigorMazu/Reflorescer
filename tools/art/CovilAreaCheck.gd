extends SceneTree
## Checks the final transition and that the boss arena remains a focused combat room.

const IGARAPE := "res://scenes/areas/IgarapeSufocado.tscn"
const COVIL := "res://scenes/areas/CovilDeKorrag.tscn"
var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var manager := root.get_node("SceneManager")
	manager.call("LoadScene", IGARAPE)
	await scene_changed
	await process_frame
	var exit_trigger := current_scene.get_node("Transitions/ToCovilDeKorrag")
	manager.call("LoadSceneAtSpawn", exit_trigger.get("TargetScene"),
		exit_trigger.get("TargetArea"), exit_trigger.get("SpawnOffset"))
	await scene_changed
	await process_frame
	await process_frame
	var area := current_scene
	var player := area.get_node("Player") as Node2D
	var entry := area.get_node("SpawnFromIgarapeSufocado") as Marker2D
	_check(area.scene_file_path == COVIL and player.global_position.distance_to(entry.global_position) < 2.0,
		"Igarapé exit enters Covil at SpawnFromIgarapeSufocado")
	var camera := player.get_node("Camera2D") as Camera2D
	_check(camera.limit_left == -640 and camera.limit_top == -280 and camera.limit_right == 640 and camera.limit_bottom == 440,
		"Covil camera is locked to the boss arena")
	var geometry := area.get_node("Geometry")
	for name in ["WallLeft", "WallRight", "Ceiling", "ArenaFloor"]:
		_check(geometry.get_node(name).get_node_or_null("CollisionShape2D") != null,
			"%s collision remains in the arena" % name)
	_check(geometry.get_node("ArenaFloor/ChurnedFloor").visible,
		"churned earth art is integrated over the arena floor")
	_check(geometry.get_node("ArenaFloor/ChurnedFloor").position.y == -84.0,
		"arena art surface shares the collision baseline used by Kairo and Korrag")
	_check(area.get_node_or_null("Boss_Javali") != null and area.get_node("Enemies").get_child_count() == 0,
		"Covil contains Korrag and no common enemies")
	_check(area.get_node("Interactables").get_node_or_null("RestorationPoint1") == null,
		"Covil has no restoration point")
	print("COVIL_AREA_DONE failures=%d" % failures)
	quit(failures)

func _check(condition: bool, description: String) -> void:
	if condition:
		print("COVIL_AREA_PASS: " + description)
	else:
		failures += 1
		push_error("COVIL_AREA_FAIL: " + description)
