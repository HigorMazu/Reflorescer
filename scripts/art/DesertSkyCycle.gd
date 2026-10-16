extends CanvasLayer
## Visual-only progression through day, dusk and night as the player advances.

const DAY: Texture2D = preload("res://assets/environment/desert/desert_day_v1.png")
const DUSK: Texture2D = preload("res://assets/environment/desert/desert_dusk_v1.png")
const NIGHT: Texture2D = preload("res://assets/environment/desert/desert_night_v1.png")

@export var progress_start_x := 0.0
@export var progress_end_x := 3840.0

var day_sprite: Sprite2D
var dusk_sprite: Sprite2D
var night_sprite: Sprite2D


func _ready() -> void:
	layer = -10
	day_sprite = _add_layer("Day", DAY)
	dusk_sprite = _add_layer("Dusk", DUSK)
	night_sprite = _add_layer("Night", NIGHT)
	get_viewport().size_changed.connect(_fit_viewport)
	_fit_viewport()
	_update_light()


func _add_layer(label: String, image: Texture2D) -> Sprite2D:
	var sprite := Sprite2D.new()
	sprite.name = label
	sprite.texture = image
	sprite.texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR
	add_child(sprite)
	return sprite


func _fit_viewport() -> void:
	var screen_size := get_viewport().get_visible_rect().size
	for sprite in [day_sprite, dusk_sprite, night_sprite]:
		sprite.position = screen_size * 0.5
		sprite.scale = screen_size / sprite.texture.get_size()


func _process(_delta: float) -> void:
	_update_light()


func _update_light() -> void:
	var player := get_tree().get_first_node_in_group("Player") as Node2D
	var x := player.global_position.x if player != null else progress_start_x
	var span := maxf(1.0, progress_end_x - progress_start_x)
	var progress := clampf((x - progress_start_x) / span, 0.0, 1.0)
	# Day -> dusk in the first half; dusk -> night in the second half.
	dusk_sprite.modulate.a = clampf(progress * 2.0, 0.0, 1.0)
	night_sprite.modulate.a = clampf((progress - 0.5) * 2.0, 0.0, 1.0)
