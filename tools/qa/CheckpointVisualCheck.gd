extends SceneTree
## C09-T3: verifies the production checkpoint's two visual states and saved state.
## The user's save_0.json is restored before this script exits.

const MENU := "res://scenes/ui/MainMenu.tscn"
const DOSSEL := "res://scenes/areas/DosselVivo.tscn"
const SAVE_FILE := "user://save_0.json"

var _backup: String
var _had_backup := false
var _failures := 0

func _initialize() -> void:
	_run.call_deferred()

func _check(ok: bool, description: String) -> void:
	if ok:
		print("CHECKPOINT_VISUAL_PASS: ", description)
	else:
		_failures += 1
		push_error("CHECKPOINT_VISUAL_FAIL: " + description)

func _frames(count: int) -> void:
	for _i in count:
		await physics_frame

func _restore_user_save() -> void:
	if _had_backup:
		var file := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		file.store_string(_backup)
		file.close()
	else:
		DirAccess.remove_absolute(SAVE_FILE)

func _run() -> void:
	_had_backup = FileAccess.file_exists(SAVE_FILE)
	if _had_backup:
		_backup = FileAccess.get_file_as_string(SAVE_FILE)

	var manager := root.get_node("SceneManager")
	var saves := root.get_node("SaveManager")
	manager.call("LoadScene", MENU)
	await scene_changed
	await _frames(3)
	current_scene.get_node("Box/NewGameButton").emit_signal("pressed")
	await scene_changed
	await _frames(3)
	manager.call("LoadScene", DOSSEL)
	await scene_changed
	await _frames(10)

	var checkpoint := current_scene.get_node("Interactables/Checkpoint1")
	var totem := checkpoint.get_node("TotemVisual")
	_check(not checkpoint.get("Activated"), "starts inactive in a new game")
	_check(not totem.get("Activated"), "totem starts visually dormant")

	var player := get_first_node_in_group("Player") as CharacterBody2D
	player.global_position = checkpoint.global_position
	await _frames(10)
	_check(checkpoint.get("Activated"), "touch activates checkpoint")
	_check(totem.get("Activated"), "touch turns on totem visual")
	_check(saves.call("IsCheckpointActivated", "cp_dossel_01"), "activation entered the save state")

	saves.call("SaveGame", 0)
	manager.call("LoadScene", MENU)
	await scene_changed
	await _frames(3)
	saves.call("ContinueGame", 0)
	await scene_changed
	await _frames(10)
	checkpoint = current_scene.get_node("Interactables/Checkpoint1")
	totem = checkpoint.get_node("TotemVisual")
	_check(checkpoint.get("Activated"), "Continue restores activated checkpoint")
	_check(totem.get("Activated"), "Continue restores activated totem visual")

	current_scene.queue_free()
	await process_frame
	_restore_user_save()
	print("CHECKPOINT_VISUAL_RESULT: %d failure(s)" % _failures)
	quit(1 if _failures > 0 else 0)
