extends Node2D
## Live review of production enemy scenes; no progress/save operations.

func title(text: String, at: Vector2, size: int = 18) -> void:
	var label := Label.new()
	label.text = text
	label.position = at
	label.add_theme_font_size_override("font_size", size)
	label.modulate = Color("e6ebd8")
	add_child(label)

func _ready() -> void:
	RenderingServer.set_default_clear_color(Color("11291f"))
	title("REFLORESCER  /  Inimigos da floresta", Vector2(44, 22), 30)
	title("Cenas reais · detalhe 2× · direita e esquerda · idle / walk / attack", Vector2(44, 64), 17)
	var names := ["Voador", "Rapido", "Robusto"]
	var captions := ["Vespa-asiática gigante", "Formiga-lava-pé", "Tartaruga-de-orelha-vermelha"]
	for column in range(3):
		var x: float = 80 + column * 410
		title(captions[column], Vector2(x, 122), 20)
		for row in range(3):
			var state: String = ["idle", "walk", "attack"][row]
			var y: float = 270 + row * 170
			title(state, Vector2(x, y - 91), 16)
			var line := Line2D.new()
			line.points = PackedVector2Array([Vector2(x, y), Vector2(x + 310, y)])
			line.default_color = Color("507557")
			line.width = 1
			add_child(line)
			for side in range(2):
				var enemy: Node2D = load("res://scenes/enemies/Enemy_" + names[column] + ".tscn").instantiate()
				enemy.position = Vector2(x + 70 + side * 170, y)
				enemy.scale = Vector2(2, 2)
				enemy.process_mode = Node.PROCESS_MODE_DISABLED
				add_child(enemy)
				var sprite := enemy.get_node("AnimatedSprite2D") as AnimatedSprite2D
				sprite.process_mode = Node.PROCESS_MODE_ALWAYS
				enemy.call("PlayAnimation", state)
				if side == 1:
					enemy.call("Flip")
	title("Escala real: até 54 px de largura (+34%). Colisões e atributos mantidos.", Vector2(44, 674), 16)
	if "--capture-enemies" in OS.get_cmdline_user_args():
		await get_tree().create_timer(0.35).timeout
		await RenderingServer.frame_post_draw
		get_viewport().get_texture().get_image().save_png("res://docs/art-review/enemy-integration.png")
		get_tree().quit()
