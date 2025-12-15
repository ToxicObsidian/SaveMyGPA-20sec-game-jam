extends Node

@export_group("Bus Settings")
@export var master_bus_name: String = "Master"
@export var sfx_bus_name: String = "SFX"
@export var bgm_bus_name: String = "BGM"

@export_range(1, 20) var sfx_bus_count: int = 8

@export_group("Tracks")
@export var bgm_tracks: Dictionary[String, AudioStream]
@export var sfx_tracks: Dictionary[String, AudioStream]

var _bgm_player: AudioStreamPlayer
var _sfx_players: Array[AudioStreamPlayer] = []


func _ready() -> void:
	# BGM player
	_bgm_player = AudioStreamPlayer.new()
	_bgm_player.bus = bgm_bus_name
	add_child(_bgm_player)

	# SFX players pool
	for i in sfx_bus_count:
		var sfx := AudioStreamPlayer.new()
		sfx.bus = sfx_bus_name
		_sfx_players.append(sfx)
		add_child(sfx)


# ======================
# BGM
# ======================

func play_bgm(bgm_name: String) -> void:
	if not bgm_tracks.has(bgm_name):
		return

	var target_stream: AudioStream = bgm_tracks[bgm_name]
	var current_stream: AudioStream = _bgm_player.stream

	if current_stream == target_stream and _bgm_player.playing:
		return

	# Enable loop based on stream type
	if target_stream is AudioStreamOggVorbis:
		target_stream.loop = true
	elif target_stream is AudioStreamWAV:
		target_stream.loop_mode = AudioStreamWAV.LOOP_FORWARD
	elif target_stream is AudioStreamMP3:
		target_stream.loop = true

	_bgm_player.stop()
	_bgm_player.stream = target_stream
	_bgm_player.play()


func pause_bgm() -> void:
	_bgm_player.stream_paused = true


func resume_bgm() -> void:
	_bgm_player.stream_paused = false


func fade_out_current_bgm(fade_out_time: float) -> void:
	var tween := create_tween()
	tween.tween_method(
		func(v: float) -> void:
			_bgm_player.volume_linear = v,
		1.0,
		0.0,
		fade_out_time
	).set_trans(Tween.TRANS_LINEAR).set_ease(Tween.EASE_OUT)

	tween.tween_callback(func() -> void:
		_bgm_player.volume_linear = 1.0
		_bgm_player.stop()
	)


# ======================
# SFX
# ======================

func play_sfx(sfx_name: String) -> void:
	if not sfx_tracks.has(sfx_name):
		return

	var target_stream: AudioStream = sfx_tracks[sfx_name]

	for channel in _sfx_players:
		if not channel.playing:
			channel.stream = target_stream
			channel.stream_paused = false
			channel.play()
			return

	# fallback: reuse first channel
	var channel := _sfx_players[0]
	channel.stream = target_stream
	channel.stream_paused = false
	channel.play()


# ======================
# Volume Control
# ======================

func set_master_volume(volume: float) -> void:
	_set_bus_volume(master_bus_name, volume)


func get_master_volume_linear() -> float:
	return _get_bus_volume_linear(master_bus_name)


func set_sfx_volume(volume: float) -> void:
	_set_bus_volume(sfx_bus_name, volume)


func get_sfx_volume_linear() -> float:
	return _get_bus_volume_linear(sfx_bus_name)


func set_bgm_volume(volume: float) -> void:
	_set_bus_volume(bgm_bus_name, volume)


func get_bgm_volume_linear() -> float:
	return _get_bus_volume_linear(bgm_bus_name)


func _set_bus_volume(bus_name: String, linear_volume: float) -> void:
	linear_volume = clamp(linear_volume, 0.0, 1.0)
	var idx := AudioServer.get_bus_index(bus_name)
	if idx >= 0:
		AudioServer.set_bus_volume_linear(idx, linear_volume)


func _get_bus_volume_linear(bus_name: String) -> float:
	var idx := AudioServer.get_bus_index(bus_name)
	if idx >= 0:
		return AudioServer.get_bus_volume_linear(idx)
	return 1.0
