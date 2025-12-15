extends Node2D
class_name ClassroomManager

signal classroom_ready
signal classroom_finished(try_again: bool)

enum LevelState {
	CREATED = -1,
	INITIALIZING = 0,
	OK = 1,
	PROCEEDING = 2,
	FINISHED = 3,
	UNLOADED = 4,
}
@export var PathController: TeacherPathController
@export var Seats: SeatManager
@export var Paper: PaperController
@export var EndGame: EndGameMenu

var state: LevelState = LevelState.CREATED:
	set(v):
		state = v
		if state == LevelState.OK:
			emit_signal("classroom_ready")

var _teacher_base: TeacherController

# loading
var _loading_cur_stage := 0
var _loading_total_stage := 0
var _loading_cur_index := 0
var _loading_total_index := 0

# answers
var _classmate_answers := {} # SeatType -> Array[[Texture2D, Vector2, bool]]
var _populated_answers: Array = []
var _points: Array = []

func load_classroom(
	# Seats
	matrix_size: Vector2i,
	matrix_offset: Vector2,
	matrix_interval: Vector2,
	seat_scene: PackedScene,
	player_coord: Vector2i,
	
	# Teacher
	teacher_base: TeacherController,
	matrix_margin_topdown: Vector2,
	teacher_collect_direction: bool,
	
	# Classmates
	classmate_textures: Array,
	special_character_coords: Array,
	classmate_nbe: Array,
	
	# Paper
	question_controller_scene: PackedScene,
	qc_setup_info: Array,
	
	# Answers
	classmate_answers: Dictionary,
	total_question_counts: int,
	hide_question_counts: int,
	question_points: Array
) -> void:
	if state != LevelState.CREATED:
		return

	if not Seats.seat_loading.is_connected(_on_seat_loading):
		Seats.seat_loading.connect(_on_seat_loading)

	_init_loading(3)
	state = LevelState.INITIALIZING

	_next_loading_stage("Loading seats")
	Seats.rows = matrix_size.x
	Seats.columns = matrix_size.y
	Seats.start_point = matrix_offset
	Seats.horizontal_interval = matrix_interval.x
	Seats.vertical_interval = matrix_interval.y
	Seats.margin_top = matrix_margin_topdown.x
	Seats.margin_bottom = matrix_margin_topdown.y
	await Seats.create_seats(seat_scene)

	_next_loading_stage("Loading classmates")
	_update_loading_index(1, 1)
	await Seats.setup_seats(
		Paper,
		player_coord,
		classmate_textures,
		special_character_coords,
		classmate_nbe,
		classmate_answers,
		total_question_counts,
		hide_question_counts
	)
	Seats.set_interaction_button_disabled(true)
	Seats.on_determined_answer.connect(_on_classmate_determined_answer)
	Seats.set_collect_direction(teacher_collect_direction)

	_next_loading_stage("Loading paper")
	_populated_answers.clear()
	_classmate_answers.clear()
	_points.clear()

	await Paper.setup_qcs(question_controller_scene, total_question_counts, hide_question_counts)

	for i in total_question_counts:
		_populated_answers.append(SeatController.SeatType.NORMAL)
		_points.append(question_points[i])

		for t in classmate_answers.keys():
			if not _classmate_answers.has(t):
				_classmate_answers[t] = []
			var a = classmate_answers[t][i]
			_classmate_answers[t].append([a[2], a[3], a[4]])

		var info = qc_setup_info[i]
		var ans_textures: Array[Texture2D]
		for t in info[1]:
			ans_textures.append(t)
		Paper.qc_setup(
			i,
			info[0],
			ans_textures,
			info[2],
			info[3] == QuestionData.QuestionType.MULTI_CHOICE,
			info[4]
		)

	_next_loading_stage("Loading teacher")
	_teacher_base = teacher_base
	PathController.set_walk_path(
		matrix_offset,
		matrix_size,
		matrix_interval,
		matrix_margin_topdown,
		Seats.get_collect_paper_area_coord(),
		player_coord
	)
	PathController.walking_started.connect(_teacher_walking_started)
	PathController.walking_finished.connect(_teacher_walking_finished)

	Seats.player.on_paper_collected.connect(stop_game)
	EndGame.on_request_try_again.connect(func(): emit_signal("classroom_finished", true))
	EndGame.on_request_back_to_menu.connect(func(): emit_signal("classroom_finished", false))

	await GameManager.game_root.get_tree().create_timer(2.0).timeout
	state = LevelState.OK

func release_classroom():
	Seats.release_seats()
	state = LevelState.UNLOADED
	queue_free()

func start_game() -> void:
	await get_tree().create_timer(0.5).timeout
	Seats.set_interaction_button_disabled(false)
	PathController.set_teacher(_teacher_base)
	PathController.start_teacher_walk(21.0, false)

func stop_game() -> void:
	await get_tree().create_timer(0.5).timeout
	Seats.set_interaction_button_disabled(true)

	var player_points := _calculate_marks()
	var total_points := _total_marks()
	var passed := _passed_exam(player_points, 0.6, total_points)

	var answered_front := 0
	for i in range(Paper.total_count - Paper.hide_count):
		var a = _populated_answers[i]
		if a != SeatController.SeatType.NORMAL and a != SeatController.SeatType.PLAYER:
			answered_front += 1

	EndGame.set_settlement(
		passed,
		player_points,
		total_points,
		not Paper.first_flipped and answered_front >= Paper.total_count - Paper.hide_count
	)

func _calculate_marks() -> float:
	var r := 0.0
	for i in _points.size():
		var t = _populated_answers[i]
		if t != SeatController.SeatType.NORMAL and t != SeatController.SeatType.PLAYER:
			if _classmate_answers[t][i][2]:
				r += _points[i]
	return r

func _total_marks() -> float:
	var r = 0
	for p in _points:
		r += p
	return r

func _passed_exam(cur: float, ratio: float, total: float) -> bool:
	if total == 0.0:
		return false
	return (cur / total) >= ratio


func _init_loading(total_stages: int) -> void:
	LevelManager.init_loading_progress(0.0, 1.0)
	_loading_cur_stage = 0
	_loading_total_stage = total_stages
	_loading_cur_index = 0
	_loading_total_index = 1

func _next_loading_stage(item: String) -> void:
	_loading_cur_stage += 1
	_loading_cur_index = 0
	_loading_total_index = 1
	_update_loading_screen()
	LevelManager.update_loading_item(item)

func _update_loading_index(cur: int, total: int) -> void:
	_loading_cur_index = cur
	_loading_total_index = total
	_update_loading_screen()

func _update_loading_screen() -> void:
	var p := (float(_loading_cur_stage - 1) + float(_loading_cur_index) / float(_loading_total_index)) / float(_loading_total_stage)
	LevelManager.update_loading_progress(p)

func _on_seat_loading(cur: int, total: int) -> void:
	_update_loading_index(cur, total)

func _on_classmate_determined_answer(t, idx: int) -> void:
	_populated_answers[idx] = t
	var a = _classmate_answers[t][idx]
	Paper.set_on_question_answer_texture(idx, a[0], a[1])
	Seats.set_interaction_button_disabled(true, idx)
	AudioManager.play_sfx("write_answer")

func _teacher_walking_started() -> void:
	pass

func _teacher_walking_finished() -> void:
	pass
