extends SceneTree
## Validates the production Korrag package without starting a save or encounter.

const REQUIRED := {
	&"idle": 2,
	&"walk": 2,
	&"intro": 2,
	&"charge_windup": 2,
	&"charge": 1,
	&"stomp": 2,
	&"attack": 3,
	&"hurt": 1,
	&"phase1_idle": 2,
	&"phase2_transition": 3,
	&"enraged_transition": 2,
	&"dead": 3,
	&"defeated": 1,
}

var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var frames: SpriteFrames = load("res://assets/sprites/korrag/KorragSpriteFrames.tres")
	_check(frames != null, "Korrag SpriteFrames loads")
	if frames != null:
		for animation: StringName in REQUIRED:
			_check(frames.has_animation(animation), "%s exists" % animation)
			if frames.has_animation(animation):
				_check(frames.get_frame_count(animation) >= REQUIRED[animation],
					"%s has at least %d frame(s)" % [animation, REQUIRED[animation]])
		var windup_frame := frames.get_frame_texture(&"charge_windup", 1) as AtlasTexture
		_check(windup_frame != null and windup_frame.region.position == Vector2(0, 280),
			"charge windup excludes the frame with the detached rear leg")
		var stomp_frame := frames.get_frame_texture(&"stomp", 0) as AtlasTexture
		_check(stomp_frame != null and stomp_frame.region.position == Vector2(0, 280),
			"stomp also excludes the defective rear-leg frame")
	var boss: Node2D = load("res://scenes/bosses/Boss_Javali.tscn").instantiate()
	root.add_child(boss)
	await process_frame
	var sprite: AnimatedSprite2D = boss.get_node("AnimatedSprite2D")
	_check(sprite.scale.is_equal_approx(Vector2(0.9, 0.9)), "boss scale reads clearly against Kairo")
	_check(sprite.position.y == -100.0, "boss feet align with the collision baseline")
	var behavior := FileAccess.get_file_as_string("res://scripts/bosses/JavaliBoss.cs")
	_check("PlayAnimation(\"charge\")" in behavior, "charge uses its own animation")
	_check("PlayAnimation(\"stomp\")" in behavior, "stomp uses its own animation")
	print("KORRAG_ANIMATION_DONE failures=%d" % failures)
	quit(failures)

func _check(condition: bool, description: String) -> void:
	if condition:
		print("KORRAG_ANIMATION_PASS: " + description)
	else:
		failures += 1
		push_error("KORRAG_ANIMATION_FAIL: " + description)
