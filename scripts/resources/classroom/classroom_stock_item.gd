extends Resource
class_name ClassroomStockItem

enum SupportedClassroomType {
	NONE = 0,
	CLASSIC = 1,
}

@export_group("Meta")
@export var classroom_name: String = "classroom"

@export var classroom_type: SupportedClassroomType = SupportedClassroomType.CLASSIC

@export var matrices: Array[ClassroomMatrixSettings] = []


@export_group("Stock Resources")
@export var classmates: ClassmateData
@export var questions: Questions
@export var classroom_scene: PackedScene
@export var seat_suite: PackedScene
@export var teacher: PackedScene
@export var question_layout: PackedScene
