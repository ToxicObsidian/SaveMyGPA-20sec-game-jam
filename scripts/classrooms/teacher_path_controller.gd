extends Node2D
class_name TeacherPathController

@export var path_line: Path2D
@export var path_follow: PathFollow2D

var start_time := -1.0
var end_time := -1.0
var _walk_tween: Tween
var _teacher_base
var _right_side := true

signal walking_started
signal walking_finished

func get_progress() -> float:
	return path_follow.progress_ratio

func get_walk_tween() -> Tween:
	return _walk_tween


func set_walk_path(
	matrix_startpoint: Vector2,
	matrix_size: Vector2i,
	matrix_interval: Vector2,
	matrix_margin_topdown: Vector2,
	collect_paper_area_coord: Vector2,
	player_coord: Vector2i
) -> float:
	path_follow.rotates = false
	path_line.curve = Curve2D.new()

	_right_side = collect_paper_area_coord.x > 0

	var start := matrix_size.x - 1 if _right_side else 0
	var end := player_coord.x - 1 if _right_side else player_coord.x + 1
	var step := -1 if _right_side else 1

	var flip_dir := false
	var i := start
	while i != end:
		var point_x = i * matrix_interval.x + collect_paper_area_coord.x
		var top_y = -matrix_margin_topdown.x
		var bottom_y = (
			matrix_margin_topdown.y + matrix_interval.y * player_coord.y + collect_paper_area_coord.y
			if i == player_coord.x
			else matrix_margin_topdown.y + matrix_interval.y * (matrix_size.y - 1)
		)

		var top_point = Vector2(point_x, top_y) + matrix_startpoint
		var bottom_point = Vector2(point_x, bottom_y) + matrix_startpoint

		if flip_dir:
			path_line.curve.add_point(bottom_point)
			path_line.curve.add_point(top_point)
		else:
			path_line.curve.add_point(top_point)
			path_line.curve.add_point(bottom_point)

		flip_dir = !flip_dir
		i += step

	return 0.0


func set_teacher(teacher_base):
	if teacher_base.get_parent():
		teacher_base.get_parent().remove_child(teacher_base)
	add_child(teacher_base)
	teacher_base.visible = false
	_teacher_base = teacher_base
	if _right_side:
		_teacher_base.scale.x = -_teacher_base.scale.x


func release_teacher() -> Node2D:
	if _teacher_base:
		_teacher_base.get_parent().remove_child(_teacher_base)
		var t = _teacher_base
		_teacher_base = null
		return t
	return null


func start_teacher_walk(walk_duration: float, hide_after_walk := true) -> void:
	if _walk_tween:
		push_error("Cannot start teacher walk, tween is busy")
		return

	var tween := create_tween()

	tween.tween_callback(func ():
		remove_child(_teacher_base)
		path_follow.add_child(_teacher_base)
		_teacher_base.visible = true
		_teacher_base.play_walk()
		_walk_tween = tween
		emit_signal("walking_started")
	)

	tween.tween_method(
		func (t: float): _teacher_movement(t),
		0.0,
		1.0,
		walk_duration
	).set_trans(Tween.TRANS_LINEAR)

	tween.tween_callback(func ():
		_walk_tween = null
		if hide_after_walk:
			_teacher_base.visible = false
		emit_signal("walking_finished")
	)


func _teacher_movement(pos_ratio: float) -> void:
	path_follow.progress_ratio = pos_ratio
	if path_follow.position.y - path_line.curve.get_point_position(0).y > 1:
		path_follow.z_index = 1
	else:
		path_follow.z_index = 0
