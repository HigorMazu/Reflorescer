extends SceneTree
## Run only in a fixture project with a separate application/save directory.
const MENU := "res://scenes/ui/MainMenu.tscn"
const GAME := "res://scenes/areas/SopeDaMata.tscn"
func _initialize() -> void:
	_run.call_deferred()
func _run() -> void:
	if ProjectSettings.get_setting("application/config/name") != "ReflorescerTitleQA" or not OS.get_user_data_dir().contains("ReflorescerTitleQA"):
		push_error("TitleFlowCheck requires an isolated ReflorescerTitleQA project/save directory")
		quit(2)
		return
	create_timer(15.0).timeout.connect(func(): quit(1))
	var saves := root.get_node("SaveManager")
	var manager := root.get_node("SceneManager")
	var args := OS.get_cmdline_user_args()
	if "--new-game" in args: saves.call("DeleteSave", 0)
	manager.call("LoadScene", MENU)
	await scene_changed
	await create_timer(0.2).timeout
	if "--new-game" in args:
		assert(not current_scene.get_node("Box/ContinueButton").visible)
		var button := current_scene.get_node("Box/NewGameButton")
		button.emit_signal("pressed")
		button.emit_signal("pressed")
		assert(button.disabled)
		await scene_changed
		await process_frame
		assert(current_scene.scene_file_path == GAME)
		saves.call("SaveGame", 0)
		assert(saves.call("HasSave", 0))
		print("TITLE_FLOW_PASS: new game and double-activation guard")
		quit()
	elif "--continue" in args:
		assert(current_scene.get_node("Box/ContinueButton").visible)
		current_scene.get_node("Box/ContinueButton").emit_signal("pressed")
		await scene_changed
		await create_timer(0.2).timeout
		assert(current_scene.scene_file_path == GAME)
		print("TITLE_FLOW_PASS: continue loads the saved scene")
		quit()
	elif "--missing-save" in args:
		assert(current_scene.get_node("Box/ContinueButton").visible)
		saves.call("DeleteSave", 0)
		current_scene.get_node("Box/ContinueButton").emit_signal("pressed")
		await create_timer(0.9).timeout
		assert(current_scene.scene_file_path == MENU)
		assert(not current_scene.get_node("Box/NewGameButton").disabled)
		assert(current_scene.get_node("Curtain").color.a < 0.01)
		print("TITLE_FLOW_PASS: missing-save recovery; exiting through Sair")
		current_scene.get_node("Box/QuitButton").emit_signal("pressed")
	else:
		quit(2)
