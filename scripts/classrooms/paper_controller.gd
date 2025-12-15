extends Sprite2D
class_name PaperController

signal paper_first_flipped

@export var front_flip: Panel
@export var back_flip: Panel
@export var front: VBoxContainer
@export var back: VBoxContainer

@export var container_spacing: int = 15:
	set(v):
		container_spacing = v
		_update_container_spacing()

var side: bool = true: # true = front, false = back
	set(v):
		side = v
		_update_paper_side()
		
var total_count: int:
	get():
		return _questions.size()

var hide_count: int:
	get():
		return _hide_count
		
var first_flipped: bool:
	get():
		return _paper_has_flipped

var _paper_has_flipped := false
var _questions: Array[QuestionController] = []
var _hide_count := 0

func _ready() -> void:
	front_flip.gui_input.connect(_on_flip_pressed)
	back_flip.gui_input.connect(_on_flip_pressed)
	_update_paper_side()
	_update_container_spacing()

func get_qc(index: int):
	if index < 0 or index >= _questions.size():
		return null
	return _questions[index]

func setup_qcs(qc_scene: PackedScene, total_count_: int, hide_count_: int) -> void:
	for c in front.get_children():
		c.queue_free()
	for c in back.get_children():
		c.queue_free()
	_questions.clear()

	_hide_count = hide_count_
	for i in total_count_:
		var qc = qc_scene.instantiate()
		_questions.append(qc)
		if i + hide_count_ >= total_count_:
			back.add_child(qc)
		else:
			front.add_child(qc)

		if (i + 1) % GameManager.BatchLPF == 0:
			await GameManager.game_root.get_tree().process_frame

func qc_setup(
	index: int,
	question_texture: Texture2D,
	answer_textures: Array[Texture2D],
	oqa_position: Vector2,
	display_answers: bool,
	vertical_layout: bool
) -> void:
	if index < 0 or index >= _questions.size():
		push_error("QC index out of range: %d" % index)
		return
	var qc = _questions[index]
	qc.setup(question_texture, answer_textures, oqa_position, display_answers)
	qc.vertical_answer_layout = vertical_layout

func set_on_question_answer_texture(index: int, oqa_texture: Texture2D, oqa_scale: Vector2) -> void:
	if index >= _questions.size():
		push_warning("OQA index mismatched.")
		return
	_questions[index].set_oqa(oqa_texture, oqa_scale)

func flip_paper() -> void:
	if not _paper_has_flipped:
		emit_signal("paper_first_flipped")
		_paper_has_flipped = true
	side = not side
	AudioManager.play_sfx("flip_paper")

func _on_flip_pressed(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		flip_paper()

func _update_paper_side() -> void:
	front.visible = side
	front_flip.visible = side
	back.visible = not side
	back_flip.visible = not side
	flip_h = not side

func _update_container_spacing() -> void:
	if not is_inside_tree():
		return
	front.add_theme_constant_override("seperation", container_spacing)
	back.add_theme_constant_override("seperation", container_spacing)
