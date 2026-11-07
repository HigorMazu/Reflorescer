extends Node2D
## Visual only: the parent owns the arena collision and visibility.
var _time := 0.0

func _process(delta: float) -> void:
	if not is_visible_in_tree():
		return
	_time += delta
	queue_redraw()

func _draw() -> void:
	# Interlaced streams with irregular edges, rooted at the walkable surface.
	for layer in range(12):
		var points := PackedVector2Array()
		var center := (layer - 5.5) * 3.2
		var width := 5.0 + sin(layer * 2.4) * 2.0
		for side in [1.0, -1.0]:
			for step in range(33):
				var row := step if side > 0 else 32 - step
				var y := -365.0 + row * 15.0
				var wave := sin(y * .035 + _time * 2.4 + layer * .7) * 7.0
				wave += sin(y * .09 - _time * 4.0 + layer) * 2.5
				points.append(Vector2(center + wave + width * side, y))
		var tint := Color("b89b64").lerp(Color("f3d9a0"), float(layer % 4) / 4.0)
		tint.a = .20
		draw_colored_polygon(points, tint)
	# Grains rise at different speeds; deterministic placement avoids flicker.
	for i in range(180):
		var y := 114.0 - fposmod(i * 37.7 + _time * (80.0 + i % 7 * 12.0), 480.0)
		var x := sin(i * 9.1) * 22.0 + sin(y * .04 + _time * 3.0 + i) * 8.0
		var alpha := .25 + .4 * pow(1.0 - abs(x) / 34.0, 2)
		draw_rect(Rect2(Vector2(x, y), Vector2(1.5 + i % 2, 2 + i % 3)), Color(.91, .77, .50, alpha))
	# Elliptical sand eddies sit on the floor, not above it.
	for i in range(5):
		var phase := fposmod(_time * .6 + i * .2, 1.0)
		var ring := PackedVector2Array()
		for j in range(41):
			var angle := j * TAU / 40.0
			ring.append(Vector2(cos(angle) * (10 + phase * 40), 112 + sin(angle) * (2 + phase * 4)))
		draw_polyline(ring, Color(.85, .68, .39, (1 - phase) * .5), 1.5, true)
