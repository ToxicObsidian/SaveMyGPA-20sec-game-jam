extends Node2D
class_name LoadingScreen

@export var loading_overlay: Control
@export var loading_progress_bar: ProgressBar
@export var tips_label: Label
@export var loading_item_label: Label

@export var stock_tips: Array[String] = []
@export var enable_tip_change: bool = true

var _is_loading: bool = false
var _cur_tips_index: int = 0


func _ready() -> void:
	if loading_overlay:
		loading_overlay.gui_input.connect(_on_click_overlay)

	if stock_tips.size() > 0:
		stock_tips.shuffle()
		tips_label.text = stock_tips[0]


# ======================
# State
# ======================

func is_loading() -> bool:
	return _is_loading


# ======================
# Loading Screen Control
# ======================

func start_loading_screen(min_value: float, max_value: float) -> void:
	_is_loading = true
	loading_item_label.visible = true
	enable_tip_change = true

	loading_progress_bar.min_value = min_value
	loading_progress_bar.max_value = max_value
	loading_progress_bar.value = min_value


func end_loading_screen() -> void:
	_is_loading = false
	loading_item_label.visible = false
	enable_tip_change = false

	loading_progress_bar.min_value = 0.0
	loading_progress_bar.max_value = 1.0
	loading_progress_bar.value = 0.0


# ======================
# Input
# ======================

func _on_click_overlay(event: InputEvent) -> void:
	if not enable_tip_change:
		return

	if event is InputEventMouseButton \
	and event.button_index == MOUSE_BUTTON_LEFT \
	and event.pressed:
		_cur_tips_index += 1

		if _cur_tips_index >= stock_tips.size():
			_cur_tips_index = 0
			stock_tips.shuffle()

		if stock_tips.size() > 0:
			tips_label.text = stock_tips[_cur_tips_index]
