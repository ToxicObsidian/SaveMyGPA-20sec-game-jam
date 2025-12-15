extends Node

@export var version_name: String = "0.0.1a"

@export_range(0, 50)
var BatchLPF: int = 5

@export_range(0, 100)
var BatchTPPF: int = 10

@export var settings: GlobalSettings

@export_group("Scenes")
@export var start_scene: PackedScene
@export var loading_screen_scene: PackedScene

var game_root: Control
var start_menu: Node2D


func start_game() -> void:
	if not settings or not settings.is_valid():
		print("Invalid settings, quit")
		quit_game(-1)
		return

	var sl_info := StartLevelInfo.new()
	sl_info.duration = settings.level_duration
	sl_info.excluded_tags = []

	sl_info.difficulty = settings.difficulty
	sl_info.wrong_ratio = settings.difficulty_map[settings.difficulty]
	sl_info.confused_ratio = settings.confused_map[settings.difficulty]
	sl_info.hesitate_ratio = settings.hesitate_map[settings.difficulty]
	sl_info.max_difficulty = settings.max_difficulty

	await LevelManager.start_level(sl_info)


func quit_game(code: int = 0) -> void:
	get_tree().quit(code)


func register_root(root: Control) -> void:
	game_root = root

	if not loading_screen_scene or not start_scene:
		printerr("PackedScene is null.")
		quit_game()
		return

	var loading_screen := loading_screen_scene.instantiate()
	if not loading_screen is LoadingScreen:
		printerr("LoadingScreenScene is not LoadingScreenManager.")
		quit_game()
		return

	root.add_child(loading_screen)

	start_menu = start_scene.instantiate()
	root.add_child(start_menu)

	LevelManager.set_loading_screen(loading_screen)


func notify_level_started() -> void:
	if start_menu:
		start_menu.visible = false
	AudioManager.play_bgm("classroom")


func notify_level_ended() -> void:
	if start_menu:
		start_menu.visible = true
	AudioManager.play_bgm("start_menu")
