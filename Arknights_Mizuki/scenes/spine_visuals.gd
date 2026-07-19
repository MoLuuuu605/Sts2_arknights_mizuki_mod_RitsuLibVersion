extends SpineSprite

@export var idle_animation := "Idle"
@export var attack_animation := "Attack"
@export var cast_animation := "Skill_1"
@export var buff_animation := "Skill_2"
@export var summon_animation := "Skill_1"
@export var dead_animation := "Die"
@export var hit_animation := ""
@export var idle_after_buff_animation := ""
@export var attack_startup_speed := 1.0
@export var attack_startup_duration := 0.0

var _is_dead := false
var _attack_speed_token := 0

func _ready():
	if has_signal("animation_completed"):
		animation_completed.connect(_on_animation_completed)
	call_deferred("_play_idle")

func play_trigger(trigger: String):
	var animation_name := _resolve_animation(trigger)
	if animation_name == "":
		return

	var state = get_animation_state()
	if state == null:
		return

	var loop := animation_name == idle_animation
	var is_dead_animation := trigger == "Dead" or trigger == "DeadTrigger" or animation_name == dead_animation
	_is_dead = is_dead_animation
	if trigger == "Buff" and idle_after_buff_animation != "":
		idle_animation = idle_after_buff_animation
	_attack_speed_token += 1
	var speed_token := _attack_speed_token
	var track_entry = state.set_animation(animation_name, loop, 0)
	if trigger == "Attack" and attack_startup_speed > 1.0 and attack_startup_duration > 0.0:
		_speed_up_attack_startup(track_entry, speed_token)
	if not loop and not is_dead_animation and idle_animation != "":
		state.add_animation(idle_animation, 0.0, true, 0)

func _play_idle():
	if _is_dead or idle_animation == "":
		return
	var state = get_animation_state()
	if state != null:
		state.set_animation(idle_animation, true, 0)

func _on_animation_completed(_track_entry = null, _animation_state = null, _spine_sprite = null):
	if not _is_dead:
		return
	var state = get_animation_state()
	if state == null:
		return
	var current = state.get_current(0)
	if current == null:
		return
	if current.has_method("set_track_time") and current.has_method("get_animation_end"):
		current.set_track_time(current.get_animation_end())
	if current.has_method("set_time_scale"):
		current.set_time_scale(0.0)
	state.update(0.0)

func _speed_up_attack_startup(track_entry, speed_token: int):
	if track_entry == null or not track_entry.has_method("set_time_scale"):
		return

	track_entry.set_time_scale(attack_startup_speed)
	await get_tree().create_timer(attack_startup_duration).timeout

	if speed_token != _attack_speed_token:
		return

	var state = get_animation_state()
	if state == null:
		return
	var current = state.get_current(0)
	if current != null and current.has_method("set_time_scale"):
		current.set_time_scale(1.0)

func _resolve_animation(trigger: String) -> String:
	match trigger:
		"Idle":
			return idle_animation
		"Attack":
			return attack_animation
		"Cast":
			return cast_animation
		"Buff":
			return buff_animation
		"Summon":
			return summon_animation
		"Dead", "DeadTrigger":
			return dead_animation
		"Hit":
			return hit_animation
		_:
			return trigger
