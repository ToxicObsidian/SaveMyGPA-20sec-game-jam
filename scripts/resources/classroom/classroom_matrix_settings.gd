extends Resource
class_name ClassroomMatrixSettings

@export var matrix_size: Vector2i:
	get:
		return _matrix_sizes
	set(value):
		_matrix_sizes = value

@export var matrix_offset: Vector2:
	get:
		return _matrix_offset
	set(value):
		_matrix_offset = value

@export var matrix_interval: Vector2:
	get:
		return _matrix_interval
	set(value):
		_matrix_interval = value

@export var allowed_player_coordinates: Array[Vector2i]:
	get:
		return _allowed_player_coordinates
	set(value):
		_allowed_player_coordinates = value

@export var matrix_margin_topdown: Vector2:
	get:
		return _matrix_margin_topdown
	set(value):
		_matrix_margin_topdown = value


var _matrix_sizes: Vector2i
var _matrix_offset: Vector2
var _matrix_interval: Vector2
var _allowed_player_coordinates: Array[Vector2i]
var _matrix_margin_topdown: Vector2
