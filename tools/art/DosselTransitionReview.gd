extends SceneTree
## Capture the Dossel entrance from the production scene.

func _initialize() -> void:
	_capture.call_deferred()


func _capture() -> void:
	change_scene_to_file("res://scenes/areas/DosselVivo.tscn")
	await scene_changed
	var player := get_first_node_in_group("Player") as CharacterBody2D
	player.set_physics_process(false)
	player.global_position = Vector2(400, 460)
	var camera := player.get_node("Camera2D") as Camera2D
	camera.position_smoothing_enabled = false
	camera.reset_smoothing()
	await create_timer(.6).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png(
		"res://docs/art-review/transition-dossel-from-sope-v15.png")
	player.global_position = Vector2(500, -6)
	camera.reset_smoothing()
	await create_timer(.3).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png(
		"res://docs/art-review/transition-dossel-upper-v15.png")
	player.global_position = Vector2(740, -82)
	camera.reset_smoothing()
	await create_timer(.3).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png(
		"res://docs/art-review/transition-dossel-higher-v15.png")
	print("DOSSEL_TRANSITION_REVIEW_DONE")
	quit()
