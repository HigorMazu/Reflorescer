extends SceneTree
## Captures the production Desert scene without touching the player's save.


func _initialize() -> void:
	_capture.call_deferred()


func _capture() -> void:
	var area := load("res://scenes/areas/Deserto.tscn").instantiate() as Node2D
	root.add_child(area)
	current_scene = area
	await create_timer(.45).timeout
	var player := area.get_node("Player") as CharacterBody2D
	player.set_physics_process(false)
	var camera := player.get_node("Camera2D") as Camera2D
	camera.position_smoothing_enabled = false
	var checkpoint := area.get_node("Interactables/CheckpointDeserto") as Area2D
	checkpoint.monitoring = false
	checkpoint.monitorable = false
	DirAccess.make_dir_recursive_absolute("res://docs/art-review")

	# Entry: portal, Kairo and the desert-specific checkpoint in the same gameplay view.
	player.global_position = Vector2(520, 453)
	camera.reset_smoothing()
	await create_timer(.3).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://docs/art-review/desert-entry-checkpoint-off-v2.png")
	var totem := checkpoint.get_node("TotemVisual")
	totem.call("SetActivated", true, false)
	await create_timer(.3).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://docs/art-review/desert-entry-checkpoint-on-v2.png")

	# Final stretch: cool night light, ruin silhouettes and the new tail attack.
	player.global_position = Vector2(3050, 453)
	(area.get_node("Dressing") as Node).set("_snake_clock", 3.62)
	camera.reset_smoothing()
	await create_timer(.25).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://docs/art-review/desert-area-night-v2.png")
	print("DESERT_AREA_CAPTURE_DONE")
	quit()
