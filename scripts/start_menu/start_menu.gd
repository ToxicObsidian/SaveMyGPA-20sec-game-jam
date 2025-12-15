extends Node2D

@export_group("Buttons")
@export var StartGameBtn: BaseButton
@export var QuitGameBtn: BaseButton
@export var AboutBtn: BaseButton
@export var SettingsBtn: BaseButton

@export_group("Prefabs")
@export var AboutPage: Control
@export var SettingsPage: Control


func _ready() -> void:
	StartGameBtn.pressed.connect(_on_start_btn_pressed)
	QuitGameBtn.pressed.connect(_on_quit_btn_pressed)
	AboutBtn.pressed.connect(_on_about_btn_pressed)
	SettingsBtn.pressed.connect(_on_settings_btn_pressed)

	AudioManager.play_bgm("start_menu")


func _on_start_btn_pressed() -> void:
	AudioManager.play_sfx("start_game")
	AudioManager.fade_out_current_bgm(1.0)
	await GameManager.start_game()


func _on_about_btn_pressed() -> void:
	if not AboutPage.visible:
		AboutPage.visible = true


func _on_settings_btn_pressed() -> void:
	if not SettingsPage.visible:
		SettingsPage.visible = true


func _on_quit_btn_pressed() -> void:
	GameManager.quit_game()
