extends Node2D
class_name BubbleController

signal on_answer_determined
signal on_answer_disposed

@export var bubble: Node2D
@export var answer: Sprite2D
@export var emotion: Sprite2D
@export var determine_answer: TextureButton
@export var dispose_answer: TextureButton

func _ready() -> void:
	if determine_answer:
		determine_answer.pressed.connect(func(): emit_signal("on_answer_determined"))
	if dispose_answer:
		dispose_answer.pressed.connect(func(): emit_signal("on_answer_disposed"))
