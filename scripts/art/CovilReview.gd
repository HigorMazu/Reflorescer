extends Node
## Captures the real boss arena without changing its game geometry.

func _ready() -> void:
	var arena: Node2D = load("res://scenes/areas/CovilDeKorrag.tscn").instantiate()
	arena.process_mode = Node.PROCESS_MODE_DISABLED
	add_child(arena)
	# The production boss falls to y=200 through physics. The frozen review places it there directly.
	arena.get_node("Boss_Javali").position.y = 200
	await get_tree().process_frame
	var player: Node = get_tree().get_first_node_in_group("Player")
	player.get_node("Camera2D").enabled = false
	get_viewport().canvas_transform = Transform2D(0.0, Vector2(640, 280))
	await get_tree().create_timer(0.2).timeout
	await RenderingServer.frame_post_draw
	get_viewport().get_texture().get_image().save_png("res://.godot/covil-arena-v1.png")
	get_tree().quit()
