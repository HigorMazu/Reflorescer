extends Node2D
## Contact sheet built from the exact SpriteFrames used by the production boss.

const FRAMES: SpriteFrames = preload("res://assets/sprites/korrag/KorragSpriteFrames.tres")
const ANIMATIONS := [
	&"idle", &"walk", &"intro", &"charge_windup",
	&"charge", &"stomp", &"attack", &"hurt",
	&"phase1_idle", &"phase2_transition", &"enraged_transition", &"dead",
	&"defeated"
]

func _ready() -> void:
	RenderingServer.set_default_clear_color(Color("182c25"))
	for index in ANIMATIONS.size():
		var animation: StringName = ANIMATIONS[index]
		var column := index % 4
		var row := index / 4
		var sprite := AnimatedSprite2D.new()
		sprite.sprite_frames = FRAMES
		sprite.animation = animation
		sprite.position = Vector2(155 + column * 315, 112 + row * 178)
		sprite.scale = Vector2(0.43, 0.43)
		sprite.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
		sprite.play()
		add_child(sprite)
		var label := Label.new()
		label.text = String(animation)
		label.position = Vector2(42 + column * 315, 8 + row * 178)
		label.add_theme_font_size_override("font_size", 18)
		add_child(label)
	await get_tree().create_timer(0.8).timeout
	if DisplayServer.get_name() == "headless":
		push_error("KorragAnimationReview requires a rendering display")
		get_tree().quit(2)
		return
	await RenderingServer.frame_post_draw
	get_viewport().get_texture().get_image().save_png("res://.godot/korrag-animation-v2.png")
	get_tree().quit()
