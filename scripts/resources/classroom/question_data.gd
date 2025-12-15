extends Resource
class_name QuestionData

enum QuestionType {
	MULTI_CHOICE,
	TRUE_FALSE,
	FILL_IN_BLANK
}

@export var type: QuestionType
@export var score: int = 10

@export_group("Question")
@export var question_texture: Texture2D

@export_group("Answers")
@export var wrong_answer_options: Array[AnswerTextureBundle]
@export var correct_answer: AnswerTextureBundle
@export var on_question_answer_position: Vector2
@export var vertical_layout: bool = false

@export_group("Meta")
@export var description: String
@export var tags: Array[String]

func is_valid() -> bool:
	if question_texture == null:
		return false
	for wrong in wrong_answer_options:
		if wrong == null or not wrong.is_valid():
			return false
	if correct_answer == null or not correct_answer.is_valid():
		return false
	return true

func all_textures() -> Array[AnswerTextureBundle]:
	var result: Array[AnswerTextureBundle] = []
	result.append(correct_answer)
	result.append_array(wrong_answer_options)
	return result
