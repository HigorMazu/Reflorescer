extends SceneTree
## C13-T1..T5 + A05/A06: percorre a demo inteira pelos caminhos reais do jogo.
## Novo jogo -> Sopé -> Dossel -> Igarapé -> Covil -> Korrag (fases e morte) -> Dash.
## Usa os TransitionTrigger de verdade (o Kairo é colocado dentro do portal), testa as voltas
## (Dossel->Sopé, Igarapé->Dossel), spawn, chão, câmera, inimigos, restauração, prompt, HUD e lama.
## Não substitui jogar: não mede dificuldade, sensação nem FPS.
## O save_0.json do usuário é guardado no início e devolvido no fim.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/DemoRunCheck.gd
const MENU := "res://scenes/ui/MainMenu.tscn"
const SOPE := "res://scenes/areas/SopeDaMata.tscn"
const DOSSEL := "res://scenes/areas/DosselVivo.tscn"
const IGARAPE := "res://scenes/areas/IgarapeSufocado.tscn"
const COVIL := "res://scenes/areas/CovilDeKorrag.tscn"
const SAVE_FILE := "user://save_0.json"
const HALF_H := 20.0

var _probe: Node
var _failures := 0
var _passes := 0
var _backup = null
var _restored: Array[String] = []
var _boss_defeated := 0
var _unlocked: Array[String] = []
var _phases: Array[int] = []
var _counts := {}

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

func _player() -> CharacterBody2D:
	return get_first_node_in_group("Player") as CharacterBody2D

func _god_mode() -> void:
	var h = _player().get("Health")
	if h: h.call("SetInvulnerable", true)

func _enemy_counts() -> Dictionary:
	var c := {"Voador": 0, "Rapido": 0, "Robusto": 0, "outros": 0}
	var stack: Array[Node] = [current_scene]
	while stack.size() > 0:
		var n: Node = stack.pop_back()
		var path := n.scene_file_path
		if path.begins_with("res://scenes/enemies/"):
			var k := path.get_file().get_basename().trim_prefix("Enemy_")
			c[k if c.has(k) else "outros"] += 1
		stack.append_array(n.get_children())
	return c

func _check_area_basics(label: String, scene: String) -> void:
	_check(current_scene.scene_file_path == scene, "%s: cena carregada" % label)
	var p := _player()
	_check(p != null, "%s: Kairo presente" % label)
	_check(get_nodes_in_group("HUD").size() == 1, "%s: um HUD" % label)
	var area := current_scene
	if area.get("UseCameraLimits"):
		var lim: Rect2i = area.get("CameraLimits")
		var cam := p.find_child("*", true, false) as Camera2D
		for n in p.find_children("*", "Camera2D", true, false):
			cam = n
		_check(cam != null and cam.limit_left == lim.position.x and cam.limit_right == lim.end.x \
			and cam.limit_top == lim.position.y and cam.limit_bottom == lim.end.y, "%s: limites de câmera aplicados %s" % [label, lim])

## Espera o Kairo assentar e confere que está no chão, perto do ponto esperado e dentro do mundo.
func _check_landing(label: String, expected: Vector2) -> void:
	await _frames(90)
	var p := _player()
	var lim: Rect2i = current_scene.get("CameraLimits")
	_check(p.is_on_floor(), "%s: Kairo em pé no chão (pos %s)" % [label, p.global_position.round()])
	_check(abs(p.global_position.x - expected.x) < 48, "%s: nasceu no x do marker (%d, esperado %d)" % [label, p.global_position.x, expected.x])
	_check(p.global_position.y < lim.end.y, "%s: não caiu fora do mundo" % label)

func _go_through(portal_path: String, target_scene: String) -> Vector2:
	var portal := current_scene.get_node(portal_path) as Area2D
	var spawn_name: String = portal.get("TargetArea")
	var offset: Vector2 = portal.get("SpawnOffset")
	_check(portal.get("TargetScene") == target_scene, "%s aponta para %s" % [portal.name, target_scene.get_file()])
	_player().global_position = portal.global_position
	await scene_changed
	await _frames(3)
	_god_mode()
	var marker := current_scene.get_node_or_null(spawn_name) as Node2D
	_check(marker != null and marker.is_in_group("SpawnPoints"), "marker de destino '%s' existe" % spawn_name)
	return (marker.global_position if marker else Vector2.ZERO) + offset

func _check_restoration(label: String) -> void:
	var points := get_nodes_in_group("RestorationPoints")
	_check(points.size() == 1, "%s: 1 ponto de restauração (achados: %d)" % [label, points.size()])
	if points.is_empty(): return
	var rp := points[0] as Area2D
	var p := _player()
	p.global_position = rp.global_position + Vector2(0, -4)
	await _frames(20)
	var prompt := get_first_node_in_group("HUD").get_node("MarginContainer/VBoxContainer/BottomRow/InteractionPrompt") as Label
	_check(prompt.visible and prompt.text.begins_with("[E]"), "%s: prompt de interação aparece perto do ponto ('%s')" % [label, prompt.text])
	var before := _restored.size()
	Input.action_press("interact")
	await _frames(2)
	Input.action_release("interact")
	await _frames(60)
	_check(_restored.size() == before + 1, "%s: E restaura o ponto (AreaRestored)" % label)
	_check(not prompt.visible, "%s: prompt some depois de restaurar" % label)

func _run() -> void:
	if FileAccess.file_exists(SAVE_FILE):
		_backup = FileAccess.get_file_as_string(SAVE_FILE)
	_probe = load("res://tools/qa/QaProbe.cs").new()
	root.add_child(_probe)
	var bus := root.get_node("EventBus")
	bus.connect("AreaRestored", func(id): _restored.append(id))
	bus.connect("BossDefeated", func(_id): _boss_defeated += 1)
	bus.connect("AbilityUnlocked", func(id): _unlocked.append(id))
	bus.connect("BossPhaseChanged", func(_id, ph): _phases.append(ph))

	print("== Novo jogo -> Sopé da Mata (C13-T1, A05-T1, A06)")
	root.get_node("SceneManager").call("LoadScene", MENU)
	await scene_changed
	await _frames(10)
	current_scene.get_node("Box/NewGameButton").emit_signal("pressed")
	await scene_changed
	await _frames(3)
	_god_mode()
	_check_area_basics("Sopé", SOPE)
	await _check_landing("Sopé/Novo jogo", current_scene.get_node("SpawnInicial").global_position)
	_counts["Sopé"] = _enemy_counts()
	print("  inimigos Sopé: ", _counts["Sopé"])
	_check(_counts["Sopé"]["Voador"] + _counts["Sopé"]["Rapido"] + _counts["Sopé"]["Robusto"] > 0, "Sopé: tem inimigos")
	var hud := get_first_node_in_group("HUD")
	var bar := hud.get_node(hud.get("HealthBarPath")) as Range
	var hp_before := bar.value
	_probe.call("Damage", _player().get("Health"), 10)
	await _frames(40)
	_check(is_equal_approx(bar.value, hp_before - 10), "HUD: barra de vida cai com dano real (%d -> %d)" % [hp_before, bar.value])
	_god_mode()
	await _check_restoration("Sopé")

	print("== Sopé -> Dossel Vivo (C13-T2, A05-T2)")
	var exp := await _go_through("Transitions/ToDosselVivo", DOSSEL)
	_check_area_basics("Dossel", DOSSEL)
	await _check_landing("Dossel vindo do Sopé", exp)
	_counts["Dossel"] = _enemy_counts()
	print("  inimigos Dossel: ", _counts["Dossel"])
	_check(_counts["Dossel"]["Voador"] > _counts["Sopé"]["Voador"], "Dossel: mais Voadores que o Sopé")
	await _check_restoration("Dossel")
	print("== volta Dossel -> Sopé")
	exp = await _go_through("Transitions/ToSopeDaMata", SOPE)
	await _check_landing("Sopé vindo do Dossel", exp)
	exp = await _go_through("Transitions/ToDosselVivo", DOSSEL)
	await _check_landing("Dossel (de novo)", exp)
	_check(get_nodes_in_group("RestorationPoints")[0].get("Restored"), "Dossel: ponto continua restaurado ao voltar")

	print("== Dossel -> Igarapé Sufocado (C13-T3, A05-T3)")
	exp = await _go_through("Transitions/ToIgarapeSufocado", IGARAPE)
	_check_area_basics("Igarapé", IGARAPE)
	await _check_landing("Igarapé vindo do Dossel", exp)
	_counts["Igarapé"] = _enemy_counts()
	print("  inimigos Igarapé: ", _counts["Igarapé"])
	var ground := func(k): return _counts[k]["Rapido"] + _counts[k]["Robusto"]
	_check(ground.call("Igarapé") > ground.call("Sopé") and ground.call("Igarapé") > ground.call("Dossel"), "Igarapé: mais Robustos/Rápidos que as áreas anteriores")
	var p := _player()
	var muds := current_scene.get_node("Hazards").get_children()
	_check(muds.size() == 3, "Igarapé: 3 poças de lama")
	for mud in muds:
		p.global_position = (mud as Node2D).global_position + Vector2(0, -10)
		await _frames(20)
		_check(is_equal_approx(p.get("TerrainSpeedMultiplier"), 0.55), "lama %s: velocidade x0,55 dentro" % mud.name)
		p.global_position = (mud as Node2D).global_position + Vector2(0, -400)
		await _frames(20)
		_check(is_equal_approx(p.get("TerrainSpeedMultiplier"), 1.0), "lama %s: volta a x1,0 fora" % mud.name)
	await _check_restoration("Igarapé")
	print("== volta Igarapé -> Dossel")
	for t in current_scene.get_node("Transitions").get_children():
		if t.get("TargetScene") == DOSSEL:
			exp = await _go_through("Transitions/" + t.name, DOSSEL)
			await _check_landing("Dossel vindo do Igarapé", exp)
			exp = await _go_through("Transitions/ToIgarapeSufocado", IGARAPE)
			await _check_landing("Igarapé (de novo)", exp)
			break

	print("== Igarapé -> Covil de Korrag (C13-T4, A04, C06-T1)")
	for t in current_scene.get_node("Transitions").get_children():
		if t.get("TargetScene") == COVIL:
			exp = await _go_through("Transitions/" + t.name, COVIL)
			break
	_check_area_basics("Covil", COVIL)
	await _check_landing("Covil vindo do Igarapé", exp)
	var c: Dictionary = _enemy_counts()
	_check(c["Voador"] + c["Rapido"] + c["Robusto"] + c["outros"] == 0, "Covil: sem inimigos comuns")
	_check(get_nodes_in_group("RestorationPoints").is_empty(), "Covil: sem ponto de restauração")
	var boss := current_scene.get_node_or_null("Boss_Javali")
	_check(boss != null, "Covil: Korrag presente")
	if boss:
		_player().global_position = boss.global_position + Vector2(-300, -10)
		for i in 180:
			await physics_frame
			if boss.get("IsActive"): break
		_check(boss.get("IsActive"), "Korrag ativa com o Kairo por perto")
		var bh = boss.get("Health")
		var maxh: int = bh.get("MaxHealth")
		var p1_cd: float = boss.call("GetCurrentPhaseCooldown")
		_probe.call("Damage", bh, int(maxh * 0.55))
		await _frames(150)
		_check(_phases.has(1), "Korrag entra na fase 2 com <=50%% de vida (fases emitidas: %s)" % [_phases])
		var p2_cd: float = boss.call("GetCurrentPhaseCooldown")
		var p2_sp: float = boss.call("GetCurrentPhaseSpeed")
		_probe.call("Damage", bh, int(maxh * 0.30))
		await _frames(150)
		_check(_phases.has(2), "Korrag entra no enraged com <=20%% de vida (fases emitidas: %s)" % [_phases])
		var p3_cd: float = boss.call("GetCurrentPhaseCooldown")
		var p3_sp: float = boss.call("GetCurrentPhaseSpeed")
		print("  cooldown fase1/2/enraged: %.2f / %.2f / %.2f  velocidade fase2/enraged: %d / %d" % [p1_cd, p2_cd, p3_cd, p2_sp, p3_sp])
		_check(p3_cd < p2_cd and p3_sp > p2_sp, "enraged mais rápido e ataca mais que a fase 2 (bugfix de 05/10)")
		_probe.call("Damage", bh, maxh)
		await _frames(30)
		var note := get_first_node_in_group("HUD").get_node_or_null("AbilityUnlockNotification") as Label
		_check(note != null and note.visible and "Dash" in note.text, "HUD mostra a notificação de habilidade nova ('%s')" % (note.text if note else ""))
		await _frames(210)
		_check(_boss_defeated == 1, "BossDefeated emitido uma única vez (%d)" % _boss_defeated)
		_check(_unlocked.count("Dash") == 1, "Dash desbloqueado uma vez (%s)" % [_unlocked])
		_check(not is_instance_valid(boss), "Korrag some depois do fade")

	if _backup != null:
		var f := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		f.store_string(_backup)
		f.close()
	else:
		DirAccess.remove_absolute(SAVE_FILE)
	print("DEMO_RUN_RESULT: %d ok, %d falha(s)" % [_passes, _failures])
	quit(1 if _failures > 0 else 0)
