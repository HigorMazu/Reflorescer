extends SceneTree
## C20-T5: essência, upgrades e loja no totem. O save_0.json é guardado e devolvido.
## Rodar: <godot_console.exe> --headless --fixed-fps 60 --path . --script res://tools/qa/EssenceShopCheck.gd
const SAVE_FILE := "user://save_0.json"
const DAMAGE := 0 # UpgradeId.Damage

var _failures := 0
var _passes := 0
var _backup = null

func _initialize() -> void:
	_run.call_deferred()

func _check(ok: bool, what: String) -> void:
	if ok: _passes += 1
	else: _failures += 1
	print("  ", "ok    " if ok else "FALHA ", what)

func _saves() -> Node:
	return root.get_node("SaveManager")

func _run() -> void:
	if FileAccess.file_exists(SAVE_FILE):
		_backup = FileAccess.get_file_as_string(SAVE_FILE)
	await _test_economy()
	await _test_legacy_save()
	_finish()

func _test_economy() -> void:
	print("== Economia: saldo, preço e limite de níveis")
	var saves := _saves()
	saves.call("StartNewGame", 0)
	await scene_changed
	_check(saves.get("Essence") == 0, "novo jogo começa sem essência")
	_check(saves.call("GetUpgradePrice", DAMAGE) == 20, "nível 1 custa 20")
	saves.call("AddEssence", 25)
	_check(saves.get("Essence") == 25, "soma essência")
	saves.call("AddEssence", -5)
	_check(saves.get("Essence") == 25, "valor negativo é ignorado")
	_check(saves.call("TryBuyUpgrade", DAMAGE), "compra nível 1")
	_check(saves.get("Essence") == 5 and saves.call("GetUpgradeLevel", DAMAGE) == 1, "cobra 20")
	_check(saves.call("GetUpgradePrice", DAMAGE) == 40, "nível 2 custa 40")
	_check(not saves.call("TryBuyUpgrade", DAMAGE), "sem saldo não compra")
	_check(saves.get("Essence") == 5 and saves.call("GetUpgradeLevel", DAMAGE) == 1, "falha não altera nada")
	saves.call("AddEssence", 200)
	saves.call("TryBuyUpgrade", DAMAGE)
	_check(saves.call("GetUpgradePrice", DAMAGE) == 70, "nível 3 custa 70")
	saves.call("TryBuyUpgrade", DAMAGE)
	_check(saves.call("GetUpgradeLevel", DAMAGE) == 3, "chega ao nível 3")
	_check(saves.call("GetUpgradePrice", DAMAGE) == -1, "sem preço no máximo")
	var left: int = saves.get("Essence")
	_check(not saves.call("TryBuyUpgrade", DAMAGE) and saves.get("Essence") == left, "limite de 3 níveis")
	_check(left == 95, "saldo final confere (%d)" % left)
	saves.call("StartNewGame", 0)
	await scene_changed
	_check(saves.get("Essence") == 0 and saves.call("GetUpgradeLevel", DAMAGE) == 0, "novo jogo zera saldo e níveis")

func _test_legacy_save() -> void:
	print("== Save antigo com o totem no Dossel")
	var saves := _saves()
	var legacy := {
		"Slot": 0,
		"CurrentScene": "res://scenes/areas/DosselVivo.tscn",
		"CurrentArea": "DosselVivo",
		"PlayerHealth": 100,
		"PlayerMaxHealth": 100,
		"LastCheckpointId": "cp_dossel_01",
		"LastCheckpointScene": "res://scenes/areas/DosselVivo.tscn",
		"ActivatedCheckpoints": ["cp_dossel_01"],
	}
	var file := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
	file.store_string(JSON.stringify(legacy))
	file.close()
	_check(saves.call("ContinueGame", 0), "save antigo carrega")
	await scene_changed
	_check(saves.get("LastCheckpointId") == "", "última bandeira antiga foi limpa")
	_check(saves.get("LastCheckpointScene") == "", "cena da bandeira antiga foi limpa")
	_check(not saves.call("IsCheckpointActivated", "cp_dossel_01"), "bandeira antiga não fica ativa")
	_check(saves.get("Essence") == 0 and saves.call("GetUpgradeLevel", DAMAGE) == 0, "save antigo começa sem essência")
	saves.call("AddEssence", 7)
	var parsed = JSON.parse_string(FileAccess.get_file_as_string(SAVE_FILE))
	_check(parsed != null and int(parsed.get("Essence", -1)) == 7, "essência é gravada no save em disco")

func _finish() -> void:
	if _backup != null:
		var file := FileAccess.open(SAVE_FILE, FileAccess.WRITE)
		file.store_string(_backup)
		file.close()
	elif FileAccess.file_exists(SAVE_FILE):
		DirAccess.remove_absolute(SAVE_FILE)
	print("ESSENCE_SHOP_RESULT: ", _passes, " ok, ", _failures, " falha(s)")
	quit(1 if _failures > 0 else 0)
