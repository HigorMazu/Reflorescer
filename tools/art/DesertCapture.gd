extends SceneTree
## Captures actual rendered views at three points in the isolated desert route.


func _initialize() -> void:
	_capture.call_deferred()


func _capture() -> void:
	var review := load("res://scenes/art/DesertReview.tscn").instantiate() as Node2D
	root.add_child(review)
	await create_timer(.2).timeout
	var player := review.get_node("Player") as CharacterBody2D
	player.set_physics_process(false)
	var camera := player.get_node("Camera2D") as Camera2D
	camera.position_smoothing_enabled = false
	DirAccess.make_dir_recursive_absolute("res://docs/art-review")
	for entry in [["dia", 350.0], ["tarde", 1575.0], ["noite", 3100.0]]:
		player.global_position = Vector2(entry[1], 453.0)
		camera.reset_smoothing()
		await create_timer(.25).timeout
		await RenderingServer.frame_post_draw
		get_root().get_texture().get_image().save_png("res://docs/art-review/desert-ingame-%s-v1.png" % entry[0])
	print("DESERT_CAPTURE_DONE")
	quit()
