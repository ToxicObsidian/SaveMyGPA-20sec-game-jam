extends Control
class_name Difficulty

@export var difficulty_slider: HSlider
@export var difficulty_desc: Label


func _ready() -> void:
	var s = GameManager.settings

	difficulty_slider.max_value = s.max_difficulty
	difficulty_slider.min_value = s.min_difficulty
	difficulty_slider.value = s.difficulty

	difficulty_desc.text = s.difficulty_desc[s.difficulty]

	difficulty_slider.value_changed.connect(_on_difficulty_changed)


func _on_difficulty_changed(value: float) -> void:
	var to_difficulty: int = int(value + 0.01)

	GameManager.settings.difficulty = to_difficulty
	difficulty_desc.text = GameManager.settings.difficulty_desc[to_difficulty]
