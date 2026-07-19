extends SpineSprite

const IDLE := "Idle_loop"

func _ready():
	get_animation_state().set_animation(IDLE, true, 0)
