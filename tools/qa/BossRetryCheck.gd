extends SceneTree
## C20-T1/T2/T3 (06/11/2026): poise dos chefes, luta que recomeça quando o Kairo renasce na mesma cena
## (parede da cobra abre de novo) e portais que reagem ao corpo do Kairo com carência no spawn.
## O save_0.json é guardado e devolvido.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/BossRetryCheck.gd
const DESERTO := "res://scenes/areas/Deserto.tscn"
const DOSSEL := "res://scenes/areas/DosselVivo.tscn"
const IGARAPE := "res://scenes/areas/IgarapeSufocado.tscn"
const SAVE_FILE := "user://save_0.json"
const HURT_STATE := 3 # BossStateType.Hurt

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

func _place(p: CharacterBody2D, pos: Vector2) -> void:
	p.global_position = pos
	p.velocity = Vector2.ZERO
	p.reset_physics_interpolation()
	await _frames(3)

func _run() -> void:
	if FileAccess.file_exists(SAVE_FILE):
		_backup = FileAccess.get_file_as_string(SAVE_FILE)
	_probe = load("res://tools/qa/QaProbe.cs").new()
	root.add_child(_probe)
	root.get_node("SaveManager").call("StartNewGame", 0)
	await scene_changed

	print("== Poise: atacar sem parar não trava o chefe (C20-T1)")
	change_scene_to_file(DESERTO)
	await scene_changed
	await _frames(10)
	var boss := current_scene.get_node("Boss_Serpente") as CharacterBody2D
	var gate := current_scene.get_node("Geometry/ArenaGate/CollisionShape2D") as CollisionShape2D
	var p := _player()
	var start_pos := boss.global_position
	await _place(p, Vector2(boss.global_position.x - 180, 453))
	await _seconds(2.6) # intro do chefe
	_check(boss.get("IsActive") and not gate.disabled, "cobra acorda e a arena fecha")
	var hurtbox = boss.get_node("Hurtbox")
	var sm = boss.get_node("BossStateMachine")
	var hurt_frames := 0
	var total := 0
	# 4 s levando um golpe a cada 0,3 s (o ritmo do ataque do Kairo)
	for i in 13:
		_probe.call("Hit", hurtbox, 1, Vector2.LEFT)
		for f in 18:
			await physics_frame
			total += 1
			if sm.get("CurrentStateType") == HURT_STATE: hurt_frames += 1
	var ratio := float(hurt_frames) / total
	_check(ratio < 0.4, "chefe fica no Hurt só %d%% do tempo sob golpes contínuos (antes ~100%%)" % int(ratio * 100))
	_check(boss.get("Health").get("CurrentHealth") < boss.get("Health").get("MaxHealth"), "os golpes continuam tirando vida")

	print("== Morrer e renascer na mesma cena reinicia a luta (C20-T2)")
	p.call("SetRespawnPoint", Vector2(300, 453))
	p.get("Health").call("SetInvulnerable", false)
	_probe.call("Damage", p.get("Health"), 100000)
	# A tela de Game Over abre depois da animação de morte
	for i in 240:
		if paused: break
		await process_frame
	# Igual ao botão "Tentar de novo" da tela de Game Over (despausa e renasce)
	_check(paused, "morrer abre o Game Over (jogo pausado)")
	get_first_node_in_group("HUD").call("OnRetryPressed")
	await _seconds(0.3)
	_check(current_scene.scene_file_path == DESERTO, "renasceu na bandeira do próprio Deserto (sem recarregar)")
	_check(gate.disabled, "parede de areia abre de novo depois do respawn")
	_check(not boss.get("IsActive"), "cobra volta a esperar o Kairo")
	_check(boss.get("Health").get("CurrentHealth") == boss.get("Health").get("MaxHealth"), "cobra volta com a vida cheia")
	_check(boss.global_position.distance_to(start_pos) < 4.0, "cobra volta pro lugar inicial")
	await _place(p, Vector2(gate.global_position.x - 120, 453))
	_check(gate.disabled, "dá pra chegar na entrada da arena")
	await _place(p, Vector2(start_pos.x - 180, 453))
	await _seconds(0.5)
	_check(boss.get("IsActive") and not gate.disabled, "entrar de novo acorda a cobra e fecha a arena")

	print("== Portais: corpo do Kairo e carência no spawn (C20-T3)")
	var sm_scene = root.get_node("SceneManager")
	sm_scene.call("LoadSceneAtSpawn", DOSSEL, "SpawnFromIgarapeSufocado", Vector2.ZERO)
	await scene_changed
	await _seconds(1.0)
	_check(current_scene.scene_file_path == DOSSEL, "chegar no Dossel vindo do Igarapé não dispara o portal de volta")
	var top := current_scene.get_node("Transitions/ToIgarapeSufocado") as Area2D
	p = _player()
	# Portal 60×100: borda de baixo em y+50. Kairo 80 px abaixo do centro: corpo (±20) fica 10 px fora,
	# mas a Hurtbox antiga (26 px acima do corpo) já entrava. Gravidade desligada pra ele não cair do lugar.
	p.set_physics_process(false)
	await _place(p, Vector2(top.global_position.x, top.global_position.y + 80))
	await _seconds(0.3)
	_check(current_scene.scene_file_path == DOSSEL, "só a cabeça (Hurtbox) dentro do portal não troca de área")
	var changed := [false]
	scene_changed.connect(func(): changed[0] = true, CONNECT_ONE_SHOT)
	await _place(p, top.global_position)
	await _seconds(0.5)
	_check(changed[0] and current_scene.scene_file_path == IGARAPE, "corpo dentro do portal troca de área")

	paused = false
	if _backup != null:
		var f := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		f.store_string(_backup)
		f.close()
	print("BOSS_RETRY_RESULT: %d ok, %d falha(s)" % [_passes, _failures])
	quit(1 if _failures > 0 else 0)
