extends SceneTree
## Captures the entrance, middle and arena in the playable Desert scene.


func _initialize() -> void:
	_capture.call_deferred()


func _capture() -> void:
	change_scene_to_file("res://scenes/areas/Deserto.tscn")
	await scene_changed
	await create_timer(0.35).timeout
	var player := get_first_node_in_group("Player") as CharacterBody2D
	player.set_physics_process(false)
	var camera := player.get_node("Camera2D") as Camera2D
	camera.position_smoothing_enabled = false
	DirAccess.make_dir_recursive_absolute("res://docs/art-review")
	for stop in [["entrance", 300.0], ["middle", 1575.0], ["arena", 3150.0]]:
		player.global_position = Vector2(stop[1], 453)
		camera.reset_smoothing()
		await create_timer(0.25).timeout
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png(
			"res://docs/art-review/desert-solid-ground-%s-v1.png" % stop[0]
		)
	print("DESERT_GROUND_ALIGNMENT_CAPTURE_DONE")
	quit()
