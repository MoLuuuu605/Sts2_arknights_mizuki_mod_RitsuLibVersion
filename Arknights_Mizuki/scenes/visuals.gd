extends AnimatedSprite2D

const IDLE := "idle_loop"

func _ready():
	animation_finished.connect(_on_animation_finished)
	if sprite_frames != null and sprite_frames.has_animation(IDLE):
		play(IDLE)

func play_trigger(trigger: String):
	if sprite_frames == null:
		return

	var animation_name := _resolve_animation(trigger)
	if animation_name != "" and sprite_frames.has_animation(animation_name):
		play(animation_name)

func _resolve_animation(trigger: String) -> String:
	match trigger:
		"Idle":
			return IDLE
		"Attack":
			return _first_existing(["Attack", "attack"])
		"Cast":
			return _first_existing(["Cast", "skill", "Buff", "skill2"])
		"Buff":
			return _first_existing(["Buff", "skill2", "Cast", "skill"])
		"Summon":
			return _first_existing(["Summon", "skill2", "Cast", "skill"])
		"Dead", "DeadTrigger":
			return _first_existing(["Dead", "die"])
		"Hit":
			return _first_existing(["Hit", "hit"])
		_:
			return trigger

func _first_existing(candidates: Array[String]) -> String:
	for candidate in candidates:
		if sprite_frames != null and sprite_frames.has_animation(candidate):
			return candidate
	return ""

func _on_animation_finished():
	if animation != "Dead" and animation != "die" and animation != IDLE and sprite_frames != null and sprite_frames.has_animation(IDLE):
		play(IDLE)
