extends SceneTree
## C12-T4: estresse do runtime .NET pra medir o crash `FATAL gchandle.is_released()`.
## Em ciclos: carrega uma área, cria inimigos colados no Kairo, ataca (dano, hitstop, timers,
## tweens), libera, e troca de cena. Sem GC forçado: o objetivo é deixar o GC agir sozinho.
## Saída 0 = completou; o crash derruba o processo (exit != 0).
## Antes do autoload ResourceKeeper (11/10): 6/6 execuções travavam. Depois: 8/8 completas com 24 ciclos.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/GcStressCheck.gd [-- --cycles=N]
const AREAS := ["res://scenes/areas/SopeDaMata.tscn", "res://scenes/areas/IgarapeSufocado.tscn", "res://scenes/areas/DosselVivo.tscn"]
const ENEMIES := ["res://scenes/enemies/Enemy_Rapido.tscn", "res://scenes/enemies/Enemy_Robusto.tscn", "res://scenes/enemies/Enemy_Voador.tscn"]

func _initialize() -> void:
	_run.call_deferred()

func _frames(n: int) -> void:
	for i in n:
		await physics_frame

func _run() -> void:
	var cycles := 12
	for a in OS.get_cmdline_user_args():
		if a.begins_with("--cycles="): cycles = int(a.get_slice("=", 1))
	var hits := 0
	for c in range(cycles):
		change_scene_to_file(AREAS[c % AREAS.size()])
		await scene_changed
		await _frames(5)
		for t in current_scene.get_node("Transitions").get_children():
			(t as Area2D).set_deferred("monitoring", false)
		var p := get_first_node_in_group("Player") as CharacterBody2D
		p.get("Health").call("SetInvulnerable", true)
		p.global_position = Vector2(600, 460)
		await _frames(10)
		for i in range(20):
			var e: Node2D = load(ENEMIES[i % ENEMIES.size()]).instantiate()
			current_scene.add_child(e)
			e.global_position = p.global_position + Vector2(40, -4)
			await _frames(2)
			Input.action_press("attack")
			await physics_frame
			Input.action_release("attack")
			await _frames(18)
			hits += 1
			e.queue_free()
			await _frames(2)
		print("GC_STRESS ciclo ", c + 1, "/", cycles, " ok")
	print("GC_STRESS_RESULT: completou %d ciclos, %d golpes" % [cycles, hits])
	quit()
