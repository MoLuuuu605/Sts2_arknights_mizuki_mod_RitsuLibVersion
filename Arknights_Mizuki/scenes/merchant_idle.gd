extends SpineSprite

const IDLE := "Relax"

func _ready():
	call_deferred("_play_idle")

func _play_idle():
	var state = get_animation_state()
	if state != null:
		state.set_animation(IDLE, true, 0)
