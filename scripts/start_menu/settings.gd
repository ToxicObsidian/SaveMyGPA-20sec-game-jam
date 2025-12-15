# Settings.gd
extends Control
class_name Settings

@export var detection_bg: ColorRect
@export var windowed_area: Control
@export var volume_master: HSlider
@export var volume_sfx: HSlider
@export var volume_bgm: HSlider

var _full_window_icon: TextureRect
var _windowed_icon: TextureRect
var _windowed: bool = false


func _ready() -> void:
	windowed_area.gui_input.connect(_on_click_change_windowed_state)
	detection_bg.gui_input.connect(_on_click_bg)

	volume_master.value_changed.connect(_on_master_volume_slider_changed)
	volume_sfx.value_changed.connect(_on_sfx_volume_slider_changed)
	volume_bgm.value_changed.connect(_on_bgm_volume_slider_changed)

	for child in windowed_area.get_children():
		var cname := child.name.to_lower()
		if cname.contains("full"):
			_full_window_icon = child
		else:
			_windowed_icon = child

	var mode := DisplayServer.window_get_mode()
	windowed = not (
		mode == DisplayServer.WINDOW_MODE_FULLSCREEN
		or mode == DisplayServer.WINDOW_MODE_EXCLUSIVE_FULLSCREEN
	)
	
	AudioManager.set_bgm_volume(0.5)
	_set_volume_sliders()


var windowed: bool:
	get:
		return _windowed
	set(value):
		_windowed = value
		_change_windowed_state()


func _change_windowed_state() -> void:
	if _windowed:
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_WINDOWED)
		_windowed_icon.visible = true
		_full_window_icon.visible = false
	else:
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_FULLSCREEN)
		_windowed_icon.visible = false
		_full_window_icon.visible = true


func _set_volume_sliders() -> void:
	volume_master.value = AudioManager.get_master_volume_linear()
	volume_sfx.value = AudioManager.get_sfx_volume_linear()
	volume_bgm.value = AudioManager.get_bgm_volume_linear()


func _on_click_change_windowed_state(event: InputEvent) -> void:
	if event is InputEventMouseButton \
		and event.button_index == MOUSE_BUTTON_LEFT \
		and event.pressed:
		windowed = not windowed


func _on_click_bg(event: InputEvent) -> void:
	if event is InputEventMouseButton \
		and event.button_index == MOUSE_BUTTON_LEFT \
		and event.pressed:
		visible = false


func _on_master_volume_slider_changed(value: float) -> void:
	AudioManager.set_master_volume(value)


func _on_sfx_volume_slider_changed(value: float) -> void:
	AudioManager.set_sfx_volume(value)


func _on_bgm_volume_slider_changed(value: float) -> void:
	AudioManager.set_bgm_volume(value)
