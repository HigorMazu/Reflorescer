extends Control

# Cartografia de exploração: nenhuma sala é desenhada antes de Kairo atravessá-la.
const CELL := Vector2(52, 44)
const AREAS := {
	"SopeDaMata": {
		"label": "SOPÉ DA MATA", "origin": Vector2(80, 375),
		"bounds": Rect2(0, -200, 2400, 720), "cols": 6, "rows": 2,
		"fill": Color("294338"), "edge": Color("96bd8b")
	},
	"DosselVivo": {
		"label": "DOSSEL VIVO", "origin": Vector2(392, 155),
		"bounds": Rect2(0, -900, 1600, 1440), "cols": 4, "rows": 6,
		"fill": Color("294e46"), "edge": Color("9ed5b1")
	},
	"IgarapeSufocado": {
		"label": "IGARAPÉ SUFOCADO", "origin": Vector2(600, 155),
		"bounds": Rect2(0, -200, 2800, 720), "cols": 7, "rows": 2,
		"fill": Color("344455"), "edge": Color("97b9c3")
	},
	"CovilDeKorrag": {
		"label": "COVIL DE KORRAG", "origin": Vector2(964, 155),
		"bounds": Rect2(-640, -280, 1280, 720), "cols": 3, "rows": 2,
		"fill": Color("514136"), "edge": Color("d0ac78")
	}
}

var _current_area := ""
var _current_col := -1
var _current_row := -1
var _settle_frames := 0
var _last_scene := ""
var _game_manager: Node
var _save_manager: Node


func _ready() -> void:
	_game_manager = get_node("/root/GameManager")
	_save_manager = get_node("/root/SaveManager")
	process_mode = Node.PROCESS_MODE_ALWAYS
	mouse_filter = Control.MOUSE_FILTER_STOP
	visible = false


func _process(_delta: float) -> void:
	var scene := get_tree().current_scene
	if scene == null:
		return
	var scene_path: String = scene.scene_file_path
	if scene_path != _last_scene:
		_last_scene = scene_path
		_settle_frames = 0
		_current_area = ""
		_current_col = -1
		_current_row = -1
	var area: String = scene_path.get_file().get_basename()
	var hint := get_node_or_null("../MarginContainer/VBoxContainer/TopRow/MapHint") as CanvasItem
	if hint != null:
		hint.visible = AREAS.has(area)
	if AREAS.has(area) and not get_tree().paused:
		# O spawn de retorno e o carregamento do save são aplicados após _ready.
		_settle_frames += 1
		if _settle_frames >= 3:
			_record_player_cell(area)
	var map_open: bool = _game_manager.IsWorldMapOpen() and AREAS.has(area)
	if visible != map_open:
		visible = map_open
	if visible:
		queue_redraw()


func _record_player_cell(area: String) -> void:
	var player := get_tree().get_first_node_in_group("Player") as Node2D
	if player == null:
		return
	var definition: Dictionary = AREAS[area]
	var bounds: Rect2 = definition.bounds
	var cols: int = definition.cols
	var rows: int = definition.rows
	var col := clampi(floori((player.global_position.x - bounds.position.x) / (bounds.size.x / cols)), 0, cols - 1)
	var row := clampi(floori((player.global_position.y - bounds.position.y) / (bounds.size.y / rows)), 0, rows - 1)
	if area == _current_area and col == _current_col and row == _current_row:
		return
	_current_area = area
	_current_col = col
	_current_row = row
	_save_manager.RevealMapCell(_cell_id(area, col, row))


func _cell_id(area: String, col: int, row: int) -> String:
	return "%s:%d:%d" % [area, col, row]


func _is_known(area: String, col: int, row: int) -> bool:
	return _save_manager.IsMapCellRevealed(_cell_id(area, col, row))


func _cell_center(area: String, col: int, row: int) -> Vector2:
	return AREAS[area].origin + Vector2((col + 0.5) * CELL.x, (row + 0.5) * CELL.y)


func _draw() -> void:
	if not visible:
		return
	var scale_factor: float = minf(size.x / 1280.0, size.y / 720.0)
	var offset := (size - Vector2(1280, 720) * scale_factor) * 0.5
	draw_set_transform(offset, 0.0, Vector2.ONE * scale_factor)
	draw_rect(Rect2(0, 0, 1280, 720), Color(0.015, 0.03, 0.027, 1.0))
	draw_rect(Rect2(24, 23, 1232, 674), Color("a8bb8b"), false, 2.0)
	draw_rect(Rect2(34, 33, 1212, 654), Color("526d57"), false, 1.0)
	_draw_corner_flourish(Vector2(54, 54), 1.0, 1.0)
	_draw_corner_flourish(Vector2(1226, 54), -1.0, 1.0)
	_draw_corner_flourish(Vector2(54, 666), 1.0, -1.0)
	_draw_corner_flourish(Vector2(1226, 666), -1.0, -1.0)
	var font := ThemeDB.fallback_font
	draw_string(font, Vector2(508, 83), "MAPA DE REFLORESCER", HORIZONTAL_ALIGNMENT_CENTER, 270, 25, Color("e5ddbc"))
	draw_line(Vector2(500, 97), Vector2(780, 97), Color("738e71"), 1.0)
	draw_string(font, Vector2(80, 616), "Apenas caminhos percorridos aparecem no mapa", HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Color("bdc7ac"))
	draw_string(font, Vector2(958, 616), "M / ESC  ·  FECHAR", HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Color("bdc7ac"))

	# Passagens são reveladas somente quando os dois lados foram visitados.
	_draw_connection("SopeDaMata", 5, 1, "DosselVivo", 0, 5)
	_draw_connection("DosselVivo", 3, 1, "IgarapeSufocado", 0, 1)
	_draw_connection("IgarapeSufocado", 6, 1, "CovilDeKorrag", 0, 1)
	for area in AREAS:
		_draw_area(area, font)
	if _current_area != "" and _is_known(_current_area, _current_col, _current_row):
		var center := _cell_center(_current_area, _current_col, _current_row)
		var pulse := 0.75 + 0.25 * sin(Time.get_ticks_msec() / 210.0)
		draw_circle(center, 10.0, Color(0.95, 0.91, 0.63, pulse))
		draw_circle(center, 4.5, Color("1a2c23"))
		draw_string(font, center + Vector2(15, -12), "KAIRO", HORIZONTAL_ALIGNMENT_LEFT, -1, 13, Color("f5ebc6"))


func _draw_area(area: String, font: Font) -> void:
	var definition: Dictionary = AREAS[area]
	var known_count := 0
	for row in definition.rows:
		for col in definition.cols:
			if not _is_known(area, col, row):
				continue
			known_count += 1
			var center := _cell_center(area, col, row)
			var rect := Rect2(center - CELL * 0.5, CELL)
			var fill: Color = definition.fill.lightened(0.035 * ((col * 3 + row) % 3))
			draw_rect(rect, fill)
			# Traços internos lembram saliências, galhos e galerias, sem grade entre salas.
			if (col + row * 2) % 3 == 0:
				draw_line(center + Vector2(-16, 9), center + Vector2(2, 9), definition.edge.darkened(0.38), 1.0)
				draw_line(center + Vector2(2, 9), center + Vector2(7, 5), definition.edge.darkened(0.38), 1.0)
			if (col * 2 + row) % 5 == 0:
				draw_arc(center + Vector2(12, -7), 5, 0.0, PI, 9, definition.edge.darkened(0.45), 1.0)
	for row in definition.rows:
		for col in definition.cols:
			if not _is_known(area, col, row):
				continue
			_draw_room_edges(area, col, row, definition.edge)
	if known_count > 0:
		var text_pos: Vector2 = definition.origin + Vector2(0, -13)
		draw_string(font, text_pos, definition.label, HORIZONTAL_ALIGNMENT_LEFT, -1, 13, definition.edge)


func _draw_room_edges(area: String, col: int, row: int, ink: Color) -> void:
	var center := _cell_center(area, col, row)
	var left := center.x - CELL.x * 0.5
	var right := center.x + CELL.x * 0.5
	var top := center.y - CELL.y * 0.5
	var bottom := center.y + CELL.y * 0.5
	if not _is_known(area, col, row - 1):
		var rise := 3.0 + float((col * 7 + row * 3) % 5)
		_draw_polyline(PackedVector2Array([
			Vector2(left, top + 2), Vector2(left + 11, top + 2),
			Vector2(left + 14, top - rise), Vector2(right - 12, top - rise),
			Vector2(right - 9, top + 2), Vector2(right, top + 2)
		]), ink)
	if not _is_known(area, col, row + 1):
		var dip := 2.0 + float((col * 5 + row) % 6)
		_draw_polyline(PackedVector2Array([
			Vector2(left, bottom - 2), Vector2(left + 8, bottom - 2),
			Vector2(left + 12, bottom + dip), Vector2(right - 14, bottom + dip),
			Vector2(right - 10, bottom - 2), Vector2(right, bottom - 2)
		]), ink)
	if not _is_known(area, col - 1, row):
		_draw_polyline(PackedVector2Array([
			Vector2(left + 2, top), Vector2(left - 2, top + 12),
			Vector2(left - 2, bottom - 10), Vector2(left + 2, bottom)
		]), ink)
	if not _is_known(area, col + 1, row):
		_draw_polyline(PackedVector2Array([
			Vector2(right - 2, top), Vector2(right + 2, top + 10),
			Vector2(right + 2, bottom - 12), Vector2(right - 2, bottom)
		]), ink)


func _draw_polyline(points: PackedVector2Array, ink: Color) -> void:
	for index in points.size() - 1:
		draw_line(points[index], points[index + 1], ink, 1.5)


func _draw_connection(a: String, ac: int, ar: int, b: String, bc: int, br: int) -> void:
	if not _is_known(a, ac, ar) or not _is_known(b, bc, br):
		return
	var start := _cell_center(a, ac, ar)
	var end := _cell_center(b, bc, br)
	var mid := Vector2((start.x + end.x) * 0.5, start.y)
	draw_line(start, mid, Color("9aab88"), 6.0)
	draw_line(mid, Vector2(mid.x, end.y), Color("9aab88"), 6.0)
	draw_line(Vector2(mid.x, end.y), end, Color("9aab88"), 6.0)


func _draw_corner_flourish(at: Vector2, horizontal: float, vertical: float) -> void:
	var ink := Color("9eb28d")
	var soft := Color("607a64")
	# Dois ramos finos saem do canto e se curvam para dentro da moldura.
	_draw_corner_curve(at, Vector2(22, 0), Vector2(45, 3), Vector2(77, 13), horizontal, vertical, ink, 1.6)
	_draw_corner_curve(at, Vector2(0, 18), Vector2(2, 38), Vector2(14, 61), horizontal, vertical, ink, 1.6)
	# Gavinhas menores substituem os círculos rígidos da primeira versão.
	_draw_corner_curve(at, Vector2(27, 3), Vector2(35, 20), Vector2(50, 19), horizontal, vertical, soft, 1.15)
	_draw_corner_curve(_corner_point(at, Vector2(50, 19), horizontal, vertical), Vector2(8, 0), Vector2(8, -10), Vector2(-1, -10), horizontal, vertical, soft, 1.15)
	_draw_corner_curve(at, Vector2(3, 26), Vector2(19, 32), Vector2(18, 47), horizontal, vertical, soft, 1.15)
	_draw_leaf(at, Vector2(31, 5), horizontal, vertical, ink)
	_draw_leaf(at, Vector2(7, 31), horizontal, vertical, ink)
	_draw_leaf(at, Vector2(60, 9), horizontal, vertical, soft)


func _draw_corner_curve(at: Vector2, c1: Vector2, c2: Vector2, finish: Vector2, horizontal: float, vertical: float, ink: Color, width: float) -> void:
	var start := at
	var control_1 := _corner_point(at, c1, horizontal, vertical)
	var control_2 := _corner_point(at, c2, horizontal, vertical)
	var end := _corner_point(at, finish, horizontal, vertical)
	var previous := start
	for step in range(1, 17):
		var point := start.bezier_interpolate(control_1, control_2, end, step / 16.0)
		draw_line(previous, point, ink, width, true)
		previous = point


func _draw_leaf(at: Vector2, local_center: Vector2, horizontal: float, vertical: float, ink: Color) -> void:
	var center := _corner_point(at, local_center, horizontal, vertical)
	var along := Vector2(horizontal, vertical).normalized()
	var across := Vector2(-along.y, along.x)
	var points := PackedVector2Array([
		center - along * 7.0,
		center + across * 3.2,
		center + along * 7.0,
		center - across * 3.2
	])
	draw_colored_polygon(points, Color(ink, 0.42))
	draw_polyline(PackedVector2Array([points[0], points[1], points[2], points[3], points[0]]), ink, 1.0, true)


func _corner_point(at: Vector2, local: Vector2, horizontal: float, vertical: float) -> Vector2:
	return at + Vector2(local.x * horizontal, local.y * vertical)
