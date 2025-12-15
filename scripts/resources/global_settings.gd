extends Resource
class_name GlobalSettings

@export var level_duration: float = 20.0
@export var difficulty: int = 0

@export var difficulty_map: Dictionary[int, float] = {}
@export var confused_map: Dictionary[int, float] = {}
@export var hesitate_map: Dictionary[int, float] = {}
@export var difficulty_desc: Dictionary[int, String] = {}

@export var multi_choice_weight: int = 5
@export var true_false_weight: int = 5
@export var fill_in_blank_weight: int = 5

@export_subgroup("Display")
@export var windowed: bool = false

var max_difficulty: int:
	get():
		return get_max_difficulty()
		
var min_difficulty: int:
	get():
		return get_min_difficulty()

# ======================
# Computed Properties
# ======================

func get_max_difficulty() -> int:
	if difficulty_map.is_empty():
		return 0
	return difficulty_map.keys().max()


func get_min_difficulty() -> int:
	if difficulty_map.is_empty():
		return 0
	return difficulty_map.keys().min()


# ======================
# Validation
# ======================

func is_valid() -> bool:
	if level_duration <= 0.0:
		return false

	if difficulty < 0:
		return false

	if not _dictionary_keys_equal(difficulty_map, confused_map):
		return false
	if not _dictionary_keys_equal(difficulty_map, hesitate_map):
		return false
	if not _dictionary_keys_equal(difficulty_map, difficulty_desc):
		return false

	if not difficulty_map.has(difficulty):
		return false

	if multi_choice_weight < 0:
		return false
	if true_false_weight < 0:
		return false
	if fill_in_blank_weight < 0:
		return false

	return true


func _dictionary_keys_equal(d1: Dictionary, d2: Dictionary) -> bool:
	if d1.size() != d2.size():
		return false

	for key in d1.keys():
		if not d2.has(key):
			return false

	return true
