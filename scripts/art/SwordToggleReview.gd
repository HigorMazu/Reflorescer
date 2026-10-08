extends Node2D
## Isolated production sprites, no checkpoint or save operations.
func _ready() -> void:
	RenderingServer.set_default_clear_color(Color("182c25"))
	var player: Node2D = load("res://scenes/player/Player.tscn").instantiate()
	player.process_mode = Node.PROCESS_MODE_DISABLED
	player.position = Vector2(220, 420)
	player.scale = Vector2(4, 4)
	add_child(player)
	player.get_node("Camera2D").enabled = false
	var blade: Node = player.get_node("Visual/RightArm/RightHand/Sword")
	blade.process_mode = Node.PROCESS_MODE_ALWAYS
	var body: AnimatedSprite2D = player.get_node("Visual/AnimatedSprite2D")
	body.stop()
	body.frame = 0
	var label := Label.new()
	label.position = Vector2(80, 520)
	label.add_theme_font_size_override("font_size", 24)
	add_child(label)
	for i in range(3):
		var enemy: Node2D = load("res://scenes/enemies/Enemy_" + ["Rapido", "Robusto", "Voador"][i] + ".tscn").instantiate()
		enemy.position = Vector2(580 + 220 * i, 500)
		enemy.scale = Vector2(4, 4)
		enemy.process_mode = Node.PROCESS_MODE_DISABLED
		add_child(enemy)
	var caption := Label.new()
	caption.position = Vector2(570, 530)
	caption.text = "Todos 4× · mobs +12% adicionais"
	caption.add_theme_font_size_override("font_size", 24)
	add_child(caption)
	DirAccess.make_dir_recursive_absolute("res://.godot/sword-toggle-frames")
	if "--poses" in OS.get_cmdline_user_args():
		player.call("SetSwordVisible", true)
		for pose in ["idle", "run", "jump", "fall", "attack"]:
			for frame in range(body.sprite_frames.get_frame_count(pose)):
				body.animation = pose
				body.frame = frame
				label.text = pose + " / " + str(frame)
				await get_tree().create_timer(0.1).timeout
				await RenderingServer.frame_post_draw
				get_viewport().get_texture().get_image().save_png("res://.godot/sword-toggle-frames/pose-" + pose + str(frame) + ".png")
		get_tree().quit()
		return
	for i in range(48):
		if i == 0: player.call("SetSwordVisible", true)
		if i == 10 or i == 28: player.call("ToggleSword")
		label.text = "Kairo · detalhe 4× · " + ("ATIVA" if player.get("HasSword") else "GUARDADA")
		await get_tree().create_timer(1.0 / 24.0).timeout
		await RenderingServer.frame_post_draw
		get_viewport().get_texture().get_image().save_png("res://.godot/sword-toggle-frames/%02d.png" % i)
	get_tree().quit()
