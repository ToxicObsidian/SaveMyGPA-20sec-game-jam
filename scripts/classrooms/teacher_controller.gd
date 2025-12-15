extends Node2D
class_name TeacherController

@export var asprite: AnimatedSprite2D

func play_walk():
	asprite.play("walk")
