extends Node2D
## Visual da área jogável do Deserto (C15-T1): céu dia → tarde → noite, luz no primeiro plano,
## faixa de chão e a prévia da cobra no fim. A colisão fica na cena da área; isto é só arte.
## Reaproveita as peças da prévia do Higor (DesertReview.gd).

const SKY: PackedScene = preload("res://scenes/art/DesertSkyCycle.tscn")
const LIGHTING: Script = preload("res://scripts/art/DesertLighting.gd")
const GROUND: Texture2D = preload("res://assets/environment/desert/desert_ground_tile_v1.png")

@export var world_width := 3840.0
@export var floor_y := 475.0
@export var progress_start_x := 150.0
@export var progress_end_x := 3000.0
## Prévia da cobra (só animação; a luta é a C15-T2). Vazio = sem cobra.
@export var snake_path: NodePath
## A partir daqui a cobra entra no ciclo de bote.
@export var snake_wake_x := 2900.0

var _snake: AnimatedSprite2D
var _snake_clock := 0.0


func _ready() -> void:
	var sky := SKY.instantiate()
	sky.set("progress_start_x", progress_start_x)
	sky.set("progress_end_x", progress_end_x)
	add_child(sky)
	var light := CanvasModulate.new()
	light.name = "DesertLighting"
	light.set_script(LIGHTING)
	light.set("progress_start_x", progress_start_x)
	light.set("progress_end_x", progress_end_x)
	add_child(light)
	for index in range(ceili(world_width / GROUND.get_width())):
		var tile := Sprite2D.new()
		tile.name = "Sandstone%02d" % index
		tile.texture = GROUND
		tile.centered = false
		tile.position = Vector2(index * GROUND.get_width(), floor_y - 10)
		tile.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
		tile.z_index = -1
		add_child(tile)
	var snake_root := get_node_or_null(snake_path)
	if snake_root != null:
		_snake = snake_root.get_node_or_null("AnimatedSprite2D") as AnimatedSprite2D


func _process(delta: float) -> void:
	if _snake == null:
		return
	var player := get_tree().get_first_node_in_group("Player") as Node2D
	if player == null or player.global_position.x < snake_wake_x:
		if _snake.animation != &"idle":
			_snake.play(&"idle")
		return
	_snake_clock = fposmod(_snake_clock + delta, 4.0)
	var next: StringName = &"idle"
	if _snake_clock >= 1.4 and _snake_clock < 2.1:
		next = &"windup"
	elif _snake_clock >= 2.1 and _snake_clock < 3.1:
		next = &"attack"
	if _snake.animation != next:
		_snake.play(next)
		if next == &"attack":
			var audio := get_node_or_null("/root/AudioManager")
			if audio != null:
				audio.call("PlaySfx", "snake_hiss.wav", -7.0, 0.82)
