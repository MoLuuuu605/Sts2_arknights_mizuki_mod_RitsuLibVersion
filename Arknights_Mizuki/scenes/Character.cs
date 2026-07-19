extends SpineSprite

const IDLE := "Idle_loop"

func _ready():
	get_animation_state().set_animation(IDLE, true, 0)

func play_trigger(trigger: String):
	match trigger:
		"Idle":
			get_animation_state().set_animation(IDLE, true, 0)
		"Attack":
			get_animation_state().set_animation("Attack", false, 0)
			get_animation_state().add_animation(IDLE, 0.0, true, 0)
