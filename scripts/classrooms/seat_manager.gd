@tool
extends Node2D
class_name SeatManager

signal seat_loading(current_index: int, total_count: int)
signal seats_loaded()
signal on_determined_answer(type: int, question_index: int)

@export var start_point: Vector2:
	get:
		return _start_point
	set(value):
		_start_point = value
		queue_redraw()
		_reposition_seats()

@export var rows: int = 5:
	get:
		return _rows
	set(value):
		_rows = value
		queue_redraw()

@export var columns: int = 5:
	get:
		return _columns
	set(value):
		_columns = value
		queue_redraw()

@export var horizontal_interval: float = 50.0:
	get:
		return _horizontal_interval
	set(value):
		_horizontal_interval = value
		queue_redraw()
		_reposition_seats()

@export var vertical_interval: float = 50.0:
	get:
		return _vertical_interval
	set(value):
		_vertical_interval = value
		queue_redraw()
		_reposition_seats()

@export var margin_top: float:
	get:
		return _margin_top
	set(value):
		_margin_top = value
		queue_redraw()

@export var margin_bottom: float:
	get:
		return _margin_bottom
	set(value):
		_margin_bottom = value
		queue_redraw()

var player: SeatController:
	get:
		return _player

var _start_point: Vector2
var _rows: int = 5
var _columns: int = 5
var _horizontal_interval: float = 50.0
var _vertical_interval: float = 50.0
var _margin_top: float = 0.0
var _margin_bottom: float = 0.0
var _player: SeatController

var _seat_anchor: Node2D = null
var _seat_matrix: Array = []
var _replyables: Array[SeatController] = []


func _ready() -> void:
	if Engine.is_editor_hint():
		queue_redraw()


func _process(_delta: float) -> void:
	if Engine.is_editor_hint():
		queue_redraw()


func create_seats(seat_prefab: PackedScene) -> void:
	if _seat_anchor != null:
		return

	_seat_anchor = Node2D.new()
	add_child(_seat_anchor)
	_seat_anchor.position = start_point

	var current_count := 0
	_seat_matrix.clear()

	for i in range(rows):
		var column: Array[SeatController] = []
		for j in range(columns):
			var node := seat_prefab.instantiate() as SeatController
			_seat_anchor.add_child(node)
			node.position = Vector2(i * horizontal_interval, j * vertical_interval)
			column.append(node)

			current_count += 1
			emit_signal("seat_loading", current_count, rows * columns)
			if current_count % GameManager.BatchLPF == 0:
				await GameManager.game_root.get_tree().process_frame
		_seat_matrix.append(column)

	emit_signal("seats_loaded")


func release_seats() -> void:
	for c in _seat_matrix:
		c.clear()
	_seat_matrix.clear()
	_seat_anchor.queue_free()


func setup_seats(
	paper: PaperController,
	player_coord: Vector2i,
	classmate_textures: Array,
	special_character_coords: Array[Vector2i],
	classmate_nbe: Array,
	classmate_answers: Dictionary,
	total_question_count: int,
	hide_question_count: int
) -> void:
	for i in range(rows):
		for j in range(columns):
			var cur_seat: SeatController = _seat_matrix[i][j]

			if player_coord.x == i and player_coord.y == j:
				cur_seat.type = SeatController.SeatType.PLAYER
				_player = cur_seat
			elif player_coord.x - 1 == i and player_coord.y == j:
				cur_seat.type = SeatController.SeatType.LEFT
			elif player_coord.x + 1 == i and player_coord.y == j:
				cur_seat.type = SeatController.SeatType.RIGHT
			elif player_coord.x == i and player_coord.y - 1 == j:
				cur_seat.type = SeatController.SeatType.FRONT

			var ctextures = classmate_textures[i][j]
			cur_seat.set_character_textures(ctextures[0], ctextures[1], ctextures[2])

			if cur_seat.replyable:
				_replyables.append(cur_seat)
				await cur_seat.setup_seat(total_question_count, hide_question_count)
				cur_seat.on_determined_answer.connect(_on_classmate_determined_answer)
				paper.paper_first_flipped.connect(cur_seat.interaction.set_all_button_visible)

				if classmate_answers.has(cur_seat.type):
					cur_seat.set_answers(classmate_answers[cur_seat.type])

	for ctt in classmate_nbe:
		var coord: Vector2i = ctt[0]
		var texture: Texture2D = ctt[1]
		var seat: SeatController = _seat_matrix[coord.x][coord.y]
		seat.set_normal_bubble_emotion(
			texture,
			special_character_coords.has(coord) and not seat.replyable
		)
		if (special_character_coords.has(coord)):
			print("%v is special, replyable is %s" % [coord, seat.replyable])


func get_collect_paper_area_coord() -> Vector2:
	if _seat_matrix == null or _seat_matrix.is_empty():
		push_error("The seat manager has not created seats yet, but queried the CPA coord. Please check the logic.")
	return _seat_matrix[0][0].get_collect_paper_area_position()


func set_collect_direction(right_collect: bool) -> void:
	if _seat_matrix == null or _seat_matrix.is_empty():
		push_error("The seat manager has not created seats yet, but requests to set CPA coord. Please check the logic.")
	for row in _seat_matrix:
		for seat in row:
			seat.collect_direction = right_collect


func set_interaction_button_disabled(disabled: bool, index: int = -1) -> void:
	for i in range(_rows):
		for j in range(_columns):
			var seat: SeatController = _seat_matrix[i][j]
			if seat.replyable:
				if index == -1:
					seat.interaction.set_all_buttons_disabled(disabled)
				else:
					seat.interaction.set_button_disabled(index, disabled)


func _draw() -> void:
	if not Engine.is_editor_hint():
		return

	draw_line(start_point + Vector2.UP * 8.0, start_point + Vector2.DOWN * 8.0, Color.YELLOW, 5)
	draw_line(start_point + Vector2.LEFT * 8.0, start_point + Vector2.RIGHT * 8.0, Color.YELLOW, 5)

	draw_line(
		Vector2(start_point.x - 50, start_point.y - margin_top),
		Vector2(start_point.x + (_columns - 1) * _horizontal_interval + 50, start_point.y - margin_top),
		Color.GREEN,
		5
	)
	draw_line(
		Vector2(start_point.x - 50, start_point.y + (_rows - 1) * _vertical_interval + margin_bottom),
		Vector2(start_point.x + (_columns - 1) * _horizontal_interval + 50, start_point.y + (_rows - 1) * _vertical_interval + margin_bottom),
		Color.GREEN,
		5
	)

	for i in range(_rows):
		for j in range(_columns):
			if i == 0 and j == 0:
				continue
			var pos := Vector2(start_point.x + i * _horizontal_interval, start_point.y + j * _vertical_interval)
			draw_circle(pos, 4.0, Color.RED)


func _reposition_seats() -> void:
	if Engine.is_editor_hint():
		return

	for i in range(_seat_matrix.size()):
		for j in range(_seat_matrix[i].size()):
			_seat_matrix[i][j].position = Vector2(i * horizontal_interval, j * vertical_interval)


func _on_classmate_determined_answer(type: int, question_index: int) -> void:
	emit_signal("on_determined_answer", type, question_index)
	for seat in _replyables:
		if seat.type != type:
			seat.notify_close_bubble(question_index)
