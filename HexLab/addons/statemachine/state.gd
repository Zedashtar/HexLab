extends Node
class_name State

signal Transitioned(state : State, new_state_name : String)

func Enter():
	pass

func Exit():
	pass
	
func Update(_delta : float):
	pass
	
func PhysicsUpdate(_delta : float):
	pass
