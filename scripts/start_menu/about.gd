extends Control
class_name About

@export var detection_bg: ColorRect
@export var version_label: Label

func _ready() -> void:
	set_process(true)

	if detection_bg:
		detection_bg.gui_input.connect(_on_detection_bg_pressed)

	# 用“显式栈”遍历所有子节点（包括孙子节点）
	var children_stack: Array[Node] = get_children()

	while children_stack.size() > 0:
		var child: Node = children_stack.pop_back()

		if child is RichTextLabel:
			child.meta_clicked.connect(_on_rtl_url_clicked)

		for node in child.get_children():
			children_stack.append(node)
			
	version_label.text = "%s" % GameManager.version_name


func _process(_delta: float) -> void:
	if detection_bg.visible and Input.is_action_just_pressed("ui_cancel"):
		visible = false


func _on_detection_bg_pressed(event: InputEvent) -> void:
	if event is InputEventMouseButton \
	and event.pressed \
	and event.button_index == MOUSE_BUTTON_LEFT:
		visible = false


func _on_rtl_url_clicked(meta: Variant) -> void:
	var url: String = str(meta)

	if url.begins_with("http"):
		print("Open url: ", url)
		OS.shell_open(url)
