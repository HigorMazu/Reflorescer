extends SceneTree
## C15-T2: luta da cobra do Deserto. Arena fecha, Kairo acerta o corpo, os três golpes acertam e têm
## aviso, mergulho deixa a cobra invulnerável e ela sai embaixo do Kairo, fases mudam, a morte salva,
## abre a arena e NÃO abre a tela de fim da demo (essa é só do Korrag).
## O save_0.json é guardado e devolvido.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/SerpentBossCheck.gd
const DESERTO := "res://scenes/areas/Deserto.tscn"
const SAVE_FILE := "user://save_0.json"

var _failures := 0
var _passes := 0
var _backup = null
var _probe: Node

func _initialize() -> void:
	_run.call_deferred()

func _check(ok: bool, what: String) -> void:
	if ok: _passes += 1
	else: _failures += 1
	print("  ", "ok    " if ok else "FALHA ", what)

func _frames(n: int) -> void:
	for i in n:
		await physics_frame

func _seconds(t: float) -> void:
	await _frames(int(t * 60.0))

func _player() -> CharacterBody2D:
	return get_first_node_in_group("Player") as CharacterBody2D

func _place(p: CharacterBody2D, x: float) -> void:
	p.global_position = Vector2(x, 453)
	p.velocity = Vector2.ZERO
	p.reset_physics_interpolation()
	await _frames(3)

func _hp() -> int:
	return _player().get("Health").get("CurrentHealth")

func _heal() -> void:
	var h = _player().get("Health")
	h.call("Reset")
	h.call("SetInvulnerable", false)

func _run() -> void:
	if FileAccess.file_exists(SAVE_FILE):
		_backup = FileAccess.get_file_as_string(SAVE_FILE)
	_probe = load("res://tools/qa/QaProbe.cs").new()
	root.add_child(_probe)
	root.get_node("SaveManager").call("StartNewGame", 0)
	await scene_changed
	change_scene_to_file(DESERTO)
	await scene_changed
	await _frames(10)

	var boss := current_scene.get_node("Boss_Serpente") as CharacterBody2D
	var gate := current_scene.get_node("Geometry/ArenaGate/CollisionShape2D") as CollisionShape2D
	_check(boss != null, "a cobra é um chefe de verdade na arena")
	_check(gate.disabled, "arena aberta antes da luta")
	var p := _player()
	await _place(p, 3150)
	await _seconds(0.5)
	_check(boss.get("IsActive"), "cobra acorda quando o Kairo entra na arena")
	_check(not gate.disabled, "parede de areia fecha a arena")
	# Controle manual dos golpes: a IA não escolhe ataques sozinha durante o teste
	boss.get_node("BossStateMachine").process_mode = Node.PROCESS_MODE_DISABLED
	await _seconds(0.3)

	# Kairo acerta o corpo enrolado
	var hp0: int = boss.get("Health").get("CurrentHealth")
	await _place(p, boss.global_position.x - 150)
	Input.action_press("move_right"); await _frames(2); Input.action_release("move_right")
	await _place(p, boss.global_position.x - 150)
	for i in 3:
		Input.action_press("attack"); await _frames(2); Input.action_release("attack"); await _seconds(0.4)
	var hp1: int = boss.get("Health").get("CurrentHealth")
	_check(hp1 < hp0, "golpe do Kairo tira vida da cobra (%d → %d)" % [hp0, hp1])

	# Bote: acerta quem está à frente, com aviso antes
	await _place(p, boss.global_position.x - 200)
	_heal()
	var before := _hp()
	boss.call("StartStrike")
	await _frames(5)
	_check(boss.get("Sprite").self_modulate != Color.WHITE or boss.get("CurrentMove") == "StrikeWindup", "bote tem aviso (piscar vermelho)")
	_check(_hp() == before, "bote não acerta durante o aviso")
	await _seconds(1.5)
	_check(_hp() < before, "bote acerta o Kairo à frente (%d → %d)" % [before, _hp()])
	_check(boss.get("CurrentMove") == "None", "bote termina")

	# Cauda: baixa, acerta quem está no chão
	await _place(p, boss.global_position.x + 220)
	_heal()
	before = _hp()
	boss.call("StartTail")
	await _seconds(1.2)
	_check(_hp() < before, "varrida de cauda acerta o Kairo no chão (%d → %d)" % [before, _hp()])

	# Mergulho: some, fica invulnerável, segue o Kairo e sai embaixo dele
	await _place(p, boss.global_position.x - 260)
	_heal()
	before = _hp()
	var target_x := p.global_position.x
	boss.call("StartBurrow")
	await _seconds(0.9)
	_check(boss.get("IsUnderground"), "cobra mergulha na areia")
	_check(not boss.get("Sprite").visible, "cobra some enquanto está embaixo da areia")
	var hb := boss.get_node("Hurtbox")
	_check(not hb.get("Active"), "embaixo da areia ela não leva dano")
	_check(boss.get_node("SandMarker").visible, "redemoinho de areia mostra onde ela está")
	await _seconds(1.6)
	_check(absf(boss.global_position.x - target_x) < 60.0, "ela sai embaixo do Kairo (x=%d, Kairo=%d)" % [boss.global_position.x, target_x])
	_check(_hp() < before, "a saída da areia machuca (%d → %d)" % [before, _hp()])
	_check(hb.get("Active") and boss.get("Sprite").visible, "depois de sair ela volta a ser atingível")

	# Fases e morte
	var health = boss.get("Health")
	_probe.call("Damage", health, int(health.get("CurrentHealth")) - int(health.get("MaxHealth") * 0.45))
	await _seconds(0.8)
	boss.get_node("BossStateMachine").process_mode = Node.PROCESS_MODE_INHERIT
	await _seconds(2.6)
	_check(boss.get("PhaseIndex") >= 1, "abaixo de 50% ela entra na fase 2")
	var p2 = _player()
	p2.get("Health").call("SetInvulnerable", true)
	_probe.call("Damage", health, 100000)
	await _seconds(5.5)
	_check(root.get_node("SaveManager").call("IsBossDefeated", "boss_serpente"), "derrota da cobra fica salva")
	_check(gate.disabled, "arena abre quando ela cai")
	var hud := get_first_node_in_group("HUD")
	var demo_end = hud.get_node_or_null("DemoEnd")
	_check(demo_end == null or not demo_end.visible, "a tela de fim da demo NÃO abre (só o Korrag abre)")
	_check(not paused, "o jogo continua depois da cobra")

	# Recarregar: a cobra não volta e a arena fica aberta
	change_scene_to_file(DESERTO)
	await scene_changed
	await _frames(10)
	_check(current_scene.get_node_or_null("Boss_Serpente") == null or current_scene.get_node("Boss_Serpente").is_queued_for_deletion(), "cobra derrotada não volta")
	_check((current_scene.get_node("Geometry/ArenaGate/CollisionShape2D") as CollisionShape2D).disabled, "arena continua aberta")

	paused = false
	if _backup != null:
		var f := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		f.store_string(_backup)
		f.close()
	print("SERPENT_RESULT: %d ok, %d falha(s)" % [_passes, _failures])
	quit(1 if _failures > 0 else 0)
