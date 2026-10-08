extends Node2D
## Replaces branch placeholders visually while retaining their collision shapes.

const BRANCHES: Texture2D = preload("res://assets/environment/tropical/dossel_branches_candidate_v1.png")
const TRUNKS: Texture2D = preload("res://assets/environment/tropical/dossel_trunks_candidate_v1.png")
const COLUMN_WIDTH := 724.0

func _ready() -> void:
	for body in get_children():
		if not _is_branch(body) and not body.name.begins_with("Trunk"):
			continue
		var visual := body.get_node_or_null("Visual") as CanvasItem
		var top := body.get_node_or_null("Top") as CanvasItem
		if visual != null:
			visual.visible = false
		if top != null:
			top.visible = false
	queue_redraw()

func _draw() -> void:
	for name in ["TrunkA", "TrunkB"]:
		var body := get_node(name) as StaticBody2D
		var collision := body.get_node("CollisionShape2D") as CollisionShape2D
		var rectangle := collision.shape as RectangleShape2D
		var wide: bool = name == "TrunkA"
		var visual_width := 86.0 if wide else 76.0
		var source := Rect2(220, 0, 480, 1024) if wide else Rect2(980, 0, 390, 1024)
		draw_texture_rect_region(TRUNKS,
			Rect2(body.position.x - visual_width * 0.5,
				body.position.y - rectangle.size.y * 0.5,
				visual_width, rectangle.size.y), source)

	var index := 0
	for body in get_children():
		if not _is_branch(body):
			continue
		var collision := body.get_node("CollisionShape2D") as CollisionShape2D
		var rectangle := collision.shape as RectangleShape2D
		var width := rectangle.size.x
		var height := 48.0 if width <= 140.0 else 56.0
		var destination := Rect2(body.position.x - width * 0.5, body.position.y - 16.0,
			width, height)
		var source := Rect2(float(index % 3) * COLUMN_WIDTH, 180.0,
			COLUMN_WIDTH, 350.0)
		draw_texture_rect_region(BRANCHES, destination, source)
		index += 1

func _is_branch(body: Node) -> bool:
	return body.name.begins_with("Branch") or body.name == "TopBranch"
