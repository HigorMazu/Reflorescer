extends Node2D
## Contact shading only. The collision and damage rectangles remain on the boss.
@onready var sprite: AnimatedSprite2D = get_parent().get_node("AnimatedSprite2D")

func _process(_delta: float) -> void:
	visible = sprite.visible
	if visible:
		queue_redraw()

func _draw() -> void:
	if not is_instance_valid(sprite):
		return
	var reach := 148.0
	var center := 0.0
	if sprite.animation == &"attack":
		reach = 184.0
		center = 25.0 if sprite.flip_h else -25.0
	elif sprite.animation == &"tail_sweep":
		reach = 196.0
	elif sprite.animation == &"burrow_enter" or sprite.animation == &"burrow_exit":
		reach = 132.0
	# Soft, shallow ellipse beneath the coils, fading out at its perimeter.
	draw_set_transform(Vector2(center, 7), 0, Vector2(1, .055))
	for layer in range(6):
		draw_circle(Vector2.ZERO, reach - layer * 10, Color(.12, .085, .045, .045))
