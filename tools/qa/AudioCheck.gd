extends SceneTree
## C11-T2: dispara cada um dos 12 SFX pelo caminho real do jogo e confere que o AudioManager tocou.
## Kairo: atacar, pular, dash, Q (guardar/sacar), tomar dano. Inimigo: acertar e matar.
## Mundo: restaurar (Sopé) e passar pela bandeira (Dossel). Korrag: pisada e derrota.
## Também falha se o AudioManager logar algum SFX faltando. O save_0.json é guardado e devolvido.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/AudioCheck.gd
const SOPE := "res://scenes/areas/SopeDaMata.tscn"
const DOSSEL := "res://scenes/areas/DosselVivo.tscn"
const COVIL := "res://scenes/areas/CovilDeKorrag.tscn"
const EXPECTED := ["player_attack.wav", "player_jump.wav", "player_dash.wav", "player_hurt.wav", "sword_on.wav", "sword_off.wav",
	"enemy_hit.wav", "enemy_death.wav", "checkpoint_activate.wav", "restoration.wav", "boss_stomp.wav", "boss_defeat.wav"]
const SAVE_FILE := "user://save_0.json"

var _heard := {}
var _audio: Node
var _probe: Node
var _backup = null

func _initialize() -> void:
	_run.call_deferred()

func _listen() -> void:
	for c in _audio.get_children():
		if c is AudioStreamPlayer and c.playing and c.stream:
			_heard[c.stream.resource_path.get_file()] = true

func _frames(n: int) -> void:
	for i in n:
		await physics_frame

func _tap(action: String) -> void:
	Input.action_press(action)
	await physics_frame
	Input.action_release(action)

func _run() -> void:
	if FileAccess.file_exists(SAVE_FILE):
		_backup = FileAccess.get_file_as_string(SAVE_FILE)
	_audio = root.get_node("AudioManager")
	process_frame.connect(_listen)
	_probe = load("res://tools/qa/QaProbe.cs").new()
	root.add_child(_probe)
	for f in EXPECTED:
		print("  arquivo ", f, ": ", "ok" if ResourceLoader.exists("res://assets/audio/sfx/" + f) else "FALTA")

	change_scene_to_file(SOPE)
	await scene_changed
	await _frames(10)
	current_scene.get_node("Enemies").queue_free()
	for t in current_scene.get_node("Transitions").get_children():
		(t as Area2D).set_deferred("monitoring", false)
	var p := get_first_node_in_group("Player") as CharacterBody2D
	var health = p.get("Health")
	p.global_position = Vector2(600, 460)
	await _frames(20)
	# Kairo
	await _tap("jump")
	await _frames(60)
	await _tap("attack")
	await _frames(30)
	await _tap("toggle_sword")
	await _frames(30)
	await _tap("toggle_sword")
	await _frames(30)
	root.get_node("AbilityManager").call("UnlockAbility", 1)  # Dash
	await _tap("dash")
	await _frames(30)
	_probe.call("Hit", p.get_node("Hurtbox"), 5, Vector2(-1, 0))
	await _frames(30)
	health.call("SetInvulnerable", true)
	# inimigo: acerta e mata (2 golpes no Rápido de 50 de vida)
	Input.action_press("move_right"); await _frames(2); Input.action_release("move_right")
	await _frames(10)
	var e: Node2D = load("res://scenes/enemies/Enemy_Rapido.tscn").instantiate()
	current_scene.add_child(e)
	e.get_node("EnemyStateMachine").process_mode = Node.PROCESS_MODE_DISABLED
	for i in 2:
		e.global_position = p.global_position + Vector2(40, 0)
		await _frames(3)
		await _tap("attack")
		await _frames(30)
	await _frames(60)
	# mundo
	var rp := current_scene.get_node("Interactables/RestorationPoint1") as Node2D
	p.global_position = rp.global_position + Vector2(0, -4)
	await _frames(20)
	await _tap("interact")
	await _frames(40)
	# a demo tem uma bandeira só, no Dossel Vivo, e ela ativa sozinha ao passar (decisão de 11/10/2026)
	change_scene_to_file(DOSSEL)
	await scene_changed
	await _frames(10)
	current_scene.get_node("Enemies").queue_free()
	p = get_first_node_in_group("Player") as CharacterBody2D
	p.get("Health").call("SetInvulnerable", true)
	var cp := current_scene.get_node("Interactables/Checkpoint1") as Node2D
	p.global_position = cp.global_position + Vector2(0, -4)
	await _frames(30)
	# Korrag
	change_scene_to_file(COVIL)
	await scene_changed
	await _frames(10)
	var boss := current_scene.get_node("Boss_Javali")
	get_first_node_in_group("Player").get("Health").call("SetInvulnerable", true)
	boss.call("PerformStomp")
	await _frames(60)
	_probe.call("Damage", boss.get("Health"), 100000)
	await _frames(60)

	var failures := 0
	for f in EXPECTED:
		var ok := _heard.has(f)
		if not ok: failures += 1
		print("  ", "ok   " if ok else "FALHA", " tocou ", f)
	if _backup != null:
		var fw := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		fw.store_string(_backup)
		fw.close()
	print("AUDIO_RESULT: %d de %d sons tocaram, %d falha(s)" % [EXPECTED.size() - failures, EXPECTED.size(), failures])
	quit(1 if failures > 0 else 0)
