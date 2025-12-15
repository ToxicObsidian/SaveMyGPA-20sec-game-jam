extends Resource
class_name ClassmateData

@export_subgroup("Outfits")
@export var boy_postures: Array[Texture2D]
@export var girl_postures: Array[Texture2D]
@export var boy_hairs: Array[Texture2D]
@export var girl_hairs: Array[Texture2D]
@export var boy_outfits: Array[Texture2D]
@export var girl_outfits: Array[Texture2D]
@export var complete_outfits: Array[ClassmateCompleteOutfitData]
@export var allow_complete_outfits_duplicate: int = 1

@export_subgroup("Emotions")
@export var classmate_reply_emotions: Dictionary[Emotions.EmotionType, Texture2D]
@export var classmate_normal_emotions: Array[Texture2D]

func _validate_property(_property: Dictionary) -> void:
	_clear_array(boy_postures)
	_clear_array(girl_postures)
	_clear_array(boy_hairs)
	_clear_array(girl_hairs)
	_clear_array(boy_outfits)
	_clear_array(girl_outfits)
	_clear_array(complete_outfits)

func _clear_array(array: Array) -> void:
	for i in range(array.size() - 1, -1, -1):
		if array[i] == null:
			array.remove_at(i)
