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
	var bank := area.get_node("Geometry/Ground/SwampBank")
	_check(bank.visible,
		"swamp bank art is integrated over the ground collision")
	_check(bank.cutout_ranges == PackedFloat32Array([492, 908, 1432, 1948, 2192, 2408]),
		"grass terrain has openings at all three mud pools")
	_check(player.z_index == 5, "Kairo renders above the mud surface")
	for name in ["Log1", "Log2", "Log3"]:
		var log := area.get_node("Geometry/" + name)
		_check(log.get_node_or_null("CollisionShape2D") != null and log.get_node("WaterloggedLog").visible,
			"%s keeps its collision and visible log art" % name)
	for name in ["Mud1", "Mud2", "Mud3"]:
		var mud := area.get_node("Hazards/" + name)
		var shape := (mud.get_node("CollisionShape2D") as CollisionShape2D).shape as RectangleShape2D
		var art := mud.get_node("Sludge") as Sprite2D
		var pool_bed := mud.get_node("PoolBed") as ColorRect
		var ground := area.get_node("Geometry/Ground") as StaticBody2D
		var ground_shape := (ground.get_node("CollisionShape2D") as CollisionShape2D).shape as RectangleShape2D
		var ground_top := ground.global_position.y - ground_shape.size.y * 0.5
		_check(is_equal_approx(float(mud.get("SpeedMultiplier")), 0.55) and art.visible and pool_bed.visible
			and absf(art.region_rect.size.x * art.scale.x - shape.size.x) < 1.0
			and art.texture.resource_path.ends_with("igarape_mud_bubble_sheet_v1.png")
			and art.region_rect.size == Vector2(1100, 140)
			and pool_bed.size.x == shape.size.x and pool_bed.size.y == 104.0
			and is_equal_approx(pool_bed.global_position.y, ground_top - 4.0)
			and art.material is ShaderMaterial and pool_bed.material is ShaderMaterial
			and art.get_script() != null and mud.get_node_or_null("Bubbles") == null
			and pool_bed.z_index < art.z_index,
			"%s fills the terrain opening with animated mud across the 55%% speed hazard" % name)
		var start_frame: int = [0, 2, 4][["Mud1", "Mud2", "Mud3"].find(name)]
		_check(art.get("frame_index") == start_frame,
			"%s begins on its own sprite-sheet frame" % name)
		art.call("_process", 0.21)
		_check(art.get("frame_index") == (start_frame + 1) % 6
			and art.region_rect.position.x == float((start_frame + 1) % 6) * 1100.0,
			"%s advances its baked bubble burst frame" % name)
	var point := area.get_node("Interactables/RestorationPoint1")
	_check(point.get_node("DeadStumpsArt").visible and not point.get_node("RenewedPlantsArt").visible,
		"three stumps and litter show before restoration")
	point.call("SetRestoredWithoutEffect")
	_check(not point.get_node("DeadStumpsArt").visible and point.get_node("RenewedPlantsArt").visible
		and point.get_node("ClearWaterArt").visible,
		"two saplings, lily and clean water show after restoration")
	_check(area.get_node("Enemies").get_child_count() == 6,
		"Igarapé retains its six enemies")
	for enemy in area.get_node("Enemies").get_children():
		_check(enemy.z_index == 5, "%s renders above the mud surface" % enemy.name)
	print("IGARAPE_AREA_DONE failures=%d" % failures)
	quit(failures)

func _check(condition: bool, description: String) -> void:
	if condition:
		print("IGARAPE_AREA_PASS: " + description)
	else:
		failures += 1
		push_error("IGARAPE_AREA_FAIL: " + description)
