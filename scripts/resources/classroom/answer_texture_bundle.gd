extends Resource
class_name AnswerTextureBundle

@export var answer_texture: Texture2D
@export var oqa_texture: Texture2D
@export var answer_scale: Vector2 = Vector2.ONE
@export var oqa_scale: Vector2 = Vector2.ONE

func is_valid() -> bool:
	return answer_texture != null \
		and oqa_texture != null \
		and oqa_scale != Vector2.ZERO
