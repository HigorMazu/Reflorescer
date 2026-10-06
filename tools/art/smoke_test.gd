extends Node
## Run only in the isolated QA copy, never against a player's real save folder.
var failures: int = 0

func check(condition: bool, message: String) -> void:
	if condition:
		print("ART_QA_PASS: " + message)
	else:
		failures += 1
		push_error("ART_QA_FAIL: " + message)

func wait(seconds: float) -> void:
	await get_tree().create_timer(seconds).timeout

func capture(filename: String) -> void:
	await RenderingServer.frame_post_draw
	get_viewport().get_texture().get_image().save_png("res://" + filename)

func _ready() -> void:
	if ProjectSettings.get_setting("application/config/custom_user_dir_name", "") != "ReflorescerArtQA_20261005":
		push_error("Art QA refuses to use a non-isolated save directory.")
		get_tree().quit(2)
		return
	await wait(0.8)
	var level := $TestLevel
	var player := level.get_node("Player")
	var sprite := player.get_node("Visual/AnimatedSprite2D") as AnimatedSprite2D
	var companion := level.get_node("Faisca")
	var companion_sprite := companion.get_node("AnimatedSprite2D") as AnimatedSprite2D
	check(get_tree().get_nodes_in_group("Companion").size() == 1, "one independent companion")
	for animation_name in ["idle", "run", "jump", "fall", "attack", "hurt", "dead"]:
		check(sprite.sprite_frames.has_animation(animation_name), "Kairo " + animation_name)
	check(player.is_on_floor(), "player lands on unchanged floor")
	await capture("game-before.png")
	Input.action_press("move_right")
	await wait(0.25)
	check(sprite.animation == &"run", "movement plays run")
	Input.action_release("move_right")
	Input.action_press("move_left")
	await wait(0.15)
	check(player.get_node("Visual").scale.x < 0, "Kairo flips via Visual scale")
	check(companion.get_parent() == level, "companion stays outside Visual hierarchy")
	Input.action_release("move_left")
	Input.action_press("jump")
	await wait(0.1)
	check(sprite.animation == &"jump", "jump plays jump")
	Input.action_release("jump")
	await wait(0.8)
	Input.action_press("attack")
	await wait(0.05)
	check(sprite.animation == &"attack", "attack plays attack")
	Input.action_release("attack")
	await wait(0.4)
	var machine := companion.get_node("FaiscaStateMachine")
	# C# enum: Follow=0, Idle=1, Investigate=2, ReturnToPlayer=4.
	for test in [[1, "idle"], [2, "investigate"], [4, "fly"]]:
		machine.call("ChangeState", test[0])
		await wait(0.12)
		check(companion_sprite.animation == StringName(test[1]), "Faísca state " + test[1])
		check(companion_sprite.is_playing(), "Faísca animation advances")
	machine.call("ChangeState", 0)
	player.global_position = Vector2(120, 460)
	player.velocity = Vector2.ZERO
	await wait(0.3)
	Input.action_press("interact")
	await wait(0.1)
	Input.action_release("interact")
	await wait(0.7)
	var point := level.get_node("RestorationPoint1")
	check(bool(point.get("Restored")), "E restores the existing point")
	check(not point.get_node("DegradedGround").visible, "degraded art hidden")
	check(point.get_node("RestoredGround").visible, "restored art visible")
	check(point.get_node("Sprout").scale.is_equal_approx(Vector2.ONE), "growth tween finishes at unit scale")
	check(companion.get_node("PointLight2D").texture != null, "independent light has a texture")
	await capture("game-restored.png")
	print("ART_QA_DONE failures=" + str(failures))
	get_tree().quit(failures)
