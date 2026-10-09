extends Node
## Captures the integrated swamp at its entrance, restoration point, and last mud pool.

func _ready() -> void:
	var area: Node2D = load("res://scenes/areas/IgarapeSufocado.tscn").instantiate()
	area.process_mode = Node.PROCESS_MODE_DISABLED
	add_child(area)
	for mud in area.get_node("Hazards").get_children():
		var sludge := mud.get_node_or_null("Sludge")
		if sludge != null:
			sludge.process_mode = Node.PROCESS_MODE_ALWAYS
	await get_tree().process_frame
	var player: Node = get_tree().get_first_node_in_group("Player")
	player.get_node("Camera2D").enabled = false
	var stage := "before"
	if "--restored" in OS.get_cmdline_user_args():
		area.get_node("Interactables/RestorationPoint1").call("SetRestoredWithoutEffect")
		stage = "restored"
	# 1520 is the rightmost playable camera origin for this 2800 px area and 1280 px viewport.
	for location in [0, 850, 1520]:
		get_viewport().canvas_transform = Transform2D(0.0, Vector2(-location, 0))
		await get_tree().create_timer(0.2).timeout
		await RenderingServer.frame_post_draw
		get_viewport().get_texture().get_image().save_png(
			"res://.godot/igarape-%s-%d.png" % [stage, location]
		)
		if location == 1520:
			for bubble_frame in range(4):
				await get_tree().create_timer(0.35).timeout
				await RenderingServer.frame_post_draw
				get_viewport().get_texture().get_image().save_png(
					"res://.godot/igarape-bubbles-%d.png" % bubble_frame
				)
	if stage == "before":
		player.global_position = Vector2(700, 480)
		area.get_node("Enemies/Rapido1").global_position = Vector2(850, 480)
		get_viewport().canvas_transform = Transform2D(0.0, Vector2.ZERO)
		await get_tree().create_timer(0.2).timeout
		await RenderingServer.frame_post_draw
		get_viewport().get_texture().get_image().save_png("res://.godot/igarape-occupied.png")
	get_tree().quit()
