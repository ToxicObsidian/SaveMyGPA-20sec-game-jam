extends Node

# Loading Scene
@export_group("Loading Scene")
@export var loading_scene_ease_duration: float = 0.5
@export var loading_scene_ease_type: Tween.EaseType
@export var loading_scene_transition_type: Tween.TransitionType

# Stock Items
@export_group("Stock Items")
@export var stock_classrooms: ClassroomStock

var busy: bool:
	get:
		return _ls_tween != null

var _loading_screen: LoadingScreen = null
var _ls_tween: Tween = null
var _ls_original_y: float = 0.0

var _current_classroom: ClassroomManager = null
var _previous_sli: StartLevelInfo = null

var _random: RandomNumberGenerator
var _img_count: int = 0

var next_single: float:
	get:
		return _random.randf()


func _ready() -> void:
	_random = RandomNumberGenerator.new()
	_random.seed = int(Time.get_unix_time_from_system())


## Instantiate and initiate a new level by the given StartLevelInfo.
func start_level(info: StartLevelInfo) -> void:
	if busy:
		return
	
	# 1. Change to loading screen.
	await _ease_in_loading_screen()
	_previous_sli = info
	
	# 2. Load classroom.
	var classroom_mgr = await _start_classroom(info)
	print("Game Root: ", GameManager.game_root.get_path())
	GameManager.game_root.add_child(classroom_mgr)
	
	# 3. Remove the loading screen.
	GameManager.notify_level_started()
	await _ease_out_loading_screen()
	
	# 4. Start game
	await classroom_mgr.start_game()


func _start_classroom(info: StartLevelInfo) -> ClassroomManager:
	# 1. Roll level info by start level info.
	var rolled_classroom_stock_item: ClassroomStockItem = stock_classrooms.get_random_one(info.excluded_tags)
	
	# Teacher
	var teacher_base: TeacherController = rolled_classroom_stock_item.teacher.instantiate()
	
	# Questions
	var qc_scene = rolled_classroom_stock_item.question_layout
	var q_total_count = 5
	var q_hide_count = 2
	var q_rolled_data = _roll_questions(
		rolled_classroom_stock_item.questions.data,
		q_total_count,
		GameManager.settings.multi_choice_weight,
		GameManager.settings.true_false_weight,
		GameManager.settings.fill_in_blank_weight
	)
	var q_cwa = Array(rolled_classroom_stock_item.questions.common_wrong_answers)
	var q_strikes = Array(rolled_classroom_stock_item.questions.strikes)
	var q_scores: Array[float] = []
	for q in q_rolled_data:
		q_scores.append(float(q.score))
	
	var qc_setup_info: Array = []
	for d in q_rolled_data:
		var all_textures = d.all_textures()
		var shuffled_textures: Array = []
		for bundle in all_textures:
			shuffled_textures.append(bundle.answer_texture)
		shuffled_textures.shuffle()
		qc_setup_info.append([
			d.question_texture,
			shuffled_textures,
			d.on_question_answer_position,
			d.type,
			d.vertical_layout
		])
	
	# Classmates
	var matrix_info = rolled_classroom_stock_item.matrices.pick_random()
	var player_coord = matrix_info.allowed_player_coordinates.pick_random()
	var clsrm_scene = rolled_classroom_stock_item.seat_suite
	var clsmt_data = rolled_classroom_stock_item.classmates
	var complete_outfit_emotions: Dictionary = {}
	var special_character_coords: Array[Vector2i] = []
	var clsmt_textures = _roll_classmate_textures(
		clsmt_data,
		matrix_info.matrix_size,
		player_coord,
		special_character_coords,
		complete_outfit_emotions
	)
	var clsmt_reply_emotions = clsmt_data.classmate_reply_emotions
	var clsmt_normal_emotions = clsmt_data.classmate_normal_emotions
	var clsmt_nbe = _roll_normal_bubble_emotions(
		matrix_info.matrix_size,
		player_coord,
		Array(clsmt_normal_emotions),
		complete_outfit_emotions
	)
	
	var clsmt_answers = await _roll_classmate_answers(
		q_rolled_data,
		q_cwa,
		q_strikes,
		clsmt_reply_emotions,
		info.difficulty,
		info.max_difficulty,
		info.wrong_ratio,
		info.confused_ratio,
		info.hesitate_ratio,
		func(_d, _r):
			pass
	)
	
	# 2. Initialize the level by info.
	var classroom_mgr: ClassroomManager = rolled_classroom_stock_item.classroom_scene.instantiate()
	_current_classroom = classroom_mgr
	await classroom_mgr.load_classroom(
		# Seats
		matrix_info.matrix_size,
		matrix_info.matrix_offset,
		matrix_info.matrix_interval,
		clsrm_scene,
		player_coord,
		
		# Teacher
		teacher_base,
		matrix_info.matrix_margin_topdown,
		next_single < 0.5,
		
		# Classmate
		clsmt_textures,
		special_character_coords,
		clsmt_nbe,
		
		# Paper
		qc_scene,
		qc_setup_info,
		
		# Answers
		clsmt_answers,
		q_total_count,
		q_hide_count,
		q_scores
	)
	_current_classroom.classroom_ready.connect(_on_level_ready)
	_current_classroom.classroom_finished.connect(_on_level_finished)
	
	return classroom_mgr


func set_loading_screen(loading_screen: LoadingScreen) -> void:
	_loading_screen = loading_screen
	
	# Disable the screen.
	_loading_screen.visible = false
	
	# Hide the loading screen.
	var child_size = _loading_screen.get_child(0).size if _loading_screen.get_child_count() > 0 else Vector2.ZERO
	var viewport_size = get_viewport().get_visible_rect().size
	_loading_screen.position.y = min(
		_loading_screen.position.y,
		-max(child_size.y, viewport_size.y)
	)
	
	_ls_original_y = _loading_screen.position.y


func _on_level_ready() -> void:
	# Nothing todo yet.
	pass


func _on_level_finished(try_again: bool) -> void:
	if try_again:
		await _request_try_again()
	else:
		await _requested_back_to_menu()


func _request_try_again() -> void:
	# 1. Change to loading screen.
	await _ease_in_loading_screen()
	
	# 2. Release current classroom.
	_current_classroom.release_classroom()
	GameManager.game_root.remove_child(_current_classroom)
	_current_classroom.queue_free()
	_current_classroom = null
	
	# 3. Load a new classroom using previous info.
	var classroom_mgr = await _start_classroom(_previous_sli)
	GameManager.game_root.add_child(classroom_mgr)
	
	# 4. Remove the loading screen.
	await _ease_out_loading_screen()
	
	# 5. Notify the game start.
	await classroom_mgr.start_game()


func _requested_back_to_menu() -> void:
	# 1. Change to loading screen.
	await _ease_in_loading_screen()
	
	init_loading_progress(0.0, 1.0)
	update_loading_item("Releasing classroom resources")
	
	# 2. Release Classroom
	_current_classroom.release_classroom()
	update_loading_progress(0.5)
	
	# 3. Delete Classroom
	_current_classroom.queue_free()
	_current_classroom = null
	update_loading_progress(1.0)
	
	# 4. Notify to bring up start menu
	GameManager.notify_level_ended()
	# 5. Hide the loading screen.
	await _ease_out_loading_screen()


func _ease_in_loading_screen() -> void:
	_loading_screen.visible = true
	_ls_tween = create_tween()
	_ls_tween.tween_method(
		_screen_slide,
		_ls_original_y,
		0.0,
		loading_scene_ease_duration
	).set_trans(loading_scene_transition_type).set_ease(loading_scene_ease_type)
	await _ls_tween.finished


func _ease_out_loading_screen() -> void:
	_loading_screen.end_loading_screen()
	_ls_tween = create_tween()
	_ls_tween.tween_method(
		_screen_slide,
		0.0,
		_ls_original_y,
		loading_scene_ease_duration
	).set_trans(loading_scene_transition_type).set_ease(loading_scene_ease_type)
	await _ls_tween.finished
	_loading_screen.visible = false
	_ls_tween = null


func _screen_slide(rel_pos: float) -> void:
	_loading_screen.position.y = rel_pos


## This function shall be called by ClassroomManager, in load_classroom method.
func init_loading_progress(min_value: float, max_value: float) -> void:
	_loading_screen.start_loading_screen(min_value, max_value)


## This function shall be called by ClassroomManager, in load_classroom method.
func update_loading_progress(current_value: float) -> void:
	_loading_screen.loading_progress_bar.value = current_value


## This function shall be called by ClassroomManager, in load_classroom method.
func update_loading_item(item: String) -> void:
	_loading_screen.loading_item_label.text = item


func _roll_questions(
	data: Array,
	total_count: int,
	multichoice_weight: int = 5,
	truefalse_weight: int = 5,
	fillinblank_weight: int = 5,
	excluded_tags: Array = []
) -> Array:
	var result: Array = []
	
	var total_weight = multichoice_weight + truefalse_weight + fillinblank_weight
	var mc_chance = float(multichoice_weight) / float(total_weight)
	var tf_chance = float(truefalse_weight) / float(total_weight)
	
	var q_mc: Array = []
	var q_tf: Array = []
	var q_fb: Array = []
	
	for q in data:
		if q != null and q.is_valid():
			var has_excluded_tag = false
			if excluded_tags.size() > 0:
				for tag in q.tags:
					if excluded_tags.has(tag):
						has_excluded_tag = true
						break
			
			if not has_excluded_tag:
				if q.type == QuestionData.QuestionType.MULTI_CHOICE:
					q_mc.append(q)
				elif q.type == QuestionData.QuestionType.TRUE_FALSE:
					q_tf.append(q)
				elif q.type == QuestionData.QuestionType.FILL_IN_BLANK:
					q_fb.append(q)
				else:
					push_error("Question data invalid type: %s" % q.type)
	
	print("Size: Total: %d, MC: %d, TF: %d, FB: %d" % [data.size(), q_mc.size(), q_tf.size(), q_fb.size()])
	q_mc.shuffle()
	q_tf.shuffle()
	q_fb.shuffle()
	
	if q_fb.size() == 0 and fillinblank_weight > 0:
		push_error("Did not configured fill-in-blank questions, but weight > 0.")
		GameManager.quit_game()
	
	var mc_count = 0
	var tf_count = 0
	var fb_count = 0
	
	for i in range(total_count):
		var c = next_single
		
		# MultiChoices
		if c < mc_chance:
			print("Selected MC")
			if mc_count >= q_mc.size():
				push_error("Warning: MultiChoice questions will have duplicates")
				mc_count = 0
			result.append(q_mc[mc_count])
			mc_count += 1
		# TrueFalse
		elif c >= mc_chance and c < (tf_chance + mc_chance):
			print("Selected TF")
			if tf_count >= q_tf.size():
				push_error("Warning: TrueFalse questions will have duplicates")
				tf_count = 0
			result.append(q_tf[tf_count])
			tf_count += 1
		# Fill In Blank
		else:
			print("Selected FB")
			if fb_count >= q_fb.size():
				push_error("Warning: FillInBlank questions will have duplicates")
				fb_count = 0
			result.append(q_fb[fb_count])
			fb_count += 1
	
	result.sort_custom(func(qd1, qd2): return int(qd1.type) < int(qd2.type))
	
	return result


## Roll the classmate outfits.
## Returns: The classmate textures in array: [posture/body, hair, outfit]
func _roll_classmate_textures(
	data: ClassmateData,
	matrix_size: Vector2i,
	player_coord: Vector2,
	special_character_coords: Array[Vector2i],
	complete_outfit_emotions: Dictionary,
	sex_ratio: float = 0.5
) -> Array:
	var result: Array = []
	
	var complete_outfits: Array = []
	for i in range(data.allow_complete_outfits_duplicate):
		complete_outfits.append_array(data.complete_outfits)
	complete_outfits.shuffle()
	
	var co_ratio = float(complete_outfits.size()) / float(matrix_size.x * matrix_size.y)
	
	for i in range(matrix_size.x):
		var row: Array = []
		for j in range(matrix_size.y):
			# Complete outfit
			if (next_single < co_ratio and 
				complete_outfits.size() > 0 and 
				player_coord.x != i and 
				player_coord.y != j):
				var outfit_data = complete_outfits[0]
				var cur_coord = Vector2i(i, j)
				row.append([outfit_data.body, outfit_data.hair, outfit_data.outfit])
				complete_outfits.remove_at(0)
				special_character_coords.append(cur_coord)
				complete_outfit_emotions[cur_coord] = Array(outfit_data.special_emotions)
				continue
			
			# Boy
			if next_single < sex_ratio:
				var tp = data.boy_postures[_random.randi_range(0, data.boy_postures.size() - 1)]
				var th = data.boy_hairs[_random.randi_range(0, data.boy_hairs.size() - 1)]
				var to = data.boy_outfits[_random.randi_range(0, data.boy_outfits.size() - 1)]
				row.append([tp, th, to])
			# Girl
			else:
				var gp = data.girl_postures[_random.randi_range(0, data.girl_postures.size() - 1)]
				var gh = data.girl_hairs[_random.randi_range(0, data.girl_hairs.size() - 1)]
				var go = data.girl_outfits[_random.randi_range(0, data.girl_outfits.size() - 1)]
				row.append([gp, gh, go])
		result.append(row)
	
	return result


## Roll classmate answers, return in: [Answer, Emotion, OQA, OQA Position, Correctness]
func _roll_classmate_answers(
	q_data: Array,
	common_wrong_answers: Array,
	strikes: Array,
	emotions: Dictionary,
	difficulty: int,
	max_difficulty: int,
	wrong_ratio: float,
	confused_ratio: float,
	hesitate_ratio: float,
	overrides: Callable
) -> Dictionary:
	var result: Dictionary = {}
	var replyables = [
		SeatController.SeatType.LEFT,
		SeatController.SeatType.RIGHT,
		SeatController.SeatType.FRONT
	]
	
	for type in replyables:
		var replies: Array = []
		for i in range(q_data.size()):
			var wrong = next_single < wrong_ratio
			var confused = next_single < confused_ratio
			var hesitate = next_single < hesitate_ratio
			
			var reply_emotion = _roll_emotion_texture(
				emotions,
				difficulty,
				max_difficulty,
				wrong,
				confused,
				hesitate,
				confused_ratio,
				hesitate_ratio
			)
			var reply_answer
			if wrong:
				reply_answer = _roll_wrong_textures(i, q_data, common_wrong_answers, confused)
			else:
				reply_answer = q_data[i].correct_answer
			
			var reply_texture = reply_answer.oqa_texture
			var oqa_texture = reply_answer.oqa_texture
			var oqa_scale = reply_answer.oqa_scale
			
			if hesitate:
				var hesitate_images = await _roll_hesitated_answer_images(
					q_data,
					common_wrong_answers,
					strikes,
					i,
					confused_ratio,
					hesitate_ratio,
					4 if q_data[i].type == QuestionData.QuestionType.MULTI_CHOICE else \
					(1 if q_data[i].type == QuestionData.QuestionType.FILL_IN_BLANK else 4)
				)
				reply_texture = _compose_hesitated_answer(hesitate_images, reply_texture)
			
			print("Correctness of @%s#%d is %s" % [SeatController.SeatType.keys()[type], i, !wrong])
			replies.append([
				reply_texture,
				reply_emotion,
				oqa_texture,
				oqa_scale,
				!wrong
			])
		result[type] = replies
	
	if overrides.is_valid():
		await overrides.call(difficulty, result)
	
	return result


func _roll_normal_bubble_emotions(
	matrix_size: Vector2i,
	player_coord: Vector2i,
	emotions: Array,
	special_emotions: Dictionary
) -> Array:
	var result: Array = []
	
	for i in range(matrix_size.x):
		for j in range(matrix_size.y):
			var cur_coord = Vector2i(i, j)
			if (cur_coord == player_coord or
				(i == player_coord.x and j - 1 == player_coord.y) or
				(j == player_coord.y and abs(i - player_coord.x) == 1)):
				continue
			
			if special_emotions.has(cur_coord):
				var sp_emotion = special_emotions[cur_coord]
				result.append([
					cur_coord,
					sp_emotion[_random.randi_range(0, sp_emotion.size() - 1)]
				])
			else:
				result.append([
					cur_coord,
					emotions[_random.randi_range(0, emotions.size() - 1)]
				])
	
	return result


func _roll_wrong_textures(
	cur_index: int,
	questions: Array,
	common_wrong_answers: Array,
	confused: bool
) -> AnswerTextureBundle:
	if confused:
		var all_t: Array = []
		for idx in range(questions.size()):
			if idx != cur_index:
				var qd = questions[idx]
				for atb in qd.all_textures():
					if atb.oqa_texture != questions[cur_index].correct_answer.oqa_texture:
						all_t.append(atb)
		all_t.append_array(common_wrong_answers)
		all_t.shuffle()
		return all_t[_random.randi_range(0, all_t.size() - 1)]
	else:
		var wrong_options = questions[cur_index].wrong_answer_options
		return wrong_options[_random.randi_range(0, wrong_options.size() - 1)]


func _roll_emotion_texture(
	emotions: Dictionary,
	difficulty: int,
	max_difficulty: int,
	wrong: bool,
	confused: bool,
	hesitate: bool,
	confuse_ratio: float,
	hesitate_ratio: float
) -> Texture2D:
	if difficulty == max_difficulty:
		return emotions[Emotions.EmotionType.I_HAVE_AN_IDEA]
	
	if wrong:
		if hesitate and next_single < confuse_ratio:
			return emotions[Emotions.EmotionType.SAD]
		elif confused:
			return emotions[Emotions.EmotionType.LAUGH_CRY]
		elif hesitate:
			return emotions[Emotions.EmotionType.THINKING]
		else:
			return emotions[Emotions.EmotionType.I_HAVE_AN_IDEA]
	else:
		if hesitate:
			return emotions[Emotions.EmotionType.THINKING]
		elif not confused and next_single < hesitate_ratio:
			return emotions[Emotions.EmotionType.I_HAVE_AN_IDEA]
	
	return emotions[Emotions.EmotionType.CONFIDENCE]


func _roll_hesitated_answer_images(
	questions: Array,
	common_wrong_answers: Array,
	strikes: Array,
	cur_index: int,
	confused_ratio: float,
	hesitate_ratio: float,
	max_count: int
) -> Array[Image]:
	var result: Array[Image] = []
	var selected_answers: Array = []
	
	var total_answers: Array = []
	for qd in questions:
		total_answers.append_array(qd.all_textures())
	total_answers.append_array(common_wrong_answers)
	total_answers.shuffle()
	
	var cur_answers = questions[cur_index].all_textures()
	
	while true:
		if next_single < confused_ratio:
			selected_answers.append(total_answers[_random.randi_range(0, total_answers.size() - 1)])
		else:
			selected_answers.append(cur_answers[_random.randi_range(0, cur_answers.size() - 1)])
		
		if not (next_single < hesitate_ratio and result.size() < max_count):
			break
	
	# 2. Cover with strikes
	for i in range(selected_answers.size()):
		var t = selected_answers[i].oqa_texture
		var timg = t.get_image()
		var tsize = timg.get_size()
		var img = Image.create_empty(tsize.x, tsize.y, false, timg.get_format())
		var strikeimg = strikes[_random.randi_range(0, strikes.size() - 1)].get_image()
		_img_count += 1
		
		img.blit_rect(timg, Rect2i(0, 0, tsize.x, tsize.y), Vector2i.ZERO)
		img.blend_rect(strikeimg, Rect2i(0, 0, tsize.x, tsize.y), Vector2i.ZERO)
		result.append(img)
		
		if _img_count % GameManager.BatchTPPF == 0:
			await GameManager.game_root.get_tree().process_frame
	
	return result


func _compose_hesitated_answer(
	hesitate_images: Array[Image],
	final_texture: Texture2D
) -> Texture2D:
	var images: Array[Image] = hesitate_images.duplicate()
	images.append(final_texture.get_image())
	print("Compose: images length: %d" % images.size())
	
	var max_height = 0
	var width = 0
	for img in images:
		var isize = img.get_size()
		max_height = max(max_height, isize.y)
		width += isize.x
	
	var cum_width = 0
	var result_img = Image.create_empty(width, max_height, false, images[0].get_format())
	print("Compose: Result size: %dx%d" % [width, max_height])
	
	for i in range(images.size()):
		var isize = images[i].get_size()
		result_img.blit_rect(
			images[i],
			Rect2i(0, 0, isize.x, isize.y),
			Vector2i(cum_width, _random.randi_range(0, max_height - isize.y))
		)
		cum_width += isize.x
	
	return ImageTexture.create_from_image(result_img)
