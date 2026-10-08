extends SceneTree
## Verifies movement poses use Kairo's production SpriteFrames.

var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var player: Node2D = load("res://scenes/player/Player.tscn").instantiate()
	root.add_child(player)
	await process_frame
	var base := player.get_node_or_null("Visual/AnimatedSprite2D") as AnimatedSprite2D
	_check(base != null and base.sprite_frames.has_animation(&"wall_slide"),
		"wall slide is part of Kairo's production SpriteFrames")
	_check(base != null and base.sprite_frames.has_animation(&"dash"),
		"dash is part of Kairo's production SpriteFrames")
	_check(player.get_node_or_null("Visual/MovementPoseOverlay") == null,
		"mismatched generated overlay was removed")
	_check(player.get_node_or_null("MovementStateEffects") != null,
		"production player includes readable dash and wall-grip accents")
	_check(player.get("IsWallSliding") == false, "PlayerController exposes the real wall-slide state")
	print("MOVEMENT_POSE_DONE failures=%d" % failures)
	quit(failures)

func _check(condition: bool, description: String) -> void:
	if condition:
		print("MOVEMENT_POSE_PASS: " + description)
	else:
		failures += 1
		push_error("MOVEMENT_POSE_FAIL: " + description)
