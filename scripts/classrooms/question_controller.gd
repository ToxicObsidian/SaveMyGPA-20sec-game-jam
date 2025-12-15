extends VBoxContainer
class_name QuestionController

@export_group("Prefab Setting")
@export var question_rect: TextureRect
@export var on_question_answer_anchor: Node2D
@export var on_question_answer_rect: TextureRect
@export var answer_layout_container: HBoxContainer
@export var qa_spacing: Control
@export var qa_offset: Control

@export_group("Question Visualization")
@export var type: QuestionData.QuestionType

@export_group("Creative/Debug")
@export var visualized_data: QuestionData

@export var display_answers: bool:
	get:
		return _display_answers
	set(value):
		_display_answers = value
		answer_layout_container.visible = value

@export var vertical_answer_layout: bool:
	get:
		return _vertical_layout
	set(value):
		_set_val(value)

@export var offset: float:
	get:
		return _qa_offset_value
	set(value):
		_qa_offset_value = value
		_update_spacing()

@export var spacing: float:
	get:
		return _qa_spacing_value
	set(value):
		_qa_spacing_value = value
		_update_spacing()


var _h_answers: HBoxContainer = HBoxContainer.new()
var _v_answers: VBoxContainer = VBoxContainer.new()
var _display_answers: bool = false
var _vertical_layout: bool = false
var _qa_offset_value: float = 0.0
var _qa_spacing_value: float = 0.0


func _ready() -> void:
	on_question_answer_rect.visible = false
	add_theme_constant_override("seperation", 0)
	answer_layout_container.add_theme_constant_override("seperation", 0)
	_h_answers.add_theme_constant_override("seperation", 0)
	_v_answers.add_theme_constant_override("seperation", 0)
	_update_spacing()


func setup(
	question_texture: Texture2D,
	answer_textures: Array[Texture2D],
	oqa_position: Vector2,
	display_answers_: bool
) -> void:
	question_rect.texture = question_texture

	for answer_texture in answer_textures:
		var rect := TextureRect.new()
		rect.texture = answer_texture
		rect.expand_mode = TextureRect.EXPAND_KEEP_SIZE
		rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT
		var container: BoxContainer = (_v_answers as BoxContainer) if _vertical_layout else (_h_answers as BoxContainer)
		container.add_child(rect)

	display_answers = display_answers_
	on_question_answer_anchor.position = oqa_position

	if _h_answers.get_parent():
		_h_answers.get_parent().remove_child(_h_answers)
	if _v_answers.get_parent():
		_v_answers.get_parent().remove_child(_v_answers)

	answer_layout_container.add_child((_v_answers as BoxContainer) if _vertical_layout else (_h_answers as BoxContainer))

	print("Visibilities: H: ", _h_answers.visible, ", V: ", _v_answers.visible)


func set_oqa(oqa_texture: Texture2D, oqa_scale: Vector2) -> void:
	var oqa_rect_size: Vector2 = oqa_texture.get_size() * oqa_scale
	var oqa_rect_position: Vector2 = -(oqa_rect_size / 2.0)
	on_question_answer_rect.position = oqa_rect_position
	on_question_answer_rect.size = oqa_rect_size
	on_question_answer_rect.texture = oqa_texture
	on_question_answer_rect.visible = true


func _set_val(set_to: bool) -> void:
	if _vertical_layout != set_to:
		var from: BoxContainer = (_v_answers as BoxContainer) if _vertical_layout else (_h_answers as BoxContainer)
		var to: BoxContainer = (_h_answers as BoxContainer) if _vertical_layout else (_v_answers as BoxContainer)

		var nodes := from.get_children()
		for node in nodes:
			from.remove_child(node)
			to.add_child(node)

		answer_layout_container.remove_child(from)
		answer_layout_container.add_child(to)

		_vertical_layout = set_to


func _update_spacing() -> void:
	if not is_inside_tree():
		return
	qa_offset.custom_minimum_size = Vector2(_qa_offset_value, 0)
	qa_spacing.custom_minimum_size = Vector2(0, _qa_spacing_value)
