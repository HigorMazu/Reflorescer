extends SceneTree
## Safe visual review: does not touch progression or activate the checkpoint.

func _initialize() -> void:
	_run.call_deferred()

func _capture(path: String) -> void:
	if DisplayServer.get_name() == "headless":
		return
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png(path)

func _run() -> void:
	DirAccess.make_dir_recursive_absolute("res://.godot/roadmap-art-review")
	if "--small" in OS.get_cmdline_user_args():
		root.size = Vector2i(960, 540)
	else:
		root.size = Vector2i(1280, 720)

	# Grounded checkpoint in the actual Dossel scene, with no player input.
	var area: Node2D = load("res://scenes/areas/DosselVivo.tscn").instantiate()
	area.process_mode = Node.PROCESS_MODE_DISABLED
	root.add_child(area)
	var checkpoint = area.get_node("Interactables/Checkpoint1")
	var totem = checkpoint.get_node("TotemVisual")
	checkpoint.process_mode = Node.PROCESS_MODE_ALWAYS
	root.canvas_transform = Transform2D(0.0, Vector2(440, -170))
	assert(checkpoint.get_node_or_null("Flag") == null, "blue placeholder removed")
	totem.call("SetActivated", false, false)
	await create_timer(0.15).timeout
	await _capture("res://.godot/roadmap-art-review/checkpoint-off.png")
	totem.call("SetActivated", true, true)
	await create_timer(0.35).timeout
	assert(totem.get("Activated"), "checkpoint activation visual is on")
	await _capture("res://.godot/roadmap-art-review/checkpoint-on.png")
	area.queue_free()
	await process_frame

	# Production boss sprite, same automatic frame correction as the game.
	root.canvas_transform = Transform2D.IDENTITY
	RenderingServer.set_default_clear_color(Color("182c25"))
	var boss = load("res://scenes/bosses/Boss_Javali.tscn").instantiate()
	boss.process_mode = Node.PROCESS_MODE_DISABLED
	boss.position = Vector2(root.size.x / 2.0, root.size.y * 0.76)
	root.add_child(boss)
	var sprite: AnimatedSprite2D = boss.get_node("AnimatedSprite2D")
	sprite.process_mode = Node.PROCESS_MODE_ALWAYS
	sprite.stop()
	for direction in [-1, 1]:
		boss.call("FaceDirection", direction)
		for frame in range(3):
			sprite.animation = &"attack"
			sprite.frame = frame
			await process_frame
			var expected_flip: bool = (direction == 1) != (frame < 2)
			assert(sprite.flip_h == expected_flip, "Korrag attack frame must face its target")
			await _capture("res://.godot/roadmap-art-review/korrag-%d-%d.png" % [direction, frame])
	sprite.animation = &"idle"
	await process_frame
	assert(sprite.flip_h, "other left-facing frames still mirror when Korrag faces right")
	boss.queue_free()
	await process_frame

	# End screen for each existing restoration count. No boss defeat or save event.
	var hud = load("res://scenes/ui/HUD.tscn").instantiate()
	root.add_child(hud)
	var prefix := "small-" if "--small" in OS.get_cmdline_user_args() else ""
	for restored in range(4):
		hud.call("ShowDemoEnd", restored)
		await create_timer(0.8, true).timeout
		var panel = hud.get_node("DemoEnd")
		assert(panel.visible and panel.get_node("Center/Card/Margin/Box/Count").text.ends_with("%d/3" % restored), "end count is correct")
		assert(root.gui_get_focus_owner() == panel.get_node("Center/Card/Margin/Box/ExploreButton"), "continue button takes keyboard focus")
		await _capture("res://.godot/roadmap-art-review/%send-%d.png" % [prefix, restored])
	paused = false
	print("ROADMAP_ART_REVIEW_PASS: checkpoint states, boss facing, demo end 0-3")
	quit()
