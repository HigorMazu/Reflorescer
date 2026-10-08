extends Node
## Captures the actual canopy geometry at lower and upper camera positions.

func _ready() -> void:
	var area: Node2D = load("res://scenes/areas/DosselVivo.tscn").instantiate()
	area.process_mode = Node.PROCESS_MODE_DISABLED
	add_child(area)
	await get_tree().process_frame
	var player: Node = get_tree().get_first_node_in_group("Player")
	player.get_node("Camera2D").enabled = false
	var stage := "branches"
	if "--restored" in OS.get_cmdline_user_args():
		area.get_node("Interactables/RestorationPoint1").call("SetRestoredWithoutEffect")
		stage = "restored"
	for frame in [Vector2(0, 0), Vector2(320, -650)]:
		get_viewport().canvas_transform = Transform2D(0.0, -frame)
		await get_tree().create_timer(0.15).timeout
		await RenderingServer.frame_post_draw
		get_viewport().get_texture().get_image().save_png(
			"res://.godot/dossel-%s-%d.png" % [stage, 0 if frame.y == 0 else 1]
		)
	get_tree().quit()
