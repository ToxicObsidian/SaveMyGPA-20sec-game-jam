extends Node2D
class_name SeatController

signal on_determined_answer(type: int, question_index: int)
signal on_paper_collected

@export var character: CharacterController
@export var interaction: InteractionController
@export var collect_paper_area_left: Area2D
@export var collect_paper_area_right: Area2D

@export var desk: Sprite2D
@export var chair: Sprite2D

@export var normal_bubble: BubbleController
@export var left_bubble: BubbleController
@export var right_bubble: BubbleController
@export var front_bubble: BubbleController

@export var player_indicator: Control

enum SeatType { NORMAL, LEFT, FRONT, RIGHT, PLAYER }

@export var type: SeatType

var _answer_count := 0
var _answer_textures: Array[Texture2D] = []
var _answer_emotions: Array[Texture2D] = []
var _answer_scales: Array[Vector2] = []
var _answer_correctness: Array[bool] = []

var _right_collect := true
var _current_answer := -1
var _is_special_character := false

var collect_paper_area: Area2D:
	get():
		return collect_paper_area_right if _right_collect else collect_paper_area_left

var collect_direction: bool:
	get():
		return _right_collect
	set(value):
		_right_collect = value
		
var replyable: bool:
	get():
		return type != SeatType.NORMAL and type != SeatType.PLAYER

func _ready() -> void:
	if not replyable:
		interaction.visible = false
	else:
		interaction.on_query_answer.connect(_on_query_answer)
		current_bubble().determine_answer.pressed.connect(_on_query_submit_bubble)
		current_bubble().dispose_answer.pressed.connect(_on_query_cancel_bubble)

	collect_paper_area.area_entered.connect(_on_paper_collected)

	normal_bubble.visible = false
	left_bubble.visible = false
	right_bubble.visible = false
	front_bubble.visible = false

	player_indicator.visible = type == SeatType.PLAYER

	if replyable:
		current_bubble().z_index += 1
		


func current_bubble() -> BubbleController:
	match type:
		SeatType.LEFT: return left_bubble
		SeatType.RIGHT: return right_bubble
		SeatType.FRONT: return front_bubble
		_: return normal_bubble


func get_collect_paper_area_position() -> Vector2:
	return (collect_paper_area.get_parent() as Node2D).position


func setup_seat(total_question_count: int, hide_question_count: int) -> void:
	await interaction.set_buttons(total_question_count, hide_question_count)
	_answer_count = total_question_count


func set_character_textures(body: Texture2D, hair: Texture2D, outfit: Texture2D) -> void:
	character.body.texture = body
	character.hair.texture = hair
	character.outfit.texture = outfit


func set_answers(answers: Array) -> void:
	_answer_textures.clear()
	_answer_emotions.clear()
	_answer_scales.clear()
	_answer_correctness.clear()

	for a in answers:
		_answer_textures.append(a[0])
		_answer_emotions.append(a[1])
		_answer_scales.append(a[3])
		_answer_correctness.append(a[4])


func notify_close_bubble(question_index: int) -> void:
	if question_index != _current_answer:
		return
	_current_answer = -1
	_ease_current_bubble(1.0, 0.0, 0.25, false)
	interaction.visible = true


func set_normal_bubble_emotion(emotion: Texture2D, is_special: bool) -> void:
	normal_bubble.emotion.texture = emotion
	_is_special_character = is_special


func _on_query_answer(question_index: int) -> void:
	interaction.visible = false
	_show_answer(question_index)
	AudioManager.play_sfx("show_bubble")


func _on_query_submit_bubble() -> void:
	emit_signal("on_determined_answer", int(type), _current_answer)
	_current_answer = -1
	_ease_current_bubble(1.0, 0.0, 0.25, false)
	interaction.visible = true


func _on_query_cancel_bubble() -> void:
	_current_answer = -1
	_ease_current_bubble(1.0, 0.0, 0.25, false)
	AudioManager.play_sfx("hide_bubble")
	interaction.visible = true


func _show_answer(index: int) -> void:
	_current_answer = index
	current_bubble().answer.texture = _answer_textures[index]
	current_bubble().answer.scale = _answer_scales[index]
	current_bubble().emotion.texture = _answer_emotions[index]
	current_bubble().visible = true
	_ease_current_bubble(0.0, 1.0, 0.25)


func _ease_current_bubble(from: float, to: float, duration: float, visible_after := true) -> void:
	var t := create_tween()
	t.tween_method(func(r): current_bubble().scale = Vector2.ONE * r, from, to, duration)\
		.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	t.tween_callback(func(): current_bubble().visible = visible_after)


func _on_paper_collected(_area: Area2D) -> void:
	collect_paper_area.get_child(0).set_deferred("disabled", true)
	emit_signal("on_paper_collected")

	if replyable:
		interaction.set_all_buttons_disabled(true)
	elif type != SeatType.PLAYER:
		_show_normal_bubble(0.3, 2.5)


func _show_normal_bubble(chance: float, duration: float) -> void:
	if randf() >= chance and not _is_special_character:
		return

	var t := create_tween()
	t.tween_callback(func(): current_bubble().visible = true)
	t.tween_method(func(r): current_bubble().scale = Vector2.ONE * r, 0.0, 1.0, 0.25)\
		.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	t.tween_interval(duration)
	t.tween_method(func(r): current_bubble().scale = Vector2.ONE * r, 1.0, 0.0, 0.25)\
		.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	t.tween_callback(func(): current_bubble().visible = false)
