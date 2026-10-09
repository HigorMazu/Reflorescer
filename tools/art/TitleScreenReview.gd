extends SceneTree
## Captures the production menu without invoking save-changing actions.

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var menu: Control = load("res://scenes/ui/MainMenu.tscn").instantiate()
	root.add_child(menu)
	if "--small" in OS.get_cmdline_user_args():
		root.size = Vector2i(960, 540)
	await create_timer(1.7).timeout
	var presentation := menu.get_node("Presentation")
	var start: float = presentation.elapsed
	await create_timer(0.25).timeout
	assert(presentation.elapsed > start, "Atmosphere clock must advance")
	assert(menu.get_node("Box/NewGameButton").visible, "New game must be visible")
	assert(menu.get_node("Box/QuitButton").visible, "Quit must be visible")
	assert(menu.get_node("Box/ContinueButton").visible == root.get_node("SaveManager").call("HasSave", 0))
	assert(root.gui_get_focus_owner() is Button, "Keyboard focus must start on an available action")
	if DisplayServer.get_name() != "headless":
		DirAccess.make_dir_recursive_absolute("res://.godot/title-review")
		for frame in range(24):
			await create_timer(0.1).timeout
			await RenderingServer.frame_post_draw
			var suffix := "small-" if "--small" in OS.get_cmdline_user_args() else ""
			root.get_texture().get_image().save_png("res://.godot/title-review/%s%02d.png" % [suffix, frame])
	menu.get_node("Box/QuitButton").grab_focus()
	await process_frame
	assert(root.gui_get_focus_owner() == menu.get_node("Box/QuitButton"))
	print("TITLE_SCREEN_PASS: animation, save-dependent continue, keyboard focus and layout loaded")
	quit()
