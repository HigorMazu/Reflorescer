extends Node

func _ready() -> void:
	RenderingServer.set_default_clear_color(Color("182c25"))
	var hud: CanvasLayer = load("res://scenes/ui/HUD.tscn").instantiate()
	add_child(hud)
	await get_tree().process_frame
	var health = hud.get_node("MarginContainer/VBoxContainer/TopRow/HealthSection/HealthBar")
	health.call("SetValues", 72, 100)
	var health_label: Label = hud.get_node("MarginContainer/VBoxContainer/TopRow/HealthSection/HealthLabel")
	health_label.text = "72 / 100"
	var prompt: Label = hud.get_node("MarginContainer/VBoxContainer/BottomRow/InteractionPrompt")
	prompt.text = "[E] Restaurar"
	prompt.visible = true
	await get_tree().create_timer(0.2).timeout
	await RenderingServer.frame_post_draw
	get_viewport().get_texture().get_image().save_png("res://.godot/hud-visual-v1.png")
	get_tree().quit()
