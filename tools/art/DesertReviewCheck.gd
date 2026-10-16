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
	for state in [&"idle", &"slither", &"windup", &"attack", &"hurt", &"dead"]:
		_check(frames.has_animation(state), "snake %s exists" % state)
		for index in frames.get_frame_count(state):
			_check(frames.get_frame_texture(state, index).get_size() == Vector2(512, 320),
				"snake %s/%d canvas" % [state, index])
	await create_timer(2.25).timeout
	_check(snake.animation == &"attack" and snake.frame > 0, "night boss preview enters animated strike")
	_check(review.get_node("DesertFloor") is StaticBody2D, "desert visual aligns to solid ground")
	_check(review.get_node("DesertWind") is AudioStreamPlayer and
		(review.get_node("DesertWind") as AudioStreamPlayer).playing, "wind ambience starts")
	_check(ResourceLoader.exists("res://assets/audio/sfx/snake_hiss.wav"), "snake hiss resource exists")
	_check(review.get_node("Enemy_AbelhaAfricanizada") != null and review.get_node("Enemy_RatoPreto") != null
		and review.get_node("Enemy_BurroSelvagem") != null, "three desert reskins are present")
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
