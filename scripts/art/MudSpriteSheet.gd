extends Sprite2D
## Advances the baked six-frame pool texture; terrain edges stay in place.

@export_range(0, 5, 1) var start_frame := 0
const FRAME_COUNT := 6
const FRAME_WIDTH := 1100.0
const FRAME_HEIGHT := 140.0
const FRAMES_PER_SECOND := 5.0

var frame_index := 0
var elapsed := 0.0

func _ready() -> void:
	frame_index = start_frame
	_update_region()

func _process(delta: float) -> void:
	elapsed += delta
	while elapsed >= 1.0 / FRAMES_PER_SECOND:
		elapsed -= 1.0 / FRAMES_PER_SECOND
		frame_index = (frame_index + 1) % FRAME_COUNT
		_update_region()

func _update_region() -> void:
	region_rect = Rect2(frame_index * FRAME_WIDTH, 0.0, FRAME_WIDTH, FRAME_HEIGHT)
