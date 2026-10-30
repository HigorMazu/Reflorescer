extends SceneTree
## Capture the Sopé -> Dossel threshold from the production scene.

func _initialize() -> void:
	_capture.call_deferred()


func _capture() -> void:
	change_scene_to_file("res://scenes/areas/SopeDaMata.tscn")
	await scene_changed
	var player := get_first_node_in_group("Player") as CharacterBody2D
	player.set_physics_process(false)
	player.global_position = Vector2(2200, 460)
	var camera := player.get_node("Camera2D") as Camera2D
	camera.position_smoothing_enabled = false
	camera.reset_smoothing()
	await create_timer(.6).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png(
		"res://docs/art-review/transition-sope-to-dossel-v14.png")
	print("SOPE_TRANSITION_REVIEW_DONE")
	quit()
