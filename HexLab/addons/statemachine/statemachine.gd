extends Node
class_name StateMachine

var states : Dictionary = {}
var current_state : State

@export var initial_state : State


func _ready() -> void:
	for child in get_children():
		if child is State :
			states[child.name.to_lower()] = child
			child.Transitioned.connect(on_state_transition)
			
	assert(initial_state, "StateMachine intitial state = null")
	initial_state.Enter()
	current_state = initial_state

func _process(delta: float) -> void:
	if current_state : current_state.Update(delta)
	

func _physics_process(delta: float) -> void:
	if current_state : current_state.PhysicsUpdate(delta)
	
	
func on_state_transition(_state : State, new_state_name : String):
	assert(current_state == _state, "Failed State Transition : Exiting State (" + _state.name + ") is different from Current State (" + new_state_name +")")
	
	var new_state : State = states.get(new_state_name.to_lower())
	
	assert(new_state, "State Machine does not contain a state with name :" + new_state_name)
	assert(current_state, "Current State is Null")
	
	current_state.Exit()
	new_state.Enter()
	current_state = new_state
	
