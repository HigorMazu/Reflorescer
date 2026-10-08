extends SceneTree
## Checks the Dossel→Igarapé hand-off and the art against live hazard geometry.

const DOSSEL := "res://scenes/areas/DosselVivo.tscn"
const IGARAPE := "res://scenes/areas/IgarapeSufocado.tscn"
var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var manager := root.get_node("SceneManager")
	manager.call("LoadScene", DOSSEL)
	await scene_changed
	await process_frame
	var exit_trigger := current_scene.get_node("Transitions/ToIgarapeSufocado")
	manager.call("LoadSceneAtSpawn", exit_trigger.get("TargetScene"),
		exit_trigger.get("TargetArea"), exit_trigger.get("SpawnOffset"))
	await scene_changed
	await process_frame
	await process_frame
	var area := current_scene
	var player := area.get_node("Player") as Node2D
	var entry := area.get_node("SpawnFromDosselVivo") as Marker2D
	_check(area.scene_file_path == IGARAPE and player.global_position.distance_to(entry.global_position) < 2.0,
		"Dossel exit enters Igarapé at SpawnFromDosselVivo")
	var camera := player.get_node("Camera2D") as Camera2D
	_check(camera.limit_left == 0 and camera.limit_top == -200 and camera.limit_right == 2800 and camera.limit_bottom == 520,
		"Igarapé camera bounds are applied")
	_check(area.get_node("Geometry/Ground/SwampBank").visible,
		"swamp bank art is integrated over the ground collision")
	for name in ["Log1", "Log2", "Log3"]:
		var log := area.get_node("Geometry/" + name)
		_check(log.get_node_or_null("CollisionShape2D") != null and log.get_node("WaterloggedLog").visible,
			"%s keeps its collision and visible log art" % name)
	for name in ["Mud1", "Mud2", "Mud3"]:
		var mud := area.get_node("Hazards/" + name)
		var shape := (mud.get_node("CollisionShape2D") as CollisionShape2D).shape as RectangleShape2D
		var art := mud.get_node("Sludge") as Sprite2D
		_check(is_equal_approx(float(mud.get("SpeedMultiplier")), 0.55) and art.visible
			and absf(art.region_rect.size.x * art.scale.x - shape.size.x) < 1.0,
			"%s art spans the 55%% speed hazard" % name)
	var point := area.get_node("Interactables/RestorationPoint1")
	_check(point.get_node("DeadStumpsArt").visible and not point.get_node("RenewedPlantsArt").visible,
		"three stumps and litter show before restoration")
	point.call("SetRestoredWithoutEffect")
	_check(not point.get_node("DeadStumpsArt").visible and point.get_node("RenewedPlantsArt").visible
		and point.get_node("ClearWaterArt").visible,
		"two saplings, lily and clean water show after restoration")
	_check(area.get_node("Enemies").get_child_count() == 6,
		"Igarapé retains its six enemies")
	print("IGARAPE_AREA_DONE failures=%d" % failures)
	quit(failures)

func _check(condition: bool, description: String) -> void:
	if condition:
		print("IGARAPE_AREA_PASS: " + description)
	else:
		failures += 1
		push_error("IGARAPE_AREA_FAIL: " + description)
