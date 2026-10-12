extends SceneTree
## Real Dossel collision/input, captured at gameplay scale; no save operations.

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var area: Node2D = load("res://scenes/areas/DosselVivo.tscn").instantiate()
	area.process_mode = Node.PROCESS_MODE_DISABLED
	root.add_child(area)
	area.get_node("Geometry").process_mode = Node.PROCESS_MODE_ALWAYS
	var player: CharacterBody2D = area.get_node("Player")
	player.process_mode = Node.PROCESS_MODE_ALWAYS
	player.get_node("Camera2D").enabled = false
	root.canvas_transform = Transform2D(0.0, Vector2(-320, 0))
	DirAccess.make_dir_recursive_absolute("res://.godot/wall-contact-review")
	for side in [1, -1]:
		player.position = Vector2(1005 if side == 1 else 1120, 200)
		player.velocity = Vector2(0, 80)
		player.reset_physics_interpolation()
		var action := "move_right" if side == 1 else "move_left"
		Input.action_press(action)
		await create_timer(0.3).timeout
		if not player.get("IsWallSliding"):
			push_error("Must capture the actual wall slide")
			quit(1)
			return
		for frame in range(12):
			await create_timer(0.05).timeout
			if DisplayServer.get_name() != "headless":
				await RenderingServer.frame_post_draw
				root.get_texture().get_image().save_png("res://.godot/wall-contact-review/%d-%02d.png" % [side, frame])
		Input.action_release(action)
	print("WALL_CONTACT_REVIEW_PASS: both Dossel trunk faces, real input and collision")
	quit()
