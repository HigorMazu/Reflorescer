extends Node2D
## Captures the real PlayerController attack path with a physical floor, no saves or progression.
func _ready() -> void:
	RenderingServer.set_default_clear_color(Color("182c25"))
	var floor := StaticBody2D.new()
	floor.position = Vector2(640, 520)
	var floor_shape := CollisionShape2D.new()
	floor_shape.shape = RectangleShape2D.new()
	floor_shape.shape.size = Vector2(1280, 20)
	floor.add_child(floor_shape)
	add_child(floor)

	var player: Node2D = load("res://scenes/player/Player.tscn").instantiate()
	player.position = Vector2(280, 510)
	player.scale = Vector2(4, 4)
	add_child(player)
	player.get_node("Camera2D").enabled = false
	player.call("SetSwordVisible", true)
	var label := Label.new()
	label.position = Vector2(70, 70)
	label.text = "Estocada: recuo → impacto → recuperação"
	label.add_theme_font_size_override("font_size", 28)
	add_child(label)

	await get_tree().physics_frame
	var press := InputEventAction.new()
	press.action = "attack"
	press.pressed = true
	Input.parse_input_event(press)
	await get_tree().physics_frame
	var release := InputEventAction.new()
	release.action = "attack"
	release.pressed = false
	Input.parse_input_event(release)

	DirAccess.make_dir_recursive_absolute("res://.godot/thrust-review-frames")
	for index in range(12):
		await get_tree().create_timer(1.0 / 30.0).timeout
		await RenderingServer.frame_post_draw
		get_viewport().get_texture().get_image().save_png("res://.godot/thrust-review-frames/%02d.png" % index)
	get_tree().quit()
