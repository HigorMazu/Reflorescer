extends Node2D
## Isolated playable art review; does not alter the demo route or save file.

const SKY: PackedScene = preload("res://scenes/art/DesertSkyCycle.tscn")
const LIGHTING: Script = preload("res://scripts/art/DesertLighting.gd")
const GROUND: Texture2D = preload("res://assets/environment/desert/desert_ground_tile_v1.png")
const PLAYER: PackedScene = preload("res://scenes/player/Player.tscn")
const BEE: PackedScene = preload("res://scenes/enemies/Enemy_AbelhaAfricanizada.tscn")
const RAT: PackedScene = preload("res://scenes/enemies/Enemy_RatoPreto.tscn")
const DONKEY: PackedScene = preload("res://scenes/enemies/Enemy_BurroSelvagem.tscn")
const SNAKE: PackedScene = preload("res://scenes/bosses/Boss_CobraDesertoVisual.tscn")
const WIND: AudioStream = preload("res://assets/audio/ambience/desert_wind.wav")

const WORLD_WIDTH := 3840.0
const FLOOR_Y := 475.0

var player: Node2D
var snake_sprite: AnimatedSprite2D
var snake_clock := 0.0
var wind_player: AudioStreamPlayer


func _ready() -> void:
	var sky := SKY.instantiate()
	sky.set("progress_start_x", 150.0)
	sky.set("progress_end_x", 3000.0)
	add_child(sky)
	var light := CanvasModulate.new()
	light.name = "DesertLighting"
	light.set_script(LIGHTING)
	add_child(light)
	_make_ground()
	_make_wall(-20.0)
	_make_wall(WORLD_WIDTH + 20.0)
	player = PLAYER.instantiate()
	player.position = Vector2(160, FLOOR_Y - 22)
	add_child(player)
	var camera := player.get_node("Camera2D") as Camera2D
	camera.limit_left = 0
	camera.limit_top = 0
	camera.limit_right = int(WORLD_WIDTH)
	camera.limit_bottom = 720
	_add_enemy(BEE, Vector2(850, FLOOR_Y - 90))
	_add_enemy(RAT, Vector2(1640, FLOOR_Y))
	_add_enemy(DONKEY, Vector2(2450, FLOOR_Y))
	var snake := SNAKE.instantiate() as Node2D
	snake.position = Vector2(3430, FLOOR_Y)
	add_child(snake)
	snake_sprite = snake.get_node("AnimatedSprite2D") as AnimatedSprite2D
	wind_player = AudioStreamPlayer.new()
	wind_player.name = "DesertWind"
	wind_player.stream = WIND
	wind_player.bus = "Music"
	wind_player.volume_db = -17.0
	wind_player.finished.connect(func() -> void: wind_player.play())
	add_child(wind_player)
	wind_player.play()


func _make_ground() -> void:
	var floor_body := StaticBody2D.new()
	floor_body.name = "DesertFloor"
	floor_body.collision_layer = 1
	floor_body.position = Vector2(WORLD_WIDTH * 0.5, FLOOR_Y + 40)
	var shape := CollisionShape2D.new()
	var rect := RectangleShape2D.new()
	rect.size = Vector2(WORLD_WIDTH, 80)
	shape.shape = rect
	floor_body.add_child(shape)
	add_child(floor_body)
	for index in range(4):
		var tile := Sprite2D.new()
		tile.name = "Sandstone%02d" % index
		tile.texture = GROUND
		tile.centered = false
		tile.position = Vector2(index * 1024, FLOOR_Y - 10)
		tile.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
		tile.z_index = -1
		add_child(tile)


func _make_wall(x: float) -> void:
	var wall := StaticBody2D.new()
	wall.name = "WorldWall"
	wall.collision_layer = 1
	wall.position = Vector2(x, 360)
	var shape := CollisionShape2D.new()
	var rect := RectangleShape2D.new()
	rect.size = Vector2(40, 720)
	shape.shape = rect
	wall.add_child(shape)
	add_child(wall)


func _add_enemy(scene: PackedScene, where: Vector2) -> void:
	var enemy := scene.instantiate() as Node2D
	enemy.position = where
	add_child(enemy)


func _process(delta: float) -> void:
	if snake_sprite == null or player == null:
		return
	if player.global_position.x < 3000.0:
		if snake_sprite.animation != &"idle":
			snake_sprite.play(&"idle")
		return
	snake_clock = fposmod(snake_clock + delta, 4.0)
	var next_animation: StringName = &"idle"
	if snake_clock >= 1.4 and snake_clock < 2.1:
		next_animation = &"windup"
	elif snake_clock >= 2.1 and snake_clock < 3.1:
		next_animation = &"attack"
	if snake_sprite.animation != next_animation:
		snake_sprite.play(next_animation)
		if next_animation == &"attack":
			var audio := get_node_or_null("/root/AudioManager")
			if audio != null:
				audio.call("PlaySfx", "snake_hiss.wav", -7.0, 0.82)
