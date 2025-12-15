extends Control
class_name VisualizeQuestion

@export var question: QuestionData
@export var controller: QuestionController

@export var question_oqa_position: Vector2:
	get:
		return Vector2.ZERO if question == null else question.on_question_answer_position
	set(value):
		if question != null:
			question.on_question_answer_position = value

@export var question_oqa_scale: Vector2:
	get:
		return Vector2.ONE if question == null else question.correct_answer.oqa_scale
	set(value):
		if question != null:
			question.correct_answer.oqa_scale = value


func _ready() -> void:
	var answers: Array[Texture2D] = []
	answers.append(question.correct_answer.answer_texture)
	
	for wrong_answer in question.wrong_answer_options:
		answers.append(wrong_answer.answer_texture)
	
	controller.setup(
		question.question_texture,
		answers,
		question.on_question_answer_position,
		true
	)
	
	question_oqa_position = question.on_question_answer_position
	question_oqa_scale = question.correct_answer.oqa_scale
	controller.vertical_answer_layout = question.vertical_layout


func _process(_delta: float) -> void:
	controller.set_oqa(question.correct_answer.oqa_texture, question_oqa_scale)
	controller.on_question_answer_anchor.position = question_oqa_position
