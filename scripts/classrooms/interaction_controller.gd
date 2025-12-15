extends Control
class_name InteractionController
signal on_query_answer(index: int)

@export var InteractContainer: GridContainer
@export var ButtonPrefab: PackedScene

@export var GridHSeperation: int = 4:
	set(value):
		GridHSeperation = value
		_update_grid_seperation()

@export var GridVSeperation: int = 4:
	set(value):
		GridVSeperation = value
		_update_grid_seperation()


func _ready() -> void:
	_update_grid_seperation()


# Set buttons, the latter ones will be hidden.
func set_buttons(total_count: int, hide_count: int) -> void:
	_clear_buttons()

	for i in range(1, total_count + 1):
		if i % GameManager.BatchLPF == 0:
			await GameManager.game_root.get_tree().process_frame

		var button = ButtonPrefab.instantiate()
		if not button is InteractionButton:
			push_error("The given button prefab is not an interaction button")
			return

		button.text = str(i)
		button.index = i
		if i + hide_count > total_count:
			button.visible = false

		button.pressed.connect(func(): _on_button_pressed(button.index))
		InteractContainer.add_child(button)


func set_all_button_visible() -> void:
	for button in InteractContainer.get_children():
		if button is Button:
			button.visible = true


func set_button_disabled(index: int, disabled: bool) -> void:
	var buttons = InteractContainer.get_children()
	if index >= 0 and index < buttons.size() and buttons[index] is BaseButton:
		var b := buttons[index] as BaseButton
		b.disabled = disabled
		b.mouse_default_cursor_shape = CURSOR_ARROW if disabled else CURSOR_POINTING_HAND


func set_all_buttons_disabled(disabled: bool) -> void:
	for button in InteractContainer.get_children():
		if button is BaseButton:
			var b := button as BaseButton
			b.disabled = disabled
			b.mouse_default_cursor_shape = CURSOR_ARROW if disabled else CURSOR_POINTING_HAND


func _clear_buttons() -> void:
	for button in InteractContainer.get_children():
		button.queue_free()


func _on_button_pressed(index: int) -> void:
	emit_signal("on_query_answer", index - 1)


func _update_grid_seperation() -> void:
	if InteractContainer:
		InteractContainer.add_theme_constant_override("h_seperation", GridHSeperation)
		InteractContainer.add_theme_constant_override("v_seperation", GridVSeperation)
