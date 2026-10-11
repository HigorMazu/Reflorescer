extends SceneTree
## C13-T2: confere com a física real do Kairo se a subida do Dossel Vivo é navegável.
## Um "bot" apoia o Kairo em cada galho e tenta chegar ao próximo apertando as ações de
## verdade (move_left/move_right/jump), com e sem pulo duplo. Inimigos são removidos.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/DosselReachCheck.gd
const DOSSEL := "res://scenes/areas/DosselVivo.tscn"
## Rota pretendida, de baixo pra cima. Cada passo precisa ser alcançável a partir do anterior.
const ROUTE := ["ForestFloor", "Branch1", "Branch2", "Branch3", "Branch4", "Branch5", "Branch6",
	"Branch7", "Branch8", "Branch9", "Branch10", "Branch11", "TopBranch"]
const HALF_H := 20.0

var _player: CharacterBody2D
var _failures := 0

func _initialize() -> void:
	_run.call_deferred()

func _rect(name: String) -> Rect2:
	var body := current_scene.get_node("Geometry/" + name) as Node2D
	var cs := body.get_node("CollisionShape2D") as CollisionShape2D
	var size: Vector2 = (cs.shape as RectangleShape2D).size
	return Rect2(cs.global_position - size / 2, size)

func _release_all() -> void:
	for a in ["move_left", "move_right", "jump"]:
		Input.action_release(a)

func _frames(n: int) -> void:
	for i in n:
		await physics_frame

func _place(x: float, top: float) -> void:
	_release_all()
	_player.global_position = Vector2(x, top - HALF_H - 1)
	_player.velocity = Vector2.ZERO
	await _frames(6)

## Uma tentativa: anda a partir de start_x, pula após `delay` quadros andando, pulo duplo em `dj` (0 = sem).
func _attempt(from: Rect2, to: Rect2, start_x: float, delay: int, dj: int) -> bool:
	if not is_instance_valid(_player):
		return false
	await _place(start_x, from.position.y)
	if not _player.is_on_floor():
		return false
	var dir := "move_right" if to.get_center().x > start_x else "move_left"
	var other := "move_left" if dir == "move_right" else "move_right"
	var center := to.get_center().x
	Input.action_press(dir)
	await _frames(delay)
	Input.action_press("jump")
	var landed := false
	for f in range(150):
		await physics_frame
		if dj > 0 and f == dj:
			Input.action_release("jump")
			await physics_frame
			Input.action_press("jump")
		var x := _player.global_position.x
		var feet := _player.global_position.y + HALF_H
		# já está acima do galho de destino e sobre ele: solta a direção pra pousar
		if feet <= to.position.y + 2 and abs(x - center) < to.size.x / 2 - 10:
			Input.action_release(dir)
			Input.action_release(other)
		if f > 5 and _player.is_on_floor():
			landed = abs(feet - to.position.y) < 3 and x > to.position.x - 12 and x < to.end.x + 12
			break
		if feet > from.position.y + 200:
			break
	_release_all()
	return landed

func _try_link(a: String, b: String) -> String:
	var from := _rect(a)
	var to := _rect(b)
	var xs := []
	# pontos de partida: perto da borda do lado do destino, no meio e na borda oposta
	var lo := from.position.x + 14
	var hi := from.end.x - 14
	if a == "ForestFloor":
		lo = clamp(to.position.x - 260, lo, hi)
		hi = clamp(to.end.x + 260, lo, hi)
	for t in [0.0, 0.25, 0.5, 0.75, 1.0]:
		xs.append(lerp(lo, hi, t))
	for dj in [0, 18, 26, 34]:
		for delay in [0, 6, 14]:
			for x in xs:
				if await _attempt(from, to, x, delay, dj):
					return "ok (%s, partida x=%d, atraso=%d)" % ["pulo simples" if dj == 0 else "pulo duplo no quadro %d" % dj, x, delay]
	return ""

func _run() -> void:
	change_scene_to_file(DOSSEL)
	await scene_changed
	await _frames(10)
	var enemies := current_scene.get_node_or_null("Enemies")
	if enemies: enemies.queue_free()
	# portais desligados: o bot não pode trocar de cena no meio das tentativas
	for t in current_scene.get_node("Transitions").get_children():
		(t as Area2D).set_deferred("monitoring", false)
	_player = get_first_node_in_group("Player")
	var health = _player.get("Health")
	if health: health.set("Invulnerable", true)
	await _frames(5)
	for i in range(ROUTE.size() - 1):
		var res := await _try_link(ROUTE[i], ROUTE[i + 1])
		if res == "":
			_failures += 1
			print("DOSSEL_REACH FALHA: ", ROUTE[i], " -> ", ROUTE[i + 1], " inalcançável nas tentativas do bot")
		else:
			print("DOSSEL_REACH ", ROUTE[i], " -> ", ROUTE[i + 1], ": ", res)
	# do galho do topo até a saída pro Igarapé
	var exit := current_scene.get_node("Transitions/ToIgarapeSufocado") as Node2D
	var top := _rect("TopBranch")
	print("DOSSEL_REACH saída ToIgarapeSufocado em ", exit.global_position, "; TopBranch topo y=", top.position.y, " x=", top.position.x, "..", top.end.x)
	print("DOSSEL_REACH_RESULT: %d falha(s)" % _failures)
	quit(1 if _failures > 0 else 0)
