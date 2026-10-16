extends SceneTree
## C11-T4: cada tela/área da demo deve iniciar sua faixa e não alterar o save pessoal.

const SCENES := [
	["res://scenes/ui/MainMenu.tscn", "title_autoral.wav"],
	["res://scenes/areas/SopeDaMata.tscn", "sope_autoral.wav"],
	["res://scenes/areas/DosselVivo.tscn", "dossel_autoral.wav"],
	["res://scenes/areas/IgarapeSufocado.tscn", "igarape_autoral.wav"],
	["res://scenes/areas/CovilDeKorrag.tscn", "covil_autoral.wav"],
]
const SAVE_FILE := "user://save_0.json"

var _had_save := false
var _save_backup := ""
var _failures := 0

func _initialize() -> void:
	_run.call_deferred()

func _backup_save() -> void:
	_had_save = FileAccess.file_exists(SAVE_FILE)
	if _had_save:
		_save_backup = FileAccess.get_file_as_string(SAVE_FILE)

func _restore_save() -> void:
	if _had_save:
		var file := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		file.store_string(_save_backup)
		file.close()
	elif FileAccess.file_exists(SAVE_FILE):
		DirAccess.remove_absolute(SAVE_FILE)

func _check(ok: bool, label: String) -> void:
	print("  ", "ok   " if ok else "FALHA", " ", label)
	if not ok:
		_failures += 1

func _run() -> void:
	_backup_save()
	for pair in SCENES:
		var path: String = pair[0]
		var expected: String = pair[1]
		_check(ResourceLoader.exists("res://assets/audio/music/" + expected), "%s existe" % expected)
		change_scene_to_file(path)
		await scene_changed
		# A troca entre áreas tem fade de 0,5 s antes da nova faixa começar.
		await create_timer(0.8).timeout
		var player := root.get_node("AudioManager/MusicPlayer") as AudioStreamPlayer
		_check(player != null and player.playing and player.stream != null and player.stream.resource_path.ends_with(expected),
			"%s toca %s" % [path.get_file(), expected])
	var manager := root.get_node("AudioManager")
	var sfx_paths := ["player_attack.wav", "enemy_hit.wav", "boss_stomp.wav"]
	# AudioCheck já chama o AudioManager pelo caminho real. Aqui ocupamos três players
	# reais do pool para conferir que o bus Music continua vivo sob sobreposição.
	for i in sfx_paths.size():
		var sfx_player := manager.get_child(i + 1) as AudioStreamPlayer
		sfx_player.stream = load("res://assets/audio/sfx/" + sfx_paths[i])
		sfx_player.play()
	await process_frame
	var active_sfx := 0
	for child in manager.get_children():
		if child is AudioStreamPlayer and child.name != "MusicPlayer" and child.playing:
			active_sfx += 1
	var music := manager.get_node("MusicPlayer") as AudioStreamPlayer
	_check(music.playing and active_sfx >= 3, "música e três SFX sobrepõem sem corte")
	_restore_save()
	print("MUSIC_RESULT: %d faixa(s), %d falha(s)" % [SCENES.size(), _failures])
	quit(1 if _failures > 0 else 0)
