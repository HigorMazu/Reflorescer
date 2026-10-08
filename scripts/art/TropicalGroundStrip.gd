extends Node2D
## Draws fixed 24 px end caps and varied 208 px middle modules.
## Geometry stays on the parent StaticBody2D; this node is visual only.

const EDGE := 24.0
const CENTER := 208.0
const VARIANT_DESIGN_SIZE := Vector2(256.0, 85.0)
const ORIGINAL: Texture2D = preload("res://assets/environment/tropical/ground_restored.png")
const VARIANT: Texture2D = preload("res://assets/environment/tropical/ground_modular_candidate_v1.png")

@export var strip_width := 256.0:
	set(value):
		strip_width = maxf(value, EDGE * 2.0)
		queue_redraw()
@export var strip_height := 84.0:
	set(value):
		strip_height = maxf(value, 1.0)
		queue_redraw()
@export var variant_seed := 0:
	set(value):
		variant_seed = value
		queue_redraw()

func _draw() -> void:
	_draw_piece(ORIGINAL, Rect2(0, 0, EDGE, 76), 0.0, EDGE)
	var middle_width := strip_width - EDGE * 2.0
	var cursor := EDGE
	var index := 0
	while middle_width > 0.001:
		var piece_width: float = minf(CENTER, middle_width)
		var texture: Texture2D = VARIANT if (index + variant_seed) % 3 == 1 else ORIGINAL
		var source := Rect2(EDGE, 0, piece_width, ORIGINAL.get_height())
		if texture == VARIANT:
			var scale := VARIANT.get_size() / VARIANT_DESIGN_SIZE
			source = Rect2(EDGE * scale.x, 15.0 * scale.y,
				piece_width * scale.x, 70.0 * scale.y)
		_draw_piece(texture, source, cursor, piece_width)
		cursor += piece_width
		middle_width -= piece_width
		index += 1
	_draw_piece(ORIGINAL, Rect2(256.0 - EDGE, 0, EDGE, 76), strip_width - EDGE, EDGE)

func _draw_piece(texture: Texture2D, source: Rect2, at_x: float, piece_width: float) -> void:
	draw_texture_rect_region(texture, Rect2(at_x, 0, piece_width, strip_height), source)
