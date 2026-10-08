extends Node2D
## Reviews the production SpriteFrames used by PlayerController.

const PLAYER := preload("res://scenes/player/Player.tscn")

func _ready() -> void:
	RenderingServer.set_default_clear_color(Color("182c25"))
	_add_pose("Wall slide", Vector2(118, 500), &"wall_slide", true)
	_add_pose("Dash", Vector2(900, 500), &"dash", false)
	for x in [80.0, 600.0]:
		var wall := ColorRect.new()
		wall.position = Vector2(x, 130)
		wall.size = Vector2(16, 390)
		wall.color = Color("45554b")
		add_child(wall)
	await get_tree().create_timer(0.2).timeout
	if DisplayServer.get_name() == "headless":
		push_error("MovementPoseReview requires a rendering display")
		get_tree().quit(2)
		return
	await RenderingServer.frame_post_draw
	get_viewport().get_texture().get_image().save_png("res://.godot/movement-poses-v1.png")
	get_tree().quit()

func _add_pose(title: String, at: Vector2, animation: StringName, face_left: bool) -> void:
	var player := PLAYER.instantiate()
	player.process_mode = Node.PROCESS_MODE_DISABLED
	player.position = at
	player.scale = Vector2(2.0, 2.0)
	add_child(player)
	player.get_node("Camera2D").enabled = false
	if face_left:
		player.get_node("Visual").scale.x = -1
	var sprite: AnimatedSprite2D = player.get_node("Visual/AnimatedSprite2D")
	sprite.animation = animation
	sprite.frame = 0
	var effects := player.get_node("MovementStateEffects")
	effects.set("PreviewState", 1 if animation == &"wall_slide" else 2)
	var label := Label.new()
	label.text = title
	label.position = at + Vector2(-10, -135)
	label.add_theme_font_size_override("font_size", 24)
	add_child(label)
