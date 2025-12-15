extends Node2D
class_name EndGameMenu

signal on_request_try_again
signal on_request_back_to_menu

@export var Points: Label
@export var Win: Control
@export var WinTips: Label
@export var WinEmote: Sprite2D
@export var Lose: Control
@export var LoseTips: Label
@export var LoseEmote: Sprite2D
@export var LoseTipsBundle: Array[String]
@export var TryAgainButton: BaseButton
@export var BackToMenuButton: BaseButton

func _ready() -> void:
	visible = false
	TryAgainButton.pressed.connect(func(): emit_signal("on_request_try_again"))
	BackToMenuButton.pressed.connect(func(): emit_signal("on_request_back_to_menu"))

func set_settlement(win: bool, got_score: float, total_score: float, notify_flipping: bool) -> void:
	Win.visible = win
	WinEmote.visible = win
	Lose.visible = not win
	LoseEmote.visible = not win

	Points.text = "Points: %d/%d" % [int(got_score), int(total_score)]

	if win:
		WinTips.text = "Congratulations."
	else:
		LoseTips.text = LoseTipsBundle[1] if notify_flipping else LoseTipsBundle[0]

	visible = true
