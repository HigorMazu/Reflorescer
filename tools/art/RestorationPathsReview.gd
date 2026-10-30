extends SceneTree
## Visual-only placement review; does not alter gameplay, collision or saves.

const OUT := "res://docs/art-review/"
const ROOT_BRIDGE := preload("res://assets/environment/tropical/restoration_root_bridge_v1.png")
const VINE_STEPS := preload("res://assets/environment/tropical/restoration_vine_steps_v1.png")
const LILY_BRIDGE := preload("res://assets/environment/tropical/restoration_lily_bridge_v1.png")


func _initialize() -> void:
	_run.call_deferred()


func _shot(scene_path: String, player_at: Vector2, texture: Texture2D,
		at: Vector2, size: Vector2, name: String, angle: float = 0.0) -> void:
	change_scene_to_file(scene_path)
	await scene_changed
	var player := get_first_node_in_group("Player") as CharacterBody2D
	player.set_physics_process(false)
	player.global_position = player_at
	var camera := player.get_node("Camera2D") as Camera2D
	camera.position_smoothing_enabled = false
	camera.reset_smoothing()
	var sprite := Sprite2D.new()
	sprite.name = "RestoredPathPreview"
	sprite.texture = texture
	sprite.centered = false
	sprite.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	sprite.position = at
	sprite.scale = size / Vector2(texture.get_size())
	sprite.rotation = angle
	sprite.z_index = 1
	current_scene.add_child(sprite)
	await create_timer(.4).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png(OUT + name)


func _run() -> void:
	await _shot("res://scenes/areas/SopeDaMata.tscn", Vector2(1150, 460),
		ROOT_BRIDGE, Vector2(1260, 415), Vector2(180, 60),
		"restoration-path-sope-preview-v2.png", 0.18)
	await _shot("res://scenes/areas/DosselVivo.tscn", Vector2(720, 220),
		VINE_STEPS, Vector2(650, 0), Vector2(170, 255),
		"restoration-path-dossel-preview-v1.png")
	await _shot("res://scenes/areas/IgarapeSufocado.tscn", Vector2(1260, 460),
		LILY_BRIDGE, Vector2(1420, 352), Vector2(540, 180),
		"restoration-path-igarape-preview-v2.png")
	print("RESTORATION_PATHS_REVIEW_DONE")
	quit()
