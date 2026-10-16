extends CanvasModulate
## Matches foreground sprites and terrain to the backdrop's time of day.

@export var progress_start_x := 150.0
@export var progress_end_x := 3000.0

const DAY_LIGHT := Color(1.0, 1.0, 1.0)
const DUSK_LIGHT := Color(0.93, 0.77, 0.76)
const NIGHT_LIGHT := Color(0.50, 0.62, 0.82)


func _process(_delta: float) -> void:
	var player := get_tree().get_first_node_in_group("Player") as Node2D
	if player == null:
		return
	var progress := clampf((player.global_position.x - progress_start_x) /
		maxf(1.0, progress_end_x - progress_start_x), 0.0, 1.0)
	if progress <= 0.5:
		color = DAY_LIGHT.lerp(DUSK_LIGHT, progress * 2.0)
	else:
		color = DUSK_LIGHT.lerp(NIGHT_LIGHT, (progress - 0.5) * 2.0)
