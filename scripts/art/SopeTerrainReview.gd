extends Node
## Captures the production Sopé geometry without running player input or save logic.
func _ready() -> void:
	var area: Node2D = load("res://scenes/areas/SopeDaMata.tscn").instantiate()
	area.process_mode = Node.PROCESS_MODE_DISABLED
	add_child(area)
	await get_tree().process_frame
	var player: Node = get_tree().get_first_node_in_group("Player")
	player.get_node("Camera2D").enabled = false
	var stage := "before" if "--before" in OS.get_cmdline_user_args() else "after"
	if "--restored" in OS.get_cmdline_user_args():
		area.get_node("Interactables/RestorationPoint1").call("SetRestoredWithoutEffect")
		stage = "restored"
	for position_x in [0, 900]:
		get_viewport().canvas_transform = Transform2D(0.0, Vector2(-position_x, 0))
		await get_tree().create_timer(0.15).timeout
		await RenderingServer.frame_post_draw
		get_viewport().get_texture().get_image().save_png(
			"res://.godot/sope-terrain-%s-%d.png" % [stage, position_x]
		)
	get_tree().quit()
