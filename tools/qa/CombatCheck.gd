extends SceneTree
## C03-T0..T3: regressão de combate e da Espada de Grama depois da estocada nova (Higor, 07-09/10).
## Usa as ações reais (attack, toggle_sword, move_*, jump) e um Rápido de verdade na frente do Kairo.
## A IA do inimigo fica congelada (corpo e EnemyStateMachine parados), só Health/Hurtbox respondem;
## antes de cada golpe ele é recolocado no lugar, porque o knockback o empurra pra fora do alcance.
## O laço de alcance cria e libera dezenas de inimigos, o que expunha uma corrida do runtime .NET do
## Godot 4.7.2 (FATAL gchandle.is_released), corrigida pelo autoload ResourceKeeper (ROADMAP_DEV C12-T4).
## Por padrão o teste ainda força o GC a cada inimigo; `-- --stress` desliga isso (critério da C12-T4).
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/CombatCheck.gd
const SOPE := "res://scenes/areas/SopeDaMata.tscn"
const RAPIDO := "res://scenes/enemies/Enemy_Rapido.tscn"
const FLOOR_Y := 460.0

var _probe: Node
var _p: CharacterBody2D
var _hb: Area2D
var _failures := 0
var _passes := 0
var _gc: Node

func _initialize() -> void:
	_run.call_deferred()

func _check(ok: bool, what: String) -> void:
	if ok:
		_passes += 1
		print("  ok   ", what)
	else:
		_failures += 1
		print("  FALHA ", what)

func _frames(n: int) -> void:
	for i in n:
		await physics_frame

func _tap(action: String) -> void:
	Input.action_press(action)
	await physics_frame
	Input.action_release(action)

func _face(dir: String) -> void:
	Input.action_press(dir)
	await _frames(2)
	Input.action_release(dir)
	await _frames(20)

## Inimigos ficam no chão, como no jogo: a origem deles é nos pés, a do Kairo no centro do corpo (20 px acima).
## (Até 11/10 o teste criava os inimigos na altura do centro do Kairo, flutuando, e não pegou o golpe que
## passava por cima da formiga e da tartaruga.)
func _feet_y() -> float:
	return _p.global_position.y + 20

func _spawn_enemy(offset_x: float) -> Node2D:
	var e: Node2D = load(RAPIDO).instantiate()
	current_scene.add_child(e)
	e.global_position = Vector2(_p.global_position.x + offset_x, _feet_y())
	e.set_physics_process(false)
	e.get_node("EnemyStateMachine").process_mode = Node.PROCESS_MODE_DISABLED
	e.set_meta("offset", offset_x)
	await _frames(3)
	e.get("Health").call("SetMaxHealth", 1000, true)
	return e

func _put_back(e: Node2D) -> void:
	e.global_position = Vector2(_p.global_position.x + e.get_meta("offset"), _feet_y())
	e.set("velocity", Vector2.ZERO)
	await _frames(2)

func _ehp(e: Node) -> int:
	return e.get("Health").get("CurrentHealth")

## Ataca e registra, quadro a quadro, quando a hitbox fica ativa (em segundos de jogo).
func _attack_and_trace() -> Array:
	var active := []
	Input.action_press("attack")
	for f in range(30):
		await physics_frame
		if f == 0: Input.action_release("attack")
		if _hb.get("Active"): active.append(f)
	return active

func _jump_peak_velocity() -> float:
	var best := 0.0
	Input.action_press("jump")
	for f in range(20):
		await physics_frame
		best = min(best, _p.velocity.y)
	Input.action_release("jump")
	await _frames(90)
	return best

func _reset_player(x: float) -> void:
	_p.global_position = Vector2(x, FLOOR_Y)
	_p.velocity = Vector2.ZERO
	await _frames(30)

func _run() -> void:
	_probe = load("res://tools/qa/QaProbe.cs").new()
	root.add_child(_probe)
	_gc = load("res://tools/qa/GcProbe.cs").new()
	root.add_child(_gc)
	root.get_node("GameManager").set("HasSword", true)
	change_scene_to_file(SOPE)
	await scene_changed
	await _frames(10)
	current_scene.get_node("Enemies").free()
	for t in current_scene.get_node("Transitions").get_children():
		(t as Area2D).set_deferred("monitoring", false)
	_p = get_first_node_in_group("Player")
	_hb = _p.get_node("AttackHitbox")
	var health = _p.get("Health")
	await _reset_player(600)

	print("== C03-T0 — combate básico com a estocada")
	var trace := await _attack_and_trace()
	var window := trace.size() / 60.0
	print("  hitbox ativa nos quadros ", trace)
	_check(trace.size() > 0 and trace[0] >= 5 and trace[0] <= 7, "sem inimigo: dano abre depois da preparação (~0,10 s; quadro %s)" % [trace[0] if trace.size() else -1])
	_check(window >= 0.08 and window <= 0.12, "janela de dano ~0,10 s (medido %.2f s; antes era 0,14 s)" % window)
	await _frames(20)

	await _face("move_right")
	var e := await _spawn_enemy(40)
	var hp0 := _ehp(e)
	var saw_hitstop := false
	var saw_flash := false
	Input.action_press("attack")
	for f in range(30):
		await physics_frame
		if f == 0: Input.action_release("attack")
		if Engine.time_scale < 0.5: saw_hitstop = true
		if e.modulate.g < 0.9: saw_flash = true
	_check(hp0 - _ehp(e) == 25, "inimigo no alcance à direita recebe 25 (%d -> %d)" % [hp0, _ehp(e)])
	_check(e.get("velocity").x > 0, "knockback empurra o inimigo pra longe do Kairo (vx=%d)" % e.get("velocity").x)
	_check(saw_hitstop, "hitstop (Engine.TimeScale baixo) ao acertar")
	_check(saw_flash, "flash vermelho no inimigo")
	await _frames(30)
	_check(is_equal_approx(Engine.time_scale, 1.0), "TimeScale volta a 1")

	# segurar o botão: um único dano por ativação, sem ataque automático
	await _put_back(e)
	hp0 = _ehp(e)
	Input.action_press("attack")
	await _frames(45)
	Input.action_release("attack")
	await _frames(5)
	_check(hp0 - _ehp(e) == 25, "segurando o ataque: dano uma vez só (%d)" % (hp0 - _ehp(e)))
	await _frames(20)

	# cooldown: segundo aperto aos 0,15 s não gera outro golpe
	await _put_back(e)
	hp0 = _ehp(e)
	await _tap("attack")
	await _frames(8)
	var can_mid: bool = _p.call("CanAttack")
	await _tap("attack")
	await _frames(30)
	_check(not can_mid, "CanAttack() falso durante o cooldown (0,30 s)")
	_check(hp0 - _ehp(e) == 25, "apertar no cooldown não dá segundo golpe (%d de dano)" % (hp0 - _ehp(e)))
	e.queue_free()
	await _frames(20)

	# esquerda e costas
	await _face("move_left")
	e = await _spawn_enemy(-40)
	hp0 = _ehp(e)
	await _attack_and_trace()
	_check(hp0 - _ehp(e) == 25, "virado à esquerda acerta inimigo à esquerda")
	_check(e.get("velocity").x < 0, "knockback pra esquerda")
	e.queue_free()
	e = await _spawn_enemy(40)
	hp0 = _ehp(e)
	await _frames(15)
	await _attack_and_trace()
	_check(hp0 == _ehp(e), "virado à esquerda não acerta inimigo nas costas")
	e.queue_free()
	await _frames(10)

	# alcance da estocada: medido dos dois lados, tem que ser simétrico
	var reach := {}
	for side in [["move_right", 1], ["move_left", -1]]:
		await _face(side[0])
		reach[side[1]] = 0
		for d in range(20, 141, 5):
			e = await _spawn_enemy(d * side[1])
			hp0 = _ehp(e)
			await _frames(5)
			await _attack_and_trace()
			var hit := hp0 - _ehp(e) == 25
			e.queue_free()
			await _frames(10)
			if not OS.get_cmdline_user_args().has("--stress"): _gc.call("Collect")
			if hit: reach[side[1]] = d
			elif reach[side[1]] > 0: break
	print("  alcance da estocada (centro a centro): direita %d px, esquerda %d px" % [reach[1], reach[-1]])
	_check(reach[1] > 0 and reach[1] == reach[-1], "alcance igual pros dois lados")
	# costas, bem de perto: virado pra um lado, inimigo colado atrás
	for side in [["move_right", -20], ["move_left", 20]]:
		await _face(side[0])
		e = await _spawn_enemy(side[1])
		hp0 = _ehp(e)
		await _frames(5)
		await _attack_and_trace()
		_check(hp0 == _ehp(e), "%s: inimigo colado nas costas (%+d px) não é atingido" % [side[0], side[1]])
		e.queue_free()
		await _frames(10)
	await _face("move_right")

	# dano no Kairo: HP cai, knockback, i-frames de 1 s
	var hurt: Area2D = _p.get_node("Hurtbox")
	var hp_p: int = health.get("CurrentHealth")
	_probe.call("Hit", hurt, 10, Vector2(-1, 0))
	await physics_frame
	_check(health.get("CurrentHealth") == hp_p - 10, "Kairo toma dano (%d -> %d)" % [hp_p, health.get("CurrentHealth")])
	_check(_p.velocity.x < 0, "Kairo leva knockback na direção do golpe")
	_check(health.get("IsInvulnerable"), "Kairo fica invulnerável depois do golpe")
	await _frames(30)
	_probe.call("Hit", hurt, 10, Vector2(-1, 0))
	_check(health.get("CurrentHealth") == hp_p - 10, "segundo golpe em 0,5 s é ignorado")
	await _frames(45)
	_check(not health.get("IsInvulnerable"), "invulnerabilidade acaba em ~1 s")
	await _reset_player(600)

	print("== C03-T1 — toggle da espada (Q)")
	var sword: Node2D = _p.get_node("Visual/RightArm/RightHand/Sword")
	var bandage: Node2D = _p.get_node("Visual/RightArm/RightHand/Bandage")
	var hud_sword := get_first_node_in_group("HUD").get_node(get_first_node_in_group("HUD").get("SwordStatusPath"))
	_check(_p.get("HasSword") and sword.get("Extension") > 0.99 and not bandage.visible, "começa com a espada: lâmina estendida, sem bandagem")
	await _tap("toggle_sword")
	await _frames(30)
	_check(not _p.get("HasSword"), "Q guarda a espada (HasSword=false)")
	_check(sword.get("Extension") < 0.01 and bandage.visible, "lâmina recolhe e a bandagem aparece")
	_check(not hud_sword.get("HasSword"), "HUD mostra espada guardada")
	_check(_p.get_node("Visual/AnimatedSprite2D").self_modulate == Color.WHITE, "sem o tom verde antigo no sprite")
	_check(root.get_node("GameManager").get("HasSword") == false, "estado guardado no GameManager (sobrevive à troca de cena)")

	print("== C03-T2 — stats duplos")
	await _reset_player(600)
	Input.action_press("move_right")
	await _frames(40)
	var v_nosword := _p.velocity.x
	Input.action_release("move_right")
	await _frames(30)
	var vy_nosword := await _jump_peak_velocity()
	await _tap("toggle_sword")
	await _frames(30)
	Input.action_press("move_right")
	await _frames(40)
	var v_sword := _p.velocity.x
	Input.action_release("move_right")
	await _frames(30)
	var vy_sword := await _jump_peak_velocity()
	print("  velocidade sem/com espada: %d / %d   pulo: %d / %d" % [v_nosword, v_sword, vy_nosword, vy_sword])
	_check(v_nosword > v_sword, "sem espada corre mais rápido")
	_check(vy_nosword < vy_sword, "sem espada pula mais alto")

	print("== C03-T3 — bloqueio e troca em combate")
	await _reset_player(600)
	await _face("move_right")
	await _tap("toggle_sword")
	await _frames(30)
	e = await _spawn_enemy(40)
	hp0 = _ehp(e)
	var t := await _attack_and_trace()
	_check(t.is_empty() and hp0 == _ehp(e), "sem espada o ataque não sai")
	await _tap("toggle_sword")
	await _frames(30)
	# guardar a espada logo depois de apertar ataque (antes do dano abrir)
	await _put_back(e)
	hp0 = _ehp(e)
	Input.action_press("attack")
	await physics_frame
	Input.action_release("attack")
	await _frames(2)
	await _tap("toggle_sword")
	var opened := false
	for f in range(30):
		await physics_frame
		if _hb.get("Active"): opened = true
	print("  preparação + Q: hitbox abriu=%s, dano=%d" % [opened, hp0 - _ehp(e)])
	_check(hp0 == _ehp(e) and not opened, "guardar a espada durante a preparação cancela o golpe")
	await _tap("toggle_sword")
	await _frames(30)
	# alternar várias vezes andando e atacando
	Input.action_press("move_right")
	for i in range(8):
		await _tap("toggle_sword")
		await _tap("attack")
		await _frames(4)
	Input.action_release("move_right")
	await _frames(40)
	if not _p.get("HasSword"):
		await _tap("toggle_sword")
		await _frames(30)
	_check(_p.is_on_floor() and abs(_p.velocity.x) < 1, "depois de alternar em movimento: parado e no chão, sem input travado")
	_check(not _hb.get("Active"), "hitbox fechada depois da sequência")
	_check(_p.get_node("Visual").scale.y == 1 and _p.get_node("Visual").position == Vector2.ZERO, "Visual volta à pose normal (sem tween preso)")
	e.queue_free()
	await _frames(5)
	await _reset_player(600)
	await _face("move_right")
	e = await _spawn_enemy(40)
	hp0 = _ehp(e)
	await _attack_and_trace()
	_check(hp0 - _ehp(e) == 25, "C03-T0 de novo depois da troca: golpe normal de 25")

	print("COMBAT_RESULT: %d ok, %d falha(s)" % [_passes, _failures])
	quit(1 if _failures > 0 else 0)
