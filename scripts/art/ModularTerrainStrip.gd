@tool
extends Node2D
## Repeats the middle of a wide terrain illustration while keeping its unique end caps.
## Collision remains on the parent StaticBody2D; this is visual-only terrain art.

@export var terrain_texture: Texture2D:
	set(value):
		terrain_texture = value
		queue_redraw()

@export var strip_width := 1280.0:
	set(value):
		strip_width = maxf(value, 1.0)
		queue_redraw()

@export var strip_height := 120.0:
	set(value):
		strip_height = maxf(value, 1.0)
		queue_redraw()

@export var cap_source_width := 180.0:
	set(value):
		cap_source_width = maxf(value, 1.0)
		queue_redraw()

@export var middle_source := Rect2(620.0, 0.0, 760.0, 724.0):
	set(value):
		middle_source = value
		queue_redraw()

@export var alternate_middle_source := Rect2(1040.0, 0.0, 760.0, 724.0):
	set(value):
		alternate_middle_source = value
		queue_redraw()

# Consecutive [left, right] pairs where another terrain material replaces this strip.
@export var cutout_ranges := PackedFloat32Array():
	set(value):
		cutout_ranges = value
		queue_redraw()

func _draw_piece(destination: Rect2, source: Rect2) -> void:
	var cursor := destination.position.x
	var right := destination.end.x
	for pair in range(0, cutout_ranges.size() - 1, 2):
		var cut_left := cutout_ranges[pair]
		var cut_right := cutout_ranges[pair + 1]
		if cut_right <= cursor or cut_left >= right:
			continue
		if cut_left > cursor:
			_draw_piece_segment(destination, source, cursor, minf(cut_left, right))
		cursor = maxf(cursor, minf(cut_right, right))
		if cursor >= right:
			return
	if cursor < right:
		_draw_piece_segment(destination, source, cursor, right)

func _draw_piece_segment(destination: Rect2, source: Rect2, left: float, right: float) -> void:
	var fraction_left := (left - destination.position.x) / destination.size.x
	var fraction_width := (right - left) / destination.size.x
	var clipped_source := Rect2(
		Vector2(source.position.x + source.size.x * fraction_left, source.position.y),
		Vector2(source.size.x * fraction_width, source.size.y)
	)
	draw_texture_rect_region(terrain_texture,
		Rect2(left, destination.position.y, right - left, destination.size.y), clipped_source)

func _draw() -> void:
	if terrain_texture == null:
		return

	var texture_size := terrain_texture.get_size()
	if texture_size.x <= 0.0 or texture_size.y <= 0.0:
		return

	var source_height := texture_size.y
	var cap_width := minf(strip_height * cap_source_width / source_height, strip_width * 0.2)
	var right_cap_source := Rect2(texture_size.x - cap_source_width, 0.0, cap_source_width, source_height)
	_draw_piece(Rect2(0.0, 0.0, cap_width, strip_height),
		Rect2(0.0, 0.0, cap_source_width, source_height))

	var remaining_width := maxf(0.0, strip_width - cap_width * 2.0)
	var cursor := cap_width
	var index := 0
	while remaining_width > 0.01:
		var source := middle_source if index % 2 == 0 else alternate_middle_source
		if source.size.x <= 0.0 or source.size.y <= 0.0:
			source = middle_source
		var ideal_width := strip_height * source.size.x / source.size.y
		var piece_width := minf(ideal_width, remaining_width)
		var used_source_width := source.size.x * piece_width / ideal_width
		_draw_piece(Rect2(cursor, 0.0, piece_width, strip_height),
			Rect2(source.position, Vector2(used_source_width, source.size.y)))
		cursor += piece_width
		remaining_width -= piece_width
		index += 1

	_draw_piece(
		Rect2(strip_width - cap_width, 0.0, cap_width, strip_height), right_cap_source)
