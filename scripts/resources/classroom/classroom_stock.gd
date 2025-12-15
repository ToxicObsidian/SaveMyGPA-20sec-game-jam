extends Resource
class_name ClassroomStock

@export var items: Array[ClassroomStockItem]

func get_random_one(excluded_names: Array[String]) -> ClassroomStockItem:
	var excluded_set := {}
	for n in excluded_names:
		excluded_set[n] = true

	var valid_set: Array[ClassroomStockItem] = []
	for item in items:
		if item != null and not excluded_set.has(item.classroom_name):
			valid_set.append(item)

	if valid_set.is_empty():
		return null

	return valid_set[randi() % valid_set.size()]

func get_item_by_name(name: String) -> ClassroomStockItem:
	for item in items:
		if item.classroom_name == name:
			return item
	push_error("%s is invalid." % name)
	return null
