extends Node2D
## Art review only: uses production textures and SpriteFrames, never saves progress.
const KAIRO = preload("res://assets/sprites/kairo_faisca/KairoFaiscaSpriteFrames.tres")
const FAISCA = preload("res://assets/sprites/faisca/FaiscaSpriteFrames.tres")
var elapsed: float = 0.0
var companion: AnimatedSprite2D
var hero: AnimatedSprite2D
var hero_root: Node2D
var state_label: Label
var restored: Sprite2D
var dry: Sprite2D

func label_at(text: String, at: Vector2, size: int = 18, tint: Color = Color("dfebd5")) -> Label:
	var label := Label.new()
	label.text = text
	label.position = at
	label.add_theme_font_size_override("font_size", size)
	label.add_theme_color_override("font_color", tint)
	add_child(label)
	return label

func image_at(path: String, at: Vector2, dimensions: Vector2) -> Sprite2D:
	var sprite := Sprite2D.new()
	sprite.texture = load(path)
	sprite.centered = false
	sprite.position = at
	sprite.scale = dimensions / sprite.texture.get_size()
	add_child(sprite)
	return sprite

func animated(frames: SpriteFrames, animation_name: String, at: Vector2, factor: float) -> AnimatedSprite2D:
	var sprite := AnimatedSprite2D.new()
	sprite.sprite_frames = frames
	sprite.position = at
	sprite.scale = Vector2.ONE * factor
	sprite.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	add_child(sprite)
	sprite.play(animation_name)
	return sprite

func _ready() -> void:
	RenderingServer.set_default_clear_color(Color("101e1b"))
	var backdrop := image_at("res://assets/environment/tropical/forest_background.png", Vector2(0, 0), Vector2(1280, 420))
	backdrop.modulate = Color(0.6, 0.7, 0.63)
	label_at("REFLORESCER", Vector2(38, 22), 32)
	label_at("A vida nasce da terra.  /  estudo de arte · 06.10.2026", Vector2(40, 65), 17)
	# Terrain shown at its own source proportions in this art board.
	image_at("res://assets/environment/tropical/platform_restored.png", Vector2(20, 205), Vector2(768, 256))
	dry = image_at("res://assets/environment/tropical/platform_degraded.png", Vector2(815, 238), Vector2(420, 140))
	restored = image_at("res://assets/environment/tropical/platform_restored.png", Vector2(815, 238), Vector2(420, 140))
	label_at("Restauração · mesma área", Vector2(900, 372), 17)
	hero_root = Node2D.new()
	hero_root.position = Vector2(340, 292)
	add_child(hero_root)
	hero = animated(KAIRO, "run", Vector2.ZERO, 0.5)
	hero.reparent(hero_root, false)
	hero.centered = false
	hero.position = Vector2(-45, -85)
	companion = animated(FAISCA, "fly", Vector2(300, 250), 0.75)
	label_at("Kairo + Faísca · escala de gameplay", Vector2(210, 392), 16)
	label_at("KAIRO / 7 animações núcleo", Vector2(36, 442), 19)
	var names := ["idle", "run", "jump", "fall", "attack", "hurt", "dead"]
	for i in range(names.size()):
		animated(KAIRO, names[i], Vector2(85 + i * 155, 541), 0.7)
		label_at(names[i], Vector2(57 + i * 155, 607), 15)
	label_at("FAÍSCA / detalhe ampliado 3×", Vector2(36, 657), 17)
	for i in range(3):
		var names_f := ["idle", "fly", "investigate"]
		animated(FAISCA, names_f[i], Vector2(470 + i * 200, 664), 2.25)
		label_at(names_f[i], Vector2(510 + i * 200, 654), 15)
	state_label = label_at("Prévia visual — não é teste de física", Vector2(912, 28), 14)
	if "--capture-art" in OS.get_cmdline_user_args():
		capture()

func _process(delta: float) -> void:
	elapsed += delta
	# Flip only Kairo's visual parent. Faísca remains an independent node.
	hero_root.scale.x = -1.0 if fmod(elapsed, 6.0) > 3.0 else 1.0
	companion.position.y = 248 + sin(elapsed * 2.0) * 3.0
	restored.modulate.a = clampf((sin(elapsed * 1.1) + 1.0) * 0.5, 0.0, 1.0)
	dry.modulate.a = 1.0 - restored.modulate.a

func capture() -> void:
	await get_tree().create_timer(0.4).timeout
	await RenderingServer.frame_post_draw
	get_viewport().get_texture().get_image().save_png("res://art-review.png")
	print("ART_REVIEW_CAPTURE_OK")
	get_tree().quit()
