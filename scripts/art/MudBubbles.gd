extends Node2D
## Tiny surface ripples mark the slow terrain without covering the player.

@export var pool_width := 440.0
var elapsed := 0.0

func _process(delta: float) -> void:
	elapsed = fmod(elapsed + delta, 2.4)
	queue_redraw()

func _draw() -> void:
	for index in range(4):
		var age: float = fposmod(elapsed + index * 0.6, 2.4) / 2.4
		var x: float = (float(index) / 3.0 - 0.5) * pool_width * 0.65
		var radius: float = 2.0 + age * 5.0
		var tint := Color(0.86, 0.75, 0.40, 0.35 * (1.0 - age))
		draw_arc(Vector2(x, 4), radius, PI * 0.1, PI * 0.9, 8, tint, 1.0)
