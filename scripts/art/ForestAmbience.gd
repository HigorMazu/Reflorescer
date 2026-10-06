extends Node2D
## Visual-only mote layer. Never changes collision, input or world state.
@export var area := Rect2(0, -320, 1500, 300)
@export_range(0, 32) var amount: int = 14
var elapsed: float = 0.0

func _process(delta: float) -> void:
	elapsed += delta
	queue_redraw()

func _draw() -> void:
	for i in range(amount):
		var phase := float(i) * 2.39996
		var x := area.position.x + fposmod(float(i) * 137.0 + elapsed * (3.0 + float(i % 3)), area.size.x)
		var y := area.position.y + fposmod(float(i) * 61.0, area.size.y) + sin(elapsed * 0.8 + phase) * 5.0
		var alpha := 0.12 + (sin(elapsed + phase) + 1.0) * 0.08
		draw_rect(Rect2(Vector2(x, y), Vector2(1, 2)), Color(0.76, 0.82, 0.52, alpha))
