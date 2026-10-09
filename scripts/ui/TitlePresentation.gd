extends Control
## Presentation only. MainMenu keeps ownership of save and navigation actions.

var elapsed := 0.0
var pointer := Vector2.ZERO
var motes: Array[Vector4] = []
var leaving := false
var intro: Tween
@onready var menu: Control = get_parent()
@onready var backdrop: TextureRect = menu.get_node("Background")
@onready var mist: ColorRect = menu.get_node("Mist")
@onready var curtain: ColorRect = menu.get_node("Curtain")

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	var rng := RandomNumberGenerator.new()
	rng.seed = 4709
	for i in range(34):
		motes.append(Vector4(rng.randf_range(0.38, 0.96), rng.randf_range(0.20, 0.90),
			rng.randf_range(0.0, TAU), rng.randf_range(0.6, 1.4)))
	curtain.mouse_filter = Control.MOUSE_FILTER_IGNORE
	curtain.color = Color.BLACK
	menu.get_node("Box").modulate.a = 0.0
	await get_tree().process_frame
	for child in menu.get_node("Box").get_children():
		if child is Button:
			child.mouse_entered.connect(func():
				if not leaving and not child.disabled: child.grab_focus())
	intro = create_tween().set_parallel(true)
	intro.tween_property(curtain, "color:a", 0.0, 1.2)
	intro.tween_property(menu.get_node("Box"), "modulate:a", 1.0, 1.0).set_delay(0.35)

func _process(delta: float) -> void:
	elapsed += delta
	var desired := (get_local_mouse_position() / size - Vector2(0.5, 0.5)).clamp(Vector2(-0.5, -0.5), Vector2(0.5, 0.5))
	pointer = pointer.lerp(desired, 1.0 - exp(-delta * 2.0))
	backdrop.material.set_shader_parameter("elapsed", elapsed)
	backdrop.material.set_shader_parameter("pointer", pointer)
	mist.material.set_shader_parameter("elapsed", elapsed)
	queue_redraw()

func _draw() -> void:
	for mote in motes:
		var phase := elapsed * 0.38 * mote.w + mote.z
		var point := Vector2(mote.x * size.x + sin(phase) * 23.0,
			mote.y * size.y + cos(phase * 0.71) * 16.0)
		var pulse := 0.3 + 0.7 * pow(0.5 + 0.5 * sin(phase * 2.0), 2.0)
		for ring in range(4, 0, -1):
			draw_circle(point, float(ring) * 2.2 * mote.w, Color(0.97, 0.73, 0.30, pulse * 0.028))
		draw_circle(point, mote.w, Color(1.0, 0.88, 0.54, pulse * 0.85))
	# A quiet sprout above the wordmark, drawn in the same gold as selection.
	var origin := Vector2(size.x * 0.085 + 9, size.y * 0.23 - 28)
	var gold := Color(0.77, 0.72, 0.45, 0.8)
	draw_line(origin, origin + Vector2(0, -18), gold, 1.2, true)
	draw_colored_polygon(PackedVector2Array([origin + Vector2(0, -9), origin + Vector2(-12, -19), origin + Vector2(-10, -9)]), gold)
	draw_colored_polygon(PackedVector2Array([origin + Vector2(0, -16), origin + Vector2(11, -25), origin + Vector2(9, -15)]), gold)
	var focus := get_viewport().gui_get_focus_owner()
	if focus is Button and focus.get_parent() == menu.get_node("Box") and not leaving:
		var p := focus.global_position - global_position + Vector2(-15, focus.size.y * 0.5)
		var pulse := 0.7 + 0.3 * sin(elapsed * 2.0)
		draw_circle(p, 6.0, Color(0.9, 0.76, 0.34, pulse * 0.07))
		draw_colored_polygon(PackedVector2Array([p + Vector2(-3, 0), p + Vector2(0, -4), p + Vector2(3, 0), p + Vector2(0, 4)]), Color(0.89, 0.80, 0.48, pulse))

func begin_exit() -> void:
	leaving = true
	if intro != null: intro.kill()
	curtain.mouse_filter = Control.MOUSE_FILTER_STOP
	create_tween().tween_property(curtain, "color:a", 1.0, 0.42)

func cancel_exit() -> void:
	leaving = false
	curtain.mouse_filter = Control.MOUSE_FILTER_IGNORE
	create_tween().tween_property(curtain, "color:a", 0.0, 0.3)
	create_tween().tween_property(menu.get_node("Box"), "modulate:a", 1.0, 0.3)
