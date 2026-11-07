extends SceneTree
## Production-scene visual review. Run with a renderer and --fixed-fps 60;
## preserve the save externally. Fixed steps keep screenshot I/O out of move timing.
const OUT := "res://docs/art-review/"

func _initialize() -> void:
	_run.call_deferred()

func _shot(name: String) -> void:
	name = name.replace("-v2", "-v3")
	await process_frame
	await RenderingServer.frame_post_draw
	var folder := "user://visual-review-frames/" if name.begins_with("combat-") else OUT
	DirAccess.make_dir_recursive_absolute(folder)
	root.get_texture().get_image().save_png(folder + name + ".png")

func _run() -> void:
	change_scene_to_file("res://scenes/areas/Deserto.tscn")
	await scene_changed
	await create_timer(.2).timeout
	var player := get_first_node_in_group("Player") as CharacterBody2D
	player.set_physics_process(false)
	player.get_node("Health").call("SetInvulnerable", true)
	var camera := player.get_node("Camera2D") as Camera2D
	camera.position_smoothing_enabled = false
	var overlay := get_first_node_in_group("TutorialOverlay")
	overlay.call("ShowIntro")
	await create_timer(.2).timeout
	await _shot("tutorial-keyboard-v2")
	root.get_node("GameManager").call("SetInputHintsForTest", true, 1)
	await create_timer(.2).timeout
	await _shot("tutorial-controller-v2")
	root.size = Vector2i(960, 540)
	await create_timer(.2).timeout
	await _shot("tutorial-controller-960-v2")
	root.get_node("GameManager").call("SetInputHintsForTest", false, 0)
	await create_timer(.2).timeout
	await _shot("tutorial-keyboard-960-v2")
	root.size = Vector2i(1280, 720)
	overlay.call("CloseIntro")
	player.global_position = Vector2(3160, 453)
	camera.reset_smoothing()
	var boss := current_scene.get_node("Boss_Serpente")
	boss.get_node("BossStateMachine").set_process(false)
	boss.get_node("BossStateMachine").set_physics_process(false)
	boss.global_position = Vector2(3500, 475)
	await create_timer(.2).timeout
	overlay.call("ShowBanner", "review", "{jump}", "Pule para evitar a varrida da cauda.")
	await create_timer(.3).timeout
	await _shot("desert-gate-banner-v2")
	if "--ui-only" in OS.get_cmdline_user_args():
		quit()
		return
	overlay.call("HideBanner", "review")
	await create_timer(1.7).timeout
	await _shot("desert-ground-contact-v3")
	for move in ["StartStrike", "StartTail", "StartBurrow"]:
		boss.call(move)
		var count := 16 if move != "StartBurrow" else 30
		for i in range(count):
			await create_timer(.08).timeout
			await _shot("combat-%s-%02d" % [move, i])
	print("VISUAL_PENDING_CAPTURE_DONE")
	quit()
