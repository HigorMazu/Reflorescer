extends SceneTree
## C20-T6 (09/11/2026): a árvore do portal de volta ao Sopé cobria o totem e o Branch1 do Dossel Vivo.
## Confere a camada efetiva: a árvore fica atrás do que é jogável e na frente do fundo.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/DosselPortalLayerCheck.gd
const DOSSEL := "res://scenes/areas/DosselVivo.tscn"

var _failures := 0
var _passes := 0

func _initialize() -> void:
	_run.call_deferred()

func _check(ok: bool, what: String) -> void:
	if ok: _passes += 1
	else: _failures += 1
	print("  ", "ok    " if ok else "FALHA ", what)

func _z(node: CanvasItem) -> int:
	var total := 0
	var current: Node = node
	while current is CanvasItem:
		var item := current as CanvasItem
		total += item.z_index
		if not item.z_as_relative:
			break
		current = current.get_parent()
	return total

func _lowest_z(node: Node) -> int:
	var lowest := 4096
	if node is CanvasItem:
		lowest = _z(node)
	for child in node.get_children():
		lowest = mini(lowest, _lowest_z(child))
	return lowest

func _run() -> void:
	var scene: Node = (load(DOSSEL) as PackedScene).instantiate()
	root.add_child(scene)
	await process_frame
	await process_frame

	var portal := scene.get_node("Transitions/ToSopeDaMata/PortalVisual") as CanvasItem
	var upper := portal.get_node("UpperCanopy") as CanvasItem
	var totem := scene.get_node("Interactables/Checkpoint1/TotemVisual") as CanvasItem
	var geometry := scene.get_node("Geometry") as CanvasItem
	var backdrop := scene.get_node("Backdrop") as CanvasItem
	var player := scene.get_node("Player")

	_check(_z(portal) < _z(totem), "árvore atrás do totem (%d < %d)" % [_z(portal), _z(totem)])
	_check(_z(portal) < _z(geometry), "árvore atrás dos galhos (%d < %d)" % [_z(portal), _z(geometry)])
	_check(_z(portal) < _lowest_z(player), "árvore atrás de todas as camadas do Kairo (%d < %d)" % [_z(portal), _lowest_z(player)])
	_check(_z(upper) > _z(backdrop), "copa continua na frente do fundo (%d > %d)" % [_z(upper), _z(backdrop)])
	_check(_z(upper) < _z(portal), "copa continua atrás do tronco (%d < %d)" % [_z(upper), _z(portal)])

	print("DosselPortalLayerCheck: ", _passes, " ok, ", _failures, " falhas")
	quit(1 if _failures > 0 else 0)
