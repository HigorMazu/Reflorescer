extends Node
## Captures the production Sopé scene, with gameplay disabled to avoid progress writes.
func _ready() -> void:
	var area: Node = load("res://scenes/areas/SopeDaMata.tscn").instantiate()
	area.process_mode = Node.PROCESS_MODE_DISABLED
	add_child(area)
	await get_tree().process_frame
	var player: Node = get_tree().get_first_node_in_group("Player")
	var hud: Node = get_tree().get_first_node_in_group("HUD")
	var indicator: Control = hud.get_node("MarginContainer/VBoxContainer/TopRow/HealthSection/SwordStatus")
	for active in [true, false]:
		if player.get("HasSword") != active:
			player.call("ToggleSword")
		player.call("SetSwordVisible", active)
		indicator.call("SetState", active, false)
		await get_tree().create_timer(0.2).timeout
		await RenderingServer.frame_post_draw
		var state: String = "active" if active else "stowed"
		get_viewport().get_texture().get_image().save_png("res://docs/art-review/sword-hud-" + state + ".png")
	get_tree().quit()
