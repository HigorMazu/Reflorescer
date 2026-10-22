extends SceneTree
## Checks the isolated desert visual review without touching the demo or saves.

var failures := 0


func _initialize() -> void:
	_run.call_deferred()


func _check(condition: bool, message: String) -> void:
	if not condition:
		failures += 1
	print("DESERT_QA_%s: %s" % ["PASS" if condition else "FAIL", message])


func _run() -> void:
	var review := load("res://scenes/art/DesertReview.tscn").instantiate() as Node2D
	root.add_child(review)
	await process_frame
	var sky := review.get_node("DesertSkyCycle")
	var player := review.get_node("Player") as Node2D
	var dusk := sky.get_node("Dusk") as Sprite2D
	var night := sky.get_node("Night") as Sprite2D
	_check(is_zero_approx(dusk.modulate.a) and is_zero_approx(night.modulate.a), "entry shows sunlit day")
	player.global_position.x = 1575.0
	await process_frame
	_check(dusk.modulate.a > .95 and night.modulate.a < .05, "middle reaches late afternoon")
	player.global_position.x = 3000.0
	await process_frame
	_check(dusk.modulate.a > .95 and night.modulate.a > .95, "boss end reaches night")
	var light := review.get_node("DesertLighting") as CanvasModulate
	_check(light.color.b > light.color.r, "night cools the ground and characters")
	var snake := review.get_node("Boss_CobraDesertoVisual/AnimatedSprite2D") as AnimatedSprite2D
	var frames := snake.sprite_frames
	for state in [&"idle", &"slither", &"windup", &"attack", &"tail_sweep",
		&"burrow_enter", &"burrow_exit", &"hurt", &"dead"]:
		_check(frames.has_animation(state), "snake %s exists" % state)
		for index in frames.get_frame_count(state):
			_check(frames.get_frame_texture(state, index).get_size() == Vector2(512, 320),
				"snake %s/%d canvas" % [state, index])
	# Drive the visual clock directly so this remains deterministic at any test frame rate.
	review.set("snake_clock", 2.1)
	await process_frame
	_check(snake.animation == &"attack" and frames.get_frame_count(&"attack") > 1,
		"night boss preview enters animated strike")
	review.set("snake_clock", 3.6)
	await process_frame
	_check(snake.animation == &"tail_sweep", "night boss preview shows the tail sweep")
	review.set("snake_clock", 5.0)
	await process_frame
	_check(snake.animation == &"burrow_enter", "night boss preview enters the sand")
	review.set("snake_clock", 6.25)
	await process_frame
	_check(snake.animation == &"burrow_exit", "night boss preview erupts from the sand")
	_check(review.get_node("DesertFloor") is StaticBody2D, "desert visual aligns to solid ground")
	_check(review.get_node("DesertWind") is AudioStreamPlayer and
		(review.get_node("DesertWind") as AudioStreamPlayer).playing, "wind ambience starts")
	_check(ResourceLoader.exists("res://assets/audio/sfx/snake_hiss.wav"), "snake hiss resource exists")
	_check(review.get_node("Enemy_AbelhaAfricanizada") != null and review.get_node("Enemy_RatoPreto") != null
		and review.get_node("Enemy_BurroSelvagem") != null, "three desert reskins are present")
	for asset in ["desert_checkpoint_off_v1.png", "desert_checkpoint_on_v1.png",
		"desert_portal_v1.png", "desert_portal_forest_v1.png"]:
		_check(ResourceLoader.exists("res://assets/environment/desert/" + asset), "%s exists" % asset)
	var module_names := ["platform_long", "ledge_short", "wall_cap", "rock_pillar",
		"ruin_arch", "buried_column", "rock_cluster", "dry_shrub"]
	for module_name in module_names:
		_check(ResourceLoader.exists("res://assets/environment/desert/modules/desert_%s_v1.png" % module_name),
			"desert module %s exists" % module_name)
	var area_backgrounds := ["portal_sol", "garganta_ruinas", "oasis_soterrado", "ninho_serpente"]
	for background_name in area_backgrounds:
		var background_path := "res://assets/environment/desert/areas/desert_%s_v1.png" % background_name
		_check(ResourceLoader.exists(background_path), "desert background %s exists" % background_name)
		var background_texture := load(background_path) as Texture2D
		if background_texture != null:
			_check(background_texture.get_size() == Vector2(1672, 941),
				"desert background %s uses the production canvas" % background_name)
	var quicksand := load("res://assets/environment/desert/hazards/DesertQuicksandSpriteFrames.tres") as SpriteFrames
	_check(quicksand != null and quicksand.has_animation(&"bubble"), "quicksand bubble loop exists")
	if quicksand != null and quicksand.has_animation(&"bubble"):
		_check(quicksand.get_frame_count(&"bubble") == 4, "quicksand loop has four stages")
		_check(quicksand.get_animation_speed(&"bubble") == 4.0 and quicksand.get_animation_loop(&"bubble"),
			"quicksand bubbles continuously at the reviewed speed")
		for index in quicksand.get_frame_count(&"bubble"):
			_check(quicksand.get_frame_texture(&"bubble", index).get_size() == Vector2(320, 128),
				"quicksand frame %d uses a stable canvas" % index)
	for index in 4:
		var restoration_path := "res://assets/environment/desert/hazards/desert_restoration_%02d.png" % index
		_check(ResourceLoader.exists(restoration_path), "restoration stage %d exists" % index)
		var restoration_texture := load(restoration_path) as Texture2D
		if restoration_texture != null:
			_check(restoration_texture.get_size() == Vector2(256, 160),
				"restoration stage %d uses a stable canvas" % index)
	var area: Node = load("res://scenes/areas/Deserto.tscn").instantiate()
	var desert_totem: Node = area.get_node("Interactables/CheckpointDeserto/TotemVisual")
	_check(desert_totem.get("Theme") == 1, "production checkpoint uses the desert visual")
	_check(not (area.get_node("Transitions/ToCovilDeKorrag/PortalVisual") is ColorRect),
		"production desert portal is no longer a debug rectangle")
	area.free()
	var den: Node = load("res://scenes/areas/CovilDeKorrag.tscn").instantiate()
	var forest_portal: Node = den.get_node("Transitions/ToDeserto/PortalVisual")
	_check(not (forest_portal is ColorRect) and forest_portal.get("ForestSide"),
		"forest side of the desert passage uses its rooted variant")
	den.free()
	print("DESERT_QA_DONE failures=%d" % failures)
	review.queue_free()
	var audio_manager := root.get_node_or_null("AudioManager")
	if audio_manager != null:
		for child in audio_manager.get_children():
			if child is AudioStreamPlayer:
				child.stop()
				child.stream = null
	await process_frame
	quit(0 if failures == 0 else 1)
